using Empyrean.Common.Infra.Settings;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Commands.DummyCommands;
using Heracles.Application.Commands.gRPC.Common;
using Heracles.Application.Commands.gRPC.EMR;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess.gRPC;
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
using Heracles.Indoor.Models;
using Heracles.Indoor.Models.UseCases;
using Heracles.Indoor.Modules;
using Heracles.Indoor.Views;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using Prism.Modularity;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.UserSessions;
using Xcc.Application.Commands;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model;
using Xcc.Application.Models;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.CommandAPI;
using Xcc.Infra.Logging;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Services;
using Heracles.Indoor.SqliteGrpcServer;
using Xcc.Infra.Services.GcbServices;
using Xcc.Infra.UserSessions.BearerToken;
using Xcc.Shared.Services;
using Xcc.Shared.Views;
using System;

namespace Heracles.Indoor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public class Val1
        {
            public int value1 = 1;
        }

        protected override Window CreateShell()
        {
            Unosquare.FFME.Library.FFmpegDirectory = @"C:\ffmpeg\";

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ReportView>();

            // Any kind of configurations
            containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());
            containerRegistry.RegisterSingleton<Xcc.Core.Models.IAppGlobals, Application.Models.AppGlobals>();
            containerRegistry.RegisterSingleton<Empyrean.Common.Application.Globals.IAppGlobals,
                Empyrean.Common.Application.Globals.AppGlobals>();

            //var appSettingsInstance = AppSettingsFactory.Create<Application.Models.AppSettings>(
            //    Container.Resolve<IConfiguration>());

            containerRegistry.RegisterSingleton<ISettingsReader, SettingsReader>();
            containerRegistry.RegisterManySingleton<HeraclesMainSettings>();
            
            containerRegistry.RegisterManySingleton<Application.Models.SystemConfiguration>(
                typeof(Core.Models.ISystemConfiguration),
                typeof(Xcc.Core.Models.ISystemConfiguration));
            
            var heraclesMainSettings = Container.Resolve<HeraclesMainSettings>();

            // Shared data stores
            containerRegistry.RegisterSingleton<ITreatmentInfoStore, TreatmentInfoStore>();
            containerRegistry.RegisterSingleton<ITreatmentInfoStoreController, TreatmentInfoStoreManager>();
            containerRegistry.RegisterSingleton<IPatientRepository, PatientRepository>();
            
            containerRegistry.RegisterSingleton<ISimulationRepository, SimulationRepository>();
            containerRegistry.RegisterSingleton<IPrescriptionRepository, PrescriptionRepository>();

            containerRegistry.RegisterSingleton<IPatientListModel, PatientListModel>();
            containerRegistry.RegisterSingleton<IAuthorizedUserStore, AuthorizedUserStore>();
            containerRegistry.RegisterSingleton<ICollimatorModel, CollimatorModel>();
            containerRegistry.RegisterSingleton<ICollimatorRepository, CollimatorRepository>();
            containerRegistry.RegisterSingleton<IPlanModel, PlanModel>();
            containerRegistry.RegisterSingleton<ITreatmentDoseCalculation, TreatmentDoseCalculation>();
            //containerRegistry.RegisterSingleton<IDiagnosisStore, DiagnosisStore>();
            containerRegistry.RegisterSingleton<ITreatmentHistoryModel, TreatmentHistoryModel>();
            containerRegistry.RegisterSingleton<IMagnetometerCorrectionsStore, MagnetometerCorrectionsStore>();
            containerRegistry.RegisterSingleton<IHeaterCurrentStore, HeaterCurrentStore>();
            containerRegistry.RegisterSingleton<ICoilConfigurationStore, CoilConfigurationStore>();
            containerRegistry.RegisterSingleton<IOutputFactorConfigurationStore, OutputFactorConfigurationStore>();
            containerRegistry.RegisterSingleton<ISystemSettingsStore, SystemSettingsStore>();
            containerRegistry.RegisterSingleton<IPlanLoading, PlanLoading>();
            containerRegistry.RegisterSingleton<LoadForTreatmentEventSource>();
            containerRegistry.RegisterSingleton<PlanEventSource>();
            containerRegistry.RegisterSingleton<ISeriesModel, SeriesModel>();
            containerRegistry.RegisterSingleton<IAcquisitionModel, AcquisitionModel>();
            containerRegistry.RegisterSingleton<IExitingModel, ExitingModel>();
            containerRegistry.RegisterSingleton<IAcquisitionResultStore, AcquisitionResultStore>();
            containerRegistry.RegisterSingleton<IQcRepository, QcRepository>();
            containerRegistry.RegisterSingleton<IQcReportListModel, QcReportListModel>();
            containerRegistry.RegisterSingleton<IUserRepository, Application.Models.UserRepository>();
            containerRegistry.RegisterSingleton<IPlanRepository, PlanRepository>();
            containerRegistry.RegisterSingleton<ISettingsModel, SettingsModel>();
            containerRegistry.RegisterManySingleton<GrpcBearerTokenUserSessionManager>();


            // Telemetry data (MonitorView)
            containerRegistry.RegisterManySingleton<MainBoardState>();
            containerRegistry.RegisterSingleton<IGCBDataStore, GCBDataStore>();
            containerRegistry.RegisterSingleton<CollimatorWatchdog>();
            containerRegistry.RegisterManySingleton<NetworkConnectionSupervisor>();

            // Services
            containerRegistry.RegisterSingleton<IPopUpService, PopUpService>();
            containerRegistry.RegisterSingleton<IDispatcherService, DispatcherService>();
            containerRegistry.RegisterManySingleton<Application.Models.Supervision.DisruptiveActionGuard>();
            containerRegistry.RegisterSingleton<IAuthorizationService, AuthorizationService>();
            containerRegistry.RegisterSingleton<SessionExpirationWatchdog>();
            containerRegistry.RegisterSingleton<IPhotoService, PhotoService>();

            


            //containerRegistry.RegisterManySingleton<Infra.Services.XRayService>(
            //    typeof(Xcc.Infra.Services.XRayService),
            //    typeof(Xcc.Application.Services.IXRayService),
            //    typeof(Application.Services.IXRayService));
            containerRegistry.RegisterSingleton<IGcbXRayCommandOperator, GcbXRayCommandOperator>();

            if (heraclesMainSettings.UseSqliteDatabase)
            {
                // Start the embedded SQLite gRPC server and register a local channel manager
                var dbPath = Path.Combine(
                    heraclesMainSettings.StorageRoot,
                    "heracles.db");
                var sqliteHost = new SqliteGrpcServerHost(dbPath);
                sqliteHost.StartAsync().GetAwaiter().GetResult();
                containerRegistry.RegisterInstance<SqliteGrpcServerHost>(sqliteHost);
                containerRegistry.RegisterManySingleton<SqliteGrpcChannelManager>();

                // Reuse the same gRPC command registrations as the real server path
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
                containerRegistry.RegisterSingleton<IEmrSeriesCommands, GrpcSeriesCommands>();
                containerRegistry.RegisterSingleton<IEmrPhotoCommands, GrpcPhotoCommands>();

                containerRegistry.RegisterSingleton<IUserCommands, GrpcUserCommands>();
                containerRegistry.RegisterManySingleton<GrpcUserRoleMappingCommands>(
                    typeof(IUserRoleMappingCommands),
                    typeof(IUserRoleMappingCommandsExt));
                containerRegistry.RegisterSingleton<IRoleCommands, GrpcRoleCommands>();
                containerRegistry.RegisterSingleton<IPermissionCommands, GrpcPermissionCommands>();

                containerRegistry.RegisterSingleton<IWarmupCommands, GrpcWarmupCommands>();
                containerRegistry.RegisterSingleton<ISafetyCheckCommands, GrpcSafetyCheckCommands>();
                containerRegistry.RegisterSingleton<IQcSampleCommands, GrpcQcSampleCommands>();
                containerRegistry.RegisterSingleton<IQcSampleFieldCommands, GrpcQcSampleFieldCommands>();
                containerRegistry.RegisterSingleton<IIntensityCommands, GrpcIntensityCommands>();
                containerRegistry.RegisterSingleton<IHeadCommands, GrpcHeadCommands>();
                containerRegistry.RegisterSingleton<ICollimatorCommands, GrpcCollimatorCommands>();
                containerRegistry.RegisterSingleton<ICollimatorConfigurationCommands, GrpcCollimatorConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICoilConfigurationCommands, GrpcCoilConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICorrectionMatrixCommands, GrpcCorrectionMatrixCommands>();
                containerRegistry.RegisterSingleton<IPresetConfigurationCommands, GrpcPresetConfigurationCommands>();
                containerRegistry.RegisterSingleton<IReferenceFieldCommands, GrpcReferenceFieldCommands>();
                containerRegistry.RegisterSingleton<IOutputFactorCommands, GrpcOutputFactorCommands>();
                containerRegistry.RegisterSingleton<IHeaterCurrentConfigCommands, GrpcHeaterCurrentConfigCommands>();
                containerRegistry.RegisterSingleton<ILogCommands, GrpcLogCommands>();
                containerRegistry.RegisterSingleton<IPhotoStreamReader, GrpcPhotoStreamReader>();
                containerRegistry.RegisterSingleton<ISystemCommands, GrpcSystemCommands>();
            }
            else if (heraclesMainSettings.UseDummyDatabase)
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
                containerRegistry.RegisterSingleton<IEmrPatientPositionCommands, EmrDummyPatientPositionCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentFieldCommands, EmrDummyTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrActualTreatmentFieldCommands, EmrDummyActualTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrEmissionTreatmentFieldCommands, EmrDummyEmissionTreatmentFieldCommands>();
                containerRegistry.RegisterSingleton<IEmrTreatmentCommands, EmrDummyTreatmentCommands>();
                containerRegistry.RegisterSingleton<IEmrSeriesCommands, EmrDummySeriesCommands>();
                containerRegistry.RegisterSingleton<IEmrPhotoCommands, EmrDummyPhotoCommands>();

                containerRegistry.RegisterSingleton<IUserCommands, DummyUserCommands>();
                containerRegistry.RegisterManySingleton<DummyUserRoleMappingCommands>(
                    typeof(IUserRoleMappingCommands),
                    typeof(IUserRoleMappingCommandsExt));
                containerRegistry.RegisterSingleton<IRoleCommands, DummyRoleCommands>();
                containerRegistry.RegisterSingleton<IPermissionCommands, DummyPermissionCommands>();

                containerRegistry.RegisterSingleton<IHeadCommands, SystemDummyHeadCommands>();
                containerRegistry.RegisterSingleton<ICollimatorCommands, SystemDummyCollimatorCommands>();
                containerRegistry.RegisterSingleton<ICollimatorConfigurationCommands, SystemDummyCollimatorConfigurationCommands>();
                containerRegistry.RegisterSingleton<IQcSampleCommands, SystemDummyQcSampleCommands>();
                containerRegistry.RegisterSingleton<IQcSampleFieldCommands, SystemDummyQcSampleFieldCommands>();
                containerRegistry.RegisterSingleton<IIntensityCommands, SystemDummyIntensityCommands>();
                containerRegistry.RegisterSingleton<IPresetConfigurationCommands, SystemDummyPresetConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICoilConfigurationCommands, SystemDummyCoilConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICorrectionMatrixCommands, SystemDummyCorrectionMatrixCommands>();
                containerRegistry.RegisterSingleton<IHeaterCurrentConfigCommands, SystemDummyHeaterCurrentConfigCommands>();
                containerRegistry.RegisterSingleton<IReferenceFieldCommands, SystemDummyReferenceFieldCommands>();
                containerRegistry.RegisterSingleton<IOutputFactorCommands, SystemDummyOutputFactorCommands>();
                containerRegistry.RegisterSingleton<IWarmupCommands, SystemDummyWarmupCommands>();
                containerRegistry.RegisterSingleton<ISafetyCheckCommands, SystemDummySafetyCheckCommands>();
                containerRegistry.RegisterSingleton<IPhotoStreamReader, DummyPhotoStreamReader>();
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
                //containerRegistry.RegisterSingleton<IUserCommands, GrpcUserCommands>();
                containerRegistry.RegisterSingleton<IEmrSeriesCommands, GrpcSeriesCommands>();
                containerRegistry.RegisterSingleton<IEmrPhotoCommands, GrpcPhotoCommands>();

                containerRegistry.RegisterSingleton<IUserCommands, GrpcUserCommands>();
                containerRegistry.RegisterManySingleton<GrpcUserRoleMappingCommands>(
                    typeof(IUserRoleMappingCommands),
                    typeof(IUserRoleMappingCommandsExt));
                containerRegistry.RegisterSingleton<IRoleCommands, GrpcRoleCommands>();
                containerRegistry.RegisterSingleton<IPermissionCommands, GrpcPermissionCommands>();

                containerRegistry.RegisterSingleton<IWarmupCommands, GrpcWarmupCommands>();
                containerRegistry.RegisterSingleton<ISafetyCheckCommands, GrpcSafetyCheckCommands>();
                containerRegistry.RegisterSingleton<IQcSampleCommands, GrpcQcSampleCommands>();
                containerRegistry.RegisterSingleton<IQcSampleFieldCommands, GrpcQcSampleFieldCommands>();
                containerRegistry.RegisterSingleton<IIntensityCommands, GrpcIntensityCommands>();
                containerRegistry.RegisterSingleton<IHeadCommands, GrpcHeadCommands>();
                containerRegistry.RegisterSingleton<ICollimatorCommands, GrpcCollimatorCommands>();
                containerRegistry.RegisterSingleton<ICollimatorConfigurationCommands, GrpcCollimatorConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICoilConfigurationCommands, GrpcCoilConfigurationCommands>();
                containerRegistry.RegisterSingleton<ICorrectionMatrixCommands, GrpcCorrectionMatrixCommands>();
                containerRegistry.RegisterSingleton<IPresetConfigurationCommands, GrpcPresetConfigurationCommands>();
                containerRegistry.RegisterSingleton<IReferenceFieldCommands, GrpcReferenceFieldCommands>();
                containerRegistry.RegisterSingleton<IOutputFactorCommands, GrpcOutputFactorCommands>();
                containerRegistry.RegisterSingleton<IHeaterCurrentConfigCommands, GrpcHeaterCurrentConfigCommands>();
                containerRegistry.RegisterSingleton<ILogCommands, GrpcLogCommands>();
                containerRegistry.RegisterSingleton<IPhotoStreamReader, GrpcPhotoStreamReader>();
                containerRegistry.RegisterSingleton<ISystemCommands, GrpcSystemCommands>();
                #endregion
            }
            // Log service should be initialized before any other service, but after AppSettings
            if (heraclesMainSettings.UseDummyDatabase)
            {
                containerRegistry.RegisterManySingleton<TextLogRepositoryAdapter>();
            }
            else
            {
                // SqliteGrpcChannelManager is already registered above when UseSqliteDatabase=true;
                // for real gRPC and SQLite modes register the log+channel manager here.
                if (!heraclesMainSettings.UseSqliteDatabase)
                    containerRegistry.RegisterManySingleton<GrpcChannelManager>();
                containerRegistry.RegisterManySingleton<DbLogRepositoryWithTextBackUp>();
            }

            containerRegistry.RegisterSingleton<IActionAuditService, ActionAuditService>();

            if (heraclesMainSettings.UseDummyServices)
            {
                containerRegistry.RegisterSingleton<ITelemetryService, DummyTelemetryService>();
            }
            else
            {
                containerRegistry.RegisterSingleton<ITelemetryService, GcbTelemetryService>();
            }


            containerRegistry.RegisterSingleton<FieldModel>();
            Container.Resolve<FieldModel>();
        }
         
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Xcc.Application.ApplicationModule>();
            moduleCatalog.AddModule<Xcc.Shared.Module>();
            moduleCatalog.AddModule<MainModule>();
        }

        protected IConfiguration AddConfiguration()
        {
            var appsettings = ApplicationArgs.GetAppSettings() ?? "appsettings.json";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettings);

            return builder.Build();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Stop the embedded SQLite gRPC server if it was started
            if (Container.IsRegistered<SqliteGrpcServerHost>())
                Container.Resolve<SqliteGrpcServerHost>().StopAsync().GetAwaiter().GetResult();

            if (Container.Resolve<IGrpcChannelManager>() is { } emrGrpcSettings)
                emrGrpcSettings.ShutdownChannel();

            DisposeResources();

            // Force-terminate the process so background threads (e.g. the Kestrel thread pool)
            // don't keep it alive after the WPF window has closed.
            Environment.Exit(e.ApplicationExitCode);
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

        private void DisposeResources()
        {
            Xcc.Application.Helpers.ContainerProviderHelper.DisposeByType<ITelemetryService>(Container);
        }
    }
}
