using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Enums;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class BeamQaViewModel : OperatePlanViewModelBase
    {
        private const int NumberOfQcDiodes = 5;

        #region Contructors
        public BeamQaViewModel()
        { }

        public BeamQaViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IHeraclesExternalSettings heraclesExternalSettings,
            IGCBDataStore gcbDataStore,
            ILogWriter logWriter,
            IUIStateMachine uiStateMachine,
            IDialogService dialogService,
            IWarmupService warmUpService,
            ICollimatorModel collimatorModel,
            IPopUpService popUpService,
            IMainBoardModel mainBoardModel,
            IGcbIndicators gcbIndicators,
            IAuthorizedUserStore userStore,
            QcReportService qcReportService,
            IDispatcherService dispatcherService,
            ICollimatorCalibrationModel collimatorCalibrationModel,
            ICollimatorConfigurationStore collimatorConfigurationStore,
            ApplicatorCompatibilityService applicatorCompatibilityService,
            IActionAuditService actionAuditService,
            ISafetyCheckModel safetyCheckModel,
            IBearerTokenUserSessionManager userSessionManager)
            : base(regionManager, eventAggregator, heraclesExternalSettings,
                gcbDataStore, uiStateMachine, logWriter, warmUpService, popUpService,
                dialogService, mainBoardModel, gcbIndicators,
                collimatorModel, collimatorConfigurationStore,
                actionAuditService, safetyCheckModel, userSessionManager)
        {
            UserStore = userStore;
            QcReportService = qcReportService;
            CollimatorCalibrationModel = collimatorCalibrationModel;
            ApplicatorCompatibilityService = applicatorCompatibilityService;
            QcPlan = new QualityCheckPlan(dispatcherService, heraclesExternalSettings.QcFieldDuration);

            QcPlan.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(QualityCheckPlan.Fields))
                {
                    FieldsViewSource.Source = QcPlan.Fields;
                }
            };

            FieldsSelectionModel = new TreatmentFieldSelectionModel(QcPlan);
            FieldsSelectionModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TreatmentFieldSelectionModel.TreatmentFieldListSelection) ||
                    e.PropertyName == nameof(TreatmentFieldSelectionModel.HoneycombSelection))
                    OnTreatmentFieldSelectionChanged();
                else if (e.PropertyName == nameof(TreatmentFieldSelectionModel.SelectedCollimatorType))
                {
                    CollimatorType = FieldsSelectionModel.SelectedCollimatorType;
                }
            };

            CollimatorModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ICollimatorModel.CollimatorConfigurations))
                {
                    GetAvailableTargetTypesAndEnergyLevels();
                }
            };

            GetAvailableTargetTypesAndEnergyLevels();
        }

        #endregion Contructors

        private IDictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo> _collimatorConfigurationsWithCalibInfo =
            new Dictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo>();

        #region Properties
        public IAuthorizedUserStore UserStore { get; }
        public QcReportService QcReportService { get; }
        public QualityCheckPlan QcPlan { get; }
        public ICollimatorCalibrationModel CollimatorCalibrationModel { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }

        private CollectionViewSource _fieldsViewSource = new();
        public CollectionViewSource FieldsViewSource
        {
            get => _fieldsViewSource;
            set => SetProperty(ref _fieldsViewSource, value);
        }

        private Energy? _energy = null!;
        public Energy? Energy
        {
            get => _energy;
            set
            {
                if (!IsModified || GetUserConfirmation())
                {
                    if (SetProperty(ref _energy, value))
                    {
                        IsModified = false;
                        OnEnergyChanged(_energy);
                    }
                }
            }
        }

        private bool _prepareButtonIsEnabled;
        public bool PrepareButtonIsEnabled
        {
            get { return _prepareButtonIsEnabled; }
            set { SetProperty(ref _prepareButtonIsEnabled, value); }
        }

        public TreatmentFieldSelectionModel FieldsSelectionModel { get; }

        private SsdType? _ssdType;
        public SsdType? SsdType
        {
            get { return _ssdType; }
            set
            {
                SetProperty(ref _ssdType, value);
            }
        }

        private TargetType _type;
        public TargetType CollimatorType
        {
            get => _type;
            set
            {
                if (SetProperty(ref _type, value))
                {
                    SsdType = (value == TargetType.TargetType_30mm_SSD_7_Fields)
                        ? Core.Enums.SsdType.SsdType30mm : Core.Enums.SsdType.SsdType50mm;
                    
                    //FieldsSelectionModel?.OnSelectedCollimatorTypeChanged(value);
                }
            }
        }

        private IEnumerable<Energy> _availableEnergyLevels;
        public IEnumerable<Energy> AvailableEnergyLevels
        {
            get => _availableEnergyLevels;
            private set
            {
                if (SetProperty(ref _availableEnergyLevels, value))
                {
                    CheckForApplicatorCompatibility();
                }
            }
        }


        private IEnumerable<TargetType> _availableTargetTypeValues;
        public IEnumerable<TargetType> AvailableTargetTypeValues
        {
            get => _availableTargetTypeValues;
            private set
            {
                if (SetProperty(ref _availableTargetTypeValues, value))
                {
                    FieldsSelectionModel.SelectedCollimatorType = _availableTargetTypeValues.FirstOrDefault();
                }
            }
        }

        private bool _fullCollection = true;
        public bool FullCollection
        {
            get { return _fullCollection; }
            set
            {
                if (!Energy.HasValue)
                {
                    SetProperty(ref _fullCollection, value);
                    return;
                }

                string message;

                if (value) // 'Full' selected
                {
                    message = Application.Common.StringConstants.TreatmentConsole.QualityCheckFullModeConfirmationMessage;
                }
                else
                {
                    message = Application.Common.StringConstants.TreatmentConsole.QualityCheckCustomModeConfirmationMessage;
                }

                if (DialogService.Confirmation(StringConstants.Common.ConfirmationDialogTitle, message))
                {
                    if (SetProperty(ref _fullCollection, value))
                    {
                        CreateEntryCollection();
                    }
                }
            }
        }

        public bool IsModified;

        #endregion Properties

        #region Commands
        private DelegateCommand? _addCommand;
        public DelegateCommand AddCommand => _addCommand ??= new DelegateCommand(
            OnAddClicked,
            canExecuteMethod: CanAdd);

        private DelegateCommand? _removeCommand;
        public DelegateCommand RemoveCommand => _removeCommand ??= new DelegateCommand(
            () =>
            {
                if (DialogService.Confirmation(
                    StringConstants.Common.DeleteDialogTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanDeleteFieldConfirmationMessage))
                {
                    CurrentTask = new Xcc.Application.Helpers.ObservableTask(Task.Run(OnRemoveClicked));
                }
            },
            canExecuteMethod: CanRemove);

        private DelegateCommand? _qcTestCommand;

        public DelegateCommand QCTestCommand => _qcTestCommand ??= new DelegateCommand(
            async () =>
            {
                // QCB hardware removed - this test command is no longer functional
                DialogService.Report("QCB", "QCB hardware has been removed from the system.", ReportType.Info);
            });

        #endregion Commands

        #region Public methods

        #endregion

        #region Private methods   

        public override void CheckForBoardRestart(GcbStateNew gcbState)
        {
            base.CheckForBoardRestart(gcbState);
            
            // we can't rely on UIStateMachine.State here, because it can be changed from other ViewModels
            if (gcbState == GcbStateNew.Startup) 
            {
                if ((IsCurrentViewModelRunning || 
                     UIStateMachine.TabName == ExternalTabName.QA) && // todo: this condition will allow to reset a QC plan even if SafetyCheck or Physics was ran 
                    UIStateMachine.IsPlanStaged)
                {
                    // unblock all the tabs, because GCB was rebooted, and we don't need to keep the QC plan
                    UIStateMachine.IsPlanStaged = false;

                    //reset the plan
                    _ = SetPlanUnloadTaskAsync();
                }
            }
        }

        private void LogInfoSystem(string message)
        {
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Info, LogRecordType.System);
        }

        protected override void UserActionAudit(string actionMessage)
        {
            actionMessage = $"QualityCheck: {actionMessage}";

            base.UserActionAudit(actionMessage);
        }
        private bool CanAdd()
        {
            return
                !FullCollection &&
                FieldsSelectionModel.HoneycombSelection != null && Energy != null &&
                !QcPlan.ContainsField(CollimatorType, Energy.Value, FieldsSelectionModel.HoneycombSelection);
        }

        //protected override bool CanPrepare()
        //{
        //    if (UIStateMachine.LeftButton == null)
        //    {
        //        PrepareButtonIsEnabled = false;
        //        return false;
        //    }

        //    PrepareButtonIsEnabled = UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment();

        //    return QcModel.Fields != null
        //        && QcModel.Fields.Count > 0
        //        && base.CanPrepare();
        //}

        protected override bool CanPrepare()
        {
            if (UIStateMachine.LeftButton == null)
            {
                PrepareButtonIsEnabled = false;
                return false;
            }

            PrepareButtonIsEnabled = UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment();

            var telemetry = GCBDataStore.SystemTelemetry;

            if (telemetry is null)
                return false;

            var stateResult = telemetry.ControlBoardState is GcbStateNew.Cold or
                                                                GcbStateNew.Primed or
                                                                GcbStateNew.Startup or
                                                                GcbStateNew.StandBy;

            return QcPlan.Fields is {Count: > 0}
                   && !CanResetTimers()
                   && !IsPreparing
                   && ApplicatorCompatibilityStatus.IsCompatible
                   && stateResult;
        }

        private bool CanRemove()
        {
            return !FullCollection &&
                (FieldsSelectionModel.TreatmentFieldListSelection != null && FieldsSelectionModel.TreatmentFieldListSelection.Count > 0);
        }

        // We cannot resume a QC plan, as it requires to redo all the measurements in a consistent way
        protected override bool CanResume()
        {
            return false;
        }


        protected override async Task PrepareAsync(bool tryKeepPrevPlan)
        {
            try
            {
                await CheckQCBoardStatusAsync();
            }
            catch (Exception ex)
            {
                // To prevent error loop on preparation callback, go to StandBy
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationForQcBoardPingErrorMessage,
                    ex);
                return;
            }

            var tabName = ExternalTabName.QA;

            try
            {
                await base.PrepareAsync(tryKeepPrevPlan: false); // don't keep any prev plan in GCB

                UIStateMachine.IsPlanLoadedForTreatment = true;
                UIStateMachine.TabName = tabName;
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationForQcErrorMessage,
                    ex);
            }
            finally
            {
                SwitchExternalTab(tabName);
                EventAggregator?.GetEvent<RequestQaTabChangeEvent>().Publish(QaTabName.QualityChecks);
            }
        }

        protected override bool GetPlanAutoExecutionFlag()
        {
            return false; // we don't auto-execute QC plans
        }

        protected override void CheckForApplicatorCompatibility()
        {
            var requiredParameters = ApplicatorParameters.FromValues(
                TargetType.TargetType_QC_Collimator,
                Core.Enums.Energy.Energy_50);
            
            ApplicatorCompatibilityStatus =
                (requiredParameters is null)
                ? ApplicatorCompatibilityStatus.Compatible
                : ApplicatorCompatibilityService.Check(requiredParameters.Value);
            HasMatchingPlanForTreatment = HasValidPlanForTreatment();

            ValidateCanExecuteCommands();
        }

        protected override void ValidateCanExecuteCommands()
        {
            base.ValidateCanExecuteCommands();

            AddCommand.RaiseCanExecuteChanged();
            RemoveCommand.RaiseCanExecuteChanged();
        }

        protected override async Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry)
        {
            try
            {
                await Semaphore.WaitAsync();

                if (GcbState == GcbStateNew.Emission ||
                    PreviousGcbState == GcbStateNew.Emission)
                {
                    int operationalPointIndex = telemetry.CurrentOperationalPoint;
                    float timerValue = telemetry.PrimaryTimerValue;

                    if (operationalPointIndex != PreviousOperationPointIndex)
                    {
                        var fields = QcPlan.Fields;
                        if (PreviousOperationPointIndex >= 0 && fields is not null && fields.Count > PreviousOperationPointIndex)
                        {
                            var previousTf = fields[PreviousOperationPointIndex];
                            previousTf.Actual = Convert.ToSingle(previousTf.Duration);
                            RecalculateInitialXrayTime();
                            XrayPointStartTime = 0;

                            await MainBoardModel.UpdatePlanPointFromGCB(PreviousOperationPointIndex);
                            var previousPoint = MainBoardModel.CurrentPlan[PreviousOperationPointIndex];

                            if (previousPoint.RemainingPointTime > 0)
                            {
                                previousTf.Actual = previousPoint.TotalPointTime - previousPoint.RemainingPointTime;
                                _ = LogWriter.LogAsync($"Query point response: TotalPointTime={previousPoint.TotalPointTime} RemainingPointTime={previousPoint.RemainingPointTime} Actual={previousTf.Actual}", LogRecordSeverity.Info, LogRecordType.System);
                            }
                            //_ = LogService.LogAsync($"UpdateEmissionTreatmentField: operationalPointIndex={operationalPointIndex}, timerValue {timerValue} _xrayTime {_xrayTime} TotalDuration {PlanModel.TotalDuration}", LogRecordSeverity.Info, LogRecordType.System);
                        }
                    }

                    if (operationalPointIndex < QcPlan.Fields?.Count)
                    {
                        var tf = QcPlan.Fields[operationalPointIndex];

                        if (operationalPointIndex != PreviousOperationPointIndex)
                        {
                            PreviousOperationPointIndex = operationalPointIndex;
                            XrayPointStartTime = tf.Actual;
                        }

                        UpdateBeamOnProgress(QcPlan.TotalDuration, Convert.ToSingle(XrayTime + timerValue));

                        tf.Actual = Convert.ToSingle(XrayPointStartTime + timerValue);
                        Debug.WriteLine($"Update treatment field {tf.DisplayValue} with actual = {tf.Actual}");

                        if (tf.DwellTime - tf.Actual < PlanCompletedThreshold)
                        {
                            tf.IsDone = true;
                        }
                    }
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        protected override async Task OnClearPlanClicked()
        {
            FieldsSelectionModel.SelectField(null);

            await base.OnClearPlanClicked();
        }

        protected override async Task OnBeamOnClicked()
        {
            IsCurrentViewModelRunning = true;

            using var tokenSource = new CancellationTokenSource();
            Task updateAfterEmissionTask = Task.CompletedTask;

            try
            {
                FieldsSelectionModel.SelectField(null);

                await CheckQCBoardStatusAsync();

                var telemetry = GCBDataStore.SystemTelemetry ?? throw new Exception("GCB telemetry connection lost.");

                // Store what was the current point before emission
                PreviousOperationPointIndex = telemetry.CurrentOperationalPoint;

                // Store initial emission time of the current point to calc progress over the plan
                RecalculateInitialXrayTime();

                XrayPointStartTime = (PreviousOperationPointIndex < QcPlan.Fields.Count)
                    ? QcPlan.Fields[PreviousOperationPointIndex].Actual
                    : 0.0;

                UpdateBeamOnProgress(QcPlan.TotalDuration, XrayTime);

                UIStateMachine.RequestStateSwitch(UIMacroState.Emission);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled}), " +
                                $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                _ = LogWriter.LogAsync($"Run QC by {UserStore.AuthorizedUser.EmailAddress}", LogRecordSeverity.Info, LogRecordType.User);

                // Run emission and wait until it gets done or gets stopped:
                foreach (var field in QcPlan.Fields)
                {
                    FieldsSelectionModel.SelectField(field);
                    var dataReadingCancellationTokenSource = new CancellationTokenSource();
                    
                    updateAfterEmissionTask = Task.Run(() => UpdateAfterEmission(tokenSource.Token), tokenSource.Token);

                    Task beamOn = MainBoardModel.BeamOnOnePoint();

                    // QCB hardware removed - QC data reading disabled
                    // Placeholder: create a completed task with empty readings
                    Task<QcReadings> dataReading = Task.FromResult(new QcReadings(Array.Empty<float>()));

                    Task firstDone = await Task.WhenAny([beamOn, dataReading] /*todo: cancellationToken from AppGlobals*/);

                    // First, react on errors
                    if (firstDone.IsFaulted)
                    {
                        if (firstDone == beamOn)
                        {
                            await dataReadingCancellationTokenSource.CancelAsync(); // cancel dataReading task
                            Debug.WriteLine("BeamOn faulted task: BeamOnOnePoint");
                            throw new Exception("Failed to collect QC data: BeamOn task failed");
                        }

                        // it is dataReading failed
                        MainBoardModel.CancelCurrentTask();
                        Debug.WriteLine("BeamOn faulted task: ReadQC");
                        throw new Exception("Failed to collect QC data: reading task failed");
                    }

                    // whatever it was, wait for the second one:
                    await Task.WhenAll([beamOn, dataReading]);

                    await updateAfterEmissionTask;

                    if (beamOn.Status == TaskStatus.RanToCompletion && dataReading.Status == TaskStatus.RanToCompletion)
                    {
                        Debug.WriteLine("BeamOn: QC point reading succeeded");
                        var readings = dataReading.Result;
                        // OK, write data readings into model
                        field.Intensities = readings;
                    }
                    else if (beamOn.IsFaulted)
                    {
                        Debug.WriteLine("BeamOn second faulted task: BeamOnOnePoint");
                        throw new Exception("Failed to collect QC data: BeamOn task failed");
                    }
                    else // it is dataReading failed
                    {
                        Debug.WriteLine("BeamOn second faulted task: ReadQC");
                        throw new Exception("Failed to collect QC data: reading task failed");
                    }
                }

                // Check if plan is actually complete:
                if (IsPlanCompleted())
                {
                    PopUpService.LogAndShowMessage(
                        Application.Common.StringConstants.TreatmentConsole.QualityCheckNotificationTitle,
                        $"{Application.Common.StringConstants.TreatmentConsole.QualityCheckCompletionNotification}{Environment.NewLine}{Application.Common.StringConstants.TreatmentConsole.SwitchToReportsSuggestionMessage}",
                        ReportType.Info, LogRecordSeverity.Info, LogRecordType.System);
                }
                else
                {
                    PopUpService.LogAndShowError(
                        Application.Common.StringConstants.TreatmentConsole.QualityCheckConsistencyErrorTitle,
                        $"{Application.Common.StringConstants.TreatmentConsole.QualityCheckConsistencyErrorMessage}{Environment.NewLine}{Application.Common.StringConstants.TreatmentConsole.SwitchToReportsSuggestionMessage}");
                }

                await MainBoardModel.ResetTimers();
                await MainBoardModel.ClearPlan();

                UIStateMachine.IsPlanStaged = false;
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);

                await QcReportService.SaveQcSampleReportAsync(QcPlan);

                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                                $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                await SetPlanUnloadTaskAsync();

                EventAggregator!.GetEvent<QualityCheckFinishedEvent>().Publish();
            }
            catch (TaskCanceledException ex)
            {
                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);

                PopUpService.ShowMessage(
                    StringConstants.TreatmentConsole.EmissionTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ReportType.Error);

                _ = LogWriter.LogAsync($"QC plan execution was cancelled: {ex.Message}", LogRecordSeverity.Info, LogRecordType.System);
            }
            catch (DataServiceException ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.SaveErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckSaveErrorMessage,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckTitle,
                    ex.Message);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckTitle,
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckStartErrorMessage,
                    ex);
            }
            finally
            {
                await tokenSource.CancelAsync();

                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);

                FieldsSelectionModel.SelectField(null);
                IsCurrentViewModelRunning = false;
            }
        }

        private async Task CheckQCBoardStatusAsync()
        {
            // QCB hardware removed - status check disabled
            // Previously checked: bool qcBoardIsAlive = await QcbReadingModel.PingBoardAsync();
            await Task.CompletedTask;
        }

        protected override Task SetPlanUnloadTaskAsync()
        {
            UIStateMachine.IsPlanLoadedForTreatment = false;

            QcPlan.ResetEntryCollectionActualTime();

            return Task.CompletedTask;
        }

        protected override void RecalculateInitialXrayTime()
        {
            var fields = QcPlan.Fields;
            if (fields is null || fields.Count == 0)
                XrayTime = 0.0;
            else
                XrayTime = fields.Sum(tf => tf.Actual);
        }

        /// <summary>
        /// Convers QcModel.Fields to our GcbOperationalDataPoint items
        /// Requires Fields to be already ordered by TargetType to not switch heaterCurrent back and forth if it's different for diff. configs
        /// It may require also to sort Fields by kV if we'll have it different
        /// </summary>
        protected override GcbEmissionPlan BuildGcbEmissionPlan()
        {
            GcbEmissionPlan plan = new();

            foreach (var field in QcPlan.Fields)
            {
                var collimatorCalibConfig = _collimatorConfigurationsWithCalibInfo[field.Configuration];

                // TODO: we don't apply magnetometer now, just get calibrated coilX/Y
                var fieldCalibConfig = collimatorCalibConfig.GetCoilConfiguration(field.Name).Value;

                float totalTime = (float)field.Duration;
                float remainingTime = totalTime - (float)field.Actual;

                GcbOperationalPoint op = new GcbOperationalPoint
                {
                    PointIndex = plan.TotalPoints,
                    TotalPointTime = totalTime,
                    RemainingPointTime = remainingTime,
                    SetpointKv = EnergyConverter.Convert(field.Energy),
                    TargetMA = Convert.ToSingle(field.Current),

                    FilamentSetpoint = (float)collimatorCalibConfig.HeaterCurrent,
                    XCoilSetpoint = (float)fieldCalibConfig.XDeflectionCurrent,
                    YCoilSetpoint = (float)fieldCalibConfig.YDeflectionCurrent,
                    FocusCoilSetpoint = (float)fieldCalibConfig.FocusCurrent,
                    AutoExecution = GetPlanAutoExecutionFlag()
                };

                plan.AddPoint(op);
            }
            return plan;
        }

        private bool GetUserConfirmation()
        {
            bool dialogResult = false;

            DialogService.Report(
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationTitle,
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationMessage,
                ReportType.Confirmation,
                result =>
                {
                    dialogResult = (result.Result == ButtonResult.OK);
                });

            return dialogResult;
        }

        private void CreateEntryCollection()
        {
            if (!Energy.HasValue)
                return;

            try
            {
                QcPlan.ResetEntries();

                var collection = new List<IQcSampleFieldEntry>();

                foreach (var (configuration, configInfo) in _collimatorConfigurationsWithCalibInfo)
                {
                    if (configuration.Energy != Energy.Value)
                        continue; // skip all other energy configurations 

                    var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(configuration.Type);

                    if (FullCollection)
                    {
                        foreach (var field in fieldNameMapping)
                        {
                            var qcSampleFieldEntry = new QcSampleFieldEntry(configuration, configInfo.HeaterCurrent)
                            {
                                Name = field.Value,
                                DisplayValue = field.Key
                            };
                            QcPlan.AddField(qcSampleFieldEntry);
                        }
                    }
                    else
                    {
                        var centralCellIndex = TargetTypeConverter.GetCentralCellIndex(configuration.Type);
                        var centralFieldName = fieldNameMapping[centralCellIndex];

                        var qcSampleFieldEntry = new QcSampleFieldEntry(configuration, configInfo.HeaterCurrent)
                        {
                            Name = centralFieldName,
                            DisplayValue = centralCellIndex
                        };
                        QcPlan.AddField(qcSampleFieldEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanCreateCollectionError,
                    ex);
            }
        }

        private void OnEnergyChanged(Energy? energy)
        {
            if (!energy.HasValue)
                return;

            try
            {
                AvailableTargetTypeValues = _collimatorConfigurationsWithCalibInfo.Keys
                    .Where(c => energy == c.Energy)
                    .Select(c => c.Type).Distinct().Order();

                CreateEntryCollection();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanCreateCollectionError,
                    ex);
            }

            CheckForApplicatorCompatibility(); 

            ValidateCanExecuteCommands();
        }

        private bool HasValidPlanForTreatment()
        {
            return ApplicatorCompatibilityStatus.IsCompatible;
        }

        private void OnTreatmentFieldSelectionChanged()
        {
            AddCommand?.RaiseCanExecuteChanged();
            RemoveCommand?.RaiseCanExecuteChanged();
        }

        private void OnRemoveClicked()
        {
            try
            {
                foreach (object selectedEntry in FieldsSelectionModel.TreatmentFieldListSelection)
                {
                    IQcSampleFieldEntry qcSampleFieldEntry = selectedEntry as IQcSampleFieldEntry;

                    if (qcSampleFieldEntry != null)
                    {
                        QcPlan.RemoveField(qcSampleFieldEntry);
                        IsModified = true;
                    }
                    else
                    {
                        ITreatmentFieldEntry treatmentFieldEntry = selectedEntry as ITreatmentFieldEntry;
                        if (treatmentFieldEntry != null)
                        {
                            QcPlan.RemoveField(CollimatorType, Energy.Value, treatmentFieldEntry);
                            IsModified = true;
                        }
                    }
                }

                ValidateCanExecuteCommands();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PlanOperationErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanRemoveFieldErrorMessage,
                    ex);
            }
        }

        private void GetAvailableTargetTypesAndEnergyLevels()
        {
            try
            {
                // We first empty both lists to prevent user from selecting anything actually unavailable
                AvailableTargetTypeValues = new List<TargetType>();
                AvailableEnergyLevels = new List<Energy>();

                CurrentTask = new ObservableTask(PrepareAvailableOptionsToSelect());
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }
        }

        private Task PrepareAvailableOptionsToSelect()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (CollimatorModel.CollimatorConfigurations is null)
                        return;

                    var collimatorConfigurations = CollimatorModel.CollimatorConfigurations.Where(c => c.Type != TargetType.TargetType_QC_Collimator);

                    var calibDataStore = await CollimatorCalibrationModel.FetchCalibrationDataAsync();

                    // filter out any configurations that do not have approved default preset
                    calibDataStore = calibDataStore.Filter(x => x.CollimatorConfiguration?.DefaultPreset?.IsApproved ?? false);
                    
                    // We select only those configurations that we have calibration data for (coil currents, heater current and mangnetometer refs)
                    // TODO: there are some concerns on persistency of these calib. data in future,
                    // maybe we need to get a copy of the list of calib info here
                    _collimatorConfigurationsWithCalibInfo =
                        collimatorConfigurations.Select(c => (c, calibDataStore[c.Id])).Where(value => value.Item2 != null).ToDictionary();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableTargetTypeValues = _collimatorConfigurationsWithCalibInfo.Keys.Select(c => c.Type).Distinct().Order();
                        AvailableEnergyLevels = _collimatorConfigurationsWithCalibInfo.Keys.Select(c => c.Energy).Distinct().Order();
                    });
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync($"PrepareAvailableOptionsToSelect failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                }
            });
        }

        private void OnAddClicked()
        {
            try
            {
                // TODO: need to refactor this coupled dependency over filamentSetpoint & configuration:
                FieldsSelectionModel.HoneycombSelection.Energy = Energy.Value;
                var (collimatorConfig, configInfo) = _collimatorConfigurationsWithCalibInfo.FirstOrDefault( kv =>
                    kv.Key.Type == CollimatorType &&
                    kv.Key.Energy == Energy.Value);

                var selectedTreatmentField = FieldsSelectionModel.HoneycombSelection;
                if (!QcPlan.ContainsField(CollimatorType, Energy.Value, selectedTreatmentField))
                {
                    double filamentSetpoint = configInfo.HeaterCurrent;
                    var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(CollimatorType);

                    FieldsSelectionModel.SelectField(
                        QcPlan.AddField(
                            new QcSampleFieldEntry(collimatorConfig, filamentSetpoint)
                            {
                                Name = selectedTreatmentField.Name,
                                DisplayValue = TargetTypeConverter.GetBackwardFieldNameMapping(
                                    fieldNameMapping, selectedTreatmentField.Name)
                            })
                        );
                    IsModified = true;
                }

                ValidateCanExecuteCommands();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PlanOperationErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanAddFieldErrorMessage,
                    ex);
            }
        }

        #endregion Private methods
    }

}
