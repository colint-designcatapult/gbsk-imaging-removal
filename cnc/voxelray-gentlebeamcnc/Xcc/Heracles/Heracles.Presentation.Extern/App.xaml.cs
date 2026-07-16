using FFmpeg.AutoGen;
using Microsoft.Extensions.Configuration;
using Xcc.Application.Models;
using Xcc.Application.Services;
using Xcc.Application.Services.AVRG;
using Xcc.Application.Services.Detector;
using Xcc.Core.Models;
using Xcc.Core.Models.RDBMS.Morpheus;
using Xcc.Infra.Services;
using Xcc.Infra.Services.AVRG;
using Xcc.Infra.Services.Detector;
using Heracles.Modules.Main;
using Heracles.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.IO;
using System.Windows;
using IParameters = Xcc.Application.Services.IParameters;

namespace Heracles
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            // location of the ffmpeg binaries
            //Unosquare.FFME.Library.FFmpegDirectory = @"C:\ffmpeg";
            Unosquare.FFME.Library.FFmpegDirectory = @"C:\Users\david\source\repos\ffmpeg-n4.4-latest-win64-gpl-shared-4.4\bin";

            //Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.MinimumFeatures;
            Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.VideoOnly;

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // services:
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IMorpheusDataService, MorpheusDataService>();
            containerRegistry.RegisterSingleton<IEmrDataService, EmrDataService>();
            containerRegistry.RegisterSingleton<IEmrService, AaronEmrService>();
            containerRegistry.RegisterSingleton<IEmrRepositoriesService, EmrRepositoriesService>();
            containerRegistry.RegisterSingleton<ILogService, LogService>();
            containerRegistry.RegisterSingleton<IAppSettings, AppSettings>();
            containerRegistry.RegisterSingleton<ITelemetryService, AaronTelemetryService>();
            //containerRegistry.RegisterSingleton<ITelemetryService, TelemetryService>();
            containerRegistry.RegisterSingleton<IUPSService, UPSService>();
            containerRegistry.RegisterSingleton<IGCBCommandsService, GCBCommandsService>();
            containerRegistry.RegisterSingleton<IGCBCommunicationService, GCBCommunicationService>();
            containerRegistry.RegisterSingleton<IXRayService, XRayService>();
            containerRegistry.RegisterSingleton<IImagingService, ImagingService>();
            containerRegistry.RegisterSingleton<IExternChannelService, ExternChannelService>();
            containerRegistry.RegisterSingleton<ITreatmentPlanSlaveService, TreatmentPlanSlaveService>();

            //containerRegistry.RegisterSingleton<IUDPServerService, UDPServerService>();
            //containerRegistry.RegisterSingleton<IUDPClientService, UDPClientService>();

            // shared data:
            containerRegistry.RegisterSingleton<ISystemConfiguration, SystemConfiguration>();
            containerRegistry.RegisterSingleton<IAppGlobals, AppGlobals>();
            containerRegistry.RegisterSingleton<ISystemTelemetry, SystemTelemetry>();
            containerRegistry.RegisterSingleton<IMonitorSystemIndicators, MonitorSystemIndicators>();
            containerRegistry.RegisterSingleton<INotificationsArea, NotificationsArea>();
            containerRegistry.RegisterSingleton<IGCBFaults, GCBFaults>();
            containerRegistry.RegisterSingleton<IGCBInterlocks, GCBInterlocks>();
            containerRegistry.RegisterSingleton<IUPSTelemetry, UPSTelemetry>();
            containerRegistry.RegisterSingleton<IGCBPushButtons, GCBPushButtons>();
            containerRegistry.RegisterSingleton<ITargetsConfigurationPreset, ActiveTargetsConfigurationPreset>();
            containerRegistry.RegisterSingleton<IMagneticCorrectionMatrices, MagneticCorrectionMatrices>();
            containerRegistry.RegisterSingleton<IAuthorizedUser, ActiveUser>();
            containerRegistry.RegisterSingleton<IPatientInTreatment, PatientInTreatment>();
            containerRegistry.RegisterSingleton<ITomoSession, TomoSession>();
            containerRegistry.RegisterSingleton<ITreatmentPlanViewExtermProperties, TreatmentPlanViewExtermProperties>();

            // scanning services:
            //const string pathToDetectorDll = "IRayDetector.dll"; //    "EmulateDetector.dll"
            const string pathToDetectorDll = "EmulateDetector.dll"; //    "IRayDetector.dll"
            const string logFilename = "ace-h.log";
            const string EmpyreanINI = "./EMPYREAN.ini";

            //todo: check objects lifetime cycle
            containerRegistry.RegisterSingleton<ISectionFactory, SectionFactory>();
            containerRegistry.RegisterSingleton<IAvrgFeaturesFactory, AvrgFeaturesFactory>();
            containerRegistry.RegisterSingleton<IAvrgStateFactory, AvrgStateFactory>();
            containerRegistry.RegisterSingleton<IImagingGrabbingService>(() => new ImagingGrabbingService(pathToDetectorDll, logFilename));
            containerRegistry.RegisterSingleton<IImagingReconstructionService>(() => new ImagingReconstructionService("Parameter.XXM",
                Container.Resolve<ISectionFactory>(),
                Container.Resolve<IAvrgStateFactory>(),
                Container.Resolve<IAvrgFeaturesFactory>()));
            containerRegistry.RegisterSingleton<ISection, Section>();
            containerRegistry.RegisterSingleton<IParameters>(() => new Parameters(EmpyreanINI, Container.Resolve<ISectionFactory>()));
            containerRegistry.Register<IIni>(() => new Ini(EmpyreanINI, Container.Resolve<ISectionFactory>()));
            containerRegistry.Register<IFlipRotateService, FlipRotateService>();
            containerRegistry.Register<IConfigurationService, ConfigurationService>();
            containerRegistry.RegisterSingleton<IDetectorConfiguration>(() => new DetectorConfiguration(
                 Container.Resolve<IFlipRotateService>()));
            containerRegistry.RegisterSingleton<IContainerProvider>(() => Container);
            containerRegistry.RegisterSingleton<IValueContainerFactory, ValueContainerFactory>();

            // instances:
            containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ExternModule>();
        }

        protected IConfiguration AddConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }
    }
}
