using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Infra.GryphonBoard;
using static System.Formats.Asn1.AsnWriter;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public class GcbXRayCommandOperator : IGcbXRayCommandOperator
    {
        protected uint packetCounter = 0;

        /// <summary>
        /// This command is used to obtain the version information of the firmware. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateVersionInfoRequestCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.VersionInfo,
                packetCounter: ++packetCounter,
                payload: [0 /* reserved field */]);
        }

        /// <summary>
        /// This command is used to obtain information about the fault which trigger the system to enter into a fault state. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateFaultInfoRequestCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.FaultInfo,
                packetCounter: ++packetCounter,
                payload: [0 /* reserved field */]);
        }

        /// <summary>
        /// This command is used to request that the board perform a conditioning cycle, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the conditioning process</param>
        /// <returns></returns>
        public byte[] GenerateConditioningCmd(float filamentSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.ConditioningCmd,
                packetCounter: ++packetCounter,
                payload: [
                    filamentSetpoint, 
                    0 /* reserved field */
                    ]);
        }

        /// <summary>
        /// This command is used to request that the board perform a warmup, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the warmup process</param>
        /// <returns></returns>
        public byte[] GenerateWarmupCmd(float filamentSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.WarmupCmd,
                packetCounter: ++packetCounter,
                payload: [filamentSetpoint]);
        }

        /// <summary>
        /// This command is used to begin staging a new treatment plan. If successful, the firmware responds with a new session ID.
        /// </summary>
        /// <param name="totalPoints">The total requested number of points for the new plan</param>
        /// <returns></returns>
        public byte[] GenerateNewSessionCmd(int totalPoints)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.NewSessionCmd,
                packetCounter: ++packetCounter,
                payload: [
                    totalPoints,
                    0 /* reserved field */
                    ]);
        }


        /// <summary>
        /// This command is used to request information on a staged treatment plan. Each command is used to request the information for a single point within the treatment plan.
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public byte[] GenerateOperationalPointQueryCmd(int pointIndex)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.OperationalPointQueryCmd,
                packetCounter: ++packetCounter,
                payload: [pointIndex]);
        }

        public byte[] GenerateReleaseTreatmentPlanCmd(GCBReleaseCommandScope scope, IGcbSessionAuthentication sessionKey)
        {
            var packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)GCBPacketType.ReleaseTreatmentPlan,
                packetCounter: ++packetCounter,
                payload: [
                    (int)scope, // Plan or Point
                    0 // authentication code, calc it below from the packet payload:
                    ]);
            return sessionKey.Sign(packet).Buffer;
        }

        /// <summary>
        /// This command is used to direct the boards to perform or begin a specific operation	
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public byte[] GenerateDirectiveCmd(GCBDirectiveCommandNew command)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.DirectiveCmd,
                packetCounter: ++packetCounter,
                payload: [
                    (int)command,        // Directive Id
                    0x01 << (int)command // Confirmation bit ('1' shifted left by 'the value of the command' times)
                    ]);
        }

        
        /// <summary>
        /// Loading or Confirmation OP command
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="op"></param>
        /// <param name="authenticationCode"></param>
        /// <returns></returns>
        public byte[] GenerateOperationalPointCmd(GCBPacketType packetType, GcbOperationalPoint op, IGcbSessionAuthentication sessionKey)
        {
            UdpPacket packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)packetType,
                packetCounter: ++packetCounter,
                payload: [
                    op.PointIndex,
                    op.TotalPointTime,
                    op.RemainingPointTime,
                    op.SetpointKv,
                    op.TargetMA,
                    op.FilamentSetpoint,
                    op.XCoilSetpoint,
                    op.YCoilSetpoint,
                    op.FocusCoilSetpoint,
                    op.AutoExecution ? 1 : 0,
                    0 // authentication code, calc it below from the packet payload:
                    ]);
            return sessionKey.Sign(packet).Buffer;
        }

        /// <summary>
        /// This command is used to request the current status of the system.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateTelemetryRequestCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.TelemetryRequest,
                packetCounter: ++packetCounter,
                payload: [0 /*reserved field*/]);
        }

        public byte[] GenerateWaitForButtonCmd(IGcbSessionAuthentication sessionKey)
        {
            var packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)GCBPacketType.WaitForButtonCmd,
                packetCounter: ++packetCounter,
                payload: [
                    0, // Reserved
                    0  // authentication code, calc it below from the packet payload:
                ]);
            return sessionKey.Sign(packet).Buffer;
        }

    }
}
