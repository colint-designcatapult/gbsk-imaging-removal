using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Supervision.DisruptiveActions;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;

using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Xcc.Application.Common;
using Xcc.Application.UI;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

using static Heracles.Application.Common.StringConstants;

namespace Heracles.Indoor.ViewModels
{    
    public abstract class TreatmentViewModelBase : BindableBase, INavigationAware
    {
        #region Constructors
        protected TreatmentViewModelBase(
            IRegionManager regionManager,
            ILogWriter logWriter,
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            IDisruptiveActionGuardService disruptiveActionGuardService,
            ITreatmentInfoStore treatmentInfoStore,
            ICollimatorModel collimatorModel,
            IPlanModel planModel)
        {
            RegionManager = regionManager;
            LogWriter = logWriter;
            EventAggregator = eventAggregator;
            DialogService = dialogService;
            DisruptiveActionGuardService = disruptiveActionGuardService;
            TreatmentInfoStore = treatmentInfoStore;
            CollimatorModel = collimatorModel;
            PlanModel = planModel;
            ApplicatorCompatibilityService = new(collimatorModel);

            //Event subscriptions
            TreatmentInfoStore.DiagnosisChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(CanCapturePhoto));
            };

            TreatmentInfoStore.SimulationChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(CanCapturePhoto));
            };

            CollimatorModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ICollimatorModel.ActiveCollimator))
                    UpdateTargetsMismatchText();
            };
            EventAggregator.GetEvent<SimulationFormChanged>().Subscribe(OnSimulationFormChanged);
            EventAggregator.GetEvent<PrescriptionFormChanged>().Subscribe(OnPrescriptionFormChanged);

            UpdateTargetsMismatchText();
        }

        private void OnPrescriptionFormChanged(PrescriptionForm? state)
        {
            RequiredApplicatorEnergy = state?.Energy;
        }

        private void OnSimulationFormChanged(ISimulationState? state)
        {
            RequiredApplicatorType = state?.TargetType;
        }
        #endregion Constructors



        #region Injected Dependencies
        public IRegionManager RegionManager { get; }
        public ILogWriter LogWriter { get; }
        public IEventAggregator EventAggregator { get; }
        public IDialogService DialogService { get; }
        public IDisruptiveActionGuardService DisruptiveActionGuardService { get; }
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public ICollimatorModel CollimatorModel { get; }
        public IPlanModel PlanModel { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }
        #endregion Injected Dependencies



        #region Properties

        private Energy? _requiredApplicatorEnergy = null;
        public Energy? RequiredApplicatorEnergy
        {
            get => _requiredApplicatorEnergy;
            set
            {
                if (SetProperty(ref _requiredApplicatorEnergy, value))
                {
                    UpdateTargetsMismatchText();
                }
            }
        }

        private TargetType? _requiredApplicatorType = null;
        public TargetType? RequiredApplicatorType
        {
            get => _requiredApplicatorType;
            set
            {
                if (SetProperty(ref _requiredApplicatorType, value))
                {
                    UpdateTargetsMismatchText();
                }
            }
        }
        #endregion Properties



        #region Commands
        private DelegateCommand? _exitCommand;
        public DelegateCommand ExitCommand => _exitCommand ??= new DelegateCommand(
            () =>
            {
                if (!InvokeQuitTreatmentAction())
                    return;

                RegionManager.Regions[Regions.MainRegion].NavigationService.Journal.GoBack();
            });





        private DelegateCommand? _capturePhotoCommand;
        public DelegateCommand CapturePhotoCommand => _capturePhotoCommand ??= new DelegateCommand(
            () =>
            {
                if (!InvokeQuitTreatmentAction())
                    return;

                RegionManager.RequestNavigate(Regions.Main.ClinicalDataRegion, "CameraView");
            }).ObservesCanExecute(() => CanCapturePhoto);

        private bool CanCapturePhoto => TreatmentInfoStore is { Diagnosis.Archived: false, Simulation: not null};
        #endregion Commands



        #region Private methods
        protected void ShowDialog(string title, string message, Xcc.Core.Enums.ReportType reportType = ReportType.Error)
        {
            var report = new Xcc.Application.Models.Report(reportType, title, message);

            DialogParameters parameters = new() { { "Report", report } };
            DialogService.ShowDialog("ReportView", parameters, result => { });
        }

        protected async Task ShowAndLogErrorAsync(string title, string dialogMessage, Exception ex = null)
        {
            ShowDialog(title, dialogMessage);

            await LogWriter.LogAsync($"{title}: {dialogMessage}. {ex?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
        }


        protected int GetCollimatorCellsCount(IPlan plan)
        {
            if (plan is null)
            {
                LogWriter.Log($"GetCollimatorCellsCount: plan is null", LogRecordSeverity.Error, LogRecordType.Error);
                return 0;
            }

            var description = plan.CollimatorType.GetAttributeOfType<DescriptionAttribute>();
            return int.Parse(description.Description);
        }

        protected bool InvokeQuitTreatmentAction()
        {
            var lockType = DisruptiveActionGuardService.GetLockType<QuitTreatmentAction>();
            switch (lockType)
            {
                case DisruptiveActionLockType.Block:
                    // Just show error and stop
                    ShowDialog(
                        StringConstants.Common.ErrorTitle,
                        StringConstants.EMR.LeaveClinicalViewUnsavedChangesError);
                    return false;
                case DisruptiveActionLockType.Warn:
                    // Show warning and act based on that (confirm or cancel the action)
                    if (DialogService.Confirmation(
                        StringConstants.EMR.LeaveClinicalViewDiscardChangesConfirmationTitle,
                        StringConstants.EMR.LeaveClinicalViewDiscardChangesConfirmationMessage))
                    {
                        break; // go ahead
                    }
                    else
                    {
                        return false; // discard exit action
                    }
                case DisruptiveActionLockType.None:
                default:
                    // Just go ahead
                    break;
            }

            DisruptiveActionGuardService.Invoke<QuitTreatmentAction>();

            return true;
        }
        #endregion Private methods



        #region INavigationAware
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion INavigationAware



        #region 'Target Mismatch' members
        private ApplicatorCompatibilityStatus _applicatorCompatibilityStatus;
        public ApplicatorCompatibilityStatus ApplicatorCompatibilityStatus
        {
            get => _applicatorCompatibilityStatus;
            set => SetProperty(ref _applicatorCompatibilityStatus, value);
        }


        private void UpdateTargetsMismatchText()
        {
            try
            {
                var requiredParameters = ApplicatorParameters.FromValues(
                    RequiredApplicatorType,
                    RequiredApplicatorEnergy);

                ApplicatorCompatibilityStatus =
                    (requiredParameters is null)
                    ? ApplicatorCompatibilityStatus.Compatible
                    : ApplicatorCompatibilityService.Check(requiredParameters.Value);
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"{ClinicalData.ApplicatorCheckErrorMessage} {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
            }
        }
        #endregion 'Target Mismatch' members
    }
}
