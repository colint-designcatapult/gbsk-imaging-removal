using System.ComponentModel;
using Xcc.Application.Models;

namespace Heracles.Core.Models
{
    public interface IEndPointsConfiguration : INotifyPropertyChanged
    {
        SystemEndPoint RecordAndVerifyEndPoint { get; set; }
        SystemEndPoint DatabaseEndpoint { get; set; }
        SystemEndPoint TreatmentHeadCamEndPoint { get; set; }
        SystemEndPoint GCBTelemetryEndPoint { get; set; }
        SystemEndPoint GCBCommandsEndPoint { get; set; }
    }
}
