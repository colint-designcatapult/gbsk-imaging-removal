using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Application.AppLayer.Patient;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess;
using Heracles.Application.Models;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using AppGlobals = Empyrean.Common.Application.Globals.AppGlobals;

namespace Heracles.Application.Helpers.DummyData
{
    public class DummyEmrData
    {
        public DummyEmrData(
            IEmrPatientCommands emrPatientCommands,
            IEmrDiagnosisCommands emrDiagnosisCommands,
            IEmrSimulationCommands emrSimulationCommands,
            IEmrVisitCommands emrVisitCommands,
            IEmrTreatmentDeviceCommands emrTreatmentDeviceCommands,
            IEmrPrescriptionCommands emrPrescriptionCommands,
            IEmrPlanCommands emrPlanCommands,
            IEmrTreatmentFieldCommands emrTreatmentFieldCommands,
            ILogWriter logWriter,
            IUserCommands emrUserCommands,
            IEmrTreatmentCommands emrTreatmentCommands,
            IEmrActualTreatmentFieldCommands emrActualTreatmentFieldCommands,
            IEmrPatientPositionCommands emrPatientPositionCommands,
            IEmrPhotoCommands emrPhotoCommands
            )
        {
            EmrPatientCommands = emrPatientCommands;
            EmrDiagnosisCommands = emrDiagnosisCommands;
            EmrSimulationCommands = emrSimulationCommands;
            EmrPrescriptionCommands = emrPrescriptionCommands;
            EmrPlanCommands = emrPlanCommands;
            EmrTreatmentFieldCommands = emrTreatmentFieldCommands;
            LogWriter = logWriter;
            EmrUserCommands = emrUserCommands;
            EmrTreatmentCommands = emrTreatmentCommands;
            EmrActualTreatmentFieldCommands = emrActualTreatmentFieldCommands;
            EmrPatientPositionCommands = emrPatientPositionCommands;
            EmrVisitCommands = emrVisitCommands;
            EmrTreatmentDeviceCommands = emrTreatmentDeviceCommands;
            EmrPhotoCommands = emrPhotoCommands;


            _photoService = new PhotoService(EmrPhotoCommands, _photoStreamReader, new AppGlobals());
        }

        public IEmrPatientCommands EmrPatientCommands { get; }
        public IEmrDiagnosisCommands EmrDiagnosisCommands { get; }
        public IEmrSimulationCommands EmrSimulationCommands { get; }
        public IEmrPrescriptionCommands EmrPrescriptionCommands { get; }
        public IEmrPlanCommands EmrPlanCommands { get; }
        public IEmrTreatmentFieldCommands EmrTreatmentFieldCommands { get; }
        public ILogWriter LogWriter { get; }
        public IUserCommands EmrUserCommands { get; }
        public IEmrTreatmentCommands EmrTreatmentCommands { get; }
        public IEmrActualTreatmentFieldCommands EmrActualTreatmentFieldCommands { get; }
        public IEmrPatientPositionCommands EmrPatientPositionCommands { get; }
        public IEmrPhotoCommands EmrPhotoCommands { get; }
        public IEmrVisitCommands EmrVisitCommands { get; }
        public IEmrTreatmentDeviceCommands EmrTreatmentDeviceCommands { get; }
        
