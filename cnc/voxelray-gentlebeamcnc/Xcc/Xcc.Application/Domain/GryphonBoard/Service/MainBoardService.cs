using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Application.Domain.GryphonBoard.Service.Actions;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Service
{
    // TODO: rename ClearOrKeepPlanOnBoard to something better, or even redesign it 
    internal class ClearOrKeepPlanOnBoard : AbstractMainBoardAction
    {
        private readonly IMainBoardStateManagement mainBoardState;
        private readonly IMainBoardAPI mainBoardAPI;
        private readonly IGcbCommandInterface gcbCommands;
        private readonly bool tryKeepSamePlan;

        public ClearOrKeepPlanOnBoard(
            IMainBoardStateManagement mainBoard,
            IMainBoardAPI mainBoardAPI,
            IGcbCommandInterface gcbCommands,
            bool tryKeepSamePlan = true)
            : base(mainBoard, [GcbStateNew.Staged], [GcbStateNew.Primed, GcbStateNew.Staged])
        {
            this.mainBoardState = mainBoard;
            this.mainBoardAPI = mainBoardAPI;
            this.gcbCommands = gcbCommands;
            this.tryKeepSamePlan = tryKeepSamePlan;
        }

        protected override async Task RunActionAsync(CancellationToken token)
        {
            var planOnBoard = await mainBoardAPI.QueryPlanFromGCB();
            bool keepThePlan = tryKeepSamePlan && planOnBoard.IsSameAs(MainBoard.CurrentPlan);

            if (keepThePlan)
            {
                mainBoardState.SetCurrentPlan(planOnBoard);
            }

            // We have a plan staged on the board, but we don't have a session for it,
            // so we need to clear it to be able to reload it again later, when we'll have a session:
            if (MainBoard.Session is null)
            {
                await gcbCommands.ResetTimers();
                await gcbCommands.ClearPlan();
            }
        }
    }

    public class MainBoardService : IMainBoardAPI
    {
        private readonly IMainBoardStateManagement mainBoardState;
        private readonly IGcbCommandInterface gcbCommandInterface;
        private readonly ILogWriter logWriter;
        private readonly ActionSequentialExecutor gcbCommandProcessor;

        public MainBoardService(
            IMainBoardStateManagement mainBoardState,
            IGcbCommandInterface gcbCommandInterface,
            ILogWriter logWriter) 
        {
            this.mainBoardState = mainBoardState;
            this.gcbCommandInterface = gcbCommandInterface;
            this.logWriter = logWriter;
            this.gcbCommandProcessor = new ActionSequentialExecutor();
        }

        public event EventHandler<GcbActionCompletionEventArgs>? GcbActionCompletionEvent;

        public void CancelCurrentTask()
        {
            gcbCommandProcessor.CancelOngoingTask();
        }

        // State changing commands:
        #region Active commands 
        public Task Initialize()
        {
            return gcbCommandProcessor.Execute(
                new InitializeBoard(mainBoardState, gcbCommandInterface));
        }

        public Task Stop()
        {
            CancelCurrentTask();

            return gcbCommandInterface.Stop();
        }

        public Task ClearFaults()
        {
            return gcbCommandProcessor.Execute(
                new ClearFaults(mainBoardState, gcbCommandInterface));
        }

        public Task ClearPlan()
        {
            throw new NotImplementedException();
        }

        #region Board command sequences
        public async Task<bool> PreparePlan(GcbEmissionPlan plan, bool tryKeepPrevPlan)
        {
            await gcbCommandProcessor.Execute(
                new MacroAction(
                    [
                    new MainBoardOptionalActionWrapper(
                        new ClearOrKeepPlanOnBoard(mainBoardState, this, gcbCommandInterface, tryKeepPrevPlan)),
                    new NewSession(mainBoardState, gcbCommandInterface),
                    new LoadAndStagePlan(mainBoardState, gcbCommandInterface, logWriter),
                    new ConfirmAndReleasePlan(mainBoardState, gcbCommandInterface, logWriter)
                    ]));
            // TODO: now we need to store a flag if plan was loaded from scratch here
            return true;
        }
        public Task BeamOn()
        {
            throw new NotImplementedException();
        }

        public Task BeamOnOnePoint()
        {
            throw new NotImplementedException();
        }

        public Task ResumePlan()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SafeWarmup(WarmupParameters warmupParameters)
        {
            IAsyncAction warmup = warmupParameters.WarmupType switch
            {
                WarmupType.Fast => new FastWarmup(mainBoardState, gcbCommandInterface, warmupParameters.HeaterCurrentSetpoint),
                WarmupType.Full => new Conditioning(mainBoardState, gcbCommandInterface, warmupParameters.HeaterCurrentSetpoint),
                _ => throw new NotImplementedException()
            };

            // TODO: need to ensure that warmup takes place? Or just rely on warmup events?

            await gcbCommandProcessor.Execute(
                new MacroAction([
                new MainBoardOptionalActionWrapper(new InitializeBoard(mainBoardState, gcbCommandInterface)),
                warmup
                ]));
            return true;
        }
        #endregion Board command sequences
        #endregion Active commands


        // Commands changing onboard data, but not its operational state:
        #region Board setup commands
        public Task ResetTimers()
        {
            throw new NotImplementedException();
        }
        #endregion Board setup commands


        // Commands that just query the board's state
        #region Board state queries
        public Task<FaultEntry> GetFaults()
        {
            return gcbCommandInterface.GetFaults();
        }

        public Task<VersionInfo> GetVersionInfo()
        {
            return gcbCommandInterface.GetVersionInfo();
        }


        public async Task<GcbEmissionPlan> QueryPlanFromGCB()
        {
            var telemetry = mainBoardState.SystemTelemetry;
            if (telemetry == null)
                return null!;

            var totalPoints = telemetry.TotalOperationalPoints;

            GcbEmissionPlan plan = new();
            for (int i = 0; i < totalPoints; i++)
            {
                var point = await QueryPointFromGCB(i);
                plan.AddPoint(point);
            }
            return plan;
        }

        public Task<GcbOperationalPoint> QueryPointFromGCB(int index)
        {
            return gcbCommandInterface.QueryPoint(index);
        }

        public Task UpdatePlanPointFromGCB(int index)
        {
            throw new NotImplementedException();
        }

        #endregion Board state queries

    }
}
