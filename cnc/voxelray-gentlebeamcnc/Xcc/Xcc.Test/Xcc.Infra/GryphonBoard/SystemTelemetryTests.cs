using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Infra.GryphonBoard
{
    internal class SystemTelemetryTests
    {
        [SetUp]
        public void SetUp()
        {
            G.SetupCulture();
        }
        
        [Test]
        public void Defaults()
        {
            var sut = new SystemTelemetry();
            
            Assert.That(sut.ControlBoardState, Is.EqualTo(GcbStateNew.Startup));
            Assert.That(sut.SystemRuntime, Is.EqualTo(0));
            Assert.That(sut.FaultFlags, Is.EqualTo(0));
            Assert.That(sut.InterlockFlags, Is.EqualTo(0));
            Assert.That(sut.RingLedState, Is.EqualTo(RingLedState.TBD));
            Assert.That(sut.BaseLedState, Is.EqualTo(BaseLedState.TBD));
            Assert.That(sut.CollimatorId1, Is.EqualTo(0));
            Assert.That(sut.CollimatorId2, Is.EqualTo(0));
            Assert.That(sut.CollimatorSerial, Is.EqualTo(0));
            Assert.That(sut.ButtonsState, Is.EqualTo(0));
            Assert.That(sut.CurrentOperationalPoint, Is.EqualTo(0));
            Assert.That(sut.TotalOperationalPoints, Is.EqualTo(0));
            Assert.That(sut.InternalTimerState, Is.EqualTo(0));
            Assert.That(sut.PrimaryTimerValue, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Timer1State, Is.EqualTo(0));
            Assert.That(sut.SecondaryTimer1Value, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Timer2State, Is.EqualTo(0));
            Assert.That(sut.SecondaryTimer2Value, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.RuntimeCounterHVPS, Is.EqualTo(0));
            Assert.That(sut.HvpsIOStatus, Is.EqualTo(0));
            Assert.That(sut.HvpsFlagStatus, Is.EqualTo(0));
            Assert.That(sut.KvSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.KvFeedback, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.EmissionCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.HeaterCurrentFeedback, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.EmissionCurrentLimit, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.HvpsPowerSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.GridSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.GridVoltage, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.XCoilCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.YCoilCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.FocusCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.IonPumpFeedback, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.WaterPressure, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.WaterFlowRate, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.WaterTemperature, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.HeatSinkTemperature, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.PeltierTemperature, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.CabinetTemperature, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Mag1, Is.Null);
            Assert.That(sut.Mag2, Is.Null);
            Assert.That(sut.Applicator, Is.EqualTo(0));
            
            Assert.That(sut.IsFaultState(), Is.False);
            Assert.That(sut.IsEmissionState(), Is.False);
        }

        [TestCase(GcbStateNew.Fault)]
        [TestCase(GcbStateNew.ColdFault)]
        [TestCase(GcbStateNew.WarmupFault)]
        public void IsFalseState_True(GcbStateNew state)
        {
            Assert.That(SystemTelemetry.IsFaultState(state), Is.True);
        }

        [TestCase(GcbStateNew.Startup)]
        [TestCase(GcbStateNew.Cold)]
        [TestCase(GcbStateNew.DailyWarmup)]
        [TestCase(GcbStateNew.Warmup)]
        [TestCase(GcbStateNew.Primed)]
        [TestCase(GcbStateNew.Staging)]
        [TestCase(GcbStateNew.Staged)]
        [TestCase(GcbStateNew.HvpsCheck)]
        [TestCase(GcbStateNew.HVSetup)]
        [TestCase(GcbStateNew.Ready)]
        [TestCase(GcbStateNew.Launching)] 
        [TestCase(GcbStateNew.Emission)]
        [TestCase(GcbStateNew.Termination)]
        [TestCase(GcbStateNew.Discharge)]
        [TestCase(GcbStateNew.SystemCrash)]
        [TestCase(GcbStateNew.LaunchingForImaging)]
        [TestCase(GcbStateNew.WaitForKey)]
        [TestCase(GcbStateNew.Imaging)]
        public void IsFalseState_False(GcbStateNew state)
        {
            Assert.That(SystemTelemetry.IsFaultState(state), Is.False);
        }

        [TestCase(GcbStateNew.Emission)]
        [TestCase(GcbStateNew.Imaging)]
        public void IsEmissionState_True(GcbStateNew state)
        {
            Assert.That(SystemTelemetry.IsEmissionState(state), Is.True);
        }

        [TestCase(GcbStateNew.Startup)]
        [TestCase(GcbStateNew.Cold)]
        [TestCase(GcbStateNew.ColdFault)]
        [TestCase(GcbStateNew.DailyWarmup)]
        [TestCase(GcbStateNew.Warmup)]
        [TestCase(GcbStateNew.WarmupFault)]
        [TestCase(GcbStateNew.Primed)]
        [TestCase(GcbStateNew.Staging)]
        [TestCase(GcbStateNew.Staged)]
        [TestCase(GcbStateNew.HvpsCheck)]
        [TestCase(GcbStateNew.HVSetup)]
        [TestCase(GcbStateNew.Ready)]
        [TestCase(GcbStateNew.Launching)] 
        [TestCase(GcbStateNew.Termination)]
        [TestCase(GcbStateNew.Discharge)]
        [TestCase(GcbStateNew.Fault)]
        [TestCase(GcbStateNew.SystemCrash)]
        [TestCase(GcbStateNew.LaunchingForImaging)]
        [TestCase(GcbStateNew.WaitForKey)]
        public void IsEmissionState_False(GcbStateNew state)
        {
            Assert.That(SystemTelemetry.IsEmissionState(state), Is.False);
        }

        [Test]
        public void ToString_Throws_With_Defaults()
        {
            var sut = new SystemTelemetry();
            
            Assert.That(() => { var result = sut.ToString(); }, Throws.Exception);
        }

        [Test]
        public void ToString_DoesNotThrow()
        {
            var sut = SystemTelemetryWithEmptyMagArrays();
            Assert.DoesNotThrow(() => { var result = sut.ToString(); });
        }
        
        [Test]
        public void ToString_Test()
        {
            var sut = SystemTelemetryWithEmptyMagArrays();
            var expectedLines = new List<string>
            {
                "ControlBoardState: Startup",
                "SystemRuntime: 0x0",
                "FaultFlags: 0x0",
                "InterlockFlags: 0x0",
                "RingLedState: TBD",
                "BaseLedState: TBD",
                "CollimatorId1: 0x0",
                "CollimatorId2: 0x0",
                "CollimatorSerial: 0x0",
                "ButtonsState: 0x0",
                "CurrentOperationalPoint: 0x0",
                "TotalOperationalPoints: 0x0",
                "InternalTimerState: 0x0",
                "PrimaryTimerValue: 0.000",
                "Timer1State: 0x0",
                "SecondaryTimer1Value: 0.000",
                "Timer2State: 0x0",
                "SecondaryTimer2Value: 0.000",
                "RuntimeCounterHVPS: 0x0",
                "HvpsIOStatus: 0x0",
                "HvpsFlagStatus: 0x0",
                "KvSetpoint: 0.000",
                "KvFeedback: 0.000",
                "EmissionCurrent: 0.000",
                "HeaterCurrentSetpoint: 0.000",
                "HeaterCurrentFeedback: 0.000",
                "EmissionCurrentLimit: 0.000",
                "HvpsPowerSetpoint: 0.000",
                "GridSetpoint: 0.000",
                "GridVoltage: 0.000",
                "XCoilCurrent: 0.000",
                "YCoilCurrent: 0.000",
                "FocusCurrent: 0.000",
                "IonPumpFeedback: 0.000",
                "WaterPressure: 0.000",
                "WaterFlowRate: 0.000",
                "WaterTemperature: 0.000",
                "HeatSinkTemperature: 0.000",
                "PeltierTemperature: 0.000",
                "CabinetTemperature: 0.000",
                "Mag1: []",
                "Mag2: []",
                "Applicator: 0x0",
            };
            
            var result = sut.ToString();
            var resultLines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            
            Assert.That(resultLines, Is.EquivalentTo(expectedLines));
        }
        
        // property: ControlBoardState
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Startup, "ControlBoardState: Startup")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Cold, "ControlBoardState: Cold")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.ColdFault, "ControlBoardState: ColdFault")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.DailyWarmup, "ControlBoardState: Conditioning")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Warmup, "ControlBoardState: Warmup")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.WarmupFault, "ControlBoardState: WarmupFault")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Primed, "ControlBoardState: Primed")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Staging, "ControlBoardState: Staging")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Staged, "ControlBoardState: Staged")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.HvpsCheck, "ControlBoardState: HvpsCheck")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.HVSetup, "ControlBoardState: HVSetup")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Ready, "ControlBoardState: Ready")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Launching, "ControlBoardState: Launching")] 
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Emission, "ControlBoardState: Emission")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Termination, "ControlBoardState: Termination")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Discharge, "ControlBoardState: Discharge")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Fault, "ControlBoardState: Fault")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.SystemCrash, "ControlBoardState: SystemCrash")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.LaunchingForImaging, "ControlBoardState: LaunchingForImaging")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.WaitForKey, "ControlBoardState: WaitForKey")]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Imaging, "ControlBoardState: Imaging")]
        // property: SystemRuntime
        [TestCase(nameof(SystemTelemetry.SystemRuntime), -1, "SystemRuntime: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.SystemRuntime), 0, "SystemRuntime: 0x0")]
        [TestCase(nameof(SystemTelemetry.SystemRuntime), 1, "SystemRuntime: 0x1")]
        // property: FaultFlags
        [TestCase(nameof(SystemTelemetry.FaultFlags), -2, "FaultFlags: 0xFFFFFFFE")]
        [TestCase(nameof(SystemTelemetry.FaultFlags), 0, "FaultFlags: 0x0")]
        [TestCase(nameof(SystemTelemetry.FaultFlags), 2, "FaultFlags: 0x2")]
        // property: InterlockFlags
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 0u, "InterlockFlags: 0x0")]
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 16u, "InterlockFlags: 0x10")]
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 0xFFFFFFFFu, "InterlockFlags: 0xFFFFFFFF")]
        // property: RingLedState
        [TestCase(nameof(SystemTelemetry.RingLedState), RingLedState.TBD, "RingLedState: TBD")]
        [TestCase(nameof(SystemTelemetry.RingLedState), RingLedState.TBD2, "RingLedState: TBD2")]
        // property: BaseLedState
        [TestCase(nameof(SystemTelemetry.BaseLedState), BaseLedState.TBD, "BaseLedState: TBD")]
        [TestCase(nameof(SystemTelemetry.BaseLedState), BaseLedState.TBD2, "BaseLedState: TBD2")]
        // property: CollimatorId1
        [TestCase(nameof(SystemTelemetry.CollimatorId1), 0u, "CollimatorId1: 0x0")]
        [TestCase(nameof(SystemTelemetry.CollimatorId1), 17u, "CollimatorId1: 0x11")]
        [TestCase(nameof(SystemTelemetry.CollimatorId1), 0xFFFFFFFFu, "CollimatorId1: 0xFFFFFFFF")]
        // property: CollimatorId2
        [TestCase(nameof(SystemTelemetry.CollimatorId2), 0u, "CollimatorId2: 0x0")]
        [TestCase(nameof(SystemTelemetry.CollimatorId2), 17u, "CollimatorId2: 0x11")]
        [TestCase(nameof(SystemTelemetry.CollimatorId2), 0xFFFFFFFFu, "CollimatorId2: 0xFFFFFFFF")]
        // property: CollimatorSerial
        [TestCase(nameof(SystemTelemetry.CollimatorSerial), 0ul, "CollimatorSerial: 0x0")]
        [TestCase(nameof(SystemTelemetry.CollimatorSerial), 16ul, "CollimatorSerial: 0x10")]
        [TestCase(nameof(SystemTelemetry.CollimatorSerial), 0xFFFFFFFFFFFFFFFFul, "CollimatorSerial: 0xFFFFFFFFFFFFFFFF")]
        // property: ButtonsState
        [TestCase(nameof(SystemTelemetry.ButtonsState), -1, "ButtonsState: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.ButtonsState), 0, "ButtonsState: 0x0")]
        [TestCase(nameof(SystemTelemetry.ButtonsState), 1, "ButtonsState: 0x1")]
        // property: CurrentOperationalPoint
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), -1, "CurrentOperationalPoint: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), 0, "CurrentOperationalPoint: 0x0")]
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), 1, "CurrentOperationalPoint: 0x1")]
        // property: TotalOperationalPoints
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), -1, "TotalOperationalPoints: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), 0, "TotalOperationalPoints: 0x0")]
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), 1, "TotalOperationalPoints: 0x1")]
        // property: InternalTimerState
        [TestCase(nameof(SystemTelemetry.InternalTimerState), -1, "InternalTimerState: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.InternalTimerState), 0, "InternalTimerState: 0x0")]
        [TestCase(nameof(SystemTelemetry.InternalTimerState), 1, "InternalTimerState: 0x1")]
        // property: PrimaryTimerValue
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), -1.234f, "PrimaryTimerValue: -1.234")]
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), 0.0f, "PrimaryTimerValue: 0.000")]
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), 123.456f, "PrimaryTimerValue: 123.456")]
        // property: Timer1State
        [TestCase(nameof(SystemTelemetry.Timer1State), -1, "Timer1State: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.Timer1State), 0, "Timer1State: 0x0")]
        [TestCase(nameof(SystemTelemetry.Timer1State), 1, "Timer1State: 0x1")]
        // property: PrimaryTimerValue
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), -1.234f, "SecondaryTimer1Value: -1.234")]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), 0.0f, "SecondaryTimer1Value: 0.000")]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), 123.456f, "SecondaryTimer1Value: 123.456")]
        // property: Timer2State
        [TestCase(nameof(SystemTelemetry.Timer2State), -1, "Timer2State: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.Timer2State), 0, "Timer2State: 0x0")]
        [TestCase(nameof(SystemTelemetry.Timer2State), 1, "Timer2State: 0x1")]
        // property: SecondaryTimer2Value
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), -1.234f, "SecondaryTimer2Value: -1.234")]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), 0.0f, "SecondaryTimer2Value: 0.000")]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), 123.456f, "SecondaryTimer2Value: 123.456")]
        // property: RuntimeCounterHVPS
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), -1, "RuntimeCounterHVPS: 0xFFFFFFFF")]
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), 0, "RuntimeCounterHVPS: 0x0")]
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), 1, "RuntimeCounterHVPS: 0x1")]
        // property: HvpsIOStatus
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 0u, "HvpsIOStatus: 0x0")]
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 16u, "HvpsIOStatus: 0x10")]
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 0xFFFFFFFFu, "HvpsIOStatus: 0xFFFFFFFF")]
        // property: HvpsFlagStatus
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 0u, "HvpsFlagStatus: 0x0")]
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 16u, "HvpsFlagStatus: 0x10")]
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 0xFFFFFFFFu, "HvpsFlagStatus: 0xFFFFFFFF")]
        // property: KvSetpoint
        [TestCase(nameof(SystemTelemetry.KvSetpoint), -1.234f, "KvSetpoint: -1.234")]
        [TestCase(nameof(SystemTelemetry.KvSetpoint), 0.0f, "KvSetpoint: 0.000")]
        [TestCase(nameof(SystemTelemetry.KvSetpoint), 123.456f, "KvSetpoint: 123.456")]
        // property: KvFeedback
        [TestCase(nameof(SystemTelemetry.KvFeedback), -1.234f, "KvFeedback: -1.234")]
        [TestCase(nameof(SystemTelemetry.KvFeedback), 0.0f, "KvFeedback: 0.000")]
        [TestCase(nameof(SystemTelemetry.KvFeedback), 123.456f, "KvFeedback: 123.456")]
        // property: EmissionCurrent
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), -1.234f, "EmissionCurrent: -1.234")]
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), 0.0f, "EmissionCurrent: 0.000")]
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), 123.456f, "EmissionCurrent: 123.456")]
        // property: HeaterCurrentSetpoint
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), -1.234f, "HeaterCurrentSetpoint: -1.234")]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), 0.0f, "HeaterCurrentSetpoint: 0.000")]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), 123.456f, "HeaterCurrentSetpoint: 123.456")]
        // property: HeaterCurrentFeedback
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), -1.234f, "HeaterCurrentFeedback: -1.234")]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), 0.0f, "HeaterCurrentFeedback: 0.000")]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), 123.456f, "HeaterCurrentFeedback: 123.456")]
        // property: EmissionCurrentLimit
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), -1.234f, "EmissionCurrentLimit: -1.234")]
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), 0.0f, "EmissionCurrentLimit: 0.000")]
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), 123.456f, "EmissionCurrentLimit: 123.456")]
        // property: EmissionCurrentLimit
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), -1.234f, "HvpsPowerSetpoint: -1.234")]
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), 0.0f, "HvpsPowerSetpoint: 0.000")]
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), 123.456f, "HvpsPowerSetpoint: 123.456")]
        // property: GridSetpoint
        [TestCase(nameof(SystemTelemetry.GridSetpoint), -1.234f, "GridSetpoint: -1.234")]
        [TestCase(nameof(SystemTelemetry.GridSetpoint), 0.0f, "GridSetpoint: 0.000")]
        [TestCase(nameof(SystemTelemetry.GridSetpoint), 123.456f, "GridSetpoint: 123.456")]
        // property: GridVoltage
        [TestCase(nameof(SystemTelemetry.GridVoltage), -1.234f, "GridVoltage: -1.234")]
        [TestCase(nameof(SystemTelemetry.GridVoltage), 0.0f, "GridVoltage: 0.000")]
        [TestCase(nameof(SystemTelemetry.GridVoltage), 123.456f, "GridVoltage: 123.456")]
        // property: XCoilCurrent
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), -1.234f, "XCoilCurrent: -1.234")]
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), 0.0f, "XCoilCurrent: 0.000")]
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), 123.456f, "XCoilCurrent: 123.456")]
        // property: YCoilCurrent
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), -1.234f, "YCoilCurrent: -1.234")]
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), 0.0f, "YCoilCurrent: 0.000")]
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), 123.456f, "YCoilCurrent: 123.456")]
        // property: FocusCurrent
        [TestCase(nameof(SystemTelemetry.FocusCurrent), -1.234f, "FocusCurrent: -1.234")]
        [TestCase(nameof(SystemTelemetry.FocusCurrent), 0.0f, "FocusCurrent: 0.000")]
        [TestCase(nameof(SystemTelemetry.FocusCurrent), 123.456f, "FocusCurrent: 123.456")]
        // property: FocusCurrent
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), -1.234f, "IonPumpFeedback: -1.234")]
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), 0.0f, "IonPumpFeedback: 0.000")]
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), 123.456f, "IonPumpFeedback: 123.456")]
        // property: WaterPressure
        [TestCase(nameof(SystemTelemetry.WaterPressure), -1.234f, "WaterPressure: -1.234")]
        [TestCase(nameof(SystemTelemetry.WaterPressure), 0.0f, "WaterPressure: 0.000")]
        [TestCase(nameof(SystemTelemetry.WaterPressure), 123.456f, "WaterPressure: 123.456")]
        // property: WaterFlowRate
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), -1.234f, "WaterFlowRate: -1.234")]
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), 0.0f, "WaterFlowRate: 0.000")]
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), 123.456f, "WaterFlowRate: 123.456")]
        // property: WaterTemperature
        [TestCase(nameof(SystemTelemetry.WaterTemperature), -1.234f, "WaterTemperature: -1.234")]
        [TestCase(nameof(SystemTelemetry.WaterTemperature), 0.0f, "WaterTemperature: 0.000")]
        [TestCase(nameof(SystemTelemetry.WaterTemperature), 123.456f, "WaterTemperature: 123.456")]
        // property: HeatSinkTemperature
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), -1.234f, "HeatSinkTemperature: -1.234")]
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), 0.0f, "HeatSinkTemperature: 0.000")]
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), 123.456f, "HeatSinkTemperature: 123.456")]
        // property: PeltierTemperature
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), -1.234f, "PeltierTemperature: -1.234")]
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), 0.0f, "PeltierTemperature: 0.000")]
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), 123.456f, "PeltierTemperature: 123.456")]
        // property: CabinetTemperature
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), -1.234f, "CabinetTemperature: -1.234")]
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), 0.0f, "CabinetTemperature: 0.000")]
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), 123.456f, "CabinetTemperature: 123.456")]
        // property: Mag1
        [TestCase(nameof(SystemTelemetry.Mag1), new float[0], "Mag1: []")]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{-1.234f}, "Mag1: [-1.234]")]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{-1.234f, 0.0f}, "Mag1: [-1.234, 0.000]")]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{-1.234f, 0.0f, 123.456f}, "Mag1: [-1.234, 0.000, 123.456]")]
        // property: Mag1
        [TestCase(nameof(SystemTelemetry.Mag2), new float[0], "Mag2: []")]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{-1.234f}, "Mag2: [-1.234]")]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{-1.234f, 0.0f}, "Mag2: [-1.234, 0.000]")]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{-1.234f, 0.0f, 123.456f}, "Mag2: [-1.234, 0.000, 123.456]")]
        // property: Applicator
        [TestCase(nameof(SystemTelemetry.Applicator), 0u, "Applicator: 0x0")]
        [TestCase(nameof(SystemTelemetry.Applicator), 16u, "Applicator: 0x10")]
        [TestCase(nameof(SystemTelemetry.Applicator), 0xFFFFFFFFu, "Applicator: 0xFFFFFFFF")]
        public void ToString_WithReflection(string propertyName, object propertyValue, string expectedStringPart)
        {
            var sut = SystemTelemetryWithEmptyMagArrays();
            sut.SetPropertyValue(propertyName, propertyValue);
            
            var result = sut.ToString();
            Assert.That(result, Contains.Substring(expectedStringPart));
        }
        
        [Test]
        public void Parse_Default_SystemTelemetry()
        {
            var expected = SystemTelemetryWithFilledMagArrays();

            var sut = ToValidUdpPacketBytes(expected); 
            
            var result = SystemTelemetry.Parse(sut);
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        // property: ControlBoardState
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Startup)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Cold)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.ColdFault)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.DailyWarmup)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Warmup)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.WarmupFault)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Primed)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Staging)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Staged)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.HvpsCheck)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.HVSetup)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Ready)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Launching)] 
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Emission)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Termination)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Discharge)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Fault)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.SystemCrash)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.LaunchingForImaging)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.WaitForKey)]
        [TestCase(nameof(SystemTelemetry.ControlBoardState), GcbStateNew.Imaging)]
        // property: SystemRuntime
        [TestCase(nameof(SystemTelemetry.SystemRuntime), -1)]
        [TestCase(nameof(SystemTelemetry.SystemRuntime), 0)]
        [TestCase(nameof(SystemTelemetry.SystemRuntime), 1)]
        // property: FaultFlags
        [TestCase(nameof(SystemTelemetry.FaultFlags), -2)]
        [TestCase(nameof(SystemTelemetry.FaultFlags), 0)]
        [TestCase(nameof(SystemTelemetry.FaultFlags), 2)]
        // property: InterlockFlags
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 0u)]
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 16u)]
        [TestCase(nameof(SystemTelemetry.InterlockFlags), 0xFFFFFFFFu)]
        // property: RingLedState
        [TestCase(nameof(SystemTelemetry.RingLedState), RingLedState.TBD)]
        [TestCase(nameof(SystemTelemetry.RingLedState), RingLedState.TBD2)]
        // property: BaseLedState
        [TestCase(nameof(SystemTelemetry.BaseLedState), BaseLedState.TBD)]
        [TestCase(nameof(SystemTelemetry.BaseLedState), BaseLedState.TBD2)]
        // property: ButtonsState
        [TestCase(nameof(SystemTelemetry.ButtonsState), -1)]
        [TestCase(nameof(SystemTelemetry.ButtonsState), 0)]
        [TestCase(nameof(SystemTelemetry.ButtonsState), 1)]
        // property: CurrentOperationalPoint
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), -1)]
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), 0)]
        [TestCase(nameof(SystemTelemetry.CurrentOperationalPoint), 1)]
        // property: TotalOperationalPoints
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), -1)]
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), 0)]
        [TestCase(nameof(SystemTelemetry.TotalOperationalPoints), 1)]
        // property: InternalTimerState
        [TestCase(nameof(SystemTelemetry.InternalTimerState), -1)]
        [TestCase(nameof(SystemTelemetry.InternalTimerState), 0)]
        [TestCase(nameof(SystemTelemetry.InternalTimerState), 1)]
        // property: PrimaryTimerValue
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), -1.234f)]
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), 0.0f)]
        [TestCase(nameof(SystemTelemetry.PrimaryTimerValue), 123.456f)]
        // property: Timer1State
        [TestCase(nameof(SystemTelemetry.Timer1State), -1)]
        [TestCase(nameof(SystemTelemetry.Timer1State), 0)]
        [TestCase(nameof(SystemTelemetry.Timer1State), 1)]
        // property: PrimaryTimerValue
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), -1.234f)]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), 0.0f)]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer1Value), 123.456f)]
        // property: Timer2State
        [TestCase(nameof(SystemTelemetry.Timer2State), -1)]
        [TestCase(nameof(SystemTelemetry.Timer2State), 0)]
        [TestCase(nameof(SystemTelemetry.Timer2State), 1)]
        // property: SecondaryTimer2Value
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), -1.234f)]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), 0.0f)]
        [TestCase(nameof(SystemTelemetry.SecondaryTimer2Value), 123.456f)]
        // property: RuntimeCounterHVPS
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), -1)]
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), 0)]
        [TestCase(nameof(SystemTelemetry.RuntimeCounterHVPS), 1)]
        // property: HvpsIOStatus
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 0u)]
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 16u)]
        [TestCase(nameof(SystemTelemetry.HvpsIOStatus), 0xFFFFFFFFu)]
        // property: HvpsFlagStatus
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 0u)]
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 16u)]
        [TestCase(nameof(SystemTelemetry.HvpsFlagStatus), 0xFFFFFFFFu)]
        // property: KvSetpoint
        [TestCase(nameof(SystemTelemetry.KvSetpoint), -1.234f)]
        [TestCase(nameof(SystemTelemetry.KvSetpoint), 0.0f)]
        [TestCase(nameof(SystemTelemetry.KvSetpoint), 123.456f)]
        // property: KvFeedback
        [TestCase(nameof(SystemTelemetry.KvFeedback), -1.234f)]
        [TestCase(nameof(SystemTelemetry.KvFeedback), 0.0f)]
        [TestCase(nameof(SystemTelemetry.KvFeedback), 123.456f)]
        // property: EmissionCurrent
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), -1.234f)]
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), 0.0f)]
        [TestCase(nameof(SystemTelemetry.EmissionCurrent), 123.456f)]
        // property: HeaterCurrentSetpoint
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), -1.234f)]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), 0.0f)]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentSetpoint), 123.456f)]
        // property: HeaterCurrentFeedback
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), -1.234f)]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), 0.0f)]
        [TestCase(nameof(SystemTelemetry.HeaterCurrentFeedback), 123.456f)]
        // property: EmissionCurrentLimit
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), -1.234f)]
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), 0.0f)]
        [TestCase(nameof(SystemTelemetry.EmissionCurrentLimit), 123.456f)]
        // property: EmissionCurrentLimit
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), -1.234f)]
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), 0.0f)]
        [TestCase(nameof(SystemTelemetry.HvpsPowerSetpoint), 123.456f)]
        // property: GridSetpoint
        [TestCase(nameof(SystemTelemetry.GridSetpoint), -1.234f)]
        [TestCase(nameof(SystemTelemetry.GridSetpoint), 0.0f)]
        [TestCase(nameof(SystemTelemetry.GridSetpoint), 123.456f)]
        // property: GridVoltage
        [TestCase(nameof(SystemTelemetry.GridVoltage), -1.234f)]
        [TestCase(nameof(SystemTelemetry.GridVoltage), 0.0f)]
        [TestCase(nameof(SystemTelemetry.GridVoltage), 123.456f)]
        // property: XCoilCurrent
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), -1.234f)]
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), 0.0f)]
        [TestCase(nameof(SystemTelemetry.XCoilCurrent), 123.456f)]
        // property: YCoilCurrent
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), -1.234f)]
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), 0.0f)]
        [TestCase(nameof(SystemTelemetry.YCoilCurrent), 123.456f)]
        // property: FocusCurrent
        [TestCase(nameof(SystemTelemetry.FocusCurrent), -1.234f)]
        [TestCase(nameof(SystemTelemetry.FocusCurrent), 0.0f)]
        [TestCase(nameof(SystemTelemetry.FocusCurrent), 123.456f)]
        // property: FocusCurrent
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), -1.234f)]
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), 0.0f)]
        [TestCase(nameof(SystemTelemetry.IonPumpFeedback), 123.456f)]
        // property: WaterPressure
        [TestCase(nameof(SystemTelemetry.WaterPressure), -1.234f)]
        [TestCase(nameof(SystemTelemetry.WaterPressure), 0.0f)]
        [TestCase(nameof(SystemTelemetry.WaterPressure), 123.456f)]
        // property: WaterFlowRate
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), -1.234f)]
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), 0.0f)]
        [TestCase(nameof(SystemTelemetry.WaterFlowRate), 123.456f)]
        // property: WaterTemperature
        [TestCase(nameof(SystemTelemetry.WaterTemperature), -1.234f)]
        [TestCase(nameof(SystemTelemetry.WaterTemperature), 0.0f)]
        [TestCase(nameof(SystemTelemetry.WaterTemperature), 123.456f)]
        // property: HeatSinkTemperature
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), -1.234f)]
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), 0.0f)]
        [TestCase(nameof(SystemTelemetry.HeatSinkTemperature), 123.456f)]
        // property: PeltierTemperature
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), -1.234f)]
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), 0.0f)]
        [TestCase(nameof(SystemTelemetry.PeltierTemperature), 123.456f)]
        // property: CabinetTemperature
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), -1.234f)]
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), 0.0f)]
        [TestCase(nameof(SystemTelemetry.CabinetTemperature), 123.456f)]
        // property: Mag1
        [TestCase(nameof(SystemTelemetry.Mag1), new []{0.0f, 0.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{0.0f, 0.0f, 1.0f})]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{0.0f, 1.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{1.0f, 0.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag1), new []{-1.234f, 34.23f, 123.456f})]
        // property: Mag2
        [TestCase(nameof(SystemTelemetry.Mag2), new []{0.0f, 0.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{0.0f, 0.0f, 1.0f})]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{0.0f, 1.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{1.0f, 0.0f, 0.0f})]
        [TestCase(nameof(SystemTelemetry.Mag2), new []{-1.234f, 34.23f, 123.456f})]
        // property: Applicator
        [TestCase(nameof(SystemTelemetry.Applicator), 0u)]
        [TestCase(nameof(SystemTelemetry.Applicator), 16u)]
        [TestCase(nameof(SystemTelemetry.Applicator), 0xFFFFFFFFu)]
        public void Parse(string propertyName, object propertyValue)
        {
            var expected = SystemTelemetryWithFilledMagArrays();
            expected.SetPropertyValue(propertyName, propertyValue);

            var sut = ToValidUdpPacketBytes(expected); 
            
            var result = SystemTelemetry.Parse(sut);
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        // property: CollimatorId1, CollimatorId2, collimatorSerial
        [TestCase(0x00000000u, 0x00000000u, 0x0000000000000000ul)]
        [TestCase(0x00000016u, 0x00000000u, 0x0000000000000016ul)]
        [TestCase(0x00000000u, 0x00000016u, 0x0000001600000000ul)]
        [TestCase(0x00000016u, 0x00000016u, 0x0000001600000016ul)]
        [TestCase(0x76543210u, 0xFFEDCBA9u, 0xFFEDCBA976543210ul)]
        [TestCase(0xFFFFFFFFu, 0x00000000u, 0x00000000FFFFFFFFul)]
        [TestCase(0x00000000u, 0xFFFFFFFFu, 0xFFFFFFFF00000000ul)]
        [TestCase(0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFFFFFFFFFul)]
        public void Parse_With_Collimator(uint collimatorId1, uint collimatorId2, ulong collimatorSerial)
        {
            var expected = SystemTelemetryWithFilledMagArrays();
            expected.SetPropertyValue(nameof(expected.CollimatorId1), collimatorId1);
            expected.SetPropertyValue(nameof(expected.CollimatorId2), collimatorId2);
            expected.SetPropertyValue(nameof(expected.CollimatorSerial), collimatorSerial);

            var sut = ToValidUdpPacketBytes(expected); 
            
            var result = SystemTelemetry.Parse(sut);
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        
        [TestCase(GCBPacketType.VersionInfo)]
        [TestCase(GCBPacketType.FaultInfo)]
        [TestCase(GCBPacketType.DirectiveCmd)]
        [TestCase(GCBPacketType.TelemetryRequest)]
        [TestCase(GCBPacketType.ConditioningCmd)]
        [TestCase(GCBPacketType.WarmupCmd)]
        [TestCase(GCBPacketType.NewSessionCmd)]
        [TestCase(GCBPacketType.OperationalPointLoadingCmd)]
        [TestCase(GCBPacketType.OperationalPointConfirmationCmd)]
        [TestCase(GCBPacketType.OperationalPointQueryCmd)]
        [TestCase(GCBPacketType.ReleaseTreatmentPlan)]
        [TestCase(GCBPacketType.WaitForButtonCmd)]
        [TestCase(GCBPacketType.VersionInfoResponse)]
        [TestCase(GCBPacketType.FaultInfoResponse)]
        [TestCase(GCBPacketType.DirectiveCmdResponse)]
        [TestCase(GCBPacketType.ConditioningResponse)]
        [TestCase(GCBPacketType.WarmupResponse)]
        [TestCase(GCBPacketType.NewSessionResponse)]
        [TestCase(GCBPacketType.OperationalPointLoadingResponse)]
        [TestCase(GCBPacketType.OperationalPointConfirmationResponse)]
        [TestCase(GCBPacketType.OperationalPointQueryResponse)]
        [TestCase(GCBPacketType.ReleaseTreatmentPlanResponse)]
        [TestCase(GCBPacketType.WaitForButtonResponse)]
        public void Parse_Throws_With_InvalidPacketType(GCBPacketType packetType)
        {
            var expected = SystemTelemetryWithFilledMagArrays();
            var udpPacketFields = SystemTelemetryUdpPacketFields(expected);
            
            var bytes = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)packetType,
                packetCounter: (uint)GCBTelemetryResponseField.PayloadFields,
                payload: udpPacketFields);

            Assert.That(() =>
            {
                var result = SystemTelemetry.Parse(bytes);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_Throws_With_InvalidFieldCount(
            [Range(0, (int)GCBTelemetryResponseField.PayloadFields - 4)] int fieldCount) // 3 fields are optional now
        {
            var expected = SystemTelemetryWithFilledMagArrays();
            var udpPacketFields = SystemTelemetryUdpPacketFields(expected).Take(fieldCount).ToList();
            
            var bytes = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: (uint)GCBTelemetryResponseField.PayloadFields,
                payload: udpPacketFields);

            Assert.That(() =>
            {
                var result = SystemTelemetry.Parse(bytes);
            }, Throws.Exception);
        }
        
        private static List<UdpPacket.Field> SystemTelemetryUdpPacketFields(SystemTelemetry telemetry)
        {
            var fields = new List<UdpPacket.Field>
            {
                (int)telemetry.ControlBoardState,
                telemetry.SystemRuntime,
                telemetry.FaultFlags,
                telemetry.InterlockFlags,
                (int)telemetry.RingLedState,
                (int)telemetry.BaseLedState,
                telemetry.CollimatorId1,
                telemetry.CollimatorId2,
                telemetry.ButtonsState,
                telemetry.CurrentOperationalPoint,
                telemetry.TotalOperationalPoints,
                telemetry.InternalTimerState,
                telemetry.PrimaryTimerValue,
                telemetry.Timer1State,
                telemetry.SecondaryTimer1Value,
                telemetry.Timer2State,
                telemetry.SecondaryTimer2Value,
                telemetry.RuntimeCounterHVPS,
                telemetry.HvpsIOStatus,
                telemetry.HvpsFlagStatus,
                telemetry.KvFeedback,
                telemetry.EmissionCurrent,
                telemetry.HeaterCurrentSetpoint,
                telemetry.HeaterCurrentFeedback,
                telemetry.GridSetpoint,
                telemetry.GridVoltage,
                telemetry.XCoilCurrent,
                telemetry.YCoilCurrent,
                telemetry.FocusCurrent,
                telemetry.IonPumpFeedback,
                telemetry.WaterPressure,
                telemetry.WaterFlowRate,
                telemetry.WaterTemperature,
                telemetry.HeatSinkTemperature,
                telemetry.PeltierTemperature,
                telemetry.CabinetTemperature,
                telemetry.Mag1[0],
                telemetry.Mag1[1],
                telemetry.Mag1[2],
                telemetry.Mag2[0],
                telemetry.Mag2[1],
                telemetry.Mag2[2],
                telemetry.Applicator,
                telemetry.KvSetpoint,
                telemetry.EmissionCurrentLimit,
                telemetry.HvpsPowerSetpoint,
            };

            return fields;
        }
        
        private static byte[] ToValidUdpPacketBytes(SystemTelemetry telemetry)
        {
            var fields = SystemTelemetryUdpPacketFields(telemetry);
            
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: (uint)GCBTelemetryResponseField.PayloadFields,
                payload: fields);
        }

        private static SystemTelemetry SystemTelemetryWithEmptyMagArrays()
        {
            var telemetry = new SystemTelemetry();
            telemetry.SetPropertyValue(nameof(telemetry.Mag1), Array.Empty<float>());
            telemetry.SetPropertyValue(nameof(telemetry.Mag2), Array.Empty<float>());
            return telemetry;
        }

        private static SystemTelemetry SystemTelemetryWithFilledMagArrays()
        {
            var telemetry = new SystemTelemetry();
            telemetry.SetPropertyValue(nameof(telemetry.Mag1), new[]{0.0f, 0.0f, 0.0f});
            telemetry.SetPropertyValue(nameof(telemetry.Mag2), new[]{0.0f, 0.0f, 0.0f});
            return telemetry;
        }
    }
}
