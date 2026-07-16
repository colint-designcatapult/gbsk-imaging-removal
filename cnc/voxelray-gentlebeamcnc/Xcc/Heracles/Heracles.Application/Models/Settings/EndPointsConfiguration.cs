using Heracles.Core.Models;
using Xcc.Application.Helpers;
using Xcc.Application.Models;

namespace Heracles.Application.Models.Settings
{
    public class EndPointsConfiguration : DirtyFlaggedBindableBase, IEndPointsConfiguration
    {
        public EndPointsConfiguration() 
        {
        }

        public EndPointsConfiguration(IEndPointsConfiguration configuration)
        {
            RecordAndVerifyEndPoint = new SystemEndPoint(configuration.RecordAndVerifyEndPoint);
            TreatmentHeadCamEndPoint = new SystemEndPoint(configuration.TreatmentHeadCamEndPoint);
            GCBTelemetryEndPoint = new SystemEndPoint(configuration.GCBTelemetryEndPoint);
            GCBCommandsEndPoint = new SystemEndPoint(configuration.GCBCommandsEndPoint);
            AcbCommandsEndPoint = new SystemEndPoint(configuration.AcbCommandsEndPoint);
            QcbCommandsEndPoint = new SystemEndPoint(configuration.QcbCommandsEndPoint);

            AcceptChanges();
        }

        #region Properties
        SystemEndPoint _recordAndVerifyEndPoint = new SystemEndPoint("127.0.0.1:3232");
        // Moses endpoint
        public SystemEndPoint RecordAndVerifyEndPoint
        {
            get => _recordAndVerifyEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _recordAndVerifyEndPoint, value);
            }
        }

        SystemEndPoint _databaseEndPoint = new SystemEndPoint("127.0.0.1:3232");
        public SystemEndPoint DatabaseEndpoint
        {
            get => _databaseEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _databaseEndPoint, value);
            }
        }

        //TODO: ask Limor about this, there is should be Uri like this:
        //rtsp://root:Empy!12@172.31.1.40:554/axis-media/media.amp
        SystemEndPoint _treatmentHeadCamEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint TreatmentHeadCamEndPoint
        {
            get => _treatmentHeadCamEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _treatmentHeadCamEndPoint, value);
            }
        }

        SystemEndPoint _gcbTelemetryEndPoint = new SystemEndPoint("172.31.1.100:20");
        public SystemEndPoint GCBTelemetryEndPoint
        {
            get => _gcbTelemetryEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _gcbTelemetryEndPoint, value);
            }
        }

        SystemEndPoint _gcbCommandsEndPoint = new SystemEndPoint("172.31.1.100:7");
        public SystemEndPoint GCBCommandsEndPoint
        {
            get => _gcbCommandsEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _gcbCommandsEndPoint, value);
            }
        }

        SystemEndPoint _acbCommandsEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint AcbCommandsEndPoint
        {
            get => _acbCommandsEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _acbCommandsEndPoint, value);
            }
        }

        SystemEndPoint _qcbCommandsEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint QcbCommandsEndPoint
        {
            get => _qcbCommandsEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _qcbCommandsEndPoint, value);
            }
        }

        #endregion Properties

        public override void AcceptChanges()
        {
            base.AcceptChanges();
            RecordAndVerifyEndPoint.AcceptChanges();
            DatabaseEndpoint.AcceptChanges();
            TreatmentHeadCamEndPoint.AcceptChanges();
            GCBTelemetryEndPoint.AcceptChanges();
            GCBCommandsEndPoint.AcceptChanges();
            AcbCommandsEndPoint.AcceptChanges();
            QcbCommandsEndPoint.AcceptChanges();
        }
        

    }
}
