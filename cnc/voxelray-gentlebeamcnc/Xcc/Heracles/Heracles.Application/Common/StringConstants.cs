using System;
using System.ComponentModel.DataAnnotations;

namespace Heracles.Application.Common
{
    public static class StringConstants
    {
        public const string GetDisplayAttributeErrorMessage = $"Failed to get {nameof(DisplayAttribute)} attribute for {{0}}.";

        public static class Common
        {
            public const string ErrorTitle = "Error";
            public const string ConfirmationDialogTitle = "Confirmation";
            public const string SaveErrorTitle = "Save Error";
            public const string DeleteDialogTitle = "Delete";
            public const string ReloadDialogTitle = "Reload";
            public const string DatabaseErrorTitle = "Database connection error";
        }

        public static class Authentication
        {
            public const string VerifyDialogTitle = "Verify";
        }

        public static class SystemSettings
        {
            public static class Validation
            {
                // Applicator Management:
                public const string NoInstalledApplicator = "No installed applicator. Please install a applicator.";
                public const string ApplicatorTypeRequired = "Applicator Size is required";
                public const string ApplicatorEnergyRequired = "Applicator Energy is required";
            }

            public const string SettingsTitle = "Settings";
            public const string RestartOnSaveNotification = "Please restart the software to apply the changes";

            public const string SettingsSaveErrorMessage = "Failed to save the settings.";

            public const string DeviceSerialIdCheckErrorTitle = "System Settings Error";
            public const string DeviceSerialIdCheckError = "Cannot check the device serial ID";

            // User Management:
            public const string FetchUserListErrorMessage = "Failed to load user list from the database. See details in the log.";
            public const string SaveUserErrorMessage = "Failed to save user record to the database. See details in the log.";
            public const string UserDeleteConfirmationMessage = "Are you sure you want to delete the user?";
            public const string DeleteUserErrorMessage = "Failed to remove the user record from the database. See details in the log.";

        }

        public static class MainTabs
        {
            public static readonly string TabSwitchUnsavedChangesWarning = $"There are unsaved changes on this tab.{Environment.NewLine}Are you sure you want to exit?";
            public static readonly string ApplicationExitConfirmationTitle = Xcc.Core.Constants.StringConstants.ConfirmExitHeader;
            public static readonly string ApplicationExitConfirmationMessage = Xcc.Core.Constants.StringConstants.ConfirmExitMessage;
        }

        public static class CameraView
        {
            public static readonly string PhotoSaveErrorMessage = "Failed to save a new photo: path error";

            public static readonly string NewImageSaveErrorTitle = "Save Image";
            public static readonly string NewImageSaveErrorMessage = "Failed to save a new image from camera";
        }

        public static class EMR
        {
            public static class Validation
            {
                // EMR Field:
                public const string FieldNameRequired = "Field Name is required";
                public const string PathologyRequired = "Pathology is required";
                public const string SiteLocationRequired = "Site Location is required";
                public const string SubcellRequired = "Subcell is required";
                public const string FieldDescriptionRequired = "Description is required";

                // Prescription:
                public const string FractionsPerWeekRequired = "Fractions Per Week is required";
                public const string TdfRequired = "TDF is required";
                public const string DailyDoseRequired = "Daily Dose is required";
                public const string DailyDoseMustBeNonZero = "Daily Dose cannot be 0";
                public const string NumberOfFractionsRequired = "Number of Fractions is required";
                public const string NumberOfFractionsMustBeNonZero = "Number of Fractions cannot be 0";
                public const string EnergyRequired = "Energy is required";
                public const string DurationMustBeGreaterZero = "Duration must be greater than 0";

                // Simulation:
                public const string ApplicatorSizeRequired = "Applicator Size is required";
                public const string LesionDepthMustBeNonZero = "Lesion Depth cannot be 0";
                public const string LesionSizeLRequired = "Lesion Size L is required";
                public const string LesionSizeLMustBeNonZero = "Lesion Size L cannot be 0";
                public const string LesionSizeWRequired = "Lesion Size W is required";
                public const string LesionSizeWMustBeNonZero = "Lesion Size W cannot be 0";
                public const string MarginSizeRequired = "Margin Size is required";
                public const string MarginSizeMustBeNonZero = "Margin Size cannot be 0";
                public const string ShieldSizeLengthRequired = "Shield Size length is required";
                public const string ShieldSizeLengthMustBeNonZero = "Shield Size length cannot be 0";
                public const string ShieldSizeWidthRequired = "Shield Size width is required";
                public const string ShieldSizeWidthMustBeNonZero = "Shield Size width cannot be 0";
                public const string TreatmentDevicesRequired = "Treatment devices are required";
                public const string TreatmentDevicesMustNotBeEmpty = "At least one treatment device must be added";
                public const string PatientPositionsRequired = "Patient positions are required";
                public const string PatientPositionsMustNotBeEmpty = "At least one patient position must be added";

            }

