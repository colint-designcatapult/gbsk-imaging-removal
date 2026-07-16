using System;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard.Comm;
using static System.Formats.Asn1.AsnWriter;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public class GcbCommandInterface : IGcbCommandInterface
    {
        #region Constants
        private const int DelayAfterSendRequestFailureMilliseconds = 1000;
        private const int AutoExecutionFieldValue = 1;
        #endregion Constants

        public GcbCommandInterface(
            IGcbXRayCommandOperator gcbXRayCommandOperator,
            IGcbCommunicationService gcbCommunicationService,
            ILogWriter logWriter)
        {
            GcbXRayCommandOperator = gcbXRayCommandOperator;
            GcbCommandsAsyncService = gcbCommunicationService;
            LogWriter = logWriter;

            gcbCommunicationService.UdpReceiveErrorEvent += (s, e) =>
            {
                _ = LogWriter.LogAsync(e.Message, LogRecordSeverity.Error, LogRecordType.System);
            };
        }

        public IGcbXRayCommandOperator GcbXRayCommandOperator { get; }
        public IGcbCommunicationService GcbCommandsAsyncService { get; }
        public ILogWriter LogWriter { get; }


        #region Public methods
        public async Task SendOperationalPoint(
            OperationalPointCmdType commandType, 
            GcbOperationalPoint operationalPoint, 
            GcbSession session)
        {
            _ = LogWriter.LogAsync(
                $"SendOperationalPoint: CoilX={operationalPoint.XCoilSetpoint}, CoilY={operationalPoint.YCoilSetpoint}, Focus={operationalPoint.FocusCoilSetpoint}",
                LogRecordSeverity.Info, LogRecordType.System);

            GCBPacketType packetType = commandType == OperationalPointCmdType.Load
                ? GCBPacketType.OperationalPointLoadingCmd
                : GCBPacketType.OperationalPointConfirmationCmd;

            byte[] data = GcbXRayCommandOperator.GenerateOperationalPointCmd(packetType, operationalPoint, GetSessionKey(session));
            byte[] rxData = await SendRequestSeveralTimes(data);

            CheckOperationalPointStatusesResponse(rxData);

#if DEBUG
            string msg = $"GCB 'OperationalPoint' command response data: {BitConverter.ToString(rxData)}";
            _ =  LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif
        }

        public async Task SendDirectiveCommand(GCBDirectiveCommandNew command)
        {
            byte[] data = GcbXRayCommandOperator.GenerateDirectiveCmd(command);
            var responseData = await SendRequestSeveralTimes(data);

            UdpPacket responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.DirectiveCmdResponse);
            var status = (GcbProcessingStatus)(int)responsePacket[0]; // according to DirectiveResponse packet specs

            if (status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to complete {command} command. Status: {status}");
            }

#if DEBUG
            string msg = $"GCB {command} command status: {status}";
            _ = LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif
        }
        
        public async Task ReleasePlan(GCBReleaseCommandScope scope, GcbSession session)
        {
            byte[] data = GcbXRayCommandOperator.GenerateReleaseTreatmentPlanCmd(scope, GetSessionKey(session));
            byte[] responseData = await SendRequestSeveralTimes(data);

            UdpPacket responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.ReleaseTreatmentPlanResponse, expectedPayloadLength: 2);
            var status = (GcbProcessingStatus)(int)responsePacket[0];
            var authCodeStatus = (GcbProcessingStatus)(int)responsePacket[1];

            if (authCodeStatus != GcbProcessingStatus.OK || status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to complete 'ReleaseTreatmentPlan' command with status: {status}. Authentication Status: {authCodeStatus}");
            }

#if DEBUG
            string msg = $"GCB 'ReleaseTreatmentPlan' command status: {status}. Authentication Status: {authCodeStatus}";
            _ = LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif
        }
        

        public async Task<GcbSession> NewSession(int totalPoints)
        {
            byte[] data = GcbXRayCommandOperator.GenerateNewSessionCmd(totalPoints);
            byte[] responseData = await SendRequestSeveralTimes(data);

            UdpPacket responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.NewSessionResponse, expectedPayloadLength: 2);
            var status = (GcbProcessingStatus)(int)responsePacket[0];
            uint sessionId = responsePacket[1];

            if (status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to complete 'NewSession' command with status: {status}");
            }

