using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Helpers.DummyData;
using Heracles.Application.Models;
using Heracles.Application.Models.Settings;
using Heracles.Application.Models.Supervision;
using Heracles.Application.UI.Views;
using Heracles.Core.Models;
using Heracles.External.Views;
using Heracles.External.Views.QualityCheck;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.UserSessions;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI;
using Xcc.Application.ViewModels;
using Xcc.Application.Views.TreatmentConsole.QualityAssurance;
using Xcc.Core.Constants;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.QualityCheck.Comm.Udp.MockServers;
using Xcc.Infra.UserSessions;

namespace Herales.External
{
    public class ExternalModule(
        IRegionManager regionManager,
        IDialogService dialogService,
        IExitingModel exitingModel,
        IHeraclesExternalSettings heraclesExternalSettings,
        IAuthorizationService authorizationService,
        IAuthorizedUserStore authorizedUserStore) : IModule
    {
        public async void OnInitialized(IContainerProvider containerProvider)
        {
            SetupEmissionPower();
#if DEBUG
            containerProvider.GetContainer().AddExtension(new Diagnostic()); // todo: just for debug
#endif
            try
            {
                // Setup user session events subscriptions before we start logging-in
                SetupSessionExpiration(containerProvider);

                // Authenticate
                await LoginAsync();

                // Now, being authenticated, we're ready to populate the DB if needed:
                if (heraclesExternalSettings.DebugPopulateEmptyDBWithDummyData)
                {
                    PopulateDatabaseWithDummyData(containerProvider);
                }


                // Get system network settings:
                ISettingsModel settingsModel = containerProvider.Resolve<ISettingsModel>();
                var settings = await settingsModel.FetchSettingsAsync();

                // Start telemetry service:
                StartGcbServices(containerProvider);

                // Prefetch collimator data to have CollimatorModel ready for use
                await FetchCollimatorDataAsync(containerProvider);

                // Run watchdog
                containerProvider.GetContainer().Resolve<CollimatorWatchdog>();
            }
            catch (Exception ex)
            {
                var logWriter = containerProvider.Resolve<ILogWriter>();
                await logWriter.LogAsync($"App initialization error: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);

                var dialogService = containerProvider.Resolve<DialogService>();
                dialogService.ReportError(StringConstants.Common.ErrorTitle, ex.Message);

                throw;
            }

            regionManager.RequestNavigate(Regions.ExternalRegion, "ExternalTabsView");
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

        private void SetupEmissionPower()
        {
            if (heraclesExternalSettings.XrayTubePower50kV is > 0.0 and <= 500.0)
            {
                CurrentCalculator.HvpsPower50kV = heraclesExternalSettings.XrayTubePower50kV;
            }
            if (heraclesExternalSettings.XrayTubePower70kV is > 0.0 and <= 500.0)
            {
                CurrentCalculator.HvpsPower70kV = heraclesExternalSettings.XrayTubePower70kV;
            }
            if (heraclesExternalSettings.XrayTubePower100kV is > 0.0 and <= 500.0)
            {
                CurrentCalculator.HvpsPower100kV = heraclesExternalSettings.XrayTubePower100kV;
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
        private async Task LoginAsync()
        {
            if (!string.IsNullOrEmpty(heraclesExternalSettings.DebugAuthUsername) && !string.IsNullOrEmpty(heraclesExternalSettings.DebugAuthPassword))
            {
                // authenticate and authorize with debug username & password
                try
                {
                    authorizedUserStore.AuthorizedUser = await authorizationService.LoginAsync(
                        heraclesExternalSettings.DebugAuthUsername,
                        heraclesExternalSettings.DebugAuthPassword);
                }
                catch (AuthorizationServiceException ex)
                {
                    dialogService.ReportError("AutoLogin authorization error", ex.Message);
                    exitingModel.ExitApplication();
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

        private static void PopulateDatabaseWithDummyData(IContainerProvider containerProvider)
        {
            var dummySystemData = containerProvider.Resolve<DummySystemData>();
            var dummyEmrData = containerProvider.Resolve<DummyEmrData>();
            System.Threading.Tasks.Task.Run(() =>
            {
                dummySystemData.PopulateDB();
                dummyEmrData.PopulateDB();
            }).GetAwaiter().GetResult();
        }

        private void StartGcbServices(IContainerProvider containerProvider)
        {
            try
            {
                var networkSupervisor = containerProvider.Resolve<NetworkConnectionSupervisor>();

                var telemetryService = containerProvider.Resolve<ITelemetryService>();
                telemetryService.Start(TelemetryServiceMode.Active);

                var gcbCommandService = containerProvider.Resolve<IGcbCommunicationService>();
                gcbCommandService.Start();
            }
            catch (Exception)
            {
                dialogService.ReportError("GCB connection error", "Please check GCB connection settings");
                throw;
            }
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TreatmentView>();
            containerRegistry.RegisterForNavigation<ExternalTabsView>();
            
            containerRegistry.RegisterForNavigation<QaTabsView>();

            containerRegistry.RegisterForNavigation<BeamQaSelectorView>();
            containerRegistry.RegisterForNavigation<BeamQaReportsView>();
            containerRegistry.RegisterForNavigation<BeamQaView>();
            
            containerRegistry.RegisterForNavigation<SafetyCheckTabView>();
            containerRegistry.RegisterForNavigation<SafetyCheckReportsView>();
            containerRegistry.RegisterForNavigation<SafetyCheckView>();
            containerRegistry.RegisterForNavigation<InterlocksDialogView>();

            containerRegistry.RegisterDialog<FaultsView>();
            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
        }
    }
}