            /// Patients
            public const string PatientListErrorTitle = "Patient List Error";
            public const string PatientListFetchError = "Failed to load the patient list.";

            public const string PatientAlreadyExistsErrorTitle = "Patient Already Exists";
            public const string PatientAlreadyExistsErrorMessage = "There is a patient with the same first name, last name, sex, and DOB in the database";

            public const string PatientSaveErrorMessage = "Failed to save the patient.";

            public const string SaveNewPatientAuditLogMessage = "Create a new patient";
            public const string SaveExistingPatientAuditLogMessage = "Update data for the patient";
            public const string SavePatientIsDoneAuditLogMessage = "Saved data for the patient";

            /// Clinical View

            public const string FieldListTitle = "Field list";
            public const string TreatmentFieldIsNullMessage = "Input treatment field value is null";
            public const string TreatmentFieldNotExistStringFormat = "Treatment field {0} does not exist in the list";
            public const string FetchFieldsMessage = "Failed to load the field list.";
            public const string SaveFieldMessage = "Failed to save the field.";
            public const string ArchiveFieldConfirmation = "Do you want to archive the field?";

            public const string FetchPrescriptionMessage = "Failed to load the prescription.";
            public const string SavePrescriptionMessage = "Failed to save the prescription.";
            public const string PrescriptionChangeDiscardPlanConfirmation = "Change the prescription? This will discard the plan.";
            public const string PrescriptionChangeConfirmation = "Change the prescription?";

            public const string FetchSimulationMessage = "Failed to load the simulation.";
            public const string SaveSimulationMessage = "Failed to save the simulation.";
            public const string SimulationChangeConfirmation = "Change the simulation? This will update the prescription.";

            public const string LeaveClinicalViewUnsavedChangesError = "Save all the changes first";

            public const string LeaveClinicalViewDiscardChangesConfirmationTitle = "Unsaved data will be lost";
            public const string LeaveClinicalViewDiscardChangesConfirmationMessage = "Do you want to proceed and discard the changes?";

            public const string PlanLoadErrorDialogTitle = "Failed to Load the Plan";
            public const string PlanTreatmentFactorsMissingDataError = "Applicator calibration data is missing";
            public const string PlanTreatmentFactorsMissingActiveApplicatorError = "Active applicator configuration is missing"; // details for missing data
            public const string PlanTreatmentFactorsMissingActiveApplicatorPresetError = "Active applicator preset is missing"; // details for missing data

            public static readonly string PlanNoApprovedPresetErrorTitle = "Treatment Physics Verification Error";
            public static readonly string PlanNoApprovedPresetErrorMessage = $"Failed to load the plan for a treatment.{Environment.NewLine}Approved calibration data is required.";

            public const string PlanDwellTimeLimitExceededErrorTitle = "Treatment field duration is out of range";
            public const string PlanDwellTimeLimitExceededErrorMessage = "Please set the fields duration to less than 300 seconds";

            public const string PlanAddTreatmentFieldErrorMessage = "Failed to add Treatment Field. See log for details.";
            public static readonly string PlanApplicatorDoesNotMatchSimulation = $"Plan applicator size does not match Simulation.{Environment.NewLine}A new Plan is created";

            // Acknowledge Simulation and Prescription dialog messages
            public const string AcknowledgePrescriptionUiMessage = "Please, select why 'Number of Fx' is changing";
            public const string AcknowledgePrescriptionMessage = "Acknowledge of changing of 'Number of Fx'";
            public const string AcknowledgeSimulationUiMessage = "Please, select why 'Lesion Depth' is not set";
            public const string AcknowledgeSimulationMessage = "Acknowledge of saving simulation without 'Lesion Depth'";

