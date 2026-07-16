using System.ComponentModel;
using Xcc.Application.Models;

namespace Heracles.Core.Models
{
    public interface IEndPointsConfiguration : INotifyPropertyChanged
    {
        SystemEndPoint RecordAndVerifyEndPoint { get; set; }
        SystemEndPoint DatabaseEndpoint { get; set; }
        SystemEndPoint ImagingHeadCamEndPoint { get; set; }
        SystemEndPoint TreatmentHeadCamEndPoint { get; set; }
        SystemEndPoint GCBTelemetryEndPoint { get; set; }
        SystemEndPoint GCBCommandsEndPoint { get; set; }
        SystemEndPoint AcbCommandsEndPoint { get; set; }
        SystemEndPoint QcbCommandsEndPoint { get; set; }

        /// <summary>
        /// data_acquisition
        /// </summary>
        SystemEndPoint ImagingServerEndPoint { get; set; }
        SystemEndPoint DCDataReconstructionServerEndPoint { get; set; }
        SystemEndPoint DCDataProgressWebSocketEndPoint { get; set; }
        SystemEndPoint DCDataReconstructionZmqEndPoint { get; set; }
        SystemEndPoint DCDatabaseEndPoint { get; set; }
    }
}
