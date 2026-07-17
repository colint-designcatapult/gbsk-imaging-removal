namespace Xcc.Core.Models;

public interface ICoreSettings
{
    public ISystemEndPoint GCBTelemetryEndPoint { get; set; }
    public ISystemEndPoint GCBCommandsEndPoint { get; set; }
    public ISystemEndPoint DataCommandsEndPoint { get; set; }

    public int GrpcTimeout { get; }
    public string StorageRoot { get; }
    public string StartupLoginUsername { get; }
    public string? CameraUriSource { get; set; }

    /// <summary>
    /// When true, the gRPC data channel uses plain HTTP/2 with no TLS.
    /// Required when the DataCommandsEndPoint points at the embedded SQLite server
    /// or any other unencrypted gRPC endpoint (e.g. on another machine on the same LAN).
    /// </summary>
    public bool UseInsecureGrpc { get; }
}