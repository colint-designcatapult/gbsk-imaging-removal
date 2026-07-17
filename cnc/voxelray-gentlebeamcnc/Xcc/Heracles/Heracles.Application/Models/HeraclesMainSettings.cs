using Empyrean.Common.Infra.Settings;

using System;
using System.IO;
using System.Net.NetworkInformation;
using Heracles.Core.Constants;
using Xcc.Application.Models;
using Xcc.Core.Models;
using Heracles.Application.Helpers;

namespace Heracles.Application.Models
{
    public class HeraclesMainSettings(ISettingsReader reader) : Core.Models.IHeraclesMainSettings
    {
        #region IHeraclesMainSetting


        #region ITextLogSettings
        public string LogFilename { get; } = reader.GetString("AppSettings:LogFilename");
        public int LogPageSize { get; } = reader.GetOptionalInt("AppSettings:LogPageSize", 0);
        public string AppLogFolder { get; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HeraclesLogs");
        #endregion ITextLogSettings


        #region IHeraclesCoreSettings
        #region ICoreSettings
        public ISystemEndPoint GCBTelemetryEndPoint { get; set; } =
            SystemEndPoint.Create(
                reader.GetOptionalString("AppSettings:EndPoints:GCBTelemetryEndPoint", NetworkProperties.GcbTelemetryEndPoint));

        public ISystemEndPoint GCBCommandsEndPoint { get; set; } =
            SystemEndPoint.Create(
                reader.GetOptionalString("AppSettings:EndPoints:GCBCommandsEndPoint", NetworkProperties.GcbCommandsEndPoint));

        public ISystemEndPoint DataCommandsEndPoint { set; get; } =
            SystemEndPoint.Create(
                reader.GetOptionalString("AppSettings:EndPoints:DataCommandsEndPoint", NetworkProperties.DataCommandsEndPoint));

        public int GrpcTimeout { get; } = reader.GetOptionalInt("AppSettings:GrpcTimeout_ms", 5000);

        public string StorageRoot { get; } = reader.GetOptionalString("AppSettings:StorageRoot", @"C:\GentleBeam\deep-color-raw-dcm\");
        public string StartupLoginUsername { get; } = reader.GetOptionalString("AppSettings:StartupLoginUsername", "Admin");
        public string? CameraUriSource { get; set; } = reader.GetOptionalString(
            "AppSettings:CameraUriString",
            defaultValue: "rtsp://Empyrean:Empyrean!2025@172.31.1.40:554/axis-media/media.amp");
        #endregion ICoreSettings
        #endregion IHeraclesCoreSettings


        #region IXRaySettings
        public double XrayTubePower { get; } = reader.GetOptionalDouble("AppSettings:XrayTubePower", CurrentCalculator.HvpsPower50kV);
        public double XrayTubePower50kV { get; } = reader.GetOptionalDouble("AppSettings:XrayTubePower50kV", CurrentCalculator.HvpsPower50kV);
        public double XrayTubePower70kV { get; } = reader.GetOptionalDouble("AppSettings:XrayTubePower70kV", CurrentCalculator.HvpsPower70kV);
        public double XrayTubePower100kV { get; } = reader.GetOptionalDouble("AppSettings:XrayTubePower100kV", CurrentCalculator.HvpsPower100kV);

        public int QcFieldDuration { get; } = reader.GetOptionalInt("AppSettings:QcFieldDuration", 20);
        public int SafetyCheckFieldDuration { get; } = reader.GetOptionalInt("AppSettings:SafetyCheckFieldDuration", 60);
        #endregion IXRaySettings


        #region IAcbSettings


        public bool UseDummyHeadActuators { get; } = reader.GetOptionalBool("AppSettings:UseDummyHeadActuators", false);
        #endregion IAcbSettings


        #region IDebugSettings
        public bool UseDummyDatabase { get; } = reader.GetOptionalBool("AppSettings:Debug:UseDummyDatabase", false);
        public bool UseSqliteDatabase { get; } = reader.GetOptionalBool("AppSettings:Debug:UseSqliteDatabase", false);
        public bool UseInsecureGrpc { get; } = reader.GetOptionalBool("AppSettings:Debug:UseInsecureGrpc", false);
        public bool UseDummyServices { get; } = reader.GetOptionalBool("AppSettings:Debug:UseDummyServices", false);
        public bool UseDummyRobot { get; } = reader.GetOptionalBool("AppSettings:Debug:UseDummyRobot", false);
        public bool UseDummyAlignmentEngine { get; } = reader.GetOptionalBool("AppSettings:Debug:UseDummyAlignmentEngine", false);
        public bool ShowDebugButtons { get; } = reader.GetOptionalBool("AppSettings:Debug:ShowDebugButtons", false);
        public string? DummyDeviceSerial { get; } = reader.GetOptionalString("AppSettings:Debug:DummyDeviceSerial");
        public bool DebugPopulateEmptyDBWithDummyData { get; } = reader.GetOptionalBool("AppSettings:Debug:PopulateEmptyDBWithDummyData", false);
        public string? DebugAuthUsername { get; } = reader.GetOptionalString("AppSettings:Debug:AuthUsername");
        public string? DebugAuthPassword { get; } = reader.GetOptionalString("AppSettings:Debug:AuthPassword");
        public string? DummyCollimatorSerial { get; } = reader.GetOptionalString("AppSettings:Debug:DummyCollimatorSerial");
        public long DebugLoadedPlanId { get; } = reader.GetOptionalLong("AppSettings:Debug:LoadedPlanId", 0);
        public string? PathToDummyImage { get; set; } = reader.GetOptionalString("AppSettings:Debug:PathToDummyImage");
        public string? PathToTagScreenshot { get; set; } = reader.GetOptionalString("AppSettings:Debug:PathToTagScreenshot");
        public bool DoNotExpandFullscreen { get; set; } = reader.GetOptionalBool("AppSettings:Debug:DoNotExpandFullscreen", false);
        public bool IsUpsActivated { get; } = reader.GetOptionalBool("AppSettings:Debug:IsUpsActivated", false);
        #endregion IDebugSettings


        #endregion IHeraclesMainSetting
    }
}
    