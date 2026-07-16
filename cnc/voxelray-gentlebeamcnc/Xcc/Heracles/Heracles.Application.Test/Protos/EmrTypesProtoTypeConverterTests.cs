using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Patients.V1;
using Com.Empyreanmed.Heracles.Photos.V1;
using Com.Empyreanmed.Heracles.Plans.V1;
using Com.Empyreanmed.Heracles.Positions.V1;
using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Com.Empyreanmed.Heracles.Simulations.V1;
using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Com.Empyreanmed.Heracles.Treatments.V1;
using Com.Empyreanmed.Heracles.Visits.V1;
using Heracles.Application.Protos;
using Heracles.Core.Models.EMR;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Heracles.Application.Test.Protos
{
    [TestFixture(typeof(IPatient), typeof(Patient))]
    [TestFixture(typeof(IDiagnosis), typeof(Diagnosis))]
    [TestFixture(typeof(IPrescription), typeof(Prescription))]
    [TestFixture(typeof(ISimulation), typeof(Simulation))]
    [TestFixture(typeof(IVisit), typeof(Visit))]
    [TestFixture(typeof(IPlan), typeof(Plan))]
    [TestFixture(typeof(ITreatmentField), typeof(TreatmentField))]
    [TestFixture(typeof(ITreatment), typeof(Treatment))]
    [TestFixture(typeof(IUser), typeof(Com.Empyreanmed.Heracles.Users.V1.User))]
    [TestFixture(typeof(ITreatmentDevice), typeof(TreatmentDevice))]
    [TestFixture(typeof(IPatientPosition), typeof(Position))]
    [TestFixture(typeof(IPhotoDescription), typeof(Photo))]
    [TestFixture(typeof(IActualTreatmentField), typeof(ActualTreatmentField))]
    [TestFixture(typeof(IEmissionTreatmentField), typeof(EmissionTreatmentField))]
    internal class EmrTypesProtoTypeConverter_NullReferenceTests<XccType, GrpcType>
        where XccType: class
        where GrpcType: class
    {
        ProtoTypesConverterInvoker<XccType> toProtoInvoker = new();
        ProtoTypesConverterInvoker<GrpcType> fromProtoInvoker = new();
        [Test]
        public void ToProto_NullReferenceTest() 
        {
            Assert.Throws<ArgumentNullException>(() => toProtoInvoker.ToProto(null!));
        }
        [Test]
        public void FromProto_NullReferenceTest()
        {
            Assert.Throws<ArgumentNullException>(() => fromProtoInvoker.FromProto(null!));
        }

    }

    internal class EmrTypesProtoTypeConverterTests
    {
        [Test]
        public void ToProto_IPatient_Test()
        {
            var patient = new Application.Models.Patient { 
                FirstName = "John", 
                LastName = "Doe", 
                Id = BaseEntry.NEW_ENTRY_ID,
                ProviderId = "Dr.John@clinic.ddd",
                Sex = Xcc.Core.Enums.Sex.Male, 
                DOB = DateOnly.FromDateTime(DateTime.Now)
            };
            var protoType = ProtoTypesConverter.ToProto(patient);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasFirstName, Is.True);
                Assert.That(protoType.FirstName, Is.EqualTo(patient.FirstName));
                Assert.That(protoType.HasLastName, Is.True);
                Assert.That(protoType.LastName, Is.EqualTo(patient.LastName));
                Assert.That(protoType.HasId, Is.False);
            });

            // now try with valid Id:
            patient.Id = 1;
            protoType = ProtoTypesConverter.ToProto(patient);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(patient.Id));
            });
        }

        [Test]
        public void FromProto_IPatient_Test()
        {
            var dob = DateOnly.FromDateTime(DateTime.Today);
            var protoType = new Patient
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                FirstName = "John",
                LastName = "Doe",
                MiddleName = string.Empty,
                Sex = Com.Empyreanmed.Heracles.Enums.V1.SEXTYPE.Male,
                PatientId = "123",
                PatientIdType = Com.Empyreanmed.Heracles.Enums.V1.PATIENTIDTYPE.Passport,
                Dob = ProtoTypesConverter.ToTimestamp(dob),
                Mrn = "123123",
                Address = "Address",
                City = "City",
                State = "State",
                Country = "Country",
                Zip = "123321",
                Email = "test@test",
                Ethnicity = "None",
                Race = "Mixed",
                Phone = "+1-123-123-1312",
                Picture = string.Empty,
                ProviderId = "housemd@test",
                Status = Com.Empyreanmed.Heracles.Enums.V1.PATIENTSTATUS.Active,
                Notes = "SomeNotes"
            };
            var patient = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(patient.FirstName, Is.EqualTo(protoType.FirstName));
                Assert.That(patient.LastName, Is.EqualTo(protoType.LastName));
                Assert.That(patient.Id, Is.EqualTo(protoType.Id));
                Assert.That(patient.DOB, Is.EqualTo(dob));
            });
        }

        [Test]
        public void ToProto_IDiagnosis_Test()
        {
            var diagnosis = new Application.Models.RDBMS.EMR.Diagnosis
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                PatientId = 1,
                Pathology = Core.Enums.Pathology.Bcc,
                SiteName = "Field 0",
                SiteLocation = Core.Enums.SiteLocation.Back,
                Referring = "Dr. Willson",
                IcdCode = Core.Enums.IcdCode.SCC_Back,
                Description = Core.Enums.Description.RapidGrowth
            };
            var protoType = ProtoTypesConverter.ToProto(diagnosis);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.SiteName, Is.EqualTo(diagnosis.SiteName));
                Assert.That(protoType.Referring, Is.EqualTo(diagnosis.Referring));
                Assert.That(protoType.IcdCode, Is.EqualTo(ICDCODE.SccBack));
                Assert.That(protoType.Pathology, Is.EqualTo(PATHOLOGY.Bcc));
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.Description, Is.EqualTo(DESCRIPTION.RapidGrowth));
            });

            // now try with valid Id:
            diagnosis.Id = 1;
            protoType = ProtoTypesConverter.ToProto(diagnosis);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(diagnosis.Id));
            });
        }

        [Test]
        public void FromProto_IDiagnosis_Test()
        {
            var protoType = new Diagnosis
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                PatientId = 1,
                Pathology = PATHOLOGY.Scc,
                SiteName = "Field 0",
                SiteLocation = SITELOCATION.Abdomen,
                Referring = "Dr. Willson",
                IcdCode = ICDCODE.SccBack,
                Description = DESCRIPTION.LargeLesion
            };
            var diagnosis = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(diagnosis.SiteName, Is.EqualTo(protoType.SiteName));
                Assert.That(diagnosis.Referring, Is.EqualTo(protoType.Referring));
                Assert.That(diagnosis.Id, Is.EqualTo(protoType.Id));
            });
        }

        [Test]
        public void ToProto_IPrescription_Test()
        {
            var prescription = new Application.Models.RDBMS.EMR.Prescription
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                DailyDose = 100,
                DwellTime = 50,
                Energy = Core.Enums.Energy.Energy_50,
                MinTdf = Core.Enums.TDF.Tdf_100,
                Tdf = Core.Enums.TDF.Tdf_100,
                NumberOfFxs = 2,
                SimulationId = 1,
                Status = Core.Enums.Status.APPROVED,
                TotalDose = 100,
                FxsPerWeek = 1,
            };

            var protoType = ProtoTypesConverter.ToProto(prescription);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasDailyDose, Is.True);
                Assert.That(protoType.DailyDose, Is.EqualTo(prescription.DailyDose));
                Assert.That(protoType.HasEnergy, Is.True);
                Assert.That(protoType.Energy, Is.EqualTo(ENERGY._50));
            });

            // now try with valid Id:
            prescription.Id = 1;
            protoType = ProtoTypesConverter.ToProto(prescription);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(prescription.Id));
            });
        }

        [Test]
        public void FromProto_IPrescription_Test()
        {
            var protoType = new Prescription
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                DailyDose = 100,
                DwellTime = 50,
                Energy = ENERGY._70,
                MinTdf = Com.Empyreanmed.Heracles.Enums.V1.TDF._102,
                Tdf = Com.Empyreanmed.Heracles.Enums.V1.TDF._102,
                NumberOfFxs = 5,
                SimulationId = 1,
                Status = STATUS.PendingApproval,
                TxsPerWeek = 2,
            };
            var prescription = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(prescription.Id, Is.EqualTo(protoType.Id));
                Assert.That(prescription.DailyDose, Is.EqualTo(protoType.DailyDose));
                Assert.That(prescription.Energy, Is.EqualTo(Core.Enums.Energy.Energy_70));
                Assert.That(prescription.NumberOfFxs, Is.EqualTo(protoType.NumberOfFxs));
            });
        }

        [Test]
        public void ToProto_ISimulation_Test()
        {
            var simulation = new Application.Models.RDBMS.EMR.Simulation
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                DiagnosisId = 1,
                LesionDepth = 2.0,
                LesionSizeL = 3.0,
                LesionSizeW = 4.0,
                MarginSizeL = 5.0,
                MarginSizeW = 5.5,
                PerformedBy = "admin@test",
                SetupNote = string.Empty,
                ShieldSizeW = 6.0,
                ShieldSizeL = 6.0,
                Status = Core.Enums.SimulationStatus.Approved,
                TargetType = Core.Enums.TargetType.TargetType_30mm_SSD_7_Fields,
                VisitId = 1,
            };

            var protoType = ProtoTypesConverter.ToProto(simulation);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasDiagnosisId, Is.True);
                Assert.That(protoType.DiagnosisId, Is.EqualTo(simulation.DiagnosisId));
                Assert.That(protoType.HasLesionSizeW, Is.True);
                Assert.That(protoType.LesionSizeW, Is.EqualTo(simulation.LesionSizeW));
                Assert.That(protoType.HasPerformedBy, Is.True);
                Assert.That(protoType.PerformedBy, Is.EqualTo(simulation.PerformedBy));
            });

            // now try with valid Id:
            simulation.Id = 1;
            protoType = ProtoTypesConverter.ToProto(simulation);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(simulation.Id));
            });
        }

        [Test]
        public void FromProto_ISimulation_Test()
        {
            var protoType = new Simulation
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                DiagnosisId = 1,
                LesionDepth = 2.0,
                LesionSizeL = 3.0,
                LesionSizeW = 4.0,
                MarginSizeL = 5.0,
                MarginSizeW = 5.5,
                PerformedBy = "admin@test",
                SetupNote = string.Empty,
                ShieldSizeW = 6.0,
                ShieldSizeL = 6.0,
                Status = STATUS.PendingApproval,
                TargetType = TARGETTYPE.QcCollimator,
                VisitId = 1
            };
            var simulation = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(simulation.Id, Is.EqualTo(protoType.Id));
                Assert.That(simulation.Status, Is.EqualTo(Core.Enums.SimulationStatus.Pending));
                Assert.That(simulation.LesionSizeL, Is.EqualTo(protoType.LesionSizeL));
                Assert.That(simulation.VisitId, Is.EqualTo(protoType.VisitId));
            });
        }

        [Test]
        public void ToProto_IVisit_Test()
        {
            var visit = new Application.Models.RDBMS.EMR.Visit
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                PatientId = 1,
                Type = Core.Enums.VisitType.SkinCheck
            };

            var protoType = ProtoTypesConverter.ToProto(visit);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasPatientId, Is.True);
                Assert.That(protoType.PatientId, Is.EqualTo(visit.PatientId));
                Assert.That(protoType.HasType, Is.True);
                Assert.That(protoType.Type, Is.EqualTo(VISITTYPE.SkinCheck));
            });

            // now try with valid Id:
            visit.Id = 1;
            protoType = ProtoTypesConverter.ToProto(visit);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(visit.Id));
            });
        }

        [Test]
        public void FromProto_IVisit_Test()
        {
            var protoType = new Visit
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                PatientId = 1,
                Type = VISITTYPE.Otv
            };
            var visit = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(visit.Id, Is.EqualTo(protoType.Id));
                Assert.That(visit.PatientId, Is.EqualTo(protoType.PatientId));
                Assert.That(visit.Type, Is.EqualTo(Core.Enums.VisitType.OTV));
            });
        }

        [Test]
        public void ToProto_IPlan_Test()
        {
            var plan = new Application.Models.RDBMS.EMR.Plan
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                PrescriptionId = 1,
                CollimatorType = Core.Enums.TargetType.TargetType_61_Fields,
                ApprovedBy = "test@test",
                Status = Core.Enums.PlanStatus.APPROVED,
                TreatmentLoadingState = Core.Enums.TreatmentLoadingState.Unloaded
            };

            var protoType = ProtoTypesConverter.ToProto(plan);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasPrescriptionId, Is.True);
                Assert.That(protoType.PrescriptionId, Is.EqualTo(plan.PrescriptionId));
                Assert.That(protoType.HasApprovedBy, Is.True);
                Assert.That(protoType.ApprovedBy, Is.EqualTo(plan.ApprovedBy));
            });

            // now try with valid Id:
            plan.Id = 1;
            protoType = ProtoTypesConverter.ToProto(plan);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(plan.Id));
            });
        }

        [Test]
        public void FromProto_IPlan_Test()
        {
            var protoType = new Plan
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                PrescriptionId = 1,
                TargetType = TARGETTYPE.ImvbCollimator5MmCell,
                ApprovedBy = "test@test",
                Status = STATUS.PendingApproval,
                TreatmentLoadingState = TREATMENTLOADINGSTATE.Unloaded,
            };
            var plan = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(plan.Id, Is.EqualTo(protoType.Id));
                Assert.That(plan.PrescriptionId, Is.EqualTo(protoType.PrescriptionId));
                Assert.That(plan.ApprovedBy, Is.EqualTo(protoType.ApprovedBy));
            });
        }

        [Test]
        public void ToProto_ITreatmentField_Test()
        {
            var treatmentField = new Application.Models.RDBMS.EMR.TreatmentField
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                PlanId = 1,
                CalculatedDose = 200.0,
                Current = 1.0,
                DwellTime = 30,
                Energy = Core.Enums.Energy.Energy_50,
                Name = Core.Enums.TreatmentFieldName.Minus1L1
            };

            var protoType = ProtoTypesConverter.ToProto(treatmentField);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasPlanId, Is.True);
                Assert.That(protoType.PlanId, Is.EqualTo(treatmentField.PlanId));
                Assert.That(protoType.HasDwellTime, Is.True);
                Assert.That(protoType.DwellTime, Is.EqualTo(treatmentField.DwellTime));
            });

            // now try with valid Id:
            treatmentField.Id = 1;
            protoType = ProtoTypesConverter.ToProto(treatmentField);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(treatmentField.Id));
            });
        }

        [Test]
        public void FromProto_ITreatmentField_Test()
        {
            var protoType = new TreatmentField
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                PlanId = 1,
                CalculatedDose = 200.0,
                Current = 1.0,
                DwellTime = 30,
                Energy = ENERGY._100,
                FieldName = FIELDNAME.Plus0L1

            };
            var treatmentField = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(treatmentField.Id, Is.EqualTo(protoType.Id));
                Assert.That(treatmentField.PlanId, Is.EqualTo(protoType.PlanId));
                Assert.That(treatmentField.DwellTime, Is.EqualTo(protoType.DwellTime));
            });
        }

        [Test]
        public void ToProto_ITreatment_Test()
        {
            var treatment = new Application.Models.RDBMS.EMR.Treatment
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                PlanId = 1,
                CumulativeDose = 500.0,
                DailyDose = 100.0,
                Fraction = 1,
                LesionDepth = 1.2,
                PerformedBy = "test@test",
                VisitId = 2
            };

            var protoType = ProtoTypesConverter.ToProto(treatment);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.False);
                Assert.That(protoType.HasPlanId, Is.True);
                Assert.That(protoType.PlanId, Is.EqualTo(treatment.PlanId));
                Assert.That(protoType.HasPerformedBy, Is.True);
                Assert.That(protoType.PerformedBy, Is.EqualTo(treatment.PerformedBy));
            });

            // now try with valid Id:
            treatment.Id = 1;
            protoType = ProtoTypesConverter.ToProto(treatment);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(treatment.Id));
            });
        }

        [Test]
        public void FromProto_ITreatment_Test()
        {
            var protoType = new Treatment
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                PlanId = 1,
                CumulativeDose = 500.0,
                DailyDose = 100.0,
                Fraction = 1,
                LesionDepth = 1.2,
                PerformedBy = "test@test",
                VisitId = 2
            };
            var treatment = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(treatment.Id, Is.EqualTo(protoType.Id));
                Assert.That(treatment.PlanId, Is.EqualTo(protoType.PlanId));
                Assert.That(treatment.PerformedBy, Is.EqualTo(protoType.PerformedBy));
            });
        }

        [Test]
        public void ToProto_IUser_Test()
        {
            var user = new UserBindable {
                Id = BaseEntry.NEW_ENTRY_ID,
                FirstName = "John",
                MiddleName = string.Empty,
                LastName = "Doe",
                Username = "John",
                Role = new UserRole("Role"),
                Picture = string.Empty,
                EmailAddress = "johndoe@example.com",
                Password = "pwd",
            };
            var protoType = ProtoTypesConverter.ToProto(user.ToUser());
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasFirstName, Is.True);
                Assert.That(protoType.FirstName, Is.EqualTo(user.FirstName));
                Assert.That(protoType.HasLastName, Is.True);
                Assert.That(protoType.LastName, Is.EqualTo(user.LastName));
                Assert.That(protoType.Password, Is.EqualTo(user.Password));
                Assert.That(protoType.HasId, Is.False);
            });

            // now try with valid Id:
            user.Id = 1;
            protoType = ProtoTypesConverter.ToProto(user.ToUser());
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(user.Id));
            });
        }

        [Test]
        public void FromProto_IUser_Test()
        {
            var protoType = new Com.Empyreanmed.Heracles.Users.V1.User
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                LastAccessed = ProtoTypesConverter.ToTimestamp(DateTime.Now),
                FirstName = "John",
                MiddleName = string.Empty,
                LastName = "Doe",
                Username = "John",
                Password = "pwd",
                Role = "Role",
                Picture = string.Empty,
                EmailAddress = "johndoe@example.com",
            };
            var user = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(user.FirstName, Is.EqualTo(protoType.FirstName));
                Assert.That(user.LastName, Is.EqualTo(protoType.LastName));
                Assert.That(user.Password, Is.EqualTo(protoType.Password));
                Assert.That(user.Id, Is.EqualTo(protoType.Id));
            });
        }

        [Test]
        public void ToProto_ITreatmentDevice_Test()
        {
            var treatmentDevice = new Application.Models.RDBMS.EMR.TreatmentDevice
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Today,
                DeviceName = Core.Enums.DeviceType.GammaPutty,
                SimulationId = 1
            };
            var protoType = ProtoTypesConverter.ToProto(treatmentDevice);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasDeviceName, Is.True);
                Assert.That(protoType.DeviceName, Is.EqualTo(DEVICETYPE.GammaPutty));
                Assert.That(protoType.HasSimulationId, Is.True);
                Assert.That(protoType.SimulationId, Is.EqualTo(treatmentDevice.SimulationId));
                Assert.That(protoType.HasId, Is.False);
            });

            // now try with valid Id:
            treatmentDevice.Id = 1;
            protoType = ProtoTypesConverter.ToProto(treatmentDevice);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(treatmentDevice.Id));
            });
        }

        [Test]
        public void FromProto_ITreatmentDevice_Test()
        {
            var protoType = new TreatmentDevice
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                DeviceName = DEVICETYPE.IntraNasal,
                SimulationId = 2,
            };
            var device = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(device.DeviceName, Is.EqualTo(Heracles.Core.Enums.DeviceType.IntraNasal));
                Assert.That(device.SimulationId, Is.EqualTo(protoType.SimulationId));
                Assert.That(device.Id, Is.EqualTo(protoType.Id));
            });
        }

        [Test]
        public void ToProto_IPatientPosition_Test()
        {
            var position = new Application.Models.RDBMS.EMR.PatientPositionEntry
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Today,
                Position = Core.Enums.PatientPosition.Sitting,
                SimulationId = 1
            };
            var protoType = ProtoTypesConverter.ToProto(position);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.PatientPosition, Is.EqualTo(POSITION.Sitting));
                Assert.That(protoType.SimulationId, Is.EqualTo(position.SimulationId));
                //Assert.That(protoType.HasId, Is.False);
            });

            // now try with valid Id:
            position.Id = 1;
            protoType = ProtoTypesConverter.ToProto(position);
            Assert.Multiple(() =>
            {
                //Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(position.Id));
            });
        }

        [Test]
        public void FromProto_IPatientPosition_Test()
        {
            var protoType = new Position
            {
                Id = 1,
                CreateDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                PatientPosition = POSITION.LyingLt,
                SimulationId = 2,
            };
            var position = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(position.Position, Is.EqualTo(Heracles.Core.Enums.PatientPosition.LyingLT));
                Assert.That(position.SimulationId, Is.EqualTo(protoType.SimulationId));
                Assert.That(position.Id, Is.EqualTo(protoType.Id));
            });
        }

        [Test]
        public void ToProto_IPhoto_Test()
        {
            var photo = new Application.Models.RDBMS.EMR.PhotoDescription
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Today,
                VisitId = 1,
                Description = string.Empty,
                TemplateType = Core.Enums.TemplateType.Treatment,
                Type = Core.Enums.PhotoType.Identification
            };
            var protoType = ProtoTypesConverter.ToProto(photo);
            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasVisitId, Is.True);
                Assert.That(protoType.VisitId, Is.EqualTo(photo.VisitId));
                Assert.That(protoType.HasDescription, Is.True);
                Assert.That(protoType.Description, Is.EqualTo(photo.Description));
                Assert.That(protoType.HasId, Is.False);
            });

            // now try with valid Id:
            photo.Id = 1;
            protoType = ProtoTypesConverter.ToProto(photo);

            Assert.Multiple(() =>
            {
                Assert.That(protoType.HasId, Is.True);
                Assert.That(protoType.Id, Is.EqualTo(photo.Id));
            });
        }

        [Test]
        public void FromProto_IPhoto_Test()
        {
            var protoType = new Photo
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                VisitId = 1,
                Description = string.Empty,
                PhotoType = PHOTOTYPE.SimulationSetup,
                TemplateType = TEMPLATETYPE.Otv
            };
            var photo = ProtoTypesConverter.FromProto(protoType);
            Assert.Multiple(() =>
            {
                Assert.That(photo.VisitId, Is.EqualTo(protoType.VisitId));
                Assert.That(photo.Description, Is.EqualTo(protoType.Description));
                Assert.That(photo.Id, Is.EqualTo(protoType.Id));
            });

            var protoType2 = new Photo
            {
                Id = 1,
                CreationDate = ProtoTypesConverter.ToTimestamp(DateTime.Today),
                VisitId = 1,
                Description = string.Empty,
                PhotoType = PHOTOTYPE.SimulationSetup,
                //TemplateType = TEMPLATETYPE.Otv
            };
            Assert.Throws<InvalidCastException>(() => ProtoTypesConverter.FromProto(protoType2));
        }
    }
}
