using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Common;
using Heracles.Application.Helpers;
using Heracles.Application.Helpers.DummyData;
using Heracles.Application.Models;
using Heracles.Application.Models.Settings;
using Heracles.Application.Models.Supervision;
using Heracles.Application.UI.Views;
using Heracles.Core.Models;
using Heracles.Indoor.Views;
using Heracles.Indoor.Views.Dialogs;
using Heracles.Indoor.Views.Patients.Patient.Treatments;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Threading.Tasks;
using Unity;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.UserSessions;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI;
using Xcc.Application.ViewModels;
using Xcc.Application.Views;
using Xcc.Application.Views.Approval;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions;
using Xcc.Shared.Views;


namespace Heracles.Indoor.Modules;

internal class MainModule(IRegionManager regionManager, IDialogService dialogService, IExitingModel exitingModel, IHeraclesMainSettings heraclesMainSettings): IModule
{
    public async void OnInitialized(IContainerProvider containerProvider)
    {
        try
        {
            SetupEmissionPower();

            containerProvider.GetContainer().AddExtension(new Diagnostic()); // todo: just for debug

            // Setup user session events subscriptions before we start logging-in
            SetupSessionExpiration(containerProvider);

            await LoginUserAsync(containerProvider);

            if (heraclesMainSettings.DebugPopulateEmptyDBWithDummyData)
            {
                PopulateDatabaseWithDummyData(containerProvider);
            }

            await CheckDeviceSerialIdAsync(containerProvider);

            StartTelemetryService(containerProvider);

            // Prefetch applicator data to have CollimatorModel ready for use
            await FetchCollimatorDataAsync(containerProvider);

            containerProvider.GetContainer().Resolve<CollimatorWatchdog>();

            regionManager.RequestNavigate(Regions.MainRegion, nameof(MainTabsView));
            regionManager.RequestNavigate(Regions.Main.Settings.UserManagementRegion, nameof(UserManagementView));
            regionManager.RequestNavigate(Regions.Main.Settings.UserPermissionsRegion, nameof(UserRolesView));
        }
        catch (Exception ex)
        {
            dialogService.ReportError($"Failed to initialize application.", ex.Message, _ => exitingModel.ExitApplication());
        }
    }

    private static void SetupSessionExpiration(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<SessionExpirationWatchdog>();
        var sessionEvents = containerProvider.Resolve<INotifyUserSessionChanged>();
        var treatmentEventSource = containerProvider.Resolve<LoadForTreatmentEventSource>();
        var planEventSource = containerProvider.Resolve<PlanEventSource>();
        sessionEvents.UserSessionChanged += (_, e) =>
        {
            switch (e.EventType)
            {
                case UserSessionEventType.Open:
                case UserSessionEventType.Unlocked:
                    treatmentEventSource.Start();
                    planEventSource.Start();
                    break;
                default:
                    treatmentEventSource.Stop();
                    planEventSource.Stop();
                    break;
            }
        };
    }

    private void SetupEmissionPower()
    {
        if (heraclesMainSettings.XrayTubePower50kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower50kV = heraclesMainSettings.XrayTubePower50kV;
        }
        if (heraclesMainSettings.XrayTubePower70kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower70kV = heraclesMainSettings.XrayTubePower70kV;
        }
        if (heraclesMainSettings.XrayTubePower100kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower100kV = heraclesMainSettings.XrayTubePower100kV;
        }
    }

    private void StartTelemetryService(IContainerProvider containerProvider)
    {
        var networkSupervisor = containerProvider.Resolve<NetworkConnectionSupervisor>();

        var telemetryService = containerProvider.Resolve<ITelemetryService>();
        telemetryService.Start(TelemetryServiceMode.Passive);
    }

    private async Task CheckDeviceSerialIdAsync(IContainerProvider containerProvider)
    {
        try
        {
            ISettingsModel settingsModel = containerProvider.Resolve<ISettingsModel>();
            var settings = await settingsModel.FetchSettingsAsync();

            if (string.IsNullOrWhiteSpace(settings.DeviceSerial))
            {
                dialogService.ShowDialog("DeviceSerialView", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        exitingModel.ExitApplication();
                    }
                });
            }
        }
        catch (Exception)
        {
            // TODO: we should log this exception, but we need to start logging after authentication
            dialogService.ReportError(
                StringConstants.SystemSettings.DeviceSerialIdCheckErrorTitle, 
                StringConstants.SystemSettings.DeviceSerialIdCheckError);
        }
    }

    private static void PopulateDatabaseWithDummyData(IContainerProvider containerProvider)
    {
        var dummySystemData = containerProvider.Resolve<DummySystemData>();
        var dummyEmrData = containerProvider.Resolve<DummyEmrData>();
        System.Threading.Tasks.Task.Run(async () =>
        {
            dummySystemData.PopulateDB();
            await dummyEmrData.PopulateDB();
        }).GetAwaiter().GetResult();
    }

    private static async Task FetchCollimatorDataAsync(IContainerProvider containerProvider)
    {
        var collimatorService = containerProvider.GetContainer().Resolve<CollimatorService>();
        var logWriter = containerProvider.GetContainer().Resolve<ILogWriter>();
        try
        {
            await collimatorService.UpdateCollimatorModelAsync();
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync($"Failed to fetch Collimators: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
        }
    }
    private async Task LoginUserAsync(IContainerProvider containerProvider)
    {
        var userStore = containerProvider.Resolve<IAuthorizedUserStore>();
        var authorizationService = containerProvider.Resolve<IAuthorizationService>();
        var eventAggregator = containerProvider.Resolve<IEventAggregator>();

        if (!string.IsNullOrEmpty(heraclesMainSettings.DebugAuthUsername) && !string.IsNullOrEmpty(heraclesMainSettings.DebugAuthPassword))
        {
            // authorize with debug username & password
            try
            {
                userStore.AuthorizedUser = await authorizationService.LoginAsync(
                    heraclesMainSettings.DebugAuthUsername,
                    heraclesMainSettings.DebugAuthPassword);
            }                
            catch (Exception ex)
            {
                dialogService.ReportError("AutoLogin authorization error", ex.Message);
                exitingModel.ExitApplication();
                throw;
            }
        }
        else
        {
            bool loginCancelled = false;
            dialogService.ShowDialog("LoginView", r =>
            {
                if (r.Result == ButtonResult.Cancel)
                {
                    exitingModel.ExitApplication();
                    loginCancelled = true;
                }
            });
            if (loginCancelled)
            {
                throw new Exception("Login cancelled");
            }
        }
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MainTabsView>();
        containerRegistry.RegisterForNavigation<ClinicalDataView>();
        containerRegistry.RegisterForNavigation<ClinicalDataTabsView>();
        containerRegistry.RegisterForNavigation<PlanView>();
        containerRegistry.RegisterForNavigation<TreatmentsView>();
        containerRegistry.RegisterForNavigation<CameraView>();
        containerRegistry.RegisterForNavigation<PatientImagesView>();

        containerRegistry.RegisterDialog<AcknowledgeSimulationView>();
        containerRegistry.RegisterDialog<AcknowledgePrescriptionView>();

        containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
        containerRegistry.RegisterDialog<ApproveView>();
        containerRegistry.RegisterDialog<ApprovalView>();
        containerRegistry.RegisterDialog<DeviceSerialView>();
        containerRegistry.RegisterDialog<InterlocksDialogView>();
        containerRegistry.RegisterDialog<FaultsView>();
    }
}