using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Events;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Application.Models;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Heracles.Application.Models
{
    public interface ITreatmentFieldEntry : ITreatmentField, INotifyPropertyChanged
    {
        /// <summary>
        /// sec
        /// </summary>
        double Actual { get; set; }
        bool IsDone { get; set; }
    }

    public interface IPlanModel : IDirtyFlaggedBindableBase
    {
        IPrescription Prescription { get; }
        ICollimatorConfiguration? CollimatorConfiguration { get; }
        IPlan Plan { get; }
        TreatmentFieldEntryObservableCollection TreatmentFields { get; }
        ITreatmentFieldEntry TreatmentField { get; }

        Task<IPlan> OnUpdatePrescriptionAsync(IPrescription? prescription);

        Task<TreatmentFieldEntryObservableCollection> FetchTreatmentFieldsAsync();
        Task<IPlan> SubmitAsync();

        Task<IPlan> ChangeStatusAsync(string username, string password, PlanStatus planStatus);
        Task UnloadFromTreatmentAsync();
        IPlan OnDatabasePlanChanged(IPlan plan); 
        void ValidatePlanDosesAndEmissionCurrent();
        ICollimatorConfiguration GetCollimatorConfiguration(TargetType collimatorType, Energy energy);
        ITreatmentFieldEntry AddOrUpdateField(ITreatmentField fieldData);
        void AddOrUpdateTreatmentField(IPrescription prescription);
        ICollimatorConfiguration SetCollimatorConfiguration(TargetType collimatorType, Energy energy);
        void ShowVerifyDialog();
    }

    public class TreatmentFieldEntry : TreatmentField, ITreatmentFieldEntry
    {
        private double _actual = 0;
        private bool _isDone = false;

        public TreatmentFieldEntry(ITreatmentField field, int displayValue)
        {
            field?.CopyProperties(this);
            this.DisplayValue = displayValue;
        }

        // TODO: where these values should be?
        public double Actual { get => _actual; set => SetProperty(ref _actual, value); }
        public bool IsDone { get => _isDone; set => SetProperty(ref _isDone, value); }
    }

    public class TreatmentFieldEntryObservableCollection : ObservableCollection<ITreatmentFieldEntry>
    {
        public TreatmentFieldEntryObservableCollection(IEnumerable<ITreatmentFieldEntry> collection)
            :base(collection)
        {
        }
        public TreatmentFieldEntryObservableCollection()
            :base()
        {            
        }

        public bool ContainsField(Core.Enums.TreatmentFieldName name)
        {
            foreach (ITreatmentFieldEntry entry in this)
            {
                if (entry.Name == name)
                    return true;
            }

            return false;
        }

        public bool TryGetValue(TreatmentFieldName name, out ITreatmentFieldEntry fieldEntry)
        {
            foreach (var entry in this)
            {
                if (entry.Name == name)
                {
                    fieldEntry = entry;
                    return true;
                }
            }

            fieldEntry = null;
            return false;
        }

        public bool RemoveField(Core.Enums.TreatmentFieldName name)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Name == name)
                {
                    this.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }

    public class PlanModel : DirtyFlaggedBindableBase, IPlanModel
    {
        public const TreatmentFieldName DefaultTreatmentFieldName = TreatmentFieldName.PlusC;

        public PlanModel(
            ITreatmentInfoStore treatmentInfoStore,
            ICollimatorModel collimatorModel,
            IAppGlobals appGlobals,
            ILogWriter logWriter,
            IDialogService dialogService,
            IActionAuditService actionAuditService,
            IAuthorizedUserStore authorizedUserStore,
            ISimulationRepository simulationRepository,
            ITreatmentDoseCalculation treatmentDoseCalculation,
            Treatment.IPrescriptionRepository prescriptionRepository,
            IPlanRepository planRepository,
            IEventAggregator eventAggregator)
        {
            TreatmentInfoStore = treatmentInfoStore;
            CollimatorModel = collimatorModel;
            AppGlobals = appGlobals;
            LogWriter = logWriter;
            DialogService = dialogService;
            ActionAuditService = actionAuditService;
            AuthorizedUserStore = authorizedUserStore;
            SimulationRepository = simulationRepository;
            PrescriptionRepository = prescriptionRepository;
            PlanRepository = planRepository;
            EventAggregator = eventAggregator;
            TreatmentDoseCalculation = treatmentDoseCalculation;
            IsModified = false;
        }

        private IPlan _plan;
        private IPrescription _prescription;
        private ISimulation _simulation;
        private TreatmentFieldEntryObservableCollection _treatmentFields = new();
        private IDictionary<int, TreatmentFieldName> _fieldNameMapping = null;
        private CancellationTokenSource _cancellationTokenSource = null;

        // A configuration matching current energy/applicator size
        public ICollimatorConfiguration? CollimatorConfiguration { get; set; }

        public IPrescription Prescription { get => _prescription; private set => SetProperty(ref _prescription, value); }
        public ISimulation Simulation { get => _simulation; private set => SetProperty(ref _simulation, value); }
        public IPlan Plan
        {
            get => _plan;
            set {
                if (SetPropertyWithDirtyFlag(ref _plan, value))
                {
                    IsModified = false;

                    if (_plan == null)
                    {
                        _fieldNameMapping = null;
                    }
                    else
                    {
                        _fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(Plan?.CollimatorType ?? Core.Enums.TargetType.TargetType_None);

                        //if (BaseEntry.IsBlankEntry(plan))
                        //    IsModified = true;
                    }
                }
                // TODO: we should the plan in TreatmentInfoStore only
                // Now we just set current plan state to it so it could be used from outside:
                TreatmentInfoStore.Plan = _plan;
            }
        }

        private Dictionary<ITreatmentFieldEntry, OutgoingActionStateMachine> TreatmentFieldActions { get; } = new();

        public TreatmentFieldEntryObservableCollection TreatmentFields
        {
            get => _treatmentFields;
            private set => SetProperty(ref _treatmentFields, value);
        }

        public ITreatmentFieldEntry TreatmentField => TreatmentFields.FirstOrDefault();

        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public ICollimatorModel CollimatorModel { get; }
        public IAppGlobals AppGlobals { get; }
        public ILogWriter LogWriter { get; }
        public IDialogService DialogService { get; }
        public IActionAuditService ActionAuditService { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public ISimulationRepository SimulationRepository { get; }
        public Treatment.IPrescriptionRepository PrescriptionRepository { get; }
        public IPlanRepository PlanRepository { get; }
        public IEventAggregator EventAggregator { get; }
        public ITreatmentDoseCalculation TreatmentDoseCalculation { get; }

        //public TreatmentParametersValidationState TreatmentParametersValidationState { get; private set; }

        #region Public methods

        public void AddOrUpdateTreatmentField(IPrescription prescription)
        {
            // add one field for one-field collimators
            var treatmentField = new TreatmentField
            {
                Name = Application.Models.PlanModel.DefaultTreatmentFieldName,
                DwellTime = prescription.DwellTime
            };

            treatmentField.CalculatedDose = TreatmentDoseCalculation.CalculateDose(
                treatmentField.Name,
                CollimatorConfiguration,
                treatmentField.DwellTime);
            treatmentField.Current = CurrentCalculator.CalculateCurrent(prescription.Energy);
            treatmentField.Energy = prescription.Energy;

            AddOrUpdateField(treatmentField);
        }

        public ITreatmentFieldEntry AddOrUpdateField(ITreatmentField fieldData)
        {
            if (fieldData == null)
            {
                throw new ArgumentNullException(nameof(fieldData), StringConstants.EMR.TreatmentFieldIsNullMessage);
            }

            if (TreatmentFields.TryGetValue(fieldData.Name, out var existingFieldEntry))
            {
                if (existingFieldEntry.Id != Empyrean.Common.Core.Domain.DataManagement.Common.BaseEntry.NewEntryId)
                {
                    fieldData.Id = existingFieldEntry.Id;
                    return UpdateField(fieldData);
                }
                else
                {
                    // Need to remove a blank field that we're updating now to not have a duplicate
                    // TODO: use state machine properly instead, or refactor all this
                    DeleteField(existingFieldEntry);
                }
            }

            return AddField(fieldData);
        }

        public void ShowVerifyDialog()
        {
            DialogParameters parameters = new()
            {
                { "CurrentStatus", Plan.Status }
            };

            DialogService.ShowDialog("ApproveView", parameters, async (result) =>
            {
                if (result.Result is not ButtonResult.OK)
                    return;

                if (result.Parameters.TryGetValue<PlanStatus>("Status", out PlanStatus status) &&
                    result.Parameters.TryGetValue<string>("Username", out string username) &&
                    result.Parameters.TryGetValue<string>("Password", out string password))

                {
                    var plan = await ChangeStatusAsync(username, password, status);

                    // As status change affects prescription and simulation, we send this event to react on it,
                    // and SimulationViewModel is supposed to refetch all the data from scratch to get a consistent DB state
                    EventAggregator.GetEvent<PlanStatusChangedEvent>().Publish(plan);
                }
            });
        }

        private ITreatmentFieldEntry AddField(ITreatmentField fieldData)
        {
            if (fieldData == null)
            {
                throw new ArgumentNullException(nameof(fieldData), StringConstants.EMR.TreatmentFieldIsNullMessage);
            }

            fieldData.PlanId = Plan.Id;
            var fieldEntry = AddTreatmentField(fieldData, OutgoingActionType.Create);

            IsModified = true;

            return fieldEntry;
        }

        private void DeleteField(ITreatmentFieldEntry field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field), "Input field is null");
            }

            if (!TreatmentFields.Contains(field) || !TreatmentFieldActions.ContainsKey(field))
            {
                throw new ArgumentException("Cannot delete the treatment field");
            }

            TreatmentFieldActions[field].AddAction(OutgoingActionType.Delete);
            TreatmentFields.Remove(field);
            IsModified = true;
        }

        public ICollimatorConfiguration GetCollimatorConfiguration(TargetType collimatorType, Energy energy)
        {
            var collimatorConfiguration = CollimatorModel.FindConfigurationByType(collimatorType, energy);
            if (collimatorConfiguration is null)
            {
                var collimatorName = collimatorType.GetDisplayName();
                var energyName = energy.GetDisplayName();
                throw new Exception(
                    string.Format(StringConstants.EMR.MissingApplicatorConfigErrorMessageStringFormat,
                        collimatorName,
                        energyName));
            }

            return collimatorConfiguration;
        }

        public ICollimatorConfiguration SetCollimatorConfiguration(TargetType collimatorType, Energy energy)
        {
            return CollimatorConfiguration = GetCollimatorConfiguration(collimatorType, energy);
        }


        public async Task<IPlan> FetchLatestPlanAsync()
        {
            Plan = null;
            TreatmentFields.Clear();
            TreatmentFieldActions.Clear();

            if (Prescription == null)
            {
                return null;
            }

            if (BaseEntry.IsBlankEntry(Prescription))
            {
                Plan = MakeNewBlankPlan();
                return Plan;
            }

            IsValid = false;
            var plan = await PlanRepository.FetchLatestPlanAsync(Prescription.Id);
            if (plan == null)
            {
                plan = MakeNewBlankPlan();
            }

            if (Prescription?.Energy is null)
                throw new NullReferenceException(StringConstants.EMR.EnergyNotSet);

            CollimatorConfiguration = GetCollimatorConfiguration(plan.CollimatorType, Prescription.Energy);

            if (plan.CollimatorType != Simulation.TargetType)
            {
                SetBlankPlan();
                //throw new Exception(StringConstants.EMR.PlanCollimatorDoesNotMatchSimulation);
            }

            Plan = plan;

            await FetchTreatmentFieldsAsync();

            return Plan;
        }

        public async Task<TreatmentFieldEntryObservableCollection> FetchTreatmentFieldsAsync()
        {
            if (Prescription?.Energy is null)
                throw new NullReferenceException(StringConstants.EMR.EnergyNotSet);

            TreatmentFields.Clear();
            TreatmentFieldActions.Clear();
            if (Plan != null && !BaseEntry.IsBlankEntry(Plan))
            {
                var fields = await PlanRepository.FetchOrderedTreatmentFieldsAsync(Plan.Id);
                // TODO: workaround - if we have more than one field, we keep only first one and remove all the others
                if (fields.Count > 1)
                {
                    fields = fields.Take(1).ToList();
                }

                foreach (var field in fields)
                {
                    // todo: why do we update the energy here?
                    field.Energy = Prescription.Energy; // TODO: verify this, as we just need to have actual plan to be updated according to its prescription
                    AddTreatmentField(field);
                }
                IsValid = false; // consider invalid until explicit validation
            }

            IsModified = false;
            return TreatmentFields;
        }

        public async Task<IPlan> OnUpdatePrescriptionAsync(IPrescription? prescription)
        {
            var previousPrescriptionValue = Prescription;
            Prescription = prescription;

            var previousSimulationValue = Simulation;
            Simulation = TreatmentInfoStore.Simulation;

            bool isSamePrescription = Prescription != null &&
                                      previousPrescriptionValue != null &&
                                      Prescription.Id == previousPrescriptionValue.Id;

            if (isSamePrescription)
            {
                if (Prescription.Status != previousPrescriptionValue.Status &&
                    previousSimulationValue.TargetType == Simulation.TargetType)
                {
                    // Just a status change, nothing to do here
                    return Plan;
                }

                var targetTypeChanged = previousSimulationValue.TargetType != Simulation.TargetType;

                if (targetTypeChanged)
                {
                    SetBlankPlan();
                }
                else
                {
                    await FetchLatestPlanAsync();

                    //// If the user just changed the prescribed doses and applicator type,
                    //// then we just remove all existing fields and try getting proper applicator
                    //if (_prescription.Energy != previousPrescriptionValue.Energy ||
                    //    _prescription.DailyDose != previousPrescriptionValue.DailyDose)
                    //{
                    //    if (TreatmentFields is not null)
                    //    {
                    //        var treatmentFieldsToRemove = new List<ITreatmentFieldEntry>(TreatmentFields);
                    //        foreach (var field in treatmentFieldsToRemove)
                    //        {
                    //            DeleteField(field);
                    //        }
                    //        // We want to apply these changes right away
                    //        // to not allow user getting invalid fields back by plan refresh
                    //        // TODO: we need to make it more robust in future
                    //        // and block on repeating this task until it succeeds
                    //        await SubmitAsync();
                    //    }
                    //}
                }
            }
            else
            {
                await FetchLatestPlanAsync();
            }

            return Plan;
        }

        public async Task<IPlan> SubmitAsync()
        {
            if (Plan.Status == PlanStatus.APPROVED)
            {
                throw new InvalidOperationException("Failed to submit a verified plan");
            }

            // save plan if necessary
            if (BaseEntry.IsBlankEntry(Plan))
            {
                Plan = await PlanRepository.CreatePlanAsync(Plan);
                ActionAuditService.RegisterAction($"New plan with id={Plan.Id} was created");

                // Update plan Id in the fields with actual value:
                foreach (var field in TreatmentFields)
                {
                    field.Plan = Plan;
                    field.PlanId = Plan.Id;
                }
            }
            //else if (Plan.Status == PlanStatus.APPROVED)
            //{
            //    // TODO: probably need to remove this and raise an exception,
            //    // as we can't and shouldn't do unapprove this way anymore:
            //    // Need to move plan state to pending, as we make changes and will need to approve the plan again
            //    IPlan updatedPendingPlan = new Plan(Plan) { Status = PlanStatus.PENDING_APPROVAL };
            //    Plan = await PlanRepository.UpdatePlanAsync(Plan, updatedPendingPlan);
            //}

            foreach (var field in TreatmentFieldActions)
            {
                //submit a field if necessary and reset or remove its state in the dict

                if (field.Value.Action == OutgoingActionType.Create)
                {
                    var createdField = await PlanRepository.CreateTreatmentFieldAsync(field.Key);
                    createdField.CopyProperties(field.Key);
                    ActionAuditService.RegisterAction($"Create treatment field id={createdField.Id} in plan id={Plan.Id}");
                }
                else if (field.Value.Action == OutgoingActionType.Update)
                {
                    var updatedField = await PlanRepository.UpdateTreatmentFieldAsync(null, field.Key);
                    updatedField.CopyProperties(field.Key);
                    ActionAuditService.RegisterAction($"Update treatment field id={updatedField.Id} in plan id={Plan.Id}");
                }
                else if (field.Value.Action == OutgoingActionType.Delete)
                {
                    await PlanRepository.DeleteTreatmentFieldAsync(field.Key.Id);
                    ActionAuditService.RegisterAction($"Delete treatment field id={field.Key.Id} in plan id={Plan.Id}");
                }
            }

            // We need to reset treatment field actions now,
            // and build a new one, for the rest of the fields (except for removed ones):
            // TODO: repetitive cleanup/reset, refactor this piece:
            TreatmentFieldActions.Clear();
            // Add list of blank actions for all treatment fields
            foreach (var field in TreatmentFields)
            {
                TreatmentFieldActions.Add(field, new OutgoingActionStateMachine());
            }

            if (TreatmentFields.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine($"PlanModel.SubmitAsync: too many treatment fields - {TreatmentFields.Count}");
            }

            IsModified = false;
            return Plan;
        }

        public async Task<IPlan> ChangeStatusAsync(string username, string password, PlanStatus planStatus)
        {
            return planStatus switch
            {
                PlanStatus.PENDING_APPROVAL => await UpdateStatusAsync(username, password, PlanStatus.PENDING_APPROVAL),
                PlanStatus.APPROVED => await UpdateStatusAsync(username, password, PlanStatus.APPROVED),
                PlanStatus.REJECTED => await UpdateStatusAsync(username, password, PlanStatus.REJECTED),
                _ => throw new ArgumentException($"ChangeStatusAsync: Plan status {planStatus} is not supported.")
            };
        }

        private async Task<IPlan> UpdateStatusAsync(string username, string password, PlanStatus status)
        {
            if (Plan.Status != status)
            {
                if (!BaseEntry.IsBlankEntry(Plan))
                {
                    var plan = await PlanRepository.UpdateStatusAsync(username, password, Plan.Id, status);
                    // todo: temporarily check Plan for null, because Moses does not return an updated one
                    if (plan == null)
                    {
                        ActionAuditService.RegisterAction($"Change status of plan id={Plan.Id} to {status}");
                        plan = new Plan(Plan) { Status = status };
                    }
                    Plan = plan;
                }
                else
                    throw new InvalidOperationException("Cannot verify unsaved Plan");
            }
            return Plan;
        }

        public async Task UnloadFromTreatmentAsync()
        {
            if (Plan == null)
                throw new NullReferenceException("There is no plan to unload");

            if (Plan.TreatmentLoadingState.Equals(TreatmentLoadingState.PendingLoad)
                || Plan.TreatmentLoadingState.Equals(TreatmentLoadingState.PartialPendingLoad))
            {
                await PlanRepository.UnloadFromTreatmentAsync(Plan.Id);
            }
        }

        public IPlan OnDatabasePlanChanged(IPlan plan)
        {
            if (plan == null)            
                return Plan;
            
            // It is either a blank was saved,
            // or it is the same plan got updated:
            if ((BaseEntry.IsNullOrBlankEntry(Plan) && Prescription.Id == plan.PrescriptionId) || 
                Plan?.Id == plan.Id)
            {
                Plan = plan;
            }
            return Plan;
        }

        public void ValidatePlanDosesAndEmissionCurrent()
        {
            IsValid = ValidateDosesAndEmissionCurrent(TreatmentFields);
        }

        public bool ValidateDosesAndEmissionCurrent(IEnumerable<ITreatmentFieldEntry> fields)
        {
            bool isActualDoseValid = false;
            bool isCurrentValid = false;

            if (CollimatorConfiguration is null)
                throw new NullReferenceException("Applicator configuration is not loaded");

            if (Prescription?.Energy is null)
                throw new NullReferenceException(StringConstants.EMR.EnergyNotSet);

            var validationResult = fields.All(
                       field => {
                           double actualCalculatedDose = TreatmentDoseCalculation.CalculateDose(field.Name, CollimatorConfiguration, field.DwellTime);
                           double actualEmissionCurrent = CurrentCalculator.CalculateCurrent(Prescription.Energy);
                           // We use 0.1 threshold here, as the DB now stores doses in decimal with 2 digits after comma
                           
                           isActualDoseValid = Math.Abs(actualCalculatedDose - field.CalculatedDose) < 0.1;
                           isCurrentValid = Math.Abs(actualEmissionCurrent - field.Current) < 0.01;
                           return isActualDoseValid && isCurrentValid;
                       });

            //TreatmentParametersValidationState = new TreatmentParametersValidationState
            //{
            //    IsActualDoseValid = isActualDoseValid,
            //    IsCurrentValid = isCurrentValid
            //};

            return validationResult;
        }

        #endregion

        #region Private methods
        private ITreatmentFieldEntry UpdateField(ITreatmentField fieldData)
        {
            if (TreatmentFields.RemoveField(fieldData.Name))
            {
                ITreatmentFieldEntry entryToRemove = null;
                foreach (var action in TreatmentFieldActions)
                {
                    if (action.Key.Name == fieldData.Name)
                    {
                        entryToRemove = action.Key;
                        break;
                    }
                }

                if (entryToRemove != null)
                    TreatmentFieldActions.Remove(entryToRemove);
            }
            else
            {
                throw new InvalidOperationException(string.Format(StringConstants.EMR.TreatmentFieldNotExistStringFormat, fieldData.Name));
            }

            fieldData.PlanId = Plan.Id;
            var fieldEntry = AddTreatmentField(fieldData, OutgoingActionType.Update);
            IsModified = true;

            return fieldEntry;
        }

        private ITreatmentFieldEntry AddTreatmentField(
            ITreatmentField safeFieldData, 
            OutgoingActionType action = OutgoingActionType.None)
        {
            var displayValue = TargetTypeConverter.GetBackwardFieldNameMapping(_fieldNameMapping, safeFieldData.Name);
            var fieldEntry = new TreatmentFieldEntry(safeFieldData, displayValue)
            {
                Actual = 0
            };
            //fieldEntry.PropertyChanged += OnTreatmentFieldChanged;
            
            TreatmentFields.Add(fieldEntry);
            TreatmentFieldActions.Add(fieldEntry, new OutgoingActionStateMachine(action));

            return fieldEntry;
        }
        
        private void OnTreatmentFieldChanged(object sender, PropertyChangedEventArgs e)
        {
            // Set dirty flag if one of the entries changes
            var field = sender as TreatmentFieldEntry;
            if (field != null && TreatmentFields.Contains(field))
            {
                TreatmentFieldActions[field].AddAction(OutgoingActionType.Update);
                IsModified = true;
            }
        }

        private void SetCollimatorConfiguration()
        {
            if (Prescription?.Energy is null)
                throw new NullReferenceException($"SetCollimatorConfiguration: {StringConstants.EMR.EnergyNotSet}");

            CollimatorConfiguration = CollimatorModel.FindConfigurationByType(Simulation.TargetType, Prescription.Energy);
            if (CollimatorConfiguration is null)
            {
                Plan = null; // To reset displayed plan/applicator if we don't have a plan anymore
                throw new NullReferenceException(StringConstants.EMR.CannotFindApplicatorConfigErrorMessage);
            }
        }

        private IPlan SetBlankPlan()
        {
            TreatmentFields.Clear();
            TreatmentFieldActions.Clear();

            var plan = MakeNewBlankPlan();

            SetCollimatorConfiguration();

            return Plan = plan;
        }

        private IPlan MakeNewBlankPlan()
        {
            return new Plan
            {
                CreationDate = DateTime.Now,
                PrescriptionId = Prescription.Id,
                Status = PlanStatus.PENDING_APPROVAL,
                CollimatorType = TreatmentInfoStore.Simulation?.TargetType ?? TargetType.TargetType_None,
                TreatmentLoadingState = TreatmentLoadingState.Unloaded,
                //ApprovedBy = AuthorizedUser.Id
                ApprovedBy = AuthorizedUserStore.AuthorizedUser.EmailAddress
            };
        }


        #endregion Private methods
    }

    //public struct TreatmentParametersValidationState
    //{
    //    public TreatmentParametersValidationState(bool stateForAll)
    //    {
    //        IsCurrentValid = stateForAll;
    //        IsActualDoseValid = stateForAll;
    //    }

    //    public bool IsCurrentValid { get; set; }
    //    public bool IsActualDoseValid { get; set; }
    //}
}