        public async Task PopulateDB()
        {
            try
            {
                if (!DBIsEmpty())
                    return;

                var random = new Random(42);

                var users = await EmrUserCommands.ReadAllAsync();
                var savedUser = users.First();
                var patients = new List<IPatient>
                {
                    new Patient
                    {
                        FirstName = "John",
                        MiddleName = "Arb.",
                        LastName = "Doe",
                        DOB = new System.DateOnly(2000, 01, 01),
                        Address = "Some St.",
                        MRN = "111",
                        PatientId = "213-123-123",
                        PatientIdType = Xcc.Core.Enums.PatientIdType.Passport,
                        Sex = Xcc.Core.Enums.Sex.Male,
                        Status = PatientStatus.Active,
                        ProviderId = savedUser.EmailAddress,
                        Notes = "Allergy to metals"
                    },
                    new Patient
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        DOB = new System.DateOnly(2003, 06, 06),
                        Address = "Home Sweet Home",
                        PatientId = "3345-787-788",
                        PatientIdType = Xcc.Core.Enums.PatientIdType.Passport,
                        Sex = Xcc.Core.Enums.Sex.Female,
                        //LastVisit = DateTime.Now,
                        Phone = "+1 234 567 89",
                        Email = "janedoe@mail.example",
                        MRN = "111",
                        Status = PatientStatus.Inactive,
                        ProviderId = savedUser.EmailAddress
                    },
                    new Patient
                    {
                        FirstName = "Jerry",
                        LastName = "Foe",
                        DOB = new System.DateOnly(1987, 05, 04),
                        Address = "Sweet Home Alabama",
                        PatientId = "3456-789-101",
                        PatientIdType = Xcc.Core.Enums.PatientIdType.Passport,
                        Sex = Xcc.Core.Enums.Sex.Male,
                        Phone = "+1 321 765 43",
                        Email = "jerryfoe@mail.example",
                        MRN = "222",
                        Status = PatientStatus.Active,
                        ProviderId = savedUser.EmailAddress
                    },
                    new Patient
                    {
                        FirstName = "Mouse",
                        LastName = "White",
                        DOB = new System.DateOnly(2023, 06, 06),
                        Address = "Home Sweet Home",
                        City = "Boca Raton",
                        Country = "USA",
                        State = "FL",
                        Zip = "33486",
                        Sex = Xcc.Core.Enums.Sex.Intersex,
                        Phone = "+1 234 456 78",
                        Email = "mylittlemouse@mail.com",
                        MRN = "1234-456-78",
                        PatientId = "901293328",
                        PatientIdType = Xcc.Core.Enums.PatientIdType.Passport,
                        Status = PatientStatus.Expired,
                        ProviderId = savedUser.EmailAddress
                    }
                };

                foreach (var patient in patients)
                {
                    IPatient savedPatient = EmrPatientCommands.CreateAsync(patient).GetAwaiter().GetResult();

                    // Add a visit:
                    var visit = EmrVisitCommands.CreateAsync(new Visit()
                    {
                        CreationDate = DateTime.Now.AddDays(-2),
                        //Date = DateTime.Now.AddDays(-1),
                        PatientId = savedPatient.Id,
                        Type = VisitType.SkinCheck
                    }).GetAwaiter().GetResult();

                    // Adding sites:
                    const int diagnosesCount = 7;
                    for (int diagnosisIndex = 0; diagnosisIndex < diagnosesCount; ++diagnosisIndex)
                    {
                        var location = SiteLocation.Scalp; //(SiteLocation)(diagnosisIndex % 24 + 1);
                        var pathology = Pathology.Bcc;//(Pathology)(diagnosisIndex % 4 + 1);
                        IDiagnosis diagnosis = new Diagnosis()
                        {
                            CreationDate = DateTime.Now,
                            PatientId = savedPatient.Id,
                            SiteName = string.Format("Field {0}", diagnosisIndex),
                            SiteLocation = location,
                            Pathology = pathology,
                            Referring = string.Format("Reffering {0}", diagnosisIndex),
                            IcdCode = Core.Constants.IcdCodes.GetCode(location, pathology),
                            SubcellOne = Celltype.Aberrant,
                            SubcellTwo = Celltype.Acantholytic,
                            Description = Description.InfundibuloCytic
                        };
                        diagnosis = EmrDiagnosisCommands.CreateAsync(diagnosis).GetAwaiter().GetResult();

                        if (diagnosisIndex != diagnosesCount - 1) // leave the last diagnosis empty
                        {
                            bool isApproved = (diagnosisIndex % 4 < 2);
                            Status status = (isApproved) ? Status.APPROVED : Status.PENDING_APPROVAL;
                            SimulationStatus simulationStatus = (isApproved) ? SimulationStatus.Approved : SimulationStatus.Pending;

                            // Add a simulation:
                            ISimulation simulation = new Simulation()
                            {
                                CreationDate = diagnosis.CreationDate,
                                DiagnosisId = diagnosis.Id,
                                LesionDepth = 12.5d,
                                LesionSizeL = 9.8,
                                LesionSizeW = 5.9,
                                MarginSizeL = 0.1,
                                MarginSizeW = 0.12,
                                ShieldSizeL = 0.21,
                                ShieldSizeW = 0.22,
                                ApplicatorSize = 5.0,
                                VisitId = visit.Id,
                                PerformedBy = savedUser.EmailAddress,
                                Status = simulationStatus,
                                TargetType = TargetType.TargetType_50mm_SSD_15mm_Field,
                                SetupNote = "Allergy to metals"
                            };

                            simulation = EmrSimulationCommands.CreateAsync(simulation).GetAwaiter().GetResult();

                            // Add one or two treatment devices to the simulation:
                            var deviceTypes = Enum.GetValues<DeviceType>();
                            for (int deviceIdx = 0; deviceIdx < simulation.Id % 2 + 1; ++deviceIdx)
                            {
                                // Ensure that device types differ for idx = 0 and idx = 1:
                                long deviceTypeIdx = (simulation.Id + deviceIdx) % deviceTypes.Length;

                                // TODO: temporary workaround for the devices Moses isn't yet able to accept:
                                var type = deviceTypes[deviceTypeIdx];
                                if (type == DeviceType.PacemakerShield || type == DeviceType.CustomFabrication)
                                {
                                    type = DeviceType.GammaPutty;
                                }

                                ITreatmentDevice device = new TreatmentDevice(simulation.Id, type);
                                EmrTreatmentDeviceCommands.CreateAsync(device).GetAwaiter().GetResult();
                            }

                            // Add positions to the simulation:
                            var positions = Enum.GetValues<PatientPosition>();
                            for (int i = 0; i < simulation.Id % 2 + 1; ++i)
                            {
                                // Ensure that device types differ for idx = 0 and idx = 1:
                                long positionIdx = (simulation.Id + i) % positions.Length;

                                IPatientPosition position = new PatientPositionEntry(simulation.Id, positions[positionIdx]);
                                EmrPatientPositionCommands.CreateAsync(position).GetAwaiter().GetResult();
                            }

                            // Add a prescription:
                            Energy prescribedEnergy = (diagnosisIndex % 4 < 2) ? Energy.Energy_50 : Energy.Energy_70;
                            double doseRate = DummySystemData.DoseRateMap[prescribedEnergy];
                            double dailyDose = 50 * (diagnosisIndex % 3 + 1);
                            //// TODO: to be able to run on current demo firmware with 6 sec emission limitation:
                            //double dwellTime = 5;
                            double dwellTime = Math.Ceiling(dailyDose / doseRate * 60);

                            int numberOfFx = (diagnosisIndex % 2 + 1) * 3;
                            IPrescription prescription = new Prescription()
                            {
                                CreationDate = simulation.CreationDate,
                                SimulationId = simulation.Id,
                                Tdf = diagnosisIndex % 2 == 0 ? TDF.Tdf_98 : TDF.Tdf_100,
                                FxsPerWeek = diagnosisIndex % 2 + 1,
                                Energy = prescribedEnergy,
                                DwellTime = dwellTime,
                                DailyDose = dailyDose,
                                NumberOfFxs = numberOfFx,
                                Status = status,
                                MinTdf = TDF.Tdf_94,
                                TotalDose = dailyDose * numberOfFx
                            };

                            prescription = EmrPrescriptionCommands.CreateAsync(prescription).GetAwaiter().GetResult();

                            // Add a plan:
                            IPlan plan = new Plan()
                            {
                                CreationDate = DateTime.Now,
                                PrescriptionId = prescription.Id,
                                Status = (status == Status.APPROVED) ? PlanStatus.APPROVED : PlanStatus.PENDING_APPROVAL,
                                CollimatorType = /*(prescription.Id % 2 == 0) ? TargetType.TargetType_61_Fields :*/ TargetType.TargetType_50mm_SSD_15mm_Field,
                                TreatmentLoadingState = TreatmentLoadingState.Unloaded,
                                ApprovedBy = savedUser.EmailAddress
                            };

                            plan = EmrPlanCommands.CreateAsync(plan).GetAwaiter().GetResult();
                            // add treatment fields:
                            var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(plan.CollimatorType).ToArray();
                            IList<TreatmentFieldName> fieldNames = (fieldNameMapping.Count() > 1)
                                ? [
                                    fieldNameMapping.First().Value,
                                    fieldNameMapping[fieldNameMapping.Length / 2].Value,
                                    fieldNameMapping.Last().Value,
                                ]
                                : [fieldNameMapping.First().Value];

                            var treatmentFieldsList = new List<ITreatmentField>();
                            for (var i = 0; i < fieldNames.Count; i++)
                            {
                                ITreatmentField tf = new TreatmentField
                                {
                                    DwellTime = dwellTime,
                                    Name = fieldNames[i],
                                    PlanId = plan.Id,
                                    Energy = prescribedEnergy,
                                    Current = CurrentCalculator.CalculateCurrent(prescribedEnergy),
                                    CalculatedDose = doseRate * dwellTime / 60
                                };
                                treatmentFieldsList.Add(EmrTreatmentFieldCommands.CreateAsync(tf).GetAwaiter().GetResult());
                            }

                            // Add treatment history
                            int treatmentRecords = plan.Id % 2 == 0 ? 1 : 2;
                            double cumulativeDose = 0;
                            bool makeUncompleteTreatment = (plan.Id % 2 == 1);
                            for (int treatmentIndex = 1; treatmentIndex <= treatmentRecords; ++treatmentIndex)
                            {
                                // Add a treatment visit:
                                //var treatmentVisit = EmrVisitCommands.CreateAsync(new Visit()
                                //{                                 
                                //    PatientId = savedPatient.Id,
                                //    Type = VisitType.Treatment
                                //}).GetAwaiter().GetResult();

                                cumulativeDose += prescription.DailyDose;

                                var treatment = EmrTreatmentCommands.CreateAsync(
                                    new Treatment
                                    {
                                        PlanId = plan.Id,
                                        VisitId = visit.Id,
                                        CreationDate = DateTime.Now,
                                        Fraction = treatmentIndex,
                                        PerformedBy = savedUser.EmailAddress,
                                        LesionDepth = simulation.LesionDepth.Value,
                                        DailyDose = prescription.DailyDose,
                                        CumulativeDose = cumulativeDose,
                                    }).GetAwaiter().GetResult();

                                bool incompleteField = makeUncompleteTreatment && treatmentIndex == treatmentRecords;
                                foreach (var tf in treatmentFieldsList)
                                {
                                    EmrActualTreatmentFieldCommands.CreateAsync(
                                        new ActualTreatmentField(tf)
                                        {
                                            TreatmentId = treatment.Id,
                                            CreationDate = DateTime.Now,
                                            ActualDuration = (incompleteField) ? tf.DwellTime / 2 : tf.DwellTime,
                                            Completed = (incompleteField) ? 0 : 1,
                                            ActualCurrent = tf.Current,
                                            ActualDose = tf.CalculatedDose,
                                            ActualEnergy = (double)tf.Energy
                                        }).GetAwaiter().GetResult();
                                }
                            }

                            // Add some photos:
                            bool isEvenDiagnosis = diagnosis.Id % 2 == 0;
                            int startIndex = isEvenDiagnosis ? 0 : DummyPhotoStreamReader.PhotoPaths.Length / 2;
                            int finishIndex = isEvenDiagnosis ? 
                                DummyPhotoStreamReader.PhotoPaths.Length / 2 :
                                DummyPhotoStreamReader.PhotoPaths.Length;

                            var photos = new List<IPhoto>();

                            for (var i = startIndex; i < finishIndex; ++i)
                            {
                                var path = DummyPhotoStreamReader.PhotoPaths[i];
                                IPhotoDescription photoDescription = new PhotoDescription
                                {
                                    CreationDate = DateTime.Now,
                                    DiagnosisId = diagnosis.Id,
                                    Path = path,
                                    Type = PhotoType.SimulationSetup,
                                    VisitId = visit.Id,
                                    Description = "Photo description",
                                    Location = "Location",
                                    TemplateType = TemplateType.Simulation,
                                    Thumbnail = "Thumbnail"
                                };

                                photoDescription = EmrPhotoCommands.CreateAsync(photoDescription).GetAwaiter().GetResult();
                                photoDescription.Path = path;

                                var photo = _photoStreamReader.ReceivePhotoAsync(photoDescription, CancellationToken.None).GetAwaiter().GetResult();
                                photos.Add(photo);
                            }
                            _photoService.SendPhotosAsync(photos).GetAwaiter().GetResult();
                        }
                    }
                }
                //EmrPlanCommands.LoadForTreatmentAsync(1).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"Failed to PopulateDB. {ex.Message}. {ex.InnerException?.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Error);
            }
        }

        private bool DBIsEmpty()
        {
            ICollection<IPatient> list = null;
            list = Task<ICollection<IPatient>>.Run(() => EmrPatientCommands.ReadAllAsync()).GetAwaiter().GetResult();
            return list == null || list.Count == 0;
        }

        private DummyPhotoStreamReader _photoStreamReader = new();
        private IPhotoService _photoService;
    }
}
