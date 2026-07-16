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
            ImagingHeadCamEndPoint = new SystemEndPoint(configuration.ImagingHeadCamEndPoint);
            TreatmentHeadCamEndPoint = new SystemEndPoint(configuration.TreatmentHeadCamEndPoint);
            GCBTelemetryEndPoint = new SystemEndPoint(configuration.GCBTelemetryEndPoint);
            GCBCommandsEndPoint = new SystemEndPoint(configuration.GCBCommandsEndPoint);
            AcbCommandsEndPoint = new SystemEndPoint(configuration.AcbCommandsEndPoint);
            QcbCommandsEndPoint = new SystemEndPoint(configuration.QcbCommandsEndPoint);
            ImagingServerEndPoint = new SystemEndPoint(configuration.ImagingServerEndPoint);
            DCDataReconstructionServerEndPoint = new SystemEndPoint(configuration.DCDataReconstructionServerEndPoint);
            DCDataProgressWebSocketEndPoint = new SystemEndPoint(configuration.DCDataProgressWebSocketEndPoint);
            DCDataReconstructionZmqEndPoint = new SystemEndPoint(configuration.DCDataReconstructionZmqEndPoint);
            DCDatabaseEndPoint = new SystemEndPoint(configuration.DCDatabaseEndPoint);

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

        //TODO: ask Limor about this, where is should be Uri like this:
        //rtsp://root:Empy!12@172.31.1.40:554/axis-media/media.amp
        SystemEndPoint _imagingHeadCamEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint ImagingHeadCamEndPoint
        {
            get => _imagingHeadCamEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _imagingHeadCamEndPoint, value);
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

        //TODO: ask Limor about this, there is should be Uri like this:
        //rtsp://root:Empy!12@172.31.1.40:554/axis-media/media.amp
        SystemEndPoint _robotCamEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint RobotCamEndPoint
        {
            get => _robotCamEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _robotCamEndPoint, value);
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

        SystemEndPoint _roboticRosEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint RoboticRosEndPoint
        {
            get => _roboticRosEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _roboticRosEndPoint, value);
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

        SystemEndPoint _imagingServerEndPoint = SystemEndPoint.LocalHost;
        public SystemEndPoint ImagingServerEndPoint
        {
            get => _imagingServerEndPoint;
            set
            {
                SetPropertyWithDirtyFlag(ref _imagingServerEndPoint, value);
            }
        }

        private SystemEndPoint _dcDataReconstructionServerEndPoint;
        public SystemEndPoint DCDataReconstructionServerEndPoint
        {
            get { return _dcDataReconstructionServerEndPoint; }
            set { SetPropertyWithDirtyFlag(ref _dcDataReconstructionServerEndPoint, value); }
        }

        private SystemEndPoint _dcDataProgressWebSocketEndPoint;
        public SystemEndPoint DCDataProgressWebSocketEndPoint
        {
            get { return _dcDataProgressWebSocketEndPoint; }
            set { SetPropertyWithDirtyFlag(ref _dcDataProgressWebSocketEndPoint, value); }
        }

        private SystemEndPoint _dcDataReconstructionZmqEndPoint;
        public SystemEndPoint DCDataReconstructionZmqEndPoint 
        { 
            get => _dcDataReconstructionZmqEndPoint;
            set => SetPropertyWithDirtyFlag(ref _dcDataReconstructionZmqEndPoint, value);
        }

        private SystemEndPoint _dcDatabaseEndPoint;
        public SystemEndPoint DCDatabaseEndPoint 
        {
            get => _dcDatabaseEndPoint;
            set => SetPropertyWithDirtyFlag(ref _dcDatabaseEndPoint, value);
        }

        #endregion Properties

        public override void AcceptChanges()
        {
            base.AcceptChanges();
            RecordAndVerifyEndPoint.AcceptChanges();
            DatabaseEndpoint.AcceptChanges();
            ImagingHeadCamEndPoint.AcceptChanges();
            TreatmentHeadCamEndPoint.AcceptChanges();
            GCBTelemetryEndPoint.AcceptChanges();
            GCBCommandsEndPoint.AcceptChanges();
            AcbCommandsEndPoint.AcceptChanges();
            ImagingServerEndPoint.AcceptChanges();
            DCDataReconstructionServerEndPoint.AcceptChanges();
            DCDataProgressWebSocketEndPoint.AcceptChanges();
            DCDataReconstructionZmqEndPoint.AcceptChanges();
            DCDatabaseEndPoint.AcceptChanges();
        }
        

    }
}