            // The following two constants are defined in XAML, but should belong to here, to have everything in one place
            public const string PlanPendingLoadText = "Treatment Plan was sent to the Treatment Console for radiation delivery";
            public const string PlanLoadedText = "Treatment Plan loaded to the Treatment Console for radiation delivery";

            // This Plan load error should not appear in normal workflow, as the actions should be blocked:
            public const string PlanNoActiveApplicatorError = "No applicator is installed"; // with PlanLoadErrorDialogTitle

            public const string PlanValidationErrorTitle = "Plan Error";
            public static readonly string PlanValidationErrorMessage = $"This plan's fields contain invalid values.{Environment.NewLine}They were calculated using other emission power or dose rate.";
            public static readonly string PlanValidationErrorConfirmation = $"{PlanValidationErrorMessage}{Environment.NewLine}Do you want to recalculate the duration?";
            public const string DailyDoseNotSetErrorMessage = "Daily dose is not set";
            public const string PlanWithoutFieldsErrorMessage = "Plan does not contain any fields";

            public const string PrescriptionTitle = "Prescription"; 
            public const string AdjustPrescriptionTitle = "Adjust Prescription"; 
            public const string UnapprovePrescriptionTitle = "Unapprove Prescription"; 
            public const string PrescriptionErrorTitle = "Prescription Error"; 
            public const string PrescriptionRepositoryPrescriptionCantBeNull = "Prescription repository error: prescription can't be null"; 
            public static readonly string PrescriptionValidationErrorMessage = $"The prescription contains values, that do not match the configuration.{Environment.NewLine}They were calculated using other emission power or dose rate.";
            public static readonly string PrescriptionValidationErrorConfirmation = $"{PrescriptionValidationErrorMessage}{Environment.NewLine}Do you want to recalculate them?";
            public static readonly string PrescriptionValidationErrorUnapproveConfirmation = $"{PrescriptionValidationErrorMessage}{Environment.NewLine}Do you want to unapprove the plan and recalculate them?";


            public const string PlanDataFetchErrorMessage = "Failed to load plan data"; // with RefreshDialogTitle

            public const string PlanVerificationErrorTitle = "Plan Verification Error";
            public const string PlanVerificationError = "Failed to verify the plan";

            public const string PlanUnloadFromTreatmentError = "Cannot unload the plan";
            public const string PlanUnloadFromTreatmentErrorLogMessage = "Unload plan error";

            public const string PlanLoadNoFractionsLeftErrorTitle = "No Treatment Fractions Left";
            public const string PlanLoadNoFractionsLeftErrorMessage = "The maximum number of prescribed fractions have been delivered";

            public const string PlanLoadForTreatmentDialogTitle = "Load for Treatment";
            public const string PendingPlanAlreadyExistsInfo = "A pending plan already exists";

            public const string PlanLoadForTreatmentError = "Failed to load the plan for treatment";

            public const string PlanTreatmentConfirmationTitle = "Treatment Confirmation";
            public const string PlanTreatmentInLessThan12hrsConfirmation = "The last treatment on this patient was performed less than 12 hours ago. Proceed anyway?";

            public const string PlanTreatmentFieldUpdateMissingConfigurationError = "Applicator configuration is missing";

            public const string PlanTreatmentFieldDeleteError = "Failed to delete the treatment field";

            public const string PlanReloadError = "Failed to reload the plan";

            public const string PrescriptionError = "Prescription Error";
            public const string PrescriptionValidationError = "Failed to validate / recalculate prescription data";
            public const string CannotCreatePrescriptionNoSimulationError = "Cannot create a prescription: there is no simulation";
            public const string CannotFindApplicatorConfigErrorMessage = "Cannot find current applicator configuration";
            public const string MissingApplicatorConfigErrorMessageStringFormat = "The specified applicator configuration is missing: {0} / {1}kv";
            public const string FailedToDetermineOutputFactorForTreatmentFieldStringFormat = "Failed to determine output factor for TreatmentField {0}";
            public const string FailedToDetermineOutputFactorMessage = "Failed to determine output factor value";
            public const string EnergyNotSet = "Energy is not set";
            public const string TreatmentSummaryNotAvailable = "Please, select a field with a prescription to display treatment information";
        }

