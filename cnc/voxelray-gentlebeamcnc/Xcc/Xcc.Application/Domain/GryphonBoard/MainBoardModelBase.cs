using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Events;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Xcc.Application.Domain.GryphonBoard
{
    public class MainBoardModelBase : IMainBoardModel
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        public GcbEmissionPlan CurrentPlan { get; protected set; } = null!;
        public ISystemTelemetry? SystemTelemetry { get; private set; }
        public GcbStateNew? State => SystemTelemetry?.ControlBoardState;
        protected IGcbCommandInterface GcbAPI { get; }
        protected IEventAggregator EventAggregator { get; }
        protected IGCBDataStore GcbDataStore { get; }
        protected ILogWriter LogWriter { get; }
        public bool IsPlanStaged { get; protected set; }
        public CancellationTokenSource CancellationTokenSource
        {
            get => _cancellationTokenSource;
            protected set
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource = value;
            }
        }
        public GcbSession? Session { get; protected set; }

        public event EventHandler<GcbActionCompletionEventArgs> GcbActionCompletionEvent = null!;

        public MainBoardModelBase(
            IGCBDataStore gcbDataStore,
            ILogWriter logWriter,
            IGcbCommandInterface gcbAPI,
            IEventAggregator eventAggregator)
        {
            GcbDataStore = gcbDataStore;
            LogWriter = logWriter;
            GcbAPI = gcbAPI;
            EventAggregator = eventAggregator;
        }


        #region public methods

        public GcbOperationalPoint CurrentPoint()
        {
            var pointIndex = SystemTelemetry?.CurrentOperationalPoint;
            if (pointIndex == null)
            {
                throw new NullReferenceException("MainBoardModelBase.CurrentPoint error: system telemetry is not set");
            }

            if (CurrentPlan is null || CurrentPlan.TotalPoints == 0)
            {
                throw new Exception("MainBoardModelBase.CurrentPoint error: current plan is not set or empty");
            }

            if (pointIndex >= CurrentPlan.TotalPoints)
            {
                throw new Exception("MainBoardModelBase.CurrentPoint error: current operational point index is out of bound");
            }

            return CurrentPlan[pointIndex.Value];
        }


        public void OnSystemTelemetryChanged(ISystemTelemetry systemTelemetry)
        {
            var previousState = State;
            SystemTelemetry = systemTelemetry;

            if (systemTelemetry != null)
            {
                GcbDataStore.Interlocks = GcbInterlocks.Create(systemTelemetry.InterlockFlags);
                GcbDataStore.Faults = new GcbFaults(systemTelemetry.FaultFlags);
                if (previousState != systemTelemetry?.ControlBoardState && systemTelemetry?.FaultFlags != 0)
                {
                    var faultList = GcbDataStore.Faults?.GetFaults();
                    var faults = (faultList == null) ? "No data" : string.Join('\n', faultList.Select(x => x.ToString()));
                    _ = LogWriter.LogAsync($"GCB went into a fault state: {systemTelemetry?.ControlBoardState}.\nReason: {faults}", LogRecordSeverity.Info, LogRecordType.System);
                }
            }
            else
            {
                GcbDataStore.Interlocks = null;
                GcbDataStore.Faults = null;

                var prevTelemetryValues = GcbDataStore.SystemTelemetry;
                if (prevTelemetryValues is { PrimaryTimerValue: > 0 })
                {
                    var (primaryTimerValue, secondaryTimer1Value, secondaryTimer2Value) 
                        = (prevTelemetryValues.PrimaryTimerValue, prevTelemetryValues.SecondaryTimer1Value, prevTelemetryValues.SecondaryTimer2Value);

                    _ = LogWriter.LogAsync(
                        $"GCB connection was lost. Last timer values are: {primaryTimerValue:F2}sec, {secondaryTimer1Value:F2}sec, {secondaryTimer2Value:F2}sec",
                        LogRecordSeverity.Info, LogRecordType.System);
                }
            }

            GcbDataStore.SystemTelemetry = SystemTelemetry;

            UpdateCurrentPlanState(systemTelemetry);
        }

        public virtual async Task Initialize()
        {
            _ = LogWriter.LogAsync("Initialize", LogRecordSeverity.Info, LogRecordType.System);

            await GcbAPI.Initialize();
        }

        protected virtual async Task Conditioning(float heaterCurrentSetpoint, CancellationToken cancellationToken)
        {
            var telemetry = SystemTelemetry ?? throw new Exception("Failed to check the GCB state before conditioning: GCB telemetry connection lost.");

            if (telemetry.ControlBoardState == GcbStateNew.Startup)
            {
                await Initialize();
            }

            _ = LogWriter.LogAsync("Conditioning", LogRecordSeverity.Info, LogRecordType.System);

            await GcbAPI.Conditioning(heaterCurrentSetpoint); // full warmup
        }

        protected virtual async Task WarmUp(float heaterCurrentSetpoint, CancellationToken cancellationToken)
        {
            _ = LogWriter.LogAsync("Warmup", LogRecordSeverity.Info, LogRecordType.System);

            await GcbAPI.WarmUp(heaterCurrentSetpoint); // fast warmup
        }

        protected async Task CreateNewSession(int operationalPointCount)
        {
            Session = await GcbAPI.NewSession(operationalPointCount);
            OnGcbActionCompletion(GcbActionType.NewSession);
        }

        public virtual async Task StagePlan()
        {
            _ = LogWriter.LogAsync("StagePlan", LogRecordSeverity.Info, LogRecordType.System);

            await GcbAPI.StagePlan();
            OnGcbActionCompletion(GcbActionType.StagePlan);
        }

        public virtual bool CanBeamOn()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            return telemetry.ControlBoardState == GcbStateNew.Ready;
        }

        public virtual async Task Stop()
        {
            _ = LogWriter.LogAsync("Stop", LogRecordSeverity.Info, LogRecordType.System);

            CancelCurrentTask();

            await GcbAPI.Stop();
            OnGcbActionCompletion(GcbActionType.Stop);
        }
        public virtual bool CanStop()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            var gcbState = telemetry.ControlBoardState;

            return gcbState == GcbStateNew.Warmup ||
                   gcbState == GcbStateNew.HVSetup ||
                   gcbState == GcbStateNew.Discharge ||
                   gcbState == GcbStateNew.HvpsCheck ||
                   gcbState == GcbStateNew.Ready ||
                   gcbState == GcbStateNew.Primed ||
                   gcbState == GcbStateNew.Launching ||
                   gcbState == GcbStateNew.LaunchingForImaging ||
                   gcbState == GcbStateNew.Emission ||
                   gcbState == GcbStateNew.Imaging;
        }

        public void CancelCurrentTask()
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = null!;
        }


        public virtual async Task ClearFaults()
        {
            _ = LogWriter.LogAsync("ClearFaults", LogRecordSeverity.Info, LogRecordType.System);

            await GcbAPI.ClearFaults();
            OnGcbActionCompletion(GcbActionType.ClearErrors);
        }

        public virtual bool CanClearFaults()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            var gcbState = telemetry.ControlBoardState;

            return
                gcbState == GcbStateNew.Fault ||
                gcbState == GcbStateNew.ColdFault ||
                gcbState == GcbStateNew.WarmupFault;
        }

        public virtual async Task ClearPlan()
        {
            _ = LogWriter.LogAsync("ClearPlan", LogRecordSeverity.Info, LogRecordType.System);

            var telemetry = SystemTelemetry;

            if (telemetry is not null)
            {

                // We can't clear plan from ready
                // without an explicit stop command turning the board to the Staged state first
                bool needToStop = telemetry.ControlBoardState == GcbStateNew.Ready;
                if (needToStop)
                {
                    await Stop();
                    var tokenSource = CancellationTokenSource = new CancellationTokenSource();
                    await WaitForState(GcbStateNew.Staged, tokenSource.Token);
                }

                if (telemetry.IsFaultState())
                    throw new InvalidOperationException("Cannot clear the plan in the Fault state");

                await GcbAPI.ClearPlan();
            }
            else
            {
                // As we lost connection to GCB,
                // we're not interested in actual ClearPlan outcome,
                // we just try our best clearing it:
                _ = GcbAPI.ClearPlan();
            }
            Session = null; // reset session, as it is tied to operational points count
            IsPlanStaged = false;
            OnGcbActionCompletion(GcbActionType.ClearPlan);
        }

        public virtual bool CanClearPlan()
        {
            var telemetry = SystemTelemetry;

            var gcbState = telemetry?.ControlBoardState ?? GcbStateNew.NoComm;

            return gcbState == GcbStateNew.Ready ||
                   gcbState == GcbStateNew.Cold ||
                   gcbState == GcbStateNew.StandBy ||
                   gcbState == GcbStateNew.Fault ||
                   gcbState == GcbStateNew.Staged ||
                   gcbState == GcbStateNew.Staging ||
                   gcbState == GcbStateNew.NoComm;
        }

        public virtual async Task<FaultEntry> GetFaults()
        {
            _ = LogWriter.LogAsync("GetFaults", LogRecordSeverity.Info, LogRecordType.System);

            return await GcbAPI.GetFaults();
        }

        public virtual async Task<VersionInfo> GetVersionInfo()
        {
            _ = LogWriter.LogAsync("GetVersionInfo", LogRecordSeverity.Info, LogRecordType.System);

            return await GcbAPI.GetVersionInfo();
        }

        public bool CanResetTimers()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            float maxTimer = Math.Max(telemetry.PrimaryTimerValue,
                Math.Max(telemetry.SecondaryTimer1Value, telemetry.SecondaryTimer2Value));

            return Math.Abs(maxTimer) > 0;
        }

        public async Task ResetTimers()
        {
            _ = LogWriter.LogAsync("ResetTimers", LogRecordSeverity.Info, LogRecordType.System);

            var localCancellationToken = CancellationTokenSource = new();
            await CallResetTimersAsync(localCancellationToken.Token);
        }

        public virtual bool CanLoadPlan()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            return telemetry.ControlBoardState == GcbStateNew.Primed;
        }

        public bool CanStartWarmUp()
        {
            var telemetry = SystemTelemetry;
            if (telemetry is null)
                return false;

            return telemetry.ControlBoardState == GcbStateNew.Cold ||
                   telemetry.ControlBoardState == GcbStateNew.StandBy;
        }

        public virtual async Task<bool> SafeWarmup(WarmupParameters warmupParameters)
        {
            var telemetry = SystemTelemetry;

            const string noTelemetryErrMsg = "Failed to check the GCB state before warmup: GCB telemetry connection lost.";
            if (telemetry == null)
            {
                throw new Exception(noTelemetryErrMsg);
            }

            var tokenSource = CancellationTokenSource = new CancellationTokenSource();

            if (telemetry.ControlBoardState == GcbStateNew.Startup)
            {
                await Initialize();
                await WaitForState(GcbStateNew.Cold, tokenSource.Token);
            }

            telemetry = SystemTelemetry;
            if (telemetry == null)
            {
                throw new Exception(noTelemetryErrMsg);
            }

            if (telemetry.ControlBoardState != GcbStateNew.Cold &&
                telemetry.ControlBoardState != GcbStateNew.StandBy)
            {
                return false;
            }

            float heaterCurrent = warmupParameters.HeaterCurrentSetpoint;
            IList<Task> expectedStates;
            Task waitForFault;
            Task? completedTask;

            if (heaterCurrent < PhysicsValueRange.HeaterCurrentMin
                || heaterCurrent > PhysicsValueRange.HeaterCurrentMax)
            {
                throw new ArgumentOutOfRangeException($"Invalid heater current configuration: value={heaterCurrent} is out of range {PhysicsValueRange.HeaterCurrentMin}..{PhysicsValueRange.HeaterCurrentMax}");
            }

            if (warmupParameters.WarmupType == WarmupType.Full)
            {
                await Conditioning(heaterCurrent, tokenSource.Token);
                // TODO: now we cancel any task through this token on any Fault state callback, so this should be refactored
                waitForFault = WaitForState(GcbStateNew.WarmupFault, tokenSource.Token);
                Task waitForConditioning = WaitForState(GcbStateNew.DailyWarmup, tokenSource.Token);
                expectedStates = new List<Task> { waitForFault, waitForConditioning };
                completedTask = await Task.WhenAny(expectedStates);

                if (completedTask.IsCanceled || completedTask == waitForFault)
                {
                    tokenSource?.Cancel(); // cancel other tasks
                    throw new TaskCanceledException();
                }

                tokenSource?.Cancel(); // cancel other tasks

                tokenSource = CancellationTokenSource = new CancellationTokenSource();

                // it goes to StandBy after Conditioning
                expectedStates = new List<Task> { 
                    WaitForState(GcbStateNew.StandBy, tokenSource.Token), 
                    WaitForState(GcbStateNew.Cold, tokenSource.Token), // It may also fall back to Cold for the old FW versions
                    WaitForState(GcbStateNew.Primed, tokenSource.Token), // It may also fall back to Primed for new FW versions
                };
            }
            else
            {
                await WarmUp(heaterCurrent, tokenSource.Token);
                expectedStates = new List<Task>
                {
                    WaitForState(GcbStateNew.Primed, tokenSource.Token),
                    WaitForState(GcbStateNew.Staged, tokenSource.Token)
                };
            }

            // TODO: now we cancel any task through this token on any Fault state callback, so this should be refactored
            waitForFault = WaitForState(GcbStateNew.WarmupFault, tokenSource.Token);
            expectedStates.Add(waitForFault);

            completedTask = await Task.WhenAny(expectedStates);
            if (completedTask.IsCanceled || completedTask == waitForFault)
            {
                tokenSource?.Cancel(); // cancel other tasks
                throw new TaskCanceledException();
            }

            tokenSource?.Cancel(); // cancel other tasks

            return true;
        }

        public virtual async Task<bool> PreparePlan(GcbEmissionPlan plan, bool tryKeepPrevPlan)
        {
            var macrocommandToken = CancellationTokenSource = new();

            var localToken = CancellationTokenSource.CreateLinkedTokenSource(macrocommandToken.Token);

            // TODO: now we cancel any task through this token on any Fault state callback,
            // so this should be refactored
            Task waitForStaged = WaitForState(GcbStateNew.Staged, localToken.Token);
            Task waitForPrimed = WaitForState(GcbStateNew.Primed, localToken.Token);

            var completedTask = await Task.WhenAny(waitForStaged, waitForPrimed);

            if (completedTask.Status == TaskStatus.Canceled)
            {
                throw new Exception("Prepare plan was cancelled.");
            }

            SetCurrentPlan(plan);

            localToken.Cancel(); //cancel other waiting tasks, and create new token

            if (completedTask == waitForPrimed && completedTask.IsCompletedSuccessfully)
            {
                _ = LogWriter.LogAsync("Prepare: went to Primed state", LogRecordSeverity.Info, LogRecordType.System);
                await LoadAndStartPlan(macrocommandToken.Token);
            }
            else if (completedTask == waitForStaged)
            {
                _ = LogWriter.LogAsync("Prepare: went to Staged state - the board already has a plan", LogRecordSeverity.Info, LogRecordType.System);
                // If tryKeepPrevPlan is true, we'll try to extract matching plan's time values from the board
                bool samePlan = tryKeepPrevPlan ? await UpdateCurrentPlanFromBoardIfMatching() : false;
                if (samePlan && Session is not null)
                {
                    // This plan is already on board, we don't want overwriting it, so we just return false:
                    _ = LogWriter.LogAsync("There is the same plan on the board, go to plan resume state", LogRecordSeverity.Info, LogRecordType.System);
                    return false;
                }
                // Here we either don't have a session yet, or we have a different plan,
                // so we need to wipe current plan from the board, then authorize and then (re-)load our version of the plan
                {
                    if (!samePlan)
                    {
                        _ = LogWriter.LogAsync("There is a different plan on the board, clearing it before loading", LogRecordSeverity.Info, LogRecordType.System);
                    }
                    else
                    {
                        _ = LogWriter.LogAsync("There is the same plan on the board, clearing it to authorize and load it again", LogRecordSeverity.Info, LogRecordType.System);
                    }
                    await CallResetTimersAsync(macrocommandToken.Token);
                    await ClearPlan();
                    _ = LogWriter.LogAsync("Wait for primed state", LogRecordSeverity.Info, LogRecordType.System);
                    await WaitForState(GcbStateNew.Primed, macrocommandToken.Token);
                    _ = LogWriter.LogAsync("Primed state", LogRecordSeverity.Info, LogRecordType.System);
                    await LoadAndStartPlan(macrocommandToken.Token);
                }
                return !samePlan;
            }
            else
            {
                throw new Exception("Warmup Fault");
            }
            return true; // Plan is loaded
        }

        public virtual void SetCurrentPlan(GcbEmissionPlan plan)
        {
            CurrentPlan = plan;
        }

        public void SetSession(GcbSession session)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanPrepare()
        {
            var telemetry = SystemTelemetry;

            if (telemetry is null)
                return false;

            return telemetry.ControlBoardState == GcbStateNew.Cold ||
                   telemetry.ControlBoardState == GcbStateNew.StandBy ||
                   telemetry.ControlBoardState == GcbStateNew.Primed ||
                   telemetry.ControlBoardState == GcbStateNew.Startup;
        }

        public async Task BeamOn()
        {
            var tokenSource = CancellationTokenSource = new();
            await StartPoint();
            // Wait for plan completion:
            await WaitForState(GcbStateNew.Staged, tokenSource.Token);

            OnGcbActionCompletion(GcbActionType.BeamOnCompleted);
        }

        public async Task BeamOnOnePoint()
        {
            var tokenSource = CancellationTokenSource = new();
            await StartPoint();

            // First wait for start emitting:
            await WaitForState(GcbStateNew.Emission, tokenSource.Token);

            // Now wait for point completion (should go either to Staged or to Ready state):
            Task waitForStaged = WaitForState(GcbStateNew.Staged, tokenSource.Token);
            Task waitForReady = WaitForState(GcbStateNew.Ready, tokenSource.Token);
            await Task.WhenAny([waitForStaged, waitForReady]);

            // todo: what if no one of these states will be reached (for example in case of error?), then how should we determine GcbActionType?
            if (waitForReady.IsCompletedSuccessfully)
                OnGcbActionCompletion(GcbActionType.OnePointCompleted);
            else if (waitForStaged.IsCompletedSuccessfully)
                OnGcbActionCompletion(GcbActionType.BeamOnCompleted);
            else if (tokenSource.IsCancellationRequested)
            {
                throw new TaskCanceledException("Emission task was canceled");
            }

            // Cancel the other waiting task:
            await tokenSource.CancelAsync();
        }


        public virtual async Task<GcbOperationalPoint> QueryPointFromGCB(int index)
        {
            _ = LogWriter.LogAsync("QueryPoint", LogRecordSeverity.Info, LogRecordType.System);

            return await GcbAPI.QueryPoint(index);
        }

        public async Task UpdatePlanPointFromGCB(int index)
        {
            CurrentPlan.UpdatePoint(await QueryPointFromGCB(index));
        }


        public virtual async Task<GcbEmissionPlan> QueryPlanFromGCB()
        {
            var telemetry = SystemTelemetry;
            if (telemetry == null)
                return null!;

            var totalPoints = telemetry.TotalOperationalPoints;

            GcbEmissionPlan plan = new();
            for (int i = 0; i < totalPoints; i++)
            {
                var point = await GcbAPI.QueryPoint(i);
                plan.AddPoint(point);
            }
            return plan;
        }

        public virtual async Task ResumePlan()
        {
            _ = LogWriter.LogAsync("ResumePlan", LogRecordSeverity.Info, LogRecordType.System);

            // Reset the cancellation token and then just proceed with the plan
            var macrocommandToken = CancellationTokenSource = new();

            var localToken = CancellationTokenSource.CreateLinkedTokenSource(macrocommandToken.Token);
            Task waitForStaged = WaitForState(GcbStateNew.Staged, localToken.Token);
            Task waitForPrimed = WaitForState(GcbStateNew.Primed, localToken.Token);

            var completedTask = await Task.WhenAny(waitForStaged, waitForPrimed);

            if (completedTask.Status == TaskStatus.Canceled)
            {
                throw new Exception("Resume plan was cancelled.");
            }

            localToken.Cancel(); //cancel other waiting tasks, and create new token

            if (completedTask.IsCompletedSuccessfully &&
               (completedTask == waitForPrimed || (completedTask == waitForStaged && Session is null)))
            {
                await CallResetTimersAsync(macrocommandToken.Token);
                await ClearPlan();
                _ = LogWriter.LogAsync("Wait for primed state", LogRecordSeverity.Info, LogRecordType.System);
                await WaitForState(GcbStateNew.Primed, macrocommandToken.Token);
                _ = LogWriter.LogAsync("Primed state", LogRecordSeverity.Info, LogRecordType.System);
                await LoadAndStartPlan(macrocommandToken.Token);
                await WaitForState(GcbStateNew.Staged, macrocommandToken.Token);
            }
            else
            {
                await WaitForState(GcbStateNew.Staged, macrocommandToken.Token);
                // Get plan from the board and set as current:
                if (!await UpdateCurrentPlanFromBoardIfMatching())
                    throw new Exception("Resume error: cannot verify plan on the board");
            }

            await ConfirmAndStartPlan();
        }

        protected void OnGcbActionCompletion(GcbActionType type)
        {
            GcbActionCompletionEvent?.Invoke(this, new GcbActionCompletionEventArgs { ActionType = type });
        }

        public static Tuple<double, double> CalculateMagnetometerCorrection(double coilX, double coilY,
                                                               Matrix2x3 correctionFront, Matrix2x3 correctionBack,
                                                               Vector3 referenceFieldFront, Vector3 referenceFieldBack,
                                                               Vector3 readOutFront, Vector3 readOutBack)
        {
            Matrix deflection = new Matrix(1, 2);

            deflection[0, 0] = coilX;
            deflection[0, 1] = coilY;

            //Correction matrices

            //Matrix correctionFront = new Matrix(2, 3);
            //Matrix correctionBack = new Matrix(2, 3);

            //correctionFront[0, 0] = -0.106124971200746 / 1000.0;
            //correctionFront[0, 1] = 2.75899076672399 / 1000.0;
            //correctionFront[0, 2] = -0.0547714005161611 / 1000.0;
            //correctionFront[1, 0] = 0.0122205376752865 / 1000.0;
            //correctionFront[1, 1] = -0.297375088821202 / 1000.0;
            //correctionFront[1, 2] = -2.73581931632667 / 1000.0;

            //correctionBack[0, 0] = 0.14091223264893 / 1000.0;
            //correctionBack[0, 1] = 2.99203147612998 / 1000.0;
            //correctionBack[0, 2] = -0.0521963071998251 / 1000.0;
            //correctionBack[1, 0] = -0.261457488378674 / 1000.0;
            //correctionBack[1, 1] = -0.322290719626623 / 1000.0;
            //correctionBack[1, 2] = -2.76130763461814 / 1000.0;


            //referenceFields

            //Matrix offsetsFront = new Matrix(3, 1);
            //Matrix offsetsBack = new Matrix(3, 1);

            //offsetsFront[0, 0] = 52.9169396315707;
            //offsetsFront[1, 0] = -25.408397765273;
            //offsetsFront[2, 0] = 10.0031021748377;

            //offsetsBack[0, 0] = 160.39733519927;
            //offsetsBack[1, 0] = -16.7956481857204;
            //offsetsBack[2, 0] = 98.7291418655644;


            Matrix calculatedCorrectionFront = new Matrix(1, 2);
            Matrix calculatedCorrectionBack = new Matrix(1, 2);

            correctionFront[0, 0] /= 1000.0;
            correctionFront[0, 1] /= 1000.0;
            correctionFront[0, 2] /= 1000.0;
            correctionFront[1, 0] /= 1000.0;
            correctionFront[1, 1] /= 1000.0;
            correctionFront[1, 2] /= 1000.0;

            correctionBack[0, 0] /= 1000.0;
            correctionBack[0, 1] /= 1000.0;
            correctionBack[0, 2] /= 1000.0;
            correctionBack[1, 0] /= 1000.0;
            correctionBack[1, 1] /= 1000.0;
            correctionBack[1, 2] /= 1000.0;

            calculatedCorrectionFront = correctionFront * (referenceFieldFront - readOutFront);
            calculatedCorrectionBack = correctionBack * (referenceFieldBack - readOutBack);


            double correctedCoilX = coilX + calculatedCorrectionFront[0, 0];
            double correctedCoilY = coilY + calculatedCorrectionFront[1, 0];
            //calculatedCorrection = deflection + correction * (offsets - readOut);
            //Matrix temp offsets - readOut;
            //Global.magnetometerValues[0];
            return new(correctedCoilX, correctedCoilY);
        }

        #endregion

        #region protected methods
        protected virtual async Task StartPlan()
        {
            _ = LogWriter.LogAsync("StartPlan", LogRecordSeverity.Info, LogRecordType.System);
            if (Session != null)
            {
                await GcbAPI.ReleasePlan(GCBReleaseCommandScope.Plan, Session.Value);
            }
            else
            {
                throw new NullReferenceException("Cannot release the plan: no session data");
            }
            OnGcbActionCompletion(GcbActionType.ReleasePlan);
        }

        protected virtual async Task StartPoint()
        {
            _ = LogWriter.LogAsync("StartPoint", LogRecordSeverity.Info, LogRecordType.System);

            var telemetry = SystemTelemetry;
            if (telemetry is null)
            {
                throw new NullReferenceException(nameof(telemetry));
            }

            int currentPoint = telemetry.CurrentOperationalPoint;
            if (currentPoint < CurrentPlan.TotalPoints)
            {
                var currentPointValue = CurrentPlan[currentPoint];
                currentPointValue.InitialRemainingPointTime = currentPointValue.RemainingPointTime;
                CurrentPlan.UpdatePoint(currentPointValue);
            }

            await GcbAPI.ReleasePlan(GCBReleaseCommandScope.Point, Session!.Value);
            OnGcbActionCompletion(GcbActionType.StartBeamOn);
        }


        protected virtual async Task LoadAndStartPlan(CancellationToken cancellationToken)
        {
            var telemetry = SystemTelemetry;
            if (telemetry is null)
            {
                throw new NullReferenceException(nameof(telemetry));
            }

            if (telemetry.PrimaryTimerValue > 0)
                throw new Exception("System not ready. Reset timers and try again");

            if (CurrentPlan == null || CurrentPlan.TotalPoints == 0)
                throw new Exception("No points to load ");

            _ = LogWriter.LogAsync($"Count of treatment fields to load = {CurrentPlan.TotalPoints}", LogRecordSeverity.Info, LogRecordType.System);

            await CreateNewSession(CurrentPlan.TotalPoints);

            await WaitForState(GcbStateNew.Staging, cancellationToken);
            await SendOperationalPoints(OperationalPointCmdType.Load);

            cancellationToken.ThrowIfCancellationRequested(); // ensure that the operation wasn't cancelled during the prev. action
            await StagePlan();

            IsPlanStaged = true;

            await WaitForState(GcbStateNew.Staged, cancellationToken);

            await ConfirmAndStartPlan();
        }

        protected virtual async Task ConfirmAndStartPlan()
        {
            await SendOperationalPoints(OperationalPointCmdType.Confirmation);

            await StartPlan();
        }

        public async Task<bool> SendOperationalPoints(
            OperationalPointCmdType commandType)
        {
            _ = LogWriter.LogAsync("SendOperationalPoints", LogRecordSeverity.Info, LogRecordType.System);

            if (Session is null)
            {
                throw new NullReferenceException("Cannot send the point to the board: no session data");
            }
            var session = Session.Value;

            foreach (var op in CurrentPlan.Points)
            {
                LogOperationalPoint(op);

                await GcbAPI.SendOperationalPoint(commandType, op, session);
            }

            return true;
        }

        protected void LogOperationalPoint(GcbOperationalPoint op)
        {
            IEnumerable<string> fields =
            [
                $"OperationalPointIndex={op.PointIndex}",
                $"Energy={op.SetpointKv}",
                $"TotalPointTime={op.TotalPointTime}",
                $"RemainingPointTime={op.RemainingPointTime:F4}",
                $"TargetMA={op.TargetMA}",
                $"FilamentSetpoint={op.FilamentSetpoint}",
                $"CoilSetpointX={op.XCoilSetpoint:F4} (correction={op.CoilSetpointCorrection.XCoil:F4})",
                $"CoilSetpointY={op.YCoilSetpoint:F4} (correction={op.CoilSetpointCorrection.YCoil:F4})"
            ];
            _ = LogWriter.LogAsync(string.Join(Environment.NewLine, fields), LogRecordSeverity.Info, LogRecordType.System);
        }

        /// <summary>
        /// Queries plan from board and verifies if this is the same plan we intend to load.
        /// If so, updates our plan's remaining times to display and preserve them.
        /// </summary>
        /// <returns>true if plan is the same, false otherwise</returns>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<bool> UpdateCurrentPlanFromBoardIfMatching()
        {
            try
            {
                GcbEmissionPlan gcbPlanState = await QueryPlanFromGCB();

                if (CurrentPlan.IsSameAs(gcbPlanState))
                {
                    foreach (var pt in gcbPlanState.Points)
                    {
                        CurrentPlan.UpdatePoint(pt);
                    }
                }
                else
                {
                    _ = LogWriter.LogAsync("Loaded Plan does not match the one is going to be loaded", LogRecordSeverity.Info, LogRecordType.System);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync("Cannot compare plan on the board to the new one", LogRecordSeverity.Info, LogRecordType.System);
                throw new Exception("There is another loaded Plan already, please clear it first", ex);
            }

            _ = LogWriter.LogAsync("Loaded Plan match the one is going to be loaded", LogRecordSeverity.Info, LogRecordType.System);

            return true;
        }

        protected async Task<GcbStateNew> WaitForState(GcbStateNew expectedState, CancellationToken token)
        {
            _ = LogWriter.LogAsync($"WaitForState {expectedState}", LogRecordSeverity.Info, LogRecordType.System);

            while (!token.IsCancellationRequested)
            {
                var telemetry = SystemTelemetry ?? throw new Exception($"Failed to wait for expected state {expectedState}: GCB telemetry connection lost.");

                if (expectedState == telemetry.ControlBoardState)
                {
                    return expectedState;
                }

                await Task.Delay(50, token);
            }

            throw new TaskCanceledException($"Failed to wait for expected state {expectedState}: task was cancelled.");
        }

        protected virtual async Task CallResetTimersAsync(CancellationToken cancellationToken)
        {
            if (SystemTelemetry?.ControlBoardState == GcbStateNew.Startup)
            {
                return; // we can't reset timers from startup state
                        // (and they shouldn't be set in that state in the first place)
            }

            await GcbAPI.ResetTimers();

            await Task.Run(async () =>
            {
                while (CanResetTimers())
                {
                    await Task.Delay(50, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

            }, cancellationToken);
        }


        protected virtual void UpdateCurrentPlanState(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry is null)
                return;

            int currentPoint = systemTelemetry.CurrentOperationalPoint;

            if (systemTelemetry.IsEmissionState() == true )
            {
                if (currentPoint < CurrentPlan.TotalPoints)
                {
                    var currentPointValue = CurrentPlan[currentPoint];

                    if (systemTelemetry.ControlBoardState is GcbStateNew.Emission)
                    {
                        currentPointValue.RemainingPointTime = currentPointValue.InitialRemainingPointTime - systemTelemetry.PrimaryTimerValue;
                    }
                    else
                    {
                        // in imaging, timers run in reverse, as a countdown:
                        currentPointValue.RemainingPointTime = systemTelemetry.PrimaryTimerValue;
                    }

                    CurrentPlan.UpdatePoint(currentPointValue);
                }
            }
            else if (systemTelemetry.ControlBoardState == GcbStateNew.Termination)
            {
                var index = currentPoint < CurrentPlan.TotalPoints ? currentPoint : CurrentPlan.TotalPoints - 1;
                var currentPointValue = CurrentPlan[index];

                float newRemainingTime = float.Max(0, currentPointValue.InitialRemainingPointTime - systemTelemetry.PrimaryTimerValue);
                if (newRemainingTime < currentPointValue.RemainingPointTime)
                {
                    currentPointValue.RemainingPointTime = newRemainingTime;
                    CurrentPlan.UpdatePoint(currentPointValue);
                }
            }
        }

        #endregion protected methods
    }
}