#if DEBUG
            string msg = $"GCB 'NewSession' command status: {status}. New session id: {sessionId}";
            _ = LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif

            return new GcbSession(sessionId, totalPoints);
        }

        public async Task Stop()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.Stop);
        }

        public async Task Initialize()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.Initialize);
        }

        public async Task StagePlan()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.StagePlan);
        }

        public async Task ClearFaults()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.ClearFaults);
        }

        public async Task ClearPlan()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.ClearPlan);
        }

        public async Task ResetTimers()
        {
            await SendDirectiveCommand(GCBDirectiveCommandNew.ResetTimers);
        }

        public async Task<FaultEntry> GetFaults()
        {
            byte[] data = GcbXRayCommandOperator.GenerateFaultInfoRequestCmd();

            var responseData = await SendRequestSeveralTimes(data);

            ParseAndValidateResponseData(responseData, GCBPacketType.FaultInfoResponse, expectedPayloadLength: 9);
            
            return FaultEntryParser.Parse(responseData);
        }

        public async Task Conditioning(float conditioningSetpoint)
        {
            byte[] data = GcbXRayCommandOperator.GenerateConditioningCmd(conditioningSetpoint);

            var responseData = await SendRequestSeveralTimes(data);

            var responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.ConditioningResponse, expectedPayloadLength: 2);

            var status = (GcbProcessingStatus)(int)responsePacket[0];
            if (status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to complete 'Conditioning' command with status: {status}");
            }

#if DEBUG
            string msg = $"GCB 'Conditioning' command status: {status}";
            _ = LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif
        }

        public async Task WarmUp(float warmupSetpoint)
        {
            byte[] data = GcbXRayCommandOperator.GenerateWarmupCmd(warmupSetpoint);

            var responseData =  await SendRequestSeveralTimes(data);
            var responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.WarmupResponse, expectedPayloadLength: 1);

            var status = (GcbProcessingStatus)(int)responsePacket[0];
            if (status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to complete 'WarmUp' command with status: {status}");
            }

#if DEBUG
            string msg = $"GCB 'WarmUp' command status: {status}";
            _ = LogWriter.LogAsync(msg, LogRecordSeverity.Info, LogRecordType.System);
