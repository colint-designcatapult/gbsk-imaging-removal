using Heracles.Application.Common;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels
{
    public class CameraViewModel : RegionViewModelBase, INavigationAware
    {
        #region Properties

        public IDialogService DialogService { get; }
        public IHeraclesMainSettings Settings { get; }
        public ILogRepository LogWriter { get; }
        public IEmrPhotoCommands PhotoCommands { get; }
        public IPatientListModel PatientListModel { get; }
        public ITreatmentInfoStore TreatmentInfoStore { get; }

        private string _cameraUriSource;
        public string CameraUriSource
        {
            get => _cameraUriSource;
            set => SetProperty(ref _cameraUriSource, value);
        }

        private string _pathToDatabase;
        public string PathToDatabase
        {
            get => _pathToDatabase;
            set => SetProperty(ref _pathToDatabase, value);
        }
        #endregion Properties


        #region Commands

        private DelegateCommand<string> _screenshotCommand;
        public DelegateCommand<string> ScreenshotCommand => _screenshotCommand ??= new DelegateCommand<string>
            ((pathToScreenshot) =>
            {
                if (pathToScreenshot == null)
                {
                    DialogService.ReportError(StringConstants.Common.SaveErrorTitle, StringConstants.CameraView.PhotoSaveErrorMessage);
                    return;
                }

                //pathToScreenshot is a path to the captured frame. If it null - some error occured and frame was not created.
                Task.Run(async () =>
                {
                    const string modality = "";
                    var seriesId = await SaveNewSeries(modality);
                    if (seriesId == 0)
                        return;

                    int studyId = 1;
                    await SaveNewImage(studyId, pathToScreenshot, PhotoType.Identification); // todo: what type of Photo should be selected here?
                    studyId++;
                });
                
                ExitCommand.Execute();
            }); 

        #endregion Commands


        #region Constructors
        public CameraViewModel(
            IRegionManager regionManager,
            IDialogService dialogService,
            IHeraclesMainSettings settings,
            ILogRepository logWriter, 
            IEmrPhotoCommands photoCommands,
            IPatientListModel patientListModel,
            ITreatmentInfoStore treatmentInfoStore)
            :base(regionManager)
        {
            DialogService = dialogService;
            Settings = settings;
            LogWriter = logWriter;
            PhotoCommands = photoCommands;
            PatientListModel = patientListModel;
            TreatmentInfoStore = treatmentInfoStore;

            CameraUriSource = settings.CameraUriSource;

            PathToDatabase = settings.StorageRoot;
        }

        public CameraViewModel() : base(null) {}
        #endregion Constructors


        #region Private methods

        #endregion Private methods


        #region INavigationAware
        #endregion INavigationAware

        protected async Task<int> SaveNewSeries(string modality)
        {
            //string name = $"series-{DateTime.Now}";

            //double lesionDepth = SimulationModel?.Simulation?.LesionDepth ?? 0.0d;

            //long visitId = 0L;

            //if (SimulationModel.Simulation != null)
            //{
            //    visitId = SimulationModel.Simulation.VisitId;
            //}
            //else if (PatientInTreatment.Patient.Visits.Count > 0)
            //{
            //    visitId = PatientInTreatment.Patient.Visits.FirstOrDefault().Id;
            //}
            //else
            //{
            //    await LogService.LogAsync("Failed to save a new Series: unknown visit_id", LogRecordSeverity.Error, LogRecordType.Error);
            //// TODO: show error
            //    return 0;
            //}

            //// todo: initialize Series parameters with correct data
            //ISeries series = new Series()
            //{
            //    VisitId = visitId,
            //    LesionDepth = lesionDepth,
            //    DiagnosisId = SimulationModel.Diagnosis.Id,
            //    Name = name,
            //    Modality = modality
            //};

            //try
            //{
            //    var resultSeries = await SeriesCommands.CreateAsync(series);
            //    if (resultSeries != null)
            //    {
            //        LogService.Log($"New Series: [{resultSeries.Name}]", LogRecordSeverity.Info, LogRecordType.Database);
            //    }

            //    return (int)resultSeries.Id;
            //} 
            //catch (Exception ex) 
            //{
            //        LogService.Log($"Failed to save a new Series: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            //// TODO: show error
            //}

            return -1;
        }

        protected async Task SaveNewImage(int diagnosisId, string location, PhotoType photoType)
        {
            // Create a new visit or get a recent one (on the same day)
            var lastVisit = await PatientListModel.GetSameDayVisitAsync(TreatmentInfoStore.Patient, VisitType.Simulation);
            TreatmentInfoStore.Patient.Visit = lastVisit;

            // todo: initialize Image parameters with correct data
            IPhotoDescription image = new PhotoDescription
            {
                DiagnosisId = diagnosisId,
                Type = photoType,
                Description = "image description",
                Location = location, 
                VisitId = lastVisit.Id                
            };

            try
            {
                var result = await PhotoCommands.CreateAsync(image);
                if (result != null)
                {
                    LogWriter.Log($"New Image: [{result.Location}]", LogRecordSeverity.Info, LogRecordType.System);
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log($"{StringConstants.CameraView.NewImageSaveErrorMessage}: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
                DialogService.ReportError(StringConstants.CameraView.NewImageSaveErrorTitle, $"{StringConstants.CameraView.NewImageSaveErrorMessage}.");
            }
        }
    }
}