        public static class QualityCheck
        {
            public const string NoApplicatorConfigurationsError = "Failed to select available applicator configurations";
            public const string QcMissingConfigurationError = "Applicator configuration is missing";
        }

        public static class ClinicalData
        {
            public const string ApplicatorWithoutSerialUiMessage = "The connected applicator has no serial";
            public const string ApplicatorMismatchUiMessage = "The connected applicator with serial {0} does not match the simulation applicator size ({1}) or energy ({2} kV)";
            public const string NoConnectedApplicatorUiMessage = "No connected applicator. Expected applicator size: {0}, energy: {1} kV";
            public const string ApplicatorCheckErrorMessage = "Check for applicator mismatch failed:";
            public const string ApplicatorRequirementsUiErrorMessage = "Plan has incomplete applicator requirements:{0} applicator size: {1}, energy: {2} kV";
        }


        public static class TreatmentConsole
        {
            public const string TreatmentTitle = "Treatment";
            public const string TryAgainMessage = "Try again in a few seconds.";
            public const string DatabaseErrorMessage = "Failed to connect to the database.";

            public const string FetchPatientDataMessage = "Failed to load patient data.";

            public const string PlanStatusTitle = "Plan Status";
            public const string AcknowledgePlanStatusLogMessage = "Failed to update plan status.";
            public static readonly string AcknowledgePlanStatusErrorMessage = $"{AcknowledgePlanStatusLogMessage}{Environment.NewLine}{DatabaseErrorMessage}{Environment.NewLine}{TryAgainMessage}";

            public const string UnloadPlanLogMessage = "Failed to unload the plan from treatment.";
            public static readonly string UnloadPlanErrorMessage = $"{UnloadPlanLogMessage}{Environment.NewLine}{DatabaseErrorMessage}{Environment.NewLine}{TryAgainMessage}";

            public const string LoadForTreatmentInitLogMessage = "Failed to load plan for treatment.";
            public static readonly string LoadForTreatmentInitErrorMessage = $"{LoadForTreatmentInitLogMessage}{Environment.NewLine}{DatabaseErrorMessage}{Environment.NewLine}{TryAgainMessage}";

            public static readonly string PlanRecoveryOldPreviousTreatmentConfirmation = $"The treatment for the plan was created more than 12 hours ago.{Environment.NewLine}Do you want to proceed anyway?";

            public static readonly string PlanResumePreviousTreatmentConfirmation = $"The previous treatment was not completed.{Environment.NewLine}Resume the delivery of the remaining dose?";

            public const string PlanRecoveryInfoTitle = "Plan Recovery";
            public const string PlanRecoveryIncompletePlanExecutionInfo = "Plan was not completed";

            public const string PlanRecoveryErrorTitle = "Plan Recovery Error";
            public const string PlanRecoveryErrorMessage = "Failed to recover the plan";

            public const string InvalidPlanApplicatorConfigurationErrorTitle = "Plan Loading Error";
            public const string InvalidHeaterCurrentConfiguration = "Invalid heater current configuration"; // need to add details format:  value={heaterCurrent} is out of range {CurrentRange.HeaterCurrentMin}..{CurrentRange.HeaterCurrentMax}

            public const string TreatmentPlanCompletionConfirmationTitle = "Treatment Delivery Completed";
            public static readonly string TreatmentPlanCompletionConfirmationMessage = $"All operational points were delivered successfully.{Environment.NewLine}The timers will be reset, and the plan will be unloaded.";

            public const string PlanPreparationEventDialogTitle = "Prepare";
            public const string PlanPreparationSafetyCheckRequest = "Please run a daily safety check first"; // with PlanPreparationEventDialogTitle

            public const string PlanExecutionConsistencyErrorTitle = "Plan Execution Consistency Error";
            public const string PlanExecutionConsistencyErrorMessage = "The emission is done, but the plan is not completed";

            public const string PlanUnloadFromTreatmentErrorTitle = "Unload Plan from Treatment";
            public const string PlanUnloadFromTreatmentErrorMessage = "Failed to unload the plan";

            public const string PlanStartTitle = "Start Plan";
            public const string PlanStartFailedError = "Failed to start the plan";

            public const string HeaterCurrentErrorTitle = "Heater Current Error";
            public const string HeaterCurrentMissingValueErrorMessage = "Failed to determine a heater current value for the specified plan";

