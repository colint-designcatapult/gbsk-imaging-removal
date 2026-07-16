using System.ComponentModel;
using System.Threading.Tasks;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Models;
using Prism.Mvvm;
using Xcc.Application.Models;

namespace Heracles.Application.Models.Settings
{
    public interface ISettingsModel : INotifyPropertyChanged
    {
        ISystemSettings Settings { get; }
        Task<ISystemSettings> FetchSettingsAsync();

        Task<ISystemSettings> SubmitSettingsAsync(ISystemSettings settings);
    }

    public class SettingsModel : BindableBase, ISettingsModel
    {
        private ISystemSettings _settings = new SystemSettings();

        public SettingsModel(
            IHeraclesCoreSettings heraclesCoreSettings,
            ISettingsCommands settingsCommands,
            ISystemSettingsStore systemSettingsStore)
        {
            HeraclesCoreSettings = heraclesCoreSettings;
            SettingsCommands = settingsCommands;
            SystemSettingsStore = systemSettingsStore;

            // Fill settings fields from appSettings by default:
            var endpoints = new EndPointsConfiguration
            {
                RecordAndVerifyEndPoint = new SystemEndPoint(heraclesCoreSettings.DataCommandsEndPoint),
                DatabaseEndpoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                ImagingHeadCamEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                TreatmentHeadCamEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                RobotCamEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                GCBTelemetryEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBTelemetryEndPoint?.Address() ?? "127.0.0.1:50020"),
                GCBCommandsEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBCommandsEndPoint?.Address() ?? "127.0.0.1:50007"),
                AcbCommandsEndPoint = new SystemEndPoint(heraclesCoreSettings.AcbCommandsEndPoint?.Address() ?? "127.0.0.1:50022"),
                QcbCommandsEndPoint = new SystemEndPoint(heraclesCoreSettings.QcbCommandsEndPoint?.Address() ?? "127.0.0.1:50023"),
                RoboticRosEndPoint =  new SystemEndPoint(heraclesCoreSettings.RobotGrpcServerEndPoint?.Address() ?? "127.0.0.1:50051"),
                ImagingServerEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),

                DCDataReconstructionServerEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                DCDataProgressWebSocketEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                DCDataReconstructionZmqEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                DCDatabaseEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost)
            };
            endpoints.AcceptChanges();
            Settings.EndPointsConfiguration = endpoints;
        }

        public IHeraclesCoreSettings HeraclesCoreSettings { get; }
        public ISettingsCommands SettingsCommands { get; }
        public ISystemSettingsStore SystemSettingsStore { get; }
        public ISystemSettings Settings { 
            get => _settings;
            private set
            {
                SetProperty(ref _settings, value);
                SystemSettingsStore.Settings = value;
                // As for the robots, they're a separate project and don't use Settings model now,
                // so we set their endpoint to the appSettings as well:
                HeraclesCoreSettings.RobotGrpcServerEndPoint = _settings.EndPointsConfiguration.RoboticRosEndPoint;
            }
        }

        public async Task<ISystemSettings> FetchSettingsAsync()
        {
            var newSettings = await SettingsCommands.GetSettingsAsync();
            
            // Ensure keeping local consts
            AddLocalConstants(ref newSettings);
            
            // Now apply new values:
            Settings = new SystemSettings(newSettings);
            return Settings;
        }

        private void AddLocalConstants(ref ISystemSettings settings)
        {
            if (settings != null)
            {
                // Overwrite temporary/external (moses) endpoint settings, as it can be defined locally only:
                settings.EndPointsConfiguration.RecordAndVerifyEndPoint = new SystemEndPoint(Settings.EndPointsConfiguration.RecordAndVerifyEndPoint);
                // TODO: also, now we overwrite ACB & QCB endpoints, as they're not stored by Moses:
                settings.EndPointsConfiguration.AcbCommandsEndPoint = new SystemEndPoint(Settings.EndPointsConfiguration.AcbCommandsEndPoint);
            }
        }

        public async Task<ISystemSettings> SubmitSettingsAsync(ISystemSettings settings)
        {
            // Ensure keeping local consts to save them into the storage
            // TODO: we may need to remove this after we clean up the settings proto from redundant staff and complete it with ACB etc.
            AddLocalConstants(ref settings);

            var updatedSettings = await SettingsCommands.UpdateSettingsAsync(Settings, settings);

            // Ensure keeping local consts anyway, whatever Moses does with them
            AddLocalConstants(ref updatedSettings);
            
            Settings = updatedSettings;
            return Settings;
        }
    }
}
