using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard
{
    public interface IGcbXRayCommandOperator
    {
        byte[] GenerateDirectiveCmd(GCBDirectiveCommandNew command);
        //byte[] GenerateOperationalPointLoadingCmd(GcbOperationalPoint op, int authenticationCode);

        /// <summary>
        /// Can be used for both Loading and Confirmation commands
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="op"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        byte[] GenerateOperationalPointCmd(GCBPacketType packetType, GcbOperationalPoint op, IGcbSessionAuthentication sessionKey);
        byte[] GenerateConditioningCmd(float filamentSetpoint);
        byte[] GenerateWarmupCmd(float filamentSetpoint);
        byte[] GenerateNewSessionCmd(int totalPoints);
        byte[] GenerateOperationalPointQueryCmd(int pointIndex);
        byte[] GenerateReleaseTreatmentPlanCmd(GCBReleaseCommandScope scope, IGcbSessionAuthentication sessionKey);
        byte[] GenerateTelemetryRequestCmd();
        byte[] GenerateVersionInfoRequestCmd();
        byte[] GenerateFaultInfoRequestCmd();
        byte[] GenerateWaitForButtonCmd(IGcbSessionAuthentication sessionKey);
    }
}
