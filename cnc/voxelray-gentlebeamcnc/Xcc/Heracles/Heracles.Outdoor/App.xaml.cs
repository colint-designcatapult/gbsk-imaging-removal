using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Empyrean.Common.Infra.Settings;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.AppLayer.Warmup;
using Heracles.Application.Commands.DummyCommands;
using Heracles.Application.Commands.gRPC.Common;
using Heracles.Application.Commands.gRPC.EMR;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.Dummy;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Settings;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Treatment;
using Heracles.Application.Services;
using Heracles.Core.Commands;
using Heracles.Core.Models;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Heracles.External.Views;
using Herales.External;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using Prism.Modularity;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Commands;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.CommandAPI;
using Xcc.Infra.Logging;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Services;
using Xcc.Infra.UserSessions.BearerToken;
using Xcc.Shared.Services;
using Xcc.Shared.Views;
using AppGlobals = Heracles.Application.Models.AppGlobals;
using SystemConfiguration = Heracles.Application.Models.SystemConfiguration;



namespace Heracles.External
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            Unosquare.FFME.Library.FFmpegDirectory = @"C:\ffmpeg\";

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ReportView>();

            // services:
            containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());
            

            containerRegistry.RegisterSingleton<ISettingsReader, SettingsReader>();
            containerRegistry.RegisterManySingleton<HeraclesExternalSettings>();

            containerRegistry.RegisterSingleton<Empyrean.Common.Application.Globals.IAppGlobals,
                Empyrean.Common.Application.Globals.AppGlobals>();
            containerRegistry.RegisterSingleton<IAppGlobals, AppGlobals>();
            containerRegistry.RegisterSingleton<IPopUpService, PopUpService>();

            containerRegistry.RegisterSingleton<ISystemSettingsStore, SystemSettingsStore>();
            containerRegistry.RegisterSingleton<ISettingsModel, SettingsModel>();
            
            
            var heraclesExternalSettings = Container.Resolve<IHeraclesExternalSettings>();

            // Log service should be initialized before any other service, but after AppSettings
            if (heraclesExternalSettings.UseDummyDatabase)
            {
                containerRegistry.RegisterManySingleton<TextLogRepositoryAdapter>();
            }
            else
            {
                containerRegistry.RegisterManySingleton<GrpcChannelManager>();

                containerRegistry.RegisterManySingleton<DbLogRepositoryWithTextBackUp>();
            }

            // Shared data stores
            //containerRegistry.RegisterSingleton<IPatientStore, PatientStore>();
            //containerRegistry.RegisterSingleton<IDiagnosisStore, DiagnosisStore>();

            // Any kind of configurations
            containerRegistry.RegisterManySingleton<SystemConfiguration>();

            // Telemetry data (MonitorView)
            containerRegistry.RegisterSingleton<IGCBDataStore, Xcc.Application.Models.GCBDataStore>();
            containerRegistry.RegisterSingleton<CollimatorWatchdog>();
            containerRegistry.RegisterManySingleton<NetworkConnectionSupervisor>();

            containerRegistry.RegisterSingleton<IUIStateMachine, UIStateMachine>();

            containerRegistry.RegisterSingleton<IGcbXRayCommandOperator, GcbXRayCommandOperator>();

            // Shared data storages:
            containerRegistry.RegisterSingleton<IAuthorizedUserStore, AuthorizedUserStore>();
            containerRegistry.RegisterSingleton<ITreatmentInfoStore, TreatmentInfoStore>();
            containerRegistry.RegisterManySingleton<CollimatorModel>();
            containerRegistry.RegisterSingleton<ICollimatorRepository, CollimatorRepository>();
            containerRegistry.RegisterSingleton<IPatientRepository, PatientRepository>();
            containerRegistry.RegisterSingleton<IPlanRepository, PlanRepository>();
            containerRegistry.RegisterSingleton<ITreatmentRepository, TreatmentRepository>();
            containerRegistry.RegisterSingleton<Models.IPlanModel, Models.PlanModel>();
            containerRegistry.RegisterSingleton<LoadForTreatmentEventSource>();
            containerRegistry.RegisterSingleton<PlanEventSource>();
            containerRegistry.RegisterSingleton<Models.ITreatmentModel, Models.TreatmentModel>();
            containerRegistry.RegisterSingleton<Models.IActualTreatmentFieldModel, Models.ActualTreatmentFieldModel>();
            containerRegistry.RegisterSingleton<ITreatmentDoseCalculation, TreatmentDoseCalculation>();
            containerRegistry.RegisterManySingleton<WarmupHistory>();
            containerRegistry.RegisterSingleton<Xcc.Application.Models.IExitingModel, Xcc.Application.Models.ExitingModel>();
            containerRegistry.RegisterSingleton<IQcRepository, QcRepository>();
            containerRegistry.RegisterSingleton<IQcReportListModel, QcReportListModel>();
            containerRegistry.RegisterSingleton<ISafetyCheckModel, SafetyCheckModel>();
            containerRegistry.RegisterSingleton<IDispatcherService, DispatcherService>();
            containerRegistry.RegisterSingleton<IUserRepository, Application.Models.UserRepository>();
            containerRegistry.RegisterSingleton<IAuthorizationService, AuthorizationService>();
            containerRegistry.RegisterManySingleton<GrpcBearerTokenUserSessionManager>();

            if (heraclesExternalSettings.UseDummyServices)
            {
                containerRegistry.RegisterSingleton<IGcbCommunicationService, DummyGcbCommunicationService>();
                containerRegistry.RegisterManySingleton<GcbCommandInterface>();
                containerRegistry.RegisterManySingleton<DummyMainBoardModel>();
                containerRegistry.RegisterSingleton<ITelemetryService, DummyTelemetryService>();
                //containerRegistry.RegisterSingleton<ILogService, TextLogService>();
            }
            else
            {
                #region real services
                containerRegistry.RegisterSingleton<IGcbCommunicationService, Xcc.Infra.Services.GcbServices.GcbCommunicationService>();
                containerRegistry.RegisterSingleton<IGcbCommandInterface, GcbCommandInterface>();
                containerRegistry.RegisterManySingleton<Xcc.Infra.Services.GcbServices.GcbTelemetryService>();
                containerRegistry.RegisterManySingleton<MainBoardModelBase>();
                #endregion
            }

            containerRegistry.RegisterSingleton<IGcbIndicators, GcbIndicators>();
            containerRegistry.RegisterManySingleton<WarmupService>();

            if (heraclesExternalSettings.UseDummyDatabase)
            {
                containerRegistry.RegisterSingleton<IAuthCommands, SystemDummyAuthCommands>();
                containerRegistry.RegisterSingleton<ISettingsCommands, SystemDummySettingsCommands>();
                containerRegistry.RegisterManySingleton<MockGrpcChannelManager>();

                containerRegistry.RegisterSingleton<IEmrPatientCommands, EmrDummyPatientCommands>();
                containerRegistry.RegisterSingleton<IEmrDiagnosisCommands, EmrDummyDiagnosisCommands>();
                containerRegistry.RegisterSingleton<IEmrSimulationCommands, EmrDummySimulationCommands>();
                containerRegistry.RegisterSingleton<IEmrPrescriptionCommands, EmrDummyPrescriptionCommands>();
                containerRegistry.RegisterManySingleton<EmrDummyPlanCommands>();
                containerRegistry.RegisterSingleton<IEmrVisitCommands, EmrDummyVisitCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentDeviceCommands, EmrDummyTreatmentDeviceCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentFieldCommands, EmrDummyTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrPatientPositionCommands, EmrDummyPatientPositionCommands>();
                containerRegistry.RegisterSingleton<IEmrActualTreatmentFieldCommands, EmrDummyActualTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrEmissionTreatmentFieldCommands, EmrDummyEmissionTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentCommands, EmrDummyTreatmentCommands>();
                containerRegistry.RegisterSingleton<IEmrPhotoCommands, EmrDummyPhotoCommands>();

                containerRegistry.RegisterSingleton<IUserCommands, DummyUserCommands>();
                containerRegistry.RegisterManySingleton<DummyUserRoleMappingCommands>(
                    typeof(IUserRoleMappingCommands),
                    typeof(IUserRoleMappingCommandsExt));
                containerRegistry.RegisterSingleton<IRoleCommands, DummyRoleCommands>();
                containerRegistry.RegisterSingleton<IPermissionCommands, DummyPermissionCommands>();


                containerRegistry.RegisterSingleton<IIntensityCommands, SystemDummyIntensityCommands>();
                containerRegistry.RegisterSingleton<IQcSampleCommands, SystemDummyQcSampleCommands>();
                containerRegistry.RegisterSingleton<IQcSampleFieldCommands, SystemDummyQcSampleFieldCommands>();
                containerRegistry.RegisterSingleton<ISafetyCheckCommands, SystemDummySafetyCheckCommands>();
                containerRegistry.RegisterSingleton<IHeadCommands, SystemDummyHeadCommands>();
                containerRegistry.RegisterSingleton<ICollimatorConfigurationCommands, SystemDummyCollimatorConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICollimatorCommands, SystemDummyCollimatorCommands>();
                containerRegistry.RegisterSingleton<IPresetConfigurationCommands, SystemDummyPresetConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICoilConfigurationCommands, SystemDummyCoilConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICorrectionMatrixCommands, SystemDummyCorrectionMatrixCommands>();
                containerRegistry.RegisterSingleton<IHeaterCurrentConfigCommands, SystemDummyHeaterCurrentConfigCommands>();
                containerRegistry.RegisterSingleton<IReferenceFieldCommands, SystemDummyReferenceFieldCommands>();
                containerRegistry.RegisterSingleton<IOutputFactorCommands, SystemDummyOutputFactorCommands>();
                containerRegistry.RegisterSingleton<IWarmupCommands, SystemDummyWarmupCommands>();
                containerRegistry.RegisterSingleton<ISystemCommands, DummySystemCommands>();
            }
            else
            {
                #region gRPC
                containerRegistry.RegisterSingleton<IAuthCommands, GrpcAuthCommands>();
                containerRegistry.RegisterSingleton<ISettingsCommands, GrpcSettingsCommands>();

                containerRegistry.RegisterSingleton<IEmrPatientCommands, GrpcPatientCommands>();
                containerRegistry.RegisterSingleton<IEmrDiagnosisCommands, GrpcDiagnosisCommands>();
                containerRegistry.RegisterSingleton<IEmrSimulationCommands, GrpcSimulationCommands>();
                containerRegistry.RegisterSingleton<IEmrPrescriptionCommands, GrpcPrescriptionCommands>();
                containerRegistry.RegisterSingleton<IEmrVisitCommands, GrpcVisitCommands>();
                containerRegistry.RegisterSingleton<IEmrPlanCommands, GrpcPlanCommands>();
                containerRegistry.RegisterSingleton<ILoadForTreatmentEventStream, GrpcLoadForTreatmentEventStream>();
                containerRegistry.RegisterSingleton<IPlanEventStream, GrpcPlanEventStream>();

                containerRegistry.RegisterSingleton<IEmrTreatmentDeviceCommands, GrpcTreatmentDeviceCommands>();
                containerRegistry.RegisterSingleton<IEmrPatientPositionCommands, GrpcPatientPositionCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentFieldCommands, GrpcTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentCommands, GrpcTreatmentCommands>();
                containerRegistry.RegisterSingleton<IEmrActualTreatmentFieldCommands, GrpcActualTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrPhotoCommands, GrpcPhotoCommands>();

                containerRegistry.RegisterSingleton<IUserCommands, GrpcUserCommands>();
                containerRegistry.RegisterManySingleton<GrpcUserRoleMappingCommands>(
                    typeof(IUserRoleMappingCommands),
                    typeof(IUserRoleMappingCommandsExt));
                containerRegistry.RegisterSingleton<IRoleCommands, GrpcRoleCommands>();
                containerRegistry.RegisterSingleton<IPermissionCommands, GrpcPermissionCommands>();

                containerRegistry.RegisterSingleton<IIntensityCommands, GrpcIntensityCommands>(); 
                containerRegistry.RegisterSingleton<IQcSampleCommands, GrpcQcSampleCommands>();
                containerRegistry.RegisterSingleton<IQcSampleFieldCommands, GrpcQcSampleFieldCommands>();
                containerRegistry.RegisterSingleton<ISafetyCheckCommands, GrpcSafetyCheckCommands>();
                containerRegistry.RegisterSingleton<IHeadCommands, GrpcHeadCommands>();
                containerRegistry.RegisterSingleton<ICollimatorConfigurationCommands, GrpcCollimatorConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICollimatorCommands, GrpcCollimatorCommands>();
                containerRegistry.RegisterSingleton<ICoilConfigurationCommands, GrpcCoilConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICorrectionMatrixCommands, GrpcCorrectionMatrixCommands>();
                containerRegistry.RegisterSingleton<IPresetConfigurationCommands, GrpcPresetConfigurationCommands>();
                containerRegistry.RegisterSingleton<IReferenceFieldCommands, GrpcReferenceFieldCommands>();
                containerRegistry.RegisterSingleton<IOutputFactorCommands, GrpcOutputFactorCommands>();
                containerRegistry.RegisterSingleton<IHeaterCurrentConfigCommands, GrpcHeaterCurrentConfigCommands>();
                containerRegistry.RegisterSingleton<IWarmupCommands, GrpcWarmupCommands>();
                containerRegistry.RegisterSingleton<ILogCommands, GrpcLogCommands>();

                //containerRegistry.RegisterSingleton<IUserCommands, GrpcUserCommands>();
                containerRegistry.RegisterSingleton<ISystemCommands, GrpcSystemCommands>();
                #endregion
            }

            containerRegistry.RegisterSingleton<ICollimatorConfigurationStore, CollimatorConfigurationStore>();
            containerRegistry.RegisterSingleton<ICollimatorCalibrationModel, CollimatorCalibrationModel>();
            containerRegistry.RegisterSingleton<ICollimatorCalibrationRepository, CollimatorCalibrationRepository>();
            containerRegistry.RegisterSingleton<CollimatorCalibrationInfoStore>();
            containerRegistry.RegisterSingleton<IActionAuditService, ActionAuditService>();


            //if (appSettings.UseDummyServices)
            //{
            //    var dummyTelemetryService = Container.Resolve<ITelemetryService>() as DummyTelemetryService;
            //    Task.Run(async () => { await Task.Delay(3000); dummyTelemetryService. });
            //}
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ExternalModule>();
            moduleCatalog.AddModule<Xcc.Shared.Module>();
        }
        protected IConfiguration AddConfiguration()
        {
            var appsettings = ApplicationArgs.GetAppSettings() ?? "appsettings.json";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettings);

            return builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // In shell launcher mode, it starts with 'C:/Windows/system32' as a current directory
            // so we need to change this
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(currentDir);

            var defaultCulture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = defaultCulture;
            Thread.CurrentThread.CurrentUICulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (Container.Resolve<IGrpcChannelManager>() is { } emrGrpcSettings)
                emrGrpcSettings.ShutdownChannel();

            DisposeResources();
        }

        private void DisposeResources()
        {
            if (Container.Resolve<IGcbCommunicationService>() is IDisposable disposable)
                disposable.Dispose();

            if (Container.Resolve<ITelemetryService>() is IDisposable telemetryService)
                telemetryService.Dispose();
        }
    }
}