#endif
        }

        public async Task<GcbOperationalPoint> QueryPoint(int pointIndex)
        {
            byte[] data = GcbXRayCommandOperator.GenerateOperationalPointQueryCmd(pointIndex);
            byte[] responseData = await SendRequestSeveralTimes(data);

            return ParseOperationalPointData(responseData);
        }

        public async Task<VersionInfo> GetVersionInfo()
        {
            byte[] data = GcbXRayCommandOperator.GenerateVersionInfoRequestCmd();

            var responseData = await SendRequestSeveralTimes(data);

            if (responseData is null || responseData.Length == 0)
                throw new Exception("Failed to get version information from GCB.");

            return VersionInfoParser.Parse(responseData);
        }

        public static UdpPacket ParseAndValidateResponseData(byte[] responseData, GCBPacketType expectedPacketType, int expectedPayloadLength = 1)
        {
            if (responseData is null || responseData.Length == 0)
            {
                throw new ArgumentNullException(nameof(responseData), "GCB command connection error: no response data.");
            }

            var packet = new UdpPacket(responseData);

            var packetType = (GCBPacketType)packet.PacketType;
            if (packetType != expectedPacketType)
            {
                throw new Exception($"GCB packet type {packetType.ToString()} does not match expected one {expectedPacketType.ToString()}");
            }

            if (packet.PayloadLength != expectedPayloadLength)
            {
                throw new Exception($"Invalid GCB response format: fields count {packet.PayloadLength}, but expected {expectedPayloadLength}");
            }

            return packet;
        }

        #endregion Public methods

        #region Private methods
        private async Task<byte[]> SendRequestSeveralTimes(byte[] data, int attempts = 3)
        {
            for (int i = 1; i <= attempts; i++)
            {
                //if (i > 1)
                //{
                //    //UdpPacket packet = new UdpPacket(data);
                //    //_ = LogWriter.LogAsync($"GCB: repeat the command {(GCBPacketType) packet.PacketType}",
                //    //    LogRecordSeverity.Warn, LogRecordType.System);
                //}

                try
                {
                    var bytes = await GcbCommandsAsyncService.SendRequestAsync(data);
                    if (_sendRequestSuccess == false)
                    {
                        _ = LogWriter.LogAsync($"GCB UdpService communication established", LogRecordSeverity.Info, LogRecordType.System);
                    }
                    _sendRequestSuccess = true;
                    return bytes;
                }
                catch (UdpException ex)
                {
                    //_ = LogWriter.LogAsync($"UdpService request sending error #{i}: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                    if (_sendRequestSuccess == true)
                    {
                        _ = LogWriter.LogAsync($"GCB UdpService communication failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                    }
                    _sendRequestSuccess = false;
                    await Task.Delay(DelayAfterSendRequestFailureMilliseconds);
                }
            }

            throw new GcbNoConnectionException("No connection. GCB does not respond");
        }

        private GcbOperationalPoint ParseOperationalPointData(byte[] responseData)
        {

            if (responseData == null || responseData.Length == 0)
            {
                throw new Exception($"Failed to parse OperationalPoint data: no response.");
            }

            // Response fields
            //1   Processing status of Query  Enum Integer Enumeration value indicating status(see information below)
            //2   Return value for Point Index    Count   Integer The index of the point within the treatment plan
            //3   Return value for Total Point Time   sec Float   The total execution time of the point
            //4   Return value for Remaining Point Time   sec Float   The remaining execution time of the point
            //5   Return value for kV setpoint    kV  Float   The kV for the point
            //6   Return value for Target mA  mA  Float   The mA for the point
            //7   Return value for Filament Setpoint  mA  Float   The filament current for the point
            //8   Return value for X Coil Setpoint    mA  Float   The x coil current for the point
            //9   Return value for Y Coil Setpoint    mA  Float   The y coil current for the point
            //10  Return value for Focus Coil Setpoint    mA  Float   The focus coil current for the point
            //11  Return value for auto - execute flag  N / A N / A The auto - execute setting for the point

            UdpPacket responsePacket = ParseAndValidateResponseData(responseData, GCBPacketType.OperationalPointQueryResponse, expectedPayloadLength: 11);

            var status = (GcbProcessingStatus)(int)responsePacket[0];
            if (status != GcbProcessingStatus.OK)
            {
                throw new Exception($"Failed to parse OperationalPoint data: status = {status.ToString()}");
            }
            return new GcbOperationalPoint()
            {
                PointIndex = responsePacket[1],
                TotalPointTime = responsePacket[2],
                // initial remaining time is the same as the regular remaining time here,
                // as the board's time value is ground truth to us:
                InitialRemainingPointTime = responsePacket[3],
                RemainingPointTime = responsePacket[3],
                SetpointKv = responsePacket[4],
                TargetMA = responsePacket[5],
                FilamentSetpoint = responsePacket[6],
                XCoilSetpoint = responsePacket[7],
                YCoilSetpoint = responsePacket[8],
                FocusCoilSetpoint = responsePacket[9],
                AutoExecution = responsePacket[10] == AutoExecutionFieldValue
            };
        }

        private void CheckOperationalPointStatusesResponse(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException($"Failed to complete 'OperationalPoint' command: no response data.");

            UdpPacket packet = new(data);

            const int expectedDataLength = 11; // according to the protocol

            if (packet.PayloadLength != expectedDataLength)
                throw new Exception($"Failed to complete 'OperationalPoint' command: invalid response data lenght ({packet.PayloadLength})");

            string[] fieldNames =
            [
                "Processing status for [Point Index]",
                "Processing status for [Total Point Time]",
                "Processing status for [Remaining Point Time]",
                "Processing status for [kV setpoint]",
                "Processing status for [Target mA]",
                "Processing status for [Filament Setpoint]",
                "Processing status for [X Coil Setpoint]",
                "Processing status for [Y Coil Setpoint]",
                "Processing status for [Focus Coil Setpoint]",
                "Processing status for [Auto-Execute Flag]",
                "Processing status for [Authentication]"
            ];

            for (int i = 0; i < expectedDataLength; i++)
            {
                GcbProcessingStatus status = (GcbProcessingStatus)(uint)packet[i];

                if (status != GcbProcessingStatus.OK)
                {
                    string msg = $"GCB 'OperationalPoint' command response data: {BitConverter.ToString(packet.Buffer)}";
                    _ = LogWriter.LogAsync(msg, LogRecordSeverity.Warn, LogRecordType.Error);

                    throw new Exception($"Failed to complete 'OperationalPoint' command: {fieldNames[i]} = {status.ToString()}, value = {(uint)packet[i]}"); //throws on any first bad status
                }
            }
        }

        private IGcbSessionAuthentication GetSessionKey(GcbSession session)
        {
            return new GcbSecretKeySessionAuthentication(session);
        }

        #endregion Private methods

        #region Private fields

        private bool _sendRequestSuccess;
        #endregion Private fields
    }
}