            public const string FullWarmupEventDialogTitle = "Full Warmup";
            public const string WarmupEventDialogTitle = "Warmup";
            public const string WarmupDbNoActiveHeadError = "Cannot save warmup result to the database. The active head could not be determined"; // with WarmupEventDialogTitle and FullWarmupEventDialogTitle
            public const string FullWarmupSaveToDbFailedError = "Failed to save full warmup results"; // with FullWarmupEventDialogTitle
            public const string FullWarmupFailedError = "Failed to perform a full warmup";  // with FullWarmupEventDialogTitle

            public const string ApplicatorCoilConfigurationLoadError = "Failed to load coil configurations";

            public const string ConditioningConfirmationTitle = "Conditioning Confirmation";
            public const string ConditioningConfirmationMessage = "This operation should be performed once a week. Do you want to proceed?";

            public const string QualityCheckCustomModeConfirmationMessage = "Do you want to switch to custom field selection mode?";
            public const string QualityCheckFullModeConfirmationMessage = "Do you want to discard the changes and generate the full set of fields?";

            public const string QualityCheckNotificationTitle = "QC Execution";
            public const string QualityCheckCompletionNotification = "The QC plan completed successfully.";
            public const string SwitchToReportsSuggestionMessage = "To check the details, you can switch to the report view.";

            public const string QualityCheckConsistencyErrorTitle = "QC Execution Consistency Error";
            public const string QualityCheckConsistencyErrorMessage = "The emission is done, but the QC plan is not completed.";

            public const string QualityCheckDiscardChangesConfirmationTitle = "Unsaved Data Will Be Lost";
            public const string QualityCheckDiscardChangesConfirmationMessage = "Do you want to proceed and discard the changes?";

            public const string QualityCheckSaveErrorMessage = "Failed to save QC data"; // common SaveError

            public const string QualityCheckTitle = "Quality Check";
            public const string QualityCheckStartErrorMessage = "Failed to perform the QC emission plan";

            public const string PlanCreateCollectionError = "Failed to create a new collection of fields";
            public const string PlanDeleteFieldConfirmationMessage = "Do you want to delete the selected fields?";

            public const string PlanOperationErrorTitle = "Field Operation Error";
            public const string PlanAddFieldErrorMessage = "Failed to add a new field";
            public const string PlanRemoveFieldErrorMessage = "Failed to remove the field";


            public const string PhysicsErrorTitle = "Physics Error";
            public const string PhysiсsStartErrorMessage = "Failed to perform the Physics emission plan";

            public const string PhysicsNotificationTitle = "Physics Execution";
            public const string PhysicsExecutionCompletionNotification = "The Physics emission plan completed";

            public const string ApprovePhysicsDataMessage = "Please approve the physics data";

            public static class Applicator 
            {
                public const string NoConnectedApplicator = "No connected applicator.";
                public const string UnregisteredApplicator = "The connected applicator is not registered in the database.";
                public const string ConnectedApplicatorStringFormat = "The connected applicator is {0} field for {1}kV.";
                public const string ApplicatorMismatchStringFormat = "The connected applicator (serial={0}) cannot be used. Please connect any {1}kV one.";
                public const string SuggestedEnergyStringFormat = "Please connect an applicator for {0}kV.";
                public const string SuggestedApplicatorStringFormat = "Please connect an applicator of {0} field for {1}kV.";
                public const string SuggestedQcApplicatorMessage = "Please connect a QC applicator.";
            }

            public static class SafetyCheck
            {
                public const string CompletionConfirmationTitle = "Safety Check";
                public static readonly string CompletionConfirmationMessage = $"The safety check completed successfully.{Environment.NewLine}The timers will be reset, and the plan will be unloaded.";

                public const string ListLoadError = "Failed to load the safety check list";

                public const string ExecutionConsistencyErrorTitle = "Safety check consistency error";
                public const string ExecutionConsistencyErrorMessage = "The emission is done, but the safety check plan is not completed";

                public const string ErrorTitle = "Safety Check Error";
                public const string StartErrorMessage = "Failed to perform the safety check";

                public const string CreatePlanErrorMessage = "Failed to create a new safety check plan";
                public const string SaveDataErrorMessage = "Failed to save safety check data";
            }
        }
    }
}
