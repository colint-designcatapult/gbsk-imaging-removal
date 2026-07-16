namespace Xcc.Core.Models;

public interface IDebugSettings
{
    bool UseDummyDatabase { get; }
    bool UseSqliteDatabase { get; }
    /// <summary>
    /// Use plain HTTP/2 (no TLS) for the gRPC data channel. Required when pointing at the
    /// embedded SQLite server running on another machine without a TLS certificate.
    /// </summary>
    bool UseInsecureGrpc { get; }
    bool UseDummyServices { get; }
    bool UseDummyRobot { get; }
    bool UseDummyAlignmentEngine { get; }
    /// <summary>
    /// Specifies that application will show controls intended for debugging.
    /// </summary>
    bool ShowDebugButtons { get; }
    string? DummyDeviceSerial { get; }
    bool DebugPopulateEmptyDBWithDummyData { get; }
    string? DebugAuthUsername { get; }
    string? DebugAuthPassword { get; }
    string? DummyCollimatorSerial { get; }
    long DebugLoadedPlanId { get; }
    /// <summary>
    /// Is specified, application will try to use this path to show image in the dcm-viewer.
    /// </summary>
    string? PathToDummyImage { get; }
    /// <summary>
    /// Is specified, application will try to use this path to april tags screenshot instead of detector camera image.
    /// </summary>
    string? PathToTagScreenshot { get; }
    /// <summary>
    /// Specifies that the window will not be maximized to full screen.
    /// </summary>
    bool DoNotExpandFullscreen { get; }
    bool IsUpsActivated { get; }
}