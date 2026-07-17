namespace Heracles.Core.Constants
{
    public class NetworkProperties
    {
        public const string DataCommandsEndPoint = "172.31.1.222:3232";
        public const string GcbTelemetryEndPoint = "172.31.1.100:20";
        public const string GcbCommandsEndPoint = "172.31.1.100:7";

        public const string GCB_IPADDRESS = "172.31.1.100";
        public const int GCB_TELEMETRY_PORT = 20;
        public const int GCB_COMMANDS_PORT = 7; // doesn't matter at this moment.

        public const int EMR_SERVICE_PORT = 3232; //8080;
        
        public const string HEAD_CAMERA_IPADDRESS = "172.31.1.80";
        public const int HEAD_CAMERA_PORT = 8080;
    }
}
