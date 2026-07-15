namespace Xcc.Core.Enums
{
    public enum GCBPacketType : int
    {
        VersionInfo = 1,
        FaultInfo = 2,
        DirectiveCmd = 3,
        TelemetryRequest = 4,
        ConditioningCmd = 5,
        WarmupCmd = 6,
        NewSessionCmd = 7,
        OperationalPointLoadingCmd = 8,
        OperationalPointConfirmationCmd = 9,
        OperationalPointQueryCmd = 10,
        ReleaseTreatmentPlan = 11,
        WaitForButtonCmd = 13,
        QcbPing = 14,
        QcbReadingsCommand = 15,

        InvalidPacket = 100,
        VersionInfoResponse = 101,
        FaultInfoResponse = 102,
        DirectiveCmdResponse = 103,
        TelemetryResponse = 104,
        ConditioningResponse = 105,
        WarmupResponse = 106,
        NewSessionResponse = 107,
        OperationalPointLoadingResponse = 108,
        OperationalPointConfirmationResponse = 109,
        OperationalPointQueryResponse = 110,
        ReleaseTreatmentPlanResponse = 111,
        WaitForButtonResponse = 113,
        QcbPingResponse = 114,
        QcbReadingsCommandResponse = 115,
    }
}
