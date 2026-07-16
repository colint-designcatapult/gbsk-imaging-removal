using Com.Empyreanmed.Heracles.Enums.V1;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Protos
{
    /// <summary>
    /// Test fixture with parameters of enum type and its invalid value to test against.
    /// For many enums not having any non-convertable values, some out-of-range value is used to test the default (error) branch on it.
    /// </summary>
    /// <typeparam name="TEnumType">Type of Xcc/Heracles to convert to a Protos enum</typeparam>
    [TestFixture(typeof(Xcc.Core.Enums.LogRecordSeverity), Xcc.Core.Enums.LogRecordSeverity.Unspecified - 1)]
    [TestFixture(typeof(Xcc.Core.Enums.LogRecordType), Xcc.Core.Enums.LogRecordType.Unspecified - 1)]
    [TestFixture(typeof(MagnetometerType), MagnetometerType.Back - 1)]
    [TestFixture(typeof(Core.Enums.DeviceType), Core.Enums.DeviceType.PrefabricatedShield - 1)]
    [TestFixture(typeof(Core.Enums.TreatmentLoadingState), Core.Enums.TreatmentLoadingState.Unloaded - 1)]
    [TestFixture(typeof(Xcc.Core.Enums.PatientIdType), Xcc.Core.Enums.PatientIdType.Unspecified - 1)]
    [TestFixture(typeof(Core.Enums.Energy), Core.Enums.Energy.Energy_50 - 1)]
    [TestFixture(typeof(Core.Enums.TDF), Core.Enums.TDF.Tdf_94 - 1)]
    [TestFixture(typeof(Core.Enums.Status), Core.Enums.Status.PENDING_APPROVAL - 1)]
    [TestFixture(typeof(Core.Enums.PlanStatus), Core.Enums.PlanStatus.PENDING_APPROVAL - 1)]
    [TestFixture(typeof(Core.Enums.SimulationStatus), Core.Enums.SimulationStatus.Active)] // active status is not convertible now
    [TestFixture(typeof(Core.Enums.TargetType), Core.Enums.TargetType.TargetType_None)]
    [TestFixture(typeof(Core.Enums.TreatmentFieldName), Core.Enums.TreatmentFieldName.Plus4L2 - 1)]
    [TestFixture(typeof(Xcc.Core.Enums.Sex), Xcc.Core.Enums.Sex.Male - 1)]
    [TestFixture(typeof(Core.Enums.PatientPosition), Core.Enums.PatientPosition.Prone - 1)]
    [TestFixture(typeof(Core.Enums.Pathology), Core.Enums.Pathology.Bcc - 1)]
    [TestFixture(typeof(Core.Enums.PatientStatus), Core.Enums.PatientStatus.Active - 1)]
    [TestFixture(typeof(Core.Enums.SiteLocation), Core.Enums.SiteLocation.Breast - 1)]
    [TestFixture(typeof(Core.Enums.PhotoType), Core.Enums.PhotoType.LesionWithMargin - 1)]
    [TestFixture(typeof(Core.Enums.TemplateType), Core.Enums.TemplateType.Simulation - 1)]
    [TestFixture(typeof(WarmupType), WarmupType.Fast - 1)]
    [TestFixture(typeof(Core.Enums.SsdType), Core.Enums.SsdType.SsdType50mm - 1)]
    [TestFixture(typeof(Core.Enums.Celltype), Core.Enums.Celltype.Aberrant - 1)]
    [TestFixture(typeof(Core.Enums.IcdCode), Core.Enums.IcdCode.BCC_Breast - 1)]
    [TestFixture(typeof(Core.Enums.Description), Core.Enums.Description.InfundibuloCytic - 1)]
    [TestFixture(typeof(Core.Enums.VisitType), Core.Enums.VisitType.Simulation - 1)]
    public class ProtoTypesConverter_EnumsToProtoTests<TEnumType> : ProtoTypesConverterInvoker<TEnumType>
        where TEnumType : struct, Enum
    {
        public TEnumType InvalidValue { get; }

        public ProtoTypesConverter_EnumsToProtoTests(TEnumType invalidValue)
        {
            InvalidValue = invalidValue;
        }

        [Test]
        public void ValidConversionTest([Values] TEnumType enumValue)
        {
            TestDelegate testCall = () => ToProto(enumValue);
            if (!enumValue.Equals(InvalidValue))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }

        [Test]
        public void InvalidConversionTest()
        {
            Assert.Throws<InvalidCastException>(() => ToProto(InvalidValue));
        }
    }

    /// <summary>
    /// Test fixture with parameters of enum type and its invalid value to test against.
    /// For some enums not having any non-convertable values, an out-of-range value is used to test the default (error) branch on it.
    /// </summary>
    /// <typeparam name="TEnumType">Type of Protos to convert from</typeparam>
    [TestFixture(typeof(SEVERITY), SEVERITY.Unspecified - 1)]
    [TestFixture(typeof(LOGTYPE), LOGTYPE.Unspecified - 1)]
    [TestFixture(typeof(MAGNETOMETERTYPE), MAGNETOMETERTYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(DEVICETYPE), DEVICETYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(TREATMENTLOADINGSTATE), TREATMENTLOADINGSTATE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(PATIENTIDTYPE), PATIENTIDTYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(ENERGY), ENERGY.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(TDF), TDF.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(STATUS), STATUS.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(FIELDNAME), FIELDNAME.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(SEXTYPE), SEXTYPE.Unspecified - 1)]
    [TestFixture(typeof(POSITION), POSITION.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(PATHOLOGY), PATHOLOGY.Unspecified - 1)]
    [TestFixture(typeof(PATIENTSTATUS), PATIENTSTATUS.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(SITELOCATION), SITELOCATION.Unspecified - 1)]
    [TestFixture(typeof(PHOTOTYPE), PHOTOTYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(TEMPLATETYPE), TEMPLATETYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(WARMUPTYPE), WARMUPTYPE.Unspecified)]
    [TestFixture(typeof(SSDTYPE), SSDTYPE.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(CELLTYPE), CELLTYPE.Unspecified - 1)]
    [TestFixture(typeof(ICDCODE), ICDCODE.Unspecified - 1)]
    [TestFixture(typeof(DESCRIPTION), DESCRIPTION.Unspecified)] // Doesn't support Unspecified value conversion
    [TestFixture(typeof(VISITTYPE), VISITTYPE.Unspecified)] // Doesn't support Unspecified value conversion
    public class ProtoTypesConverter_EnumsFromProtoTests<TEnumType> : ProtoTypesConverterInvoker<TEnumType>
        where TEnumType : struct, Enum
    {
        public TEnumType InvalidValue { get; }

        public ProtoTypesConverter_EnumsFromProtoTests(TEnumType invalidValue)
        {
            InvalidValue = invalidValue;
        }

        /// <summary>
        /// Tests for the conversion of all the defined values of the enum type
        /// </summary>
        /// <param name="enumValue"></param>
        [Test]
        public void ValidConversionTest([Values] TEnumType enumValue)
        {
            TestDelegate testCall = () => FromProto(enumValue);
            if (!enumValue.Equals(InvalidValue))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }

        /// <summary>
        /// Tests for an invalid value conversion in case if it's out of range of the enum type
        /// </summary>
        [Test]
        public void InvalidConversionTest()
        {
            Assert.Throws<InvalidCastException>(() => FromProto(InvalidValue));
        }
    }

    // As we have same proto STATUS for different entries with different display requirements,
    // we cast it to different internal types 
    public class ProtoTypesConverter_PlanStatusFromProtoTest : ProtoTypesConverterInvoker<STATUS>
    {
        [Test]
        public void ConversionTest([Values] STATUS enumValue)
        {
            TestDelegate testCall = () => FromProto(enumValue);
            if (!enumValue.Equals(STATUS.Unspecified))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }
        public override object? FromProto(STATUS value)
        {
            return InvokeConverter("FromProtoToPlanStatus", value);
        }
    }

    // As we have same proto STATUS for different entries with different display requirements,
    // we cast it to different internal types 
    public class ProtoTypesConverter_SimulationStatusFromProtoTest : ProtoTypesConverterInvoker<STATUS>
    {
        [Test]
        public void ConversionTest([Values] STATUS enumValue)
        {
            TestDelegate testCall = () => FromProto(enumValue);
            if (enumValue.Equals(STATUS.Approved) || enumValue.Equals(STATUS.PendingApproval))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }
        public override object? FromProto(STATUS value)
        {
            return InvokeConverter("FromProtoToSimulationStatus", value);
        }
    }

    public class ProtoTypesConverter_TargetTypeFromProtoTest : ProtoTypesConverterInvoker<TARGETTYPE>
    {
        [Test]
        public void ConversionTest([Values] TARGETTYPE enumValue)
        {
            TestDelegate testCall = () => FromProto(enumValue);
            // Allow only these collimator types now:
            IList<TARGETTYPE> types = [
                TARGETTYPE._50MmSsd15MmField,
                TARGETTYPE._50MmSsd20MmField,
                TARGETTYPE._50MmSsd30MmField,
                TARGETTYPE._50MmSsd40MmField,
                TARGETTYPE._50MmSsd50MmField,
                TARGETTYPE.ImvbCollimator5MmCell,
                TARGETTYPE.ImvbCollimator6MmspotLargecentralCell,
                TARGETTYPE.ImvbCollimator5CmSsd0Point5CmField05MmCell,
                TARGETTYPE.QcCollimator];
            if (types.Contains(enumValue))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }
    }

}
