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
                TreatmentHeadCamEndPoint = new SystemEndPoint(SystemEndPoint.LocalHost),
                GCBTelemetryEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBTelemetryEndPoint?.Address() ?? "127.0.0.1:50020"),
                GCBCommandsEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBCommandsEndPoint?.Address() ?? "127.0.0.1:50007")
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
