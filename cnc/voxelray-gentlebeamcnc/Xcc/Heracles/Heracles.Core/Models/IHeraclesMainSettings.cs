using Empyrean.Common.Infra.Settings;
using System.Net.NetworkInformation;
using Xcc.Core.Models;

namespace Heracles.Core.Models
{
    public interface IHeraclesCoreSettings : ICoreSettings
    {
        public ISystemEndPoint RobotGrpcServerEndPoint { set; get; }
        public ISystemEndPoint AcbCommandsEndPoint { get; set; }
    }

    public interface IHeraclesMainSettings : ITextLogSettings, IHeraclesCoreSettings, IXRaySettings, IAcbSettings, IDebugSettings
    {
        public ISystemEndPoint RobotGrpcServerEndPoint { set; get; }
        public Uri RobotGrpcServerUri { get; }

        [Obsolete]
        public PhysicalAddress RobotGrpcServerMac { get; }

        public double RobotSafeZoneThresholdZmm { set; get; }
        public double RobotSafeZoneThresholdYmm { set; get; }
    }

    public interface IHeraclesExternalSettings : ITextLogSettings, IHeraclesCoreSettings, IXRaySettings, IWarmUpSettings, IDebugSettings
    {
    }
}
