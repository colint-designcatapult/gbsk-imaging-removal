using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public class GcbNoConnectionException : System.Exception
    {
        public GcbNoConnectionException(string message) : base(message) { }
    }

    public interface IGcbCommandInterface
    {
        Task SendOperationalPoint(OperationalPointCmdType commandType, GcbOperationalPoint operationalPoint, GcbSession session);
        Task SendDirectiveCommand(GCBDirectiveCommandNew command);
        Task ReleasePlan(GCBReleaseCommandScope scope, GcbSession session);
        Task<GcbSession> NewSession(int totalPoints);
        Task Stop();
        Task Initialize();
        Task StagePlan();
        Task ClearFaults();
        Task ClearPlan();
        Task ResetTimers();
        Task<FaultEntry> GetFaults();
        Task Conditioning(float conditioningSetpoint);
        Task WarmUp(float warmupSetpoint);
        Task<GcbOperationalPoint> QueryPoint(int pointIndex);
        Task<VersionInfo> GetVersionInfo();
    }
}
