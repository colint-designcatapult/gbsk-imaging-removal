using Empyrean.Common.Infra.Settings;
using System.Net.NetworkInformation;
using Xcc.Core.Models;

namespace Heracles.Core.Models
{
    public interface IHeraclesCoreSettings : ICoreSettings
    {
        ISystemEndPoint AcbCommandsEndPoint { get; set; }
        ISystemEndPoint QcbCommandsEndPoint { get; set; }
        ISystemEndPoint DataCommandsEndPoint { get; set; }
        int GrpcTimeout { get; }
        string StorageRoot { get; }
        string StartupLoginUsername { get; }
        string? CameraUriSource { get; set; }
    }

    public interface IHeraclesMainSettings : IHeraclesCoreSettings, ITextLogSettings, IXRaySettings, IDebugSettings
    {
        ISystemEndPoint UpsBroadcastServiceEndPoint { get; set; }
        int AcbReceiveTimeout { get; }
        bool UseDummyHeadActuators { get; }
    }

    public interface IHeraclesExternalSettings : ITextLogSettings, IHeraclesCoreSettings, IXRaySettings, IWarmUpSettings, IDebugSettings
    {
    }
}
