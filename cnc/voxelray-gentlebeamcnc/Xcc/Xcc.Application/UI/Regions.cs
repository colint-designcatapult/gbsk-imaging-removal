namespace Xcc.Application.UI;

public static class Regions
{
    // Main applications regions
    public const string MainRegion = "MainRegion";

    public static class Main
    {
        public const string ClinicalDataRegion = "ClinicalDataRegion";

        public static class ClinicalData
        {
            public const string TreatmentsRegion = "TreatmentsRegion";
            public const string PlanRegion = "PlanRegion";
            
            public const string ImagesRegion = "ImagesRegion";

            public static class Images
            {
                public const string ViewerRegion = "ViewerRegion";
            }
        }

        public const string PhysicsRegion = "PhysicsRegion";

        public const string SettingsRegion = "SettingsRegion";

        public static class Settings
        {
            public const string NetworkSettingsRegion = "NetworkSettingsRegion";
            public const string UserManagementRegion = "UserManagementRegion";
            public const string UserPermissionsRegion = "UserPermissionsRegion";
            public const string HeadManagementRegion = "HeadManagementRegion";
        }
    }


    // External applications regions
    public const string ExternalRegion = "TreatmentConsoleRegion";

    public static class External
    {
        public const string TreatmentRegion = "TreatmentRegion";

        public const string QualityAssuranceRegion = "QualityAssuranceRegion";

        public static class QualityAssurance
        {
            public const string QualityChecksTabRegion = "QualityChecksTabRegion";
            public const string QualityChecksViewRegion = "QualityChecksViewRegion";

            public const string SafetyChecksTabRegion = "SafetyChecksTabRegion";
            public const string SafetyChecksViewRegion = "SafetyChecksViewRegion";

            public const string BrightFieldCalibrationTabRegion = "BrightFieldCalibrationTabRegion";
        }
    }
}