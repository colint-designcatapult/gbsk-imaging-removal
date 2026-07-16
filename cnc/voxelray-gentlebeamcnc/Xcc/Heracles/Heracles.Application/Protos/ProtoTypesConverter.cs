using System;

using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Intensities.V1;
using Com.Empyreanmed.Heracles.Logs.V1;
using Com.Empyreanmed.Heracles.Photos.V1;
using Com.Empyreanmed.Heracles.Plans.V1;
using Com.Empyreanmed.Heracles.Positions.V1;
using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Com.Empyreanmed.Heracles.Settings.V1;
using Com.Empyreanmed.Heracles.Simulations.V1;
using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Com.Empyreanmed.Heracles.Treatments.V1;
using Com.Empyreanmed.Heracles.Visits.V1;
using Com.Empyreanmed.Heracles.Warmups.V1;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Models;
using Heracles.Application.Models.Settings;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Core.Models.RDBMS;

using Xcc.Application.Domain.System;
using Xcc.Application.Models;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models.RDBMS;
using Xcc.Infra.Logging;
using CoilConfiguration = Com.Empyreanmed.Heracles.CoilConfigurations.V1.CoilConfiguration;
using CorrectionMatrix = Com.Empyreanmed.Heracles.CorrectionMatrix.V1.CorrectionMatrix;
using HeaterCurrentConfig = Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1.HeaterCurrentConfig;
using Patient = Com.Empyreanmed.Heracles.Patients.V1.Patient;
using ReferenceField = Com.Empyreanmed.Heracles.ReferenceFields.V1.ReferenceField;
using TDF = Com.Empyreanmed.Heracles.Enums.V1.TDF;

namespace Heracles.Application.Protos
{
    public class ProtoTypesConverter : Xcc.Infra.Persistence.DataAccess.gRPC.ProtoTypesConverter
    {
        #region enums
        private static PermissionType FromProto(PERMISSION permission)
        {
            switch (permission)
            {
                case PERMISSION.PatientsClinicalData:
                    return PermissionType.ClinicalData;
                case PERMISSION.PatientsTreatment:
                    return PermissionType.Treatment;
                case PERMISSION.SystemCalibration:
                    return PermissionType.SystemCalibration;
                case PERMISSION.QualityAssurance:
                    return PermissionType.QualityAssurance;
                case PERMISSION.SystemSettings:
                    return PermissionType.SystemSettings;
                case PERMISSION.UserManagement:
                    return PermissionType.UserManagement;
                case PERMISSION.Services:
                    return PermissionType.Services;
                case PERMISSION.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast PermissionType {permission}");
            }
        }

        private static PERMISSION ToProto(PermissionType permission)
        {
            switch (permission)
            {
                case PermissionType.ClinicalData:
                    return PERMISSION.PatientsClinicalData;
                case PermissionType.Treatment:
                    return PERMISSION.PatientsTreatment;
                case PermissionType.SystemCalibration:
                    return PERMISSION.SystemCalibration;
                case PermissionType.QualityAssurance:
                    return PERMISSION.QualityAssurance;
                case PermissionType.SystemSettings:
                    return PERMISSION.SystemSettings;
                case PermissionType.UserManagement:
                    return PERMISSION.UserManagement;
                case PermissionType.Services:
                    return PERMISSION.Services;
                default:
                    throw new InvalidCastException($"Failed to cast PermissionType {permission}");
            }
        }
        public static Core.Enums.VisitType FromProto(VISITTYPE visitType)
        {
            switch (visitType)
            {
                case VISITTYPE.Simulation:
                    return Core.Enums.VisitType.Simulation;
                case VISITTYPE.Treatment:
                    return Core.Enums.VisitType.Treatment;
                case VISITTYPE.Otv:
                    return Core.Enums.VisitType.OTV;
                case VISITTYPE.NonEncounterNotes:
                    return Core.Enums.VisitType.NonEncounterNotes;
                case VISITTYPE.FollowUp:
                    return Core.Enums.VisitType.FollowUp;
                case VISITTYPE.SkinCheck:
                    return Core.Enums.VisitType.SkinCheck;
                case VISITTYPE.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast VisitType {visitType.ToString()}");
            }
        }

        public static VISITTYPE ToProto(Core.Enums.VisitType visitType)
        {
            switch (visitType)
            {
                case Core.Enums.VisitType.Simulation:
                    return VISITTYPE.Simulation;
                case Core.Enums.VisitType.Treatment:
                    return VISITTYPE.Treatment;
                case Core.Enums.VisitType.OTV:
                    return VISITTYPE.Otv;
                case Core.Enums.VisitType.NonEncounterNotes:
                    return VISITTYPE.NonEncounterNotes;
                case Core.Enums.VisitType.FollowUp:
                    return VISITTYPE.FollowUp;
                case Core.Enums.VisitType.SkinCheck:
                    return VISITTYPE.SkinCheck;
                default:
                    throw new InvalidCastException($"Failed to cast VisitType {visitType.ToString()}");
            }
        }

        public static Core.Enums.Description FromProto(DESCRIPTION description)
        {
            switch (description)
            {
                case DESCRIPTION.InfundibuloCytic:
                    return Core.Enums.Description.InfundibuloCytic;
                case DESCRIPTION.UlceratedLongStanding:
                    return Core.Enums.Description.UIceratedLongStanding;
                case DESCRIPTION.Adenosquamous:
                    return Core.Enums.Description.Adenosquamous;
                case DESCRIPTION.DesmoplasticMetaplastic:
                    return Core.Enums.Description.DesmoplasticMetaplastic;
                case DESCRIPTION.RecurrentLesionPostSurgery:
                    return Core.Enums.Description.RecurrentLesionPostSurgery;
                case DESCRIPTION.LargeLesion:
                    return Core.Enums.Description.LargeLesion;
                case DESCRIPTION.DeepLesion:
                    return Core.Enums.Description.DeepLesion;
                case DESCRIPTION.RapidGrowth:
                    return Core.Enums.Description.RapidGrowth;
                case DESCRIPTION.ExtensionIntoHairFollicle:
                    return Core.Enums.Description.ExtensionIntoHairFollicle;
                case DESCRIPTION.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast Description {description.ToString()}");
            }
        }

        public static DESCRIPTION ToProto(Core.Enums.Description? description)
        {
            switch (description)
            {
                case Core.Enums.Description.InfundibuloCytic:
                    return DESCRIPTION.InfundibuloCytic;
                case Core.Enums.Description.UIceratedLongStanding:
                    return DESCRIPTION.UlceratedLongStanding;
                case Core.Enums.Description.Adenosquamous:
                    return DESCRIPTION.Adenosquamous;
                case Core.Enums.Description.DesmoplasticMetaplastic:
                    return DESCRIPTION.DesmoplasticMetaplastic;
                case Core.Enums.Description.RecurrentLesionPostSurgery:
                    return DESCRIPTION.RecurrentLesionPostSurgery;
                case Core.Enums.Description.LargeLesion:
                    return DESCRIPTION.LargeLesion;
                case Core.Enums.Description.DeepLesion:
                    return DESCRIPTION.DeepLesion;
                case Core.Enums.Description.RapidGrowth:
                    return DESCRIPTION.RapidGrowth;
                case Core.Enums.Description.ExtensionIntoHairFollicle:
                    return DESCRIPTION.ExtensionIntoHairFollicle;
                default:
                    throw new InvalidCastException($"Failed to cast Description {description.ToString()}");
            }
        }

        public static Core.Enums.IcdCode? FromProto(ICDCODE icdCode)
        {
            switch (icdCode)
            {
                case ICDCODE.BccBreast:
                    return Core.Enums.IcdCode.BCC_Breast;
                case ICDCODE.SccBreast:
                    return Core.Enums.IcdCode.SCC_Breast;
                case ICDCODE.SccIsBreast:
                    return Core.Enums.IcdCode.SCC_IS_Breast;
                case ICDCODE.BccLeftEar:
                    return Core.Enums.IcdCode.BCC_LeftEar;
                case ICDCODE.SccLeftEar:
                    return Core.Enums.IcdCode.SCC_LeftEar;
                case ICDCODE.SccIsLeftEar:
                    return Core.Enums.IcdCode.SCC_IS_LeftEar;
                case ICDCODE.BccRightEar:
                    return Core.Enums.IcdCode.BCC_RightEar;
                case ICDCODE.SccRightEar:
                    return Core.Enums.IcdCode.SCC_RightEar;
                case ICDCODE.SccIsRightEar:
                    return Core.Enums.IcdCode.SCC_IS_RightEar;
                case ICDCODE.BccFace:
                    return Core.Enums.IcdCode.BCC_Face;
                case ICDCODE.SccFace:
                    return Core.Enums.IcdCode.SCC_Face;
                case ICDCODE.SccIsFace:
                    return Core.Enums.IcdCode.SCC_IS_Face;
                case ICDCODE.BccLip:
                    return Core.Enums.IcdCode.BCC_Lip;
                case ICDCODE.SccLip:
                    return Core.Enums.IcdCode.SCC_Lip;
                case ICDCODE.SccIsLip:
                    return Core.Enums.IcdCode.SCC_IS_Lip;
                case ICDCODE.BccNeck:
                    return Core.Enums.IcdCode.BCC_Neck;
                case ICDCODE.SccNeck:
                    return Core.Enums.IcdCode.SCC_Neck;
                case ICDCODE.SccIsNeck:
                    return Core.Enums.IcdCode.SCC_IS_Neck;
                case ICDCODE.BccNose:
                    return Core.Enums.IcdCode.BCC_Nose;
                case ICDCODE.SccNose:
                    return Core.Enums.IcdCode.SCC_Nose;
                case ICDCODE.SccIsNose:
                    return Core.Enums.IcdCode.SCC_IS_Nose;
                case ICDCODE.BccScalp:
                    return Core.Enums.IcdCode.BCC_Scalp;
                case ICDCODE.SccScalp:
                    return Core.Enums.IcdCode.SCC_Scalp;
                case ICDCODE.SccIsScalp:
                    return Core.Enums.IcdCode.SCC_IS_Scalp;
                case ICDCODE.BccPostauricular:
                    return Core.Enums.IcdCode.BCC_PostAuricular;
                case ICDCODE.SccPostauricular:
                    return Core.Enums.IcdCode.SCC_PostAuricular;
                case ICDCODE.SccIsPostauricular:
                    return Core.Enums.IcdCode.SCC_IS_PostAuricular;
                case ICDCODE.BccTrunk:
                    return Core.Enums.IcdCode.BCC_Trunk;
                case ICDCODE.SccTrunk:
                    return Core.Enums.IcdCode.SCC_Trunk;
                case ICDCODE.SccIsTrunk:
                    return Core.Enums.IcdCode.SCC_IS_Trunk;
                case ICDCODE.BccChest:
                    return Core.Enums.IcdCode.BCC_Chest;
                case ICDCODE.SccChest:
                    return Core.Enums.IcdCode.SCC_Chest;
                case ICDCODE.SccIsChest:
                    return Core.Enums.IcdCode.SCC_IS_Chest;
                case ICDCODE.BccAbdomen:
                    return Core.Enums.IcdCode.BCC_Abdomen;
                case ICDCODE.SccAbdomen:
                    return Core.Enums.IcdCode.SCC_Abdomen;
                case ICDCODE.SccIsAbdomen:
                    return Core.Enums.IcdCode.SCC_IS_Abdomen;
                case ICDCODE.BccBack:
                    return Core.Enums.IcdCode.BCC_Back;
                case ICDCODE.SccBack:
                    return Core.Enums.IcdCode.SCC_Back;
                case ICDCODE.SccIsBack:
                    return Core.Enums.IcdCode.SCC_IS_Back;
                case ICDCODE.BccLeftLowerLimb:
                    return Core.Enums.IcdCode.BCC_LeftLowerLimb;
                case ICDCODE.SccLeftLowerLimb:
                    return Core.Enums.IcdCode.SCC_LeftLowerLimb;
                case ICDCODE.SccIsLeftLowerLimb:
                    return Core.Enums.IcdCode.SCC_IS_LeftLowerLimb;
                case ICDCODE.BccRightLowerLimb:
                    return Core.Enums.IcdCode.BCC_RightLowerLimb;
                case ICDCODE.SccRightLowerLimb:
                    return Core.Enums.IcdCode.SCC_RightLowerLimb;
                case ICDCODE.SccIsRightLowerLimb:
                    return Core.Enums.IcdCode.SCC_IS_RightLowerLimb;
                case ICDCODE.BccLeftUpperLimb:
                    return Core.Enums.IcdCode.BCC_LeftUpperLimb;
                case ICDCODE.SccLeftUpperLimb:
                    return Core.Enums.IcdCode.SCC_LeftUpperLimb;
                case ICDCODE.SccIsLeftUpperLimb:
                    return Core.Enums.IcdCode.SCC_IS_LeftUpperLimb;
                case ICDCODE.BccRightUpperLimb:
                    return Core.Enums.IcdCode.BCC_RightUpperLimb;
                case ICDCODE.SccRightUpperLimb:
                    return Core.Enums.IcdCode.SCC_RightUpperLimb;
                case ICDCODE.SccIsRightUpperLimb:
                    return Core.Enums.IcdCode.SCC_IS_RightUpperLimb;
                case ICDCODE.BccRightUpperEyelid:
                    return Core.Enums.IcdCode.BCC_RightUpperEyelid;
                case ICDCODE.SccRightUpperEyelid:
                    return Core.Enums.IcdCode.SCC_RightUpperEyelid;
                case ICDCODE.SccIsRightUpperEyelid:
                    return Core.Enums.IcdCode.SCC_IS_RightUpperEyelid;
                case ICDCODE.BccRightLowerEyelid:
                    return Core.Enums.IcdCode.BCC_RightLowerEyelid;
                case ICDCODE.SccRightLowerEyelid:
                    return Core.Enums.IcdCode.SCC_RightLowerEyelid;
                case ICDCODE.SccIsRightLowerEyelid:
                    return Core.Enums.IcdCode.SCC_IS_RightLowerEyelid;
                case ICDCODE.BccLeftUpperEyelid:
                    return Core.Enums.IcdCode.BCC_LeftUpperEyelid;
                case ICDCODE.SccLeftUpperEyelid:
                    return Core.Enums.IcdCode.SCC_LeftUpperEyelid;
                case ICDCODE.SccIsLeftUpperEyelid:
                    return Core.Enums.IcdCode.SCC_IS_LeftUpperEyelid;
                case ICDCODE.BccLeftLowerEyelid:
                    return Core.Enums.IcdCode.BCC_LeftLowerEyelid;
                case ICDCODE.SccLeftLowerEyelid:
                    return Core.Enums.IcdCode.SCC_LeftLowerEyelid;
                case ICDCODE.SccIsLeftLowerEyelid:
                    return Core.Enums.IcdCode.SCC_IS_LeftLowerEyelid;

                case ICDCODE.BasosquamousBreast:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Breast;
                case ICDCODE.BasosquamousLeftEar:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_LeftEar;
                case ICDCODE.BasosquamousRightEar:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_RightEar;
                case ICDCODE.BasosquamousFace:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Face;
                case ICDCODE.BasosquamousLip:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Lip;
                case ICDCODE.BasosquamousNeck:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Neck;
                case ICDCODE.BasosquamousNose:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Nose;
                case ICDCODE.BasosquamousScalp:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Scalp;
                case ICDCODE.BasosquamousPostauricular:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_PostAuricular;
                case ICDCODE.BasosquamousTrunk:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Trunk;
                case ICDCODE.BasosquamousChest:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Chest;
                case ICDCODE.BasosquamousAbdomen:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Abdomen;
                case ICDCODE.BasosquamousBack:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_Back;
                case ICDCODE.BasosquamousLeftLowerLimb:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_LeftLowerLimb;
                case ICDCODE.BasosquamousRightLowerLimb:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_RightLowerLimb;
                case ICDCODE.BasosquamousLeftUpperLimb:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_LeftUpperLimb;
                case ICDCODE.BasosquamousRightUpperLimb:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_RightUpperLimb;
                case ICDCODE.BasosquamousRightUpperEyelid:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_RightUpperEyelid;
                case ICDCODE.BasosquamousRightLowerEyelid:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_RightLowerEyelid;
                case ICDCODE.BasosquamousLeftUpperEyelid:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_LeftUpperEyelid;
                case ICDCODE.BasosquamousLeftLowerEyelid:
                    return Core.Enums.IcdCode.BASOSQUAMOUS_LeftLowerEyelid;

                case ICDCODE.None:
                    return Core.Enums.IcdCode.None;

                case ICDCODE.Unspecified:
                    return null;
                default:
                    throw new InvalidCastException($"Failed to cast ICD code {icdCode.ToString()}");
            }
        }

        public static ICDCODE ToProto(Core.Enums.IcdCode? icdCode)
        {
            switch (icdCode)
            {
                case null:
                    return ICDCODE.Unspecified;
                case Core.Enums.IcdCode.BCC_Breast:
                    return ICDCODE.BccBreast;
                case Core.Enums.IcdCode.SCC_Breast:
                    return ICDCODE.SccBreast;
                case Core.Enums.IcdCode.SCC_IS_Breast:
                    return ICDCODE.SccIsBreast;
                case Core.Enums.IcdCode.BCC_LeftEar:
                    return ICDCODE.BccLeftEar;
                case Core.Enums.IcdCode.SCC_LeftEar:
                    return ICDCODE.SccLeftEar;
                case Core.Enums.IcdCode.SCC_IS_LeftEar:
                    return ICDCODE.SccIsLeftEar;
                case Core.Enums.IcdCode.BCC_RightEar:
                    return ICDCODE.BccRightEar;
                case Core.Enums.IcdCode.SCC_RightEar:
                    return ICDCODE.SccRightEar;
                case Core.Enums.IcdCode.SCC_IS_RightEar:
                    return ICDCODE.SccIsRightEar;
                case Core.Enums.IcdCode.BCC_Face:
                    return ICDCODE.BccFace;
                case Core.Enums.IcdCode.SCC_Face:
                    return ICDCODE.SccFace;
                case Core.Enums.IcdCode.SCC_IS_Face:
                    return ICDCODE.SccIsFace;
                case Core.Enums.IcdCode.BCC_Lip:
                    return ICDCODE.BccLip;
                case Core.Enums.IcdCode.SCC_Lip:
                    return ICDCODE.SccLip;
                case Core.Enums.IcdCode.SCC_IS_Lip:
                    return ICDCODE.SccIsLip;
                case Core.Enums.IcdCode.BCC_Neck:
                    return ICDCODE.BccNeck;
                case Core.Enums.IcdCode.SCC_Neck:
                    return ICDCODE.SccNeck;
                case Core.Enums.IcdCode.SCC_IS_Neck:
                    return ICDCODE.SccIsNeck;
                case Core.Enums.IcdCode.BCC_Nose:
                    return ICDCODE.BccNose;
                case Core.Enums.IcdCode.SCC_Nose:
                    return ICDCODE.SccNose;
                case Core.Enums.IcdCode.SCC_IS_Nose:
                    return ICDCODE.SccIsNose;
                case Core.Enums.IcdCode.BCC_Scalp:
                    return ICDCODE.BccScalp;
                case Core.Enums.IcdCode.SCC_Scalp:
                    return ICDCODE.SccScalp;
                case Core.Enums.IcdCode.SCC_IS_Scalp:
                    return ICDCODE.SccIsScalp;
                case Core.Enums.IcdCode.BCC_PostAuricular:
                    return ICDCODE.BccPostauricular;
                case Core.Enums.IcdCode.SCC_PostAuricular:
                    return ICDCODE.SccPostauricular;
                case Core.Enums.IcdCode.SCC_IS_PostAuricular:
                    return ICDCODE.SccIsPostauricular;
                case Core.Enums.IcdCode.BCC_Trunk:
                    return ICDCODE.BccTrunk;
                case Core.Enums.IcdCode.SCC_Trunk:
                    return ICDCODE.SccTrunk;
                case Core.Enums.IcdCode.SCC_IS_Trunk:
                    return ICDCODE.SccIsTrunk;
                case Core.Enums.IcdCode.BCC_Chest:
                    return ICDCODE.BccChest;
                case Core.Enums.IcdCode.SCC_Chest:
                    return ICDCODE.SccChest;
                case Core.Enums.IcdCode.SCC_IS_Chest:
                    return ICDCODE.SccIsChest;
                case Core.Enums.IcdCode.BCC_Abdomen:
                    return ICDCODE.BccAbdomen;
                case Core.Enums.IcdCode.SCC_Abdomen:
                    return ICDCODE.SccAbdomen;
                case Core.Enums.IcdCode.SCC_IS_Abdomen:
                    return ICDCODE.SccIsAbdomen;
                case Core.Enums.IcdCode.BCC_Back:
                    return ICDCODE.BccBack;
                case Core.Enums.IcdCode.SCC_Back:
                    return ICDCODE.SccBack;
                case Core.Enums.IcdCode.SCC_IS_Back:
                    return ICDCODE.SccIsBack;
                case Core.Enums.IcdCode.BCC_LeftLowerLimb:
                    return ICDCODE.BccLeftLowerLimb;
                case Core.Enums.IcdCode.SCC_LeftLowerLimb:
                    return ICDCODE.SccLeftLowerLimb;
                case Core.Enums.IcdCode.SCC_IS_LeftLowerLimb:
                    return ICDCODE.SccIsLeftLowerLimb;
                case Core.Enums.IcdCode.BCC_RightLowerLimb:
                    return ICDCODE.BccRightLowerLimb;
                case Core.Enums.IcdCode.SCC_RightLowerLimb:
                    return ICDCODE.SccRightLowerLimb;
                case Core.Enums.IcdCode.SCC_IS_RightLowerLimb:
                    return ICDCODE.SccIsRightLowerLimb;
                case Core.Enums.IcdCode.BCC_LeftUpperLimb:
                    return ICDCODE.BccLeftUpperLimb;
                case Core.Enums.IcdCode.SCC_LeftUpperLimb:
                    return ICDCODE.SccLeftUpperLimb;
                case Core.Enums.IcdCode.SCC_IS_LeftUpperLimb:
                    return ICDCODE.SccIsLeftUpperLimb;
                case Core.Enums.IcdCode.BCC_RightUpperLimb:
                    return ICDCODE.BccRightUpperLimb;
                case Core.Enums.IcdCode.SCC_RightUpperLimb:
                    return ICDCODE.SccRightUpperLimb;
                case Core.Enums.IcdCode.SCC_IS_RightUpperLimb:
                    return ICDCODE.SccIsRightUpperLimb;
                case Core.Enums.IcdCode.BCC_RightUpperEyelid:
                    return ICDCODE.BccRightUpperEyelid;
                case Core.Enums.IcdCode.SCC_RightUpperEyelid:
                    return ICDCODE.SccRightUpperEyelid;
                case Core.Enums.IcdCode.SCC_IS_RightUpperEyelid:
                    return ICDCODE.SccIsRightUpperEyelid;
                case Core.Enums.IcdCode.BCC_RightLowerEyelid:
                    return ICDCODE.BccRightLowerEyelid;
                case Core.Enums.IcdCode.SCC_RightLowerEyelid:
                    return ICDCODE.SccRightLowerEyelid;
                case Core.Enums.IcdCode.SCC_IS_RightLowerEyelid:
                    return ICDCODE.SccIsRightLowerEyelid;
                case Core.Enums.IcdCode.BCC_LeftUpperEyelid:
                    return ICDCODE.BccLeftUpperEyelid;
                case Core.Enums.IcdCode.SCC_LeftUpperEyelid:
                    return ICDCODE.SccLeftUpperEyelid;
                case Core.Enums.IcdCode.SCC_IS_LeftUpperEyelid:
                    return ICDCODE.SccIsLeftUpperEyelid;
                case Core.Enums.IcdCode.BCC_LeftLowerEyelid:
                    return ICDCODE.BccLeftLowerEyelid;
                case Core.Enums.IcdCode.SCC_LeftLowerEyelid:
                    return ICDCODE.SccLeftLowerEyelid;
                case Core.Enums.IcdCode.SCC_IS_LeftLowerEyelid:
                    return ICDCODE.SccIsLeftLowerEyelid;

                case Core.Enums.IcdCode.BASOSQUAMOUS_Breast:
                    return ICDCODE.BasosquamousBreast;
                case Core.Enums.IcdCode.BASOSQUAMOUS_LeftEar:
                    return ICDCODE.BasosquamousLeftEar;
                case Core.Enums.IcdCode.BASOSQUAMOUS_RightEar:
                    return ICDCODE.BasosquamousRightEar;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Face:
                    return ICDCODE.BasosquamousFace;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Lip:
                    return ICDCODE.BasosquamousLip;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Neck:
                    return ICDCODE.BasosquamousNeck;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Nose:
                    return ICDCODE.BasosquamousNose;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Scalp:
                    return ICDCODE.BasosquamousScalp;
                case Core.Enums.IcdCode.BASOSQUAMOUS_PostAuricular:
                    return ICDCODE.BasosquamousPostauricular;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Trunk:
                    return ICDCODE.BasosquamousTrunk;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Chest:
                    return ICDCODE.BasosquamousChest;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Abdomen:
                    return ICDCODE.BasosquamousAbdomen;
                case Core.Enums.IcdCode.BASOSQUAMOUS_Back:
                    return ICDCODE.BasosquamousBack;
                case Core.Enums.IcdCode.BASOSQUAMOUS_LeftLowerLimb:
                    return ICDCODE.BasosquamousLeftLowerLimb;
                case Core.Enums.IcdCode.BASOSQUAMOUS_RightLowerLimb:
                    return ICDCODE.BasosquamousRightLowerLimb;
                case Core.Enums.IcdCode.BASOSQUAMOUS_LeftUpperLimb:
                    return ICDCODE.BasosquamousLeftUpperLimb;
                case Core.Enums.IcdCode.BASOSQUAMOUS_RightUpperLimb:
                    return ICDCODE.BasosquamousRightUpperLimb;
                case Core.Enums.IcdCode.BASOSQUAMOUS_RightUpperEyelid:
                    return ICDCODE.BasosquamousRightUpperEyelid;
                case Core.Enums.IcdCode.BASOSQUAMOUS_RightLowerEyelid:
                    return ICDCODE.BasosquamousRightLowerEyelid;
                case Core.Enums.IcdCode.BASOSQUAMOUS_LeftUpperEyelid:
                    return ICDCODE.BasosquamousLeftUpperEyelid;
                case Core.Enums.IcdCode.BASOSQUAMOUS_LeftLowerEyelid:
                    return ICDCODE.BasosquamousLeftLowerEyelid;

                case Core.Enums.IcdCode.None:
                    return ICDCODE.None;

                default:
                    throw new InvalidCastException($"Failed to cast ICD code {icdCode.ToString()}");
            }
        }

        public static CELLTYPE ToProto(Core.Enums.Celltype? cellType)
        {
            switch (cellType)
            {
                case Core.Enums.Celltype.Aberrant:
                    return CELLTYPE.Aberrant;
                case Core.Enums.Celltype.Adenoid:
                    return CELLTYPE.Adenoid;
                case Core.Enums.Celltype.AtypicalBasaloidProliferation:
                    return CELLTYPE.AtypicalBasaloidProliferation;
                case Core.Enums.Celltype.BasosquamousMetatypical:
                    return CELLTYPE.BasosquamousMetatypical;
                case Core.Enums.Celltype.AdnexalDifferentiation:
                    return CELLTYPE.AdnexalDifferentiation;
                case Core.Enums.Celltype.SquamousDifferentiation:
                    return CELLTYPE.SquamousDifferentiation;
                case Core.Enums.Celltype.ClearRing:
                    return CELLTYPE.ClearRing;
                case Core.Enums.Celltype.CysticCellCarcinoma:
                    return CELLTYPE.CysticCellCarcinoma;
                case Core.Enums.Celltype.FibroepitheliomaOfPinkus:
                    return CELLTYPE.FibroepitheliomaOfPinkus;
                case Core.Enums.Celltype.Infiltrative:
                    return CELLTYPE.Infiltrative;
                case Core.Enums.Celltype.Keratotic:
                    return CELLTYPE.Keratotic;
                case Core.Enums.Celltype.MicroNodular:
                    return CELLTYPE.MicroNodular;
                case Core.Enums.Celltype.MixedPattern:
                    return CELLTYPE.MixedPattern;
                case Core.Enums.Celltype.MorphoeicSclerosingFibrosing:
                    return CELLTYPE.MorphoeicSclerosingFibrosing;
                case Core.Enums.Celltype.NodularClassicBasalCell:
                    return CELLTYPE.NodularClassicBasalCell;
                case Core.Enums.Celltype.Nodulocystic:
                    return CELLTYPE.Nodulocystic;
                case Core.Enums.Celltype.Pigmented:
                    return CELLTYPE.Pigmented;
                case Core.Enums.Celltype.Pleomorphic:
                    return CELLTYPE.Pleomorphic;
                case Core.Enums.Celltype.Polypoid:
                    return CELLTYPE.Polypoid;
                case Core.Enums.Celltype.PoreLike:
                    return CELLTYPE.PoreLike;
                case Core.Enums.Celltype.RodentUlcerJacobiUlcer:
                    return CELLTYPE.RodentUlcerJacobiUlcer;
                case Core.Enums.Celltype.SuperficialMulticentric:
                    return CELLTYPE.SuperficialMulticentric;
                case Core.Enums.Celltype.Acantholytic:
                    return CELLTYPE.Acantholytic;
                case Core.Enums.Celltype.AdenoidPseudoglandular:
                    return CELLTYPE.AdenoidPseudoglandular;
                case Core.Enums.Celltype.AtypicalSquamousProliferation:
                    return CELLTYPE.AtypicalSquamousProliferation;
                case Core.Enums.Celltype.Basaloid:
                    return CELLTYPE.Basaloid;
                case Core.Enums.Celltype.ClearCell:
                    return CELLTYPE.ClearCell;
                case Core.Enums.Celltype.Erythroplasia:
                    return CELLTYPE.Erythroplasia;
                case Core.Enums.Celltype.Intraepidermal:
                    return CELLTYPE.Intraepidermal;
                case Core.Enums.Celltype.Invasive:
                    return CELLTYPE.Invasive;
                case Core.Enums.Celltype.Keratoacanthoma:
                    return CELLTYPE.Keratoacanthoma;
                case Core.Enums.Celltype.LargeCellKeratinizing:
                    return CELLTYPE.LargeCellKeratinizing;
                case Core.Enums.Celltype.LargeCellNonKeratinizing:
                    return CELLTYPE.LargeCellNonKeratinizing;
                case Core.Enums.Celltype.Metaplasia:
                    return CELLTYPE.Metaplasia;
                case Core.Enums.Celltype.ModeratelyDifferentiated:
                    return CELLTYPE.ModeratelyDifferentiated;
                case Core.Enums.Celltype.PoorlyDifferentiated:
                    return CELLTYPE.PoorlyDifferentiated;
                case Core.Enums.Celltype.PapillaryCarcinoma:
                    return CELLTYPE.PapillaryCarcinoma;
                case Core.Enums.Celltype.SignetRing:
                    return CELLTYPE.SignetRingCell;
                case Core.Enums.Celltype.SmallCellKeratinizing:
                    return CELLTYPE.SmallCellKeratinizing;
                case Core.Enums.Celltype.Superficial:
                    return CELLTYPE.Superficial;
                case Core.Enums.Celltype.SpindleCell:
                    return CELLTYPE.SpindleCell;
                case Core.Enums.Celltype.Verrucous:
                    return CELLTYPE.Verrucous;
                case Core.Enums.Celltype.WellDifferentiated:
                    return CELLTYPE.WellDifferentiated;
                case Core.Enums.Celltype.SuperficiallyInvasive:
                    return CELLTYPE.SuperficiallyInvasive;
                case Core.Enums.Celltype.Other:
                    return CELLTYPE.Other;

                case Core.Enums.Celltype.None:
                case null:
                    return CELLTYPE.None;
                default:
                    throw new InvalidCastException($"Failed to cast CellType {cellType.ToString()}");
            }
        }

        public static Core.Enums.Celltype? FromProto(CELLTYPE cellType)
        {
            switch (cellType)
            {
                case CELLTYPE.Aberrant:
                    return Core.Enums.Celltype.Aberrant;
                case CELLTYPE.Adenoid:
                    return Core.Enums.Celltype.Adenoid;
                case CELLTYPE.AtypicalBasaloidProliferation:
                    return Core.Enums.Celltype.AtypicalBasaloidProliferation;
                case CELLTYPE.BasosquamousMetatypical:
                    return Core.Enums.Celltype.BasosquamousMetatypical;
                case CELLTYPE.AdnexalDifferentiation:
                    return Core.Enums.Celltype.AdnexalDifferentiation;
                case CELLTYPE.SquamousDifferentiation:
                    return Core.Enums.Celltype.SquamousDifferentiation;
                case CELLTYPE.ClearRing:
                    return Core.Enums.Celltype.ClearRing;
                case CELLTYPE.CysticCellCarcinoma:
                    return Core.Enums.Celltype.CysticCellCarcinoma;
                case CELLTYPE.FibroepitheliomaOfPinkus:
                    return Core.Enums.Celltype.FibroepitheliomaOfPinkus;
                case CELLTYPE.Infiltrative:
                    return Core.Enums.Celltype.Infiltrative;
                case CELLTYPE.Keratotic:
                    return Core.Enums.Celltype.Keratotic;
                case CELLTYPE.MicroNodular:
                    return Core.Enums.Celltype.MicroNodular;
                case CELLTYPE.MixedPattern:
                    return Core.Enums.Celltype.MixedPattern;
                case CELLTYPE.MorphoeicSclerosingFibrosing:
                    return Core.Enums.Celltype.MorphoeicSclerosingFibrosing;
                case CELLTYPE.NodularClassicBasalCell:
                    return Core.Enums.Celltype.NodularClassicBasalCell;
                case CELLTYPE.Nodulocystic:
                    return Core.Enums.Celltype.Nodulocystic;
                case CELLTYPE.Pigmented:
                    return Core.Enums.Celltype.Pigmented;
                case CELLTYPE.Pleomorphic:
                    return Core.Enums.Celltype.Pleomorphic;
                case CELLTYPE.Polypoid:
                    return Core.Enums.Celltype.Polypoid;
                case CELLTYPE.PoreLike:
                    return Core.Enums.Celltype.PoreLike;
                case CELLTYPE.RodentUlcerJacobiUlcer:
                    return Core.Enums.Celltype.RodentUlcerJacobiUlcer;
                case CELLTYPE.SuperficialMulticentric:
                    return Core.Enums.Celltype.SuperficialMulticentric;
                case CELLTYPE.Acantholytic:
                    return Core.Enums.Celltype.Acantholytic;
                case CELLTYPE.AdenoidPseudoglandular:
                    return Core.Enums.Celltype.AdenoidPseudoglandular;
                case CELLTYPE.AtypicalSquamousProliferation:
                    return Core.Enums.Celltype.AtypicalSquamousProliferation;
                case CELLTYPE.Basaloid:
                    return Core.Enums.Celltype.Basaloid;
                case CELLTYPE.ClearCell:
                    return Core.Enums.Celltype.ClearCell;
                case CELLTYPE.Erythroplasia:
                    return Core.Enums.Celltype.Erythroplasia;
                case CELLTYPE.Intraepidermal:
                    return Core.Enums.Celltype.Intraepidermal;
                case CELLTYPE.Invasive:
                    return Core.Enums.Celltype.Invasive;
                case CELLTYPE.Keratoacanthoma:
                    return Core.Enums.Celltype.Keratoacanthoma;
                case CELLTYPE.LargeCellKeratinizing:
                    return Core.Enums.Celltype.LargeCellKeratinizing;
                case CELLTYPE.LargeCellNonKeratinizing:
                    return Core.Enums.Celltype.LargeCellNonKeratinizing;
                case CELLTYPE.Metaplasia:
                    return Core.Enums.Celltype.Metaplasia;
                case CELLTYPE.ModeratelyDifferentiated:
                    return Core.Enums.Celltype.ModeratelyDifferentiated;
                case CELLTYPE.PoorlyDifferentiated:
                    return Core.Enums.Celltype.PoorlyDifferentiated;
                case CELLTYPE.PapillaryCarcinoma:
                    return Core.Enums.Celltype.PapillaryCarcinoma;
                case CELLTYPE.SignetRingCell:
                    return Core.Enums.Celltype.SignetRing;
                case CELLTYPE.SmallCellKeratinizing:
                    return Core.Enums.Celltype.SmallCellKeratinizing;
                case CELLTYPE.Superficial:
                    return Core.Enums.Celltype.Superficial;
                case CELLTYPE.SpindleCell:
                    return Core.Enums.Celltype.SpindleCell;
                case CELLTYPE.Verrucous:
                    return Core.Enums.Celltype.Verrucous;
                case CELLTYPE.WellDifferentiated:
                    return Core.Enums.Celltype.WellDifferentiated;
                case CELLTYPE.SuperficiallyInvasive:
                    return Core.Enums.Celltype.SuperficiallyInvasive;
                case CELLTYPE.Other:
                    return Core.Enums.Celltype.Other;
                case CELLTYPE.None:
                    return Core.Enums.Celltype.None;

                case CELLTYPE.Unspecified:
                    return null;

                default:
                    throw new InvalidCastException($"Failed to cast CellType {cellType.ToString()}");
            }
        }

        public static Core.Enums.TemplateType FromProto(TEMPLATETYPE templateType)
        {
            switch (templateType)
            {
                case TEMPLATETYPE.Simulation:
                    return Core.Enums.TemplateType.Simulation;
                case TEMPLATETYPE.Treatment:
                    return Core.Enums.TemplateType.Treatment;
                case TEMPLATETYPE.Followup:
                    return Core.Enums.TemplateType.FollowUp;
                case TEMPLATETYPE.Otv:
                    return Core.Enums.TemplateType.OTV;
                case TEMPLATETYPE.Other:
                    return Core.Enums.TemplateType.Other;
                case TEMPLATETYPE.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast TemplateType {templateType.ToString()}");
            }
        }

        public static TEMPLATETYPE ToProto(Core.Enums.TemplateType templateType)
        {
            switch (templateType)
            {
                case Core.Enums.TemplateType.Simulation:
                    return TEMPLATETYPE.Simulation;
                case Core.Enums.TemplateType.Treatment:
                    return TEMPLATETYPE.Treatment;
                case Core.Enums.TemplateType.FollowUp:
                    return TEMPLATETYPE.Followup;
                case Core.Enums.TemplateType.OTV:
                    return TEMPLATETYPE.Otv;
                case Core.Enums.TemplateType.Other:
                    return TEMPLATETYPE.Other;
                default:
                    throw new InvalidCastException($"Failed to cast TemplateType {templateType.ToString()}");
            }
        }

        public static Core.Enums.PhotoType FromProto(PHOTOTYPE photoType)
        {
            switch (photoType)
            {
                case PHOTOTYPE.LesionWithMargin:
                    return Core.Enums.PhotoType.LesionWithMargin;
                case PHOTOTYPE.FieldWithShield:
                    return Core.Enums.PhotoType.FieldWithShield;
                case PHOTOTYPE.SimulationSetup:
                    return Core.Enums.PhotoType.SimulationSetup;
                case PHOTOTYPE.Identification:
                    return Core.Enums.PhotoType.Identification;
                case PHOTOTYPE.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast PhotoType {photoType.ToString()}");
            }
        }

        public static PHOTOTYPE ToProto(Core.Enums.PhotoType photoType)
        {
            switch (photoType)
            {
                case Core.Enums.PhotoType.LesionWithMargin:
                    return PHOTOTYPE.LesionWithMargin;
                case Core.Enums.PhotoType.FieldWithShield:
                    return PHOTOTYPE.FieldWithShield;
                case Core.Enums.PhotoType.SimulationSetup:
                    return PHOTOTYPE.SimulationSetup;
                case Core.Enums.PhotoType.Identification:
                    return PHOTOTYPE.Identification;
                default:
                    throw new InvalidCastException($"Failed to cast PhotoType {photoType.ToString()}");
            }
        }

        public static Core.Enums.SsdType FromProto(SSDTYPE ssd)
        {
            switch (ssd)
            {
                case SSDTYPE._50Mm:
                    return Core.Enums.SsdType.SsdType50mm;
                case SSDTYPE._30Mm:
                    return Core.Enums.SsdType.SsdType30mm;
                default:
                    throw new InvalidCastException($"Failed to cast SsdType {ssd.ToString()}");
            }
        }
        public static SSDTYPE ToProto(Core.Enums.SsdType ssd)
        {
            switch (ssd)
            {
                case Core.Enums.SsdType.SsdType50mm:
                    return SSDTYPE._50Mm;
                case Core.Enums.SsdType.SsdType30mm:
                    return SSDTYPE._30Mm;
                default:
                    throw new InvalidCastException($"Failed to cast SsdType {ssd.ToString()}");
            }
        }

        public static WarmupType FromProto(WARMUPTYPE warmupType)
        {
            switch (warmupType)
            {
                case WARMUPTYPE.Fast:
                    return WarmupType.Fast;
                case WARMUPTYPE.Full:
                    return WarmupType.Full;
                case WARMUPTYPE.Unspecified:
                default:
                    throw new InvalidCastException($"Failed to cast WarmupType {warmupType}");
            }
        }
        public static WARMUPTYPE ToProto(WarmupType warmupType)
        {
            switch (warmupType)
            {
                case WarmupType.Fast:
                    return WARMUPTYPE.Fast;
                case WarmupType.Full:
                    return WARMUPTYPE.Full;
                default:
                    throw new InvalidCastException($"Failed to cast WarmupType {warmupType.ToString()}");
            }
        }

        public static SEVERITY ToProto(LogRecordSeverity severity)
        {
            switch (severity)
            {
                case LogRecordSeverity.Unspecified:
                    return SEVERITY.Unspecified;
                case LogRecordSeverity.Info:
                    return SEVERITY.Info;
                case LogRecordSeverity.Warn:
                    return SEVERITY.Warn;
                case LogRecordSeverity.Error:
                    return SEVERITY.Error;
                default:
                    throw new InvalidCastException($"Failed to cast severity {severity.ToString()}");
            }
        }

        public static LogRecordSeverity FromProto(SEVERITY severity)
        {
            switch (severity)
            {
                case SEVERITY.Unspecified:
                    return LogRecordSeverity.Unspecified;
                case SEVERITY.Info:
                    return LogRecordSeverity.Info;
                case SEVERITY.Warn:
                    return LogRecordSeverity.Warn;
                case SEVERITY.Error:
                    return LogRecordSeverity.Error;
                default:
                    throw new InvalidCastException($"Failed to cast severity {severity.ToString()}");
            }
        }

        public static LOGTYPE ToProto(LogRecordType type)
        {
            switch (type)
            {
                case LogRecordType.Unspecified:
                    return LOGTYPE.Unspecified;
                case LogRecordType.System:
                    return LOGTYPE.System;
                case LogRecordType.Security:
                    return LOGTYPE.Security;
                case LogRecordType.User:
                    return LOGTYPE.User;
                case LogRecordType.Database: // todo: check LogRecordType.Database
                case LogRecordType.Error:
                    return LOGTYPE.Error;
                default:
                    throw new InvalidCastException($"Failed to cast logType {type.ToString()}");
            }
        }

        public static LogRecordType FromProto(LOGTYPE type)
        {
            switch (type)
            {
                case LOGTYPE.Unspecified:
                    return LogRecordType.Unspecified;
                case LOGTYPE.System:
                    return LogRecordType.System;
                case LOGTYPE.User:
                    return LogRecordType.User;
                case LOGTYPE.Error:
                    return LogRecordType.Error;
                case LOGTYPE.Security:
                    return LogRecordType.Security;
                default:
                    throw new InvalidCastException($"Failed to cast logType {type.ToString()}");
            }
        }

        public static MagnetometerType FromProto(MAGNETOMETERTYPE magnetometerType)
        {
            switch (magnetometerType)
            {
                case MAGNETOMETERTYPE.Back:
                    return MagnetometerType.Back;
                case MAGNETOMETERTYPE.Front:
                    return MagnetometerType.Front;
                case MAGNETOMETERTYPE.Unspecified:
                default:
                    throw new InvalidCastException("Unknown argument: " + magnetometerType.ToString());
            }
        }
        public static MAGNETOMETERTYPE ToProto(MagnetometerType magnetometerType)
        {
            switch (magnetometerType)
            {
                case MagnetometerType.Back:
                    return MAGNETOMETERTYPE.Back;
                case MagnetometerType.Front:
                    return MAGNETOMETERTYPE.Front;
                default:
                    throw new InvalidCastException("Unknown argument: " + magnetometerType.ToString());
            }
        }


        public static DEVICETYPE ToProto(Core.Enums.DeviceType deviceName)
        {
            switch (deviceName)
            {
                case Core.Enums.DeviceType.CustomFabrication:
                    return DEVICETYPE.CustomFabrication;

                case Core.Enums.DeviceType.InternalEye:
                    return DEVICETYPE.InternalEye;

                case Core.Enums.DeviceType.GammaPutty:
                    return DEVICETYPE.GammaPutty;

                case Core.Enums.DeviceType.ExternalEye:
                    return DEVICETYPE.ExternalEye;

                case Core.Enums.DeviceType.EarCanal:
                    return DEVICETYPE.EarCanal;

                case Core.Enums.DeviceType.IntraNasal:
                    return DEVICETYPE.IntraNasal;

                case Core.Enums.DeviceType.Mastoid:
                    return DEVICETYPE.Mastoid;

                case Core.Enums.DeviceType.DentalPacemaker:
                    return DEVICETYPE.DentalPacemaker;

                case Core.Enums.DeviceType.LeadApron:
                    return DEVICETYPE.LeadApron;

                case Core.Enums.DeviceType.Thyroid:
                    return DEVICETYPE.Thyroid;

                case Core.Enums.DeviceType.LeadGlasses:
                    return DEVICETYPE.LeadGlasses;

                case Core.Enums.DeviceType.HeadHolder:
                    return DEVICETYPE.HeadHolder;

                case Core.Enums.DeviceType.Pillow:
                    return DEVICETYPE.Pillow;

                case Core.Enums.DeviceType.PacemakerShield:
                    return DEVICETYPE.PacemakerShield;

                case Core.Enums.DeviceType.PrefabricatedShield:
                    return DEVICETYPE.PrefabricatedShield;

                case DeviceType.NoShield:
                    return DEVICETYPE.NoShield;

                default:
                    throw new InvalidCastException("Unknown DeviceType: " + deviceName.ToString());
            }
        }

        public static Core.Enums.DeviceType FromProto(DEVICETYPE deviceName)
        {
            switch (deviceName)
            {
                case DEVICETYPE.CustomFabrication:
                    return Core.Enums.DeviceType.CustomFabrication;

                case DEVICETYPE.InternalEye:
                    return Core.Enums.DeviceType.InternalEye;

                case DEVICETYPE.GammaPutty:
                    return Core.Enums.DeviceType.GammaPutty;

                case DEVICETYPE.ExternalEye:
                    return Core.Enums.DeviceType.ExternalEye;

                case DEVICETYPE.EarCanal:
                    return Core.Enums.DeviceType.EarCanal;

                case DEVICETYPE.IntraNasal:
                    return Core.Enums.DeviceType.IntraNasal;

                case DEVICETYPE.Mastoid:
                    return Core.Enums.DeviceType.Mastoid;

                case DEVICETYPE.DentalPacemaker:
                    return Core.Enums.DeviceType.DentalPacemaker;

                case DEVICETYPE.LeadApron:
                    return Core.Enums.DeviceType.LeadApron;

                case DEVICETYPE.Thyroid:
                    return Core.Enums.DeviceType.Thyroid;

                case DEVICETYPE.LeadGlasses:
                    return Core.Enums.DeviceType.LeadGlasses;

                case DEVICETYPE.HeadHolder:
                    return Core.Enums.DeviceType.HeadHolder;

                case DEVICETYPE.Pillow:
                    return Core.Enums.DeviceType.Pillow;

                case DEVICETYPE.PacemakerShield:
                    return Core.Enums.DeviceType.PacemakerShield;

                case DEVICETYPE.PrefabricatedShield:
                    return Core.Enums.DeviceType.PrefabricatedShield;

                case DEVICETYPE.NoShield:
                    return Core.Enums.DeviceType.NoShield;

                case DEVICETYPE.Unspecified:
                default:
                    throw new InvalidCastException("Unknown argument: " + deviceName.ToString());
            }
        }

        public static TREATMENTLOADINGSTATE ToProto(Core.Enums.TreatmentLoadingState treatmentLoadingState)
        {
            switch (treatmentLoadingState)
            {
                case Core.Enums.TreatmentLoadingState.Unloaded:
                    return TREATMENTLOADINGSTATE.Unloaded;
                case Core.Enums.TreatmentLoadingState.PendingLoad:
                    return TREATMENTLOADINGSTATE.Pendingload;
                case Core.Enums.TreatmentLoadingState.PartialPendingLoad:
                    return TREATMENTLOADINGSTATE.Partialpendingload;
                case Core.Enums.TreatmentLoadingState.Loaded:
                    return TREATMENTLOADINGSTATE.Loaded;
                default:
                    throw new InvalidCastException($"Unknown argument: {treatmentLoadingState.ToString()}");
            }
        }

        public static Core.Enums.TreatmentLoadingState FromProto(TREATMENTLOADINGSTATE treatmentLoadingState)
        {
            switch (treatmentLoadingState)
            {
                case TREATMENTLOADINGSTATE.Unloaded:
                    return Core.Enums.TreatmentLoadingState.Unloaded;
                case TREATMENTLOADINGSTATE.Pendingload:
                    return Core.Enums.TreatmentLoadingState.PendingLoad;
                case TREATMENTLOADINGSTATE.Partialpendingload:
                    return Core.Enums.TreatmentLoadingState.PartialPendingLoad;
                case TREATMENTLOADINGSTATE.Loaded:
                    return Core.Enums.TreatmentLoadingState.Loaded;
                case TREATMENTLOADINGSTATE.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {treatmentLoadingState.ToString()}");
            }
        }

        public static PatientIdType FromProto(PATIENTIDTYPE patientIdType)
        {
            switch (patientIdType)
            {
                case PATIENTIDTYPE.Ssn:
                    return PatientIdType.Ssn;

                case PATIENTIDTYPE.Passport:
                    return PatientIdType.Passport;

                case PATIENTIDTYPE.Other:
                    return PatientIdType.Other;

                case PATIENTIDTYPE.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {patientIdType.ToString()}");
            }
        }

        public static PATIENTIDTYPE ToProto(Xcc.Core.Enums.PatientIdType idType)
        {
            switch (idType)
            {
                case PatientIdType.Unspecified:
                    return PATIENTIDTYPE.Unspecified;
                case PatientIdType.Ssn:
                    return PATIENTIDTYPE.Ssn;
                case PatientIdType.Passport:
                    return PATIENTIDTYPE.Passport;
                case PatientIdType.Other:
                    return PATIENTIDTYPE.Other;
                default:
                    throw new InvalidCastException($"Unknown argument: {idType.ToString()}");
            }
        }

        public static Core.Enums.PatientStatus FromProto(PATIENTSTATUS patientStatus)
        {
            switch (patientStatus)
            {
                case PATIENTSTATUS.Active:
                    return Core.Enums.PatientStatus.Active;

                case PATIENTSTATUS.Inactive:
                    return Core.Enums.PatientStatus.Inactive;

                case PATIENTSTATUS.Expired:
                    return Core.Enums.PatientStatus.Expired;

                case PATIENTSTATUS.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {patientStatus.ToString()}");
            }
        }

        public static PATIENTSTATUS ToProto(Core.Enums.PatientStatus patientStatus)
        {
            switch (patientStatus)
            {
                case Core.Enums.PatientStatus.Active:
                    return PATIENTSTATUS.Active;

                case Core.Enums.PatientStatus.Inactive:
                    return PATIENTSTATUS.Inactive;

                case Core.Enums.PatientStatus.Expired:
                    return PATIENTSTATUS.Expired;

                default:
                    throw new InvalidCastException($"Unknown argument: {patientStatus.ToString()}");
            }
        }

        public static Core.Enums.Energy FromProto(ENERGY energy)
        {
            switch (energy)
            {
                case ENERGY._50:
                    return Core.Enums.Energy.Energy_50;

                case ENERGY._70:
                    return Core.Enums.Energy.Energy_70;

                case ENERGY._100:
                    return Core.Enums.Energy.Energy_100;

                case ENERGY.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {energy.ToString()}");
            }
        }

        public static ENERGY ToProto(Core.Enums.Energy? energy)
        {
            switch (energy)
            {
                case Core.Enums.Energy.Energy_50:
                    return ENERGY._50;
                case Core.Enums.Energy.Energy_70:
                    return ENERGY._70;
                case Core.Enums.Energy.Energy_100:
                    return ENERGY._100;
                default:
                    throw new InvalidCastException($"Unknown argument: {energy.ToString()}");
            }
        }

        public static Core.Enums.TDF FromProto(Com.Empyreanmed.Heracles.Enums.V1.TDF tdf)
        {
            switch (tdf)
            {
                case TDF._94:
                    return Core.Enums.TDF.Tdf_94;
                case TDF._95:
                    return Core.Enums.TDF.Tdf_95;
                case TDF._96:
                    return Core.Enums.TDF.Tdf_96;
                case TDF._97:
                    return Core.Enums.TDF.Tdf_97;
                case TDF._98:
                    return Core.Enums.TDF.Tdf_98;
                case TDF._99:
                    return Core.Enums.TDF.Tdf_99;
                case TDF._100:
                    return Core.Enums.TDF.Tdf_100;
                case TDF._101:
                    return Core.Enums.TDF.Tdf_101;
                case TDF._102:
                    return Core.Enums.TDF.Tdf_102;
                case TDF.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {tdf.ToString()}");
            }
        }

        public static Com.Empyreanmed.Heracles.Enums.V1.TDF ToProto(Core.Enums.TDF? tdf)
        {
            switch (tdf)
            {
                case Core.Enums.TDF.Tdf_94:
                    return TDF._94;
                case Core.Enums.TDF.Tdf_95:
                    return TDF._95;
                case Core.Enums.TDF.Tdf_96:
                    return TDF._96;
                case Core.Enums.TDF.Tdf_97:
                    return TDF._97;
                case Core.Enums.TDF.Tdf_98:
                    return TDF._98;
                case Core.Enums.TDF.Tdf_99:
                    return TDF._99;
                case Core.Enums.TDF.Tdf_100:
                    return TDF._100;
                case Core.Enums.TDF.Tdf_101:
                    return TDF._101;
                case Core.Enums.TDF.Tdf_102:
                    return TDF._102;
                default:
                    throw new InvalidCastException($"Unknown argument: {tdf.ToString()}");
            }
        }

        public static STATUS ToProto(Core.Enums.Status status)
        {
            switch (status)
            {
                case Core.Enums.Status.PENDING_APPROVAL:
                    return STATUS.PendingApproval;
                case Core.Enums.Status.APPROVED:
                    return STATUS.Approved;
                case Core.Enums.Status.REJECTED:
                    return STATUS.Rejected;
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        public static STATUS ToProto(Core.Enums.PlanStatus status)
        {
            switch (status)
            {
                case Core.Enums.PlanStatus.PENDING_APPROVAL:
                    return STATUS.PendingApproval;
                case Core.Enums.PlanStatus.APPROVED:
                    return STATUS.Approved;
                case Core.Enums.PlanStatus.REJECTED:
                    return STATUS.Rejected;
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        /// <summary>
        /// Workaround to make a distinction between Status and PlanStatus enums
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static Core.Enums.PlanStatus FromProtoToPlanStatus(STATUS status)
        {
            switch (status)
            {
                case STATUS.PendingApproval:
                    return Core.Enums.PlanStatus.PENDING_APPROVAL;
                case STATUS.Approved:
                    return Core.Enums.PlanStatus.APPROVED;
                case STATUS.Rejected:
                    return Core.Enums.PlanStatus.REJECTED;
                case STATUS.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        public static STATUS ToProto(Core.Enums.SimulationStatus status)
        {
            switch (status)
            {
                case Core.Enums.SimulationStatus.Pending:
                    return STATUS.PendingApproval;
                case Core.Enums.SimulationStatus.Approved:
                    return STATUS.Approved;
                //case Core.Enums.SimulationStatus.Active:                    
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        public static Core.Enums.SimulationStatus FromProtoToSimulationStatus(STATUS status)
        {
            switch (status)
            {
                case STATUS.PendingApproval:
                    return Core.Enums.SimulationStatus.Pending;
                case STATUS.Approved:
                    return Core.Enums.SimulationStatus.Approved;
                //case STATUS.:
                //    return Core.Enums.SimulationStatus.Active;
                case STATUS.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        public static Core.Enums.Status FromProto(STATUS status)
        {
            switch (status)
            {
                case STATUS.PendingApproval:
                    return Core.Enums.Status.PENDING_APPROVAL;
                case STATUS.Approved:
                    return Core.Enums.Status.APPROVED;
                case STATUS.Rejected:
                    return Core.Enums.Status.REJECTED;
                case STATUS.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {status.ToString()}");
            }
        }

        public static Core.Enums.TargetType FromProto(TARGETTYPE targetType)
        {
            switch (targetType)
            {
                case TARGETTYPE.QcCollimator:
                    return Core.Enums.TargetType.TargetType_QC_Collimator;

                case TARGETTYPE.ImvbCollimator5MmCell:
                    return Core.Enums.TargetType.TargetType_61_Fields;

                case TARGETTYPE.ImvbCollimator6MmspotLargecentralCell:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_13_Fields;

                case TARGETTYPE._50MmSsd15MmField:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_15mm_Field;

                case TARGETTYPE._50MmSsd20MmField:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_20mm_Field;

                case TARGETTYPE._50MmSsd30MmField:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_30mm_Field;

                case TARGETTYPE._50MmSsd40MmField:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_40mm_Field;

                case TARGETTYPE._50MmSsd50MmField:
                    return Core.Enums.TargetType.TargetType_50mm_SSD_50mm_Field;

                case TARGETTYPE.ImvbCollimator5CmSsd0Point5CmField05MmCell:
                    return Core.Enums.TargetType.TargetType_30mm_SSD_7_Fields;

                default:
                    throw new InvalidCastException($"Unsupported applicator: {targetType.ToString()}");
            }
        }

        public static TARGETTYPE ToProto(Core.Enums.TargetType targetType)
        {
            switch (targetType)
            {
                case Core.Enums.TargetType.TargetType_QC_Collimator:
                    return TARGETTYPE.QcCollimator;

                case Core.Enums.TargetType.TargetType_61_Fields:
                    return TARGETTYPE.ImvbCollimator5MmCell;

                case Core.Enums.TargetType.TargetType_50mm_SSD_13_Fields:
                    return TARGETTYPE.ImvbCollimator6MmspotLargecentralCell;

                case Core.Enums.TargetType.TargetType_50mm_SSD_15mm_Field:
                    return TARGETTYPE._50MmSsd15MmField;

                case Core.Enums.TargetType.TargetType_50mm_SSD_20mm_Field:
                    return TARGETTYPE._50MmSsd20MmField;

                case Core.Enums.TargetType.TargetType_50mm_SSD_30mm_Field:
                    return TARGETTYPE._50MmSsd30MmField;

                case Core.Enums.TargetType.TargetType_50mm_SSD_40mm_Field:
                    return TARGETTYPE._50MmSsd40MmField;

                case Core.Enums.TargetType.TargetType_50mm_SSD_50mm_Field:
                    return TARGETTYPE._50MmSsd50MmField;

                case Core.Enums.TargetType.TargetType_30mm_SSD_7_Fields:
                    return TARGETTYPE.ImvbCollimator5CmSsd0Point5CmField05MmCell;

                default:
                    throw new InvalidCastException($"Unsupported applicator: {targetType.ToString()}");
            }
        }

        /// <summary>
        /// Converts Treatment field name type from protos enum, supposing that the names are the same
        /// </summary>
        public static Core.Enums.TreatmentFieldName FromProto(FIELDNAME fieldName)
        {
            if (!Enum.IsDefined(fieldName))
            {
                throw new InvalidCastException($"Invalid fieldName value: {fieldName}");
            }
            try
            {
                return (Core.Enums.TreatmentFieldName)Enum.Parse(typeof(Core.Enums.TreatmentFieldName), fieldName.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Unsupported treatment field name value: {fieldName.ToString()}", ex);
            }
        }

        /// <summary>
        /// Converts Treatment field name type to protos enum, supposing that the names are the same
        /// </summary>
        public static FIELDNAME ToProto(Core.Enums.TreatmentFieldName fieldName)
        {
            if (!Enum.IsDefined(fieldName))
            {
                throw new InvalidCastException($"Invalid input fieldName value: {fieldName}");
            }
            try
            {
                return (FIELDNAME)Enum.Parse(typeof(FIELDNAME), fieldName.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Unsupported treatment field name value: {fieldName.ToString()}", ex);
            }
        }

        /// <summary>
        /// Converts Site location type from protos enum, supposing that the names are the same
        /// </summary>
        public static Core.Enums.SiteLocation? FromProto(SITELOCATION location)
        {
            if (!Enum.IsDefined(location))
            {
                throw new InvalidCastException($"Invalid site location value: {location}");
            }
            try
            {
                if (location == SITELOCATION.Unspecified)
                {
                    return null;
                }
                else
                {
                    return (Core.Enums.SiteLocation)Enum.Parse(typeof(Core.Enums.SiteLocation), location.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Unsupported site location value: {location.ToString()}", ex);
            }
        }

        /// <summary>
        /// Converts Site location type to protos enum, supposing that the names are the same
        /// </summary>
        public static SITELOCATION ToProto(Core.Enums.SiteLocation? location)
        {
            if (location is null || !Enum.IsDefined(location.Value))
            {
                throw new InvalidCastException($"Invalid input site location value: {location}");
            }
            try
            {
                return (SITELOCATION)Enum.Parse(typeof(SITELOCATION), location.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Unsupported site location value: {location.ToString()}", ex);
            }
        }

        public static Sex? FromProto(SEXTYPE sex)
        {
            switch (sex)
            {

                case SEXTYPE.Male:
                    return Sex.Male;

                case SEXTYPE.Female:
                    return Sex.Female;

                case SEXTYPE.Intersex:
                    return Sex.Intersex;

                case SEXTYPE.Unspecified:
                    return null;

                default:
                    throw new InvalidCastException($"Unknown argument: {sex.ToString()}");
            }
        }

        public static PATHOLOGY ToProto(Core.Enums.Pathology? pathology)
        {
            switch (pathology)
            {
                case Core.Enums.Pathology.Bcc:
                    return PATHOLOGY.Bcc;

                case Core.Enums.Pathology.Scc:
                    return PATHOLOGY.Scc;

                case Core.Enums.Pathology.SccIs:
                    return PATHOLOGY.SccIs;

                case Core.Enums.Pathology.Keloid:
                    return PATHOLOGY.Keloid;

                case Core.Enums.Pathology.Basosquamous:
                    return PATHOLOGY.Basosquamous;

                default:
                    throw new InvalidCastException($"Unknown argument: {pathology.ToString()}");
            }
        }

        public static Core.Enums.Pathology? FromProto(PATHOLOGY pathology)
        {
            switch (pathology)
            {
                case PATHOLOGY.Bcc:
                    return Core.Enums.Pathology.Bcc;

                case PATHOLOGY.Scc:
                    return Core.Enums.Pathology.Scc;

                case PATHOLOGY.SccIs:
                    return Core.Enums.Pathology.SccIs;

                case PATHOLOGY.Keloid:
                    return Core.Enums.Pathology.Keloid;

                case PATHOLOGY.Basosquamous:
                    return Core.Enums.Pathology.Basosquamous;

                case PATHOLOGY.Unspecified:
                    return null;
                default:
                    throw new InvalidCastException($"Unknown argument: {pathology.ToString()}");
            }
        }

        public static SEXTYPE ToProto(Sex? sex)
        {
            switch (sex)
            {
                case null:
                    return SEXTYPE.Unspecified;

                case Sex.Male:
                    return SEXTYPE.Male;

                case Sex.Female:
                    return SEXTYPE.Female;

                case Sex.Intersex:
                    return SEXTYPE.Intersex;

                default:
                    throw new InvalidCastException($"Unknown argument: {sex.ToString()}");
            }
        }
        public static Core.Enums.PatientPosition FromProto(POSITION patientPosition)
        {
            switch (patientPosition)
            {
                case POSITION.Prone:
                    return Core.Enums.PatientPosition.Prone;

                case POSITION.Supine:
                    return Core.Enums.PatientPosition.Supine;

                case POSITION.Sitting:
                    return Core.Enums.PatientPosition.Sitting;

                case POSITION.LyingRt:
                    return Core.Enums.PatientPosition.LyingRT;

                case POSITION.LyingLt:
                    return Core.Enums.PatientPosition.LyingLT;

                case POSITION.HeadLeft:
                    return Core.Enums.PatientPosition.HeadLeft;

                case POSITION.HeadRight:
                    return Core.Enums.PatientPosition.HeadRight;

                case POSITION.Unspecified:
                default:
                    throw new InvalidCastException($"Unknown argument: {patientPosition.ToString()}");
            }
        }

        public static POSITION ToProto(Core.Enums.PatientPosition patientPosition)
        {
            switch (patientPosition)
            {
                case Core.Enums.PatientPosition.Prone:
                    return POSITION.Prone;

                case Core.Enums.PatientPosition.Supine:
                    return POSITION.Supine;

                case Core.Enums.PatientPosition.Sitting:
                    return POSITION.Sitting;

                case Core.Enums.PatientPosition.LyingRT:
                    return POSITION.LyingRt;

                case Core.Enums.PatientPosition.LyingLT:
                    return POSITION.LyingLt;

                case Core.Enums.PatientPosition.HeadLeft:
                    return POSITION.HeadLeft;

                case Core.Enums.PatientPosition.HeadRight:
                    return POSITION.HeadRight;

                default:
                    throw new InvalidCastException($"Unknown argument: {patientPosition.ToString()}");
            }
        }

        #endregion

        public static IPatient FromProto(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            return new Heracles.Application.Models.Patient()
            {
                Id = patient.Id,
                CreationDate = FromTimestamp(patient.CreationDate),
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                MiddleName = patient.MiddleName,
                Sex = FromProto(patient.Sex),
                PatientId = patient.PatientId,
                PatientIdType = FromProto(patient.PatientIdType),
                DOB = DateFromTimestamp(patient.Dob),
                MRN = patient.Mrn,
                Notes = patient.Notes,
                Address = patient.Address,
                City = patient.City,
                Country = patient.Country,
                Email = string.IsNullOrEmpty(patient.Email) ? null : patient.Email,
                Ethnicity = patient.Ethnicity,
                ProviderId = patient.ProviderId,
                Race = patient.Race,
                State = patient.State,
                Zip = patient.Zip,
                Phone = string.IsNullOrEmpty(patient.Phone) ? null : patient.Phone,
                Picture = patient.Picture,
                Status = FromProto(patient.Status)
            };
        }

        public static Patient ToProto(IPatient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            var protoPatient = new Patient
            {
                Address = patient.Address ?? string.Empty,
                City = patient.City ?? string.Empty,
                State = patient.State ?? string.Empty,
                Country = patient.Country ?? string.Empty,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                MiddleName = patient.MiddleName ?? string.Empty,
                Mrn = patient.MRN ?? string.Empty,
                Zip = patient.Zip ?? string.Empty,
                PatientId = patient.PatientId ?? string.Empty,
                PatientIdType = ToProto(patient.PatientIdType),
                Email = patient.Email ?? string.Empty,
                Dob = ToTimestamp(patient.DOB.Value),
                Sex = ToProto(patient.Sex),
                Race = patient.Race ?? string.Empty,
                ProviderId = patient.ProviderId,
                Ethnicity = patient.Ethnicity ?? string.Empty,
                Phone = patient.Phone ?? string.Empty,
                Notes = patient.Notes ?? string.Empty,
                Picture = patient.Picture ?? string.Empty,
                Status = ToProto(patient.Status)
            };
            if (!BaseEntry.IsBlankEntry(patient))
            {
                protoPatient.Id = patient.Id;
            }
            return protoPatient;
        }

        public static IDiagnosis FromProto(Diagnosis diagnosis)
        {
            if (diagnosis == null) throw new ArgumentNullException(nameof(diagnosis));

            var d = new Application.Models.RDBMS.EMR.Diagnosis
            {
                CreationDate = FromTimestamp(diagnosis.CreationDate),
                Id = diagnosis.Id,
                Pathology = FromProto(diagnosis.Pathology),
                PatientId = diagnosis.PatientId,
                Referring = diagnosis.Referring,
                SiteName = diagnosis.SiteName,
                SiteLocation = FromProto(diagnosis.SiteLocation),
                SubcellOne = diagnosis.SubCellTypeOne == CELLTYPE.None ? null : FromProto(diagnosis.SubCellTypeOne),
                SubcellTwo = diagnosis.SubCellTypeTwo == CELLTYPE.None ? null : FromProto(diagnosis.SubCellTypeTwo),
                Description = diagnosis.HasDescription ? FromProto(diagnosis.Description) : null,
                IcdCode = FromProto(diagnosis.IcdCode),
                Archived = diagnosis.Archived
            };

            return d;
        }

        public static Diagnosis ToProto(IDiagnosis diagnosis)
        {
            if (diagnosis == null) throw new ArgumentNullException(nameof(diagnosis));

            var proto = new Diagnosis
            {
                Pathology = ToProto(diagnosis.Pathology),
                PatientId = diagnosis.PatientId,
                Referring = diagnosis.Referring,
                SiteName = diagnosis.SiteName,
                SiteLocation = ToProto(diagnosis.SiteLocation),
                SubCellTypeOne = diagnosis.SubcellOne == null ? CELLTYPE.None : ToProto(diagnosis.SubcellOne),
                SubCellTypeTwo = diagnosis.SubcellTwo == null ? CELLTYPE.None : ToProto(diagnosis.SubcellTwo),
                IcdCode = ToProto(diagnosis.IcdCode),
                Archived = diagnosis.Archived                
            };

            if (diagnosis.Description is not null)
                proto.Description = ToProto(diagnosis.Description);

            if (!BaseEntry.IsBlankEntry(diagnosis))
            {
                proto.Id = diagnosis.Id;
            }
            return proto;
        }

        public static IPrescription FromProto(Prescription prescription)
        {
            if (prescription == null)
                throw new ArgumentNullException(nameof(prescription));

            return new Application.Models.RDBMS.EMR.Prescription
            {
                Id = prescription.Id,
                CreationDate = FromTimestamp(prescription.CreationDate),
                DailyDose = prescription.DailyDose,
                DwellTime = prescription.DwellTime,
                Energy = FromProto(prescription.Energy),
                //MinTdf = ToProto(prescription.MinTdf) //todo: minTdf should be type of Tdf
                MinTdf = FromProto((TDF)prescription.MinTdf),
                TotalDose = prescription.TotalDose,
                NumberOfFxs = prescription.NumberOfFxs,
                SimulationId = prescription.SimulationId,
                Status = FromProto(prescription.Status),
                Tdf = FromProto(prescription.Tdf),
                FxsPerWeek = prescription.TxsPerWeek
            };
        }

        public static Prescription ToProto(IPrescription prescription)
        {
            if (prescription == null)
                throw new ArgumentNullException(nameof(prescription));

            var protoPrescription = new Prescription
            {
                MinTdf = ToProto(prescription.MinTdf),
                NumberOfFxs = prescription.NumberOfFxs,
                SimulationId = prescription.SimulationId,
                Status = ToProto(prescription.Status),
                Tdf = ToProto(prescription.Tdf),
                TxsPerWeek = prescription.FxsPerWeek,
                DwellTime = prescription.DwellTime,
                Energy = ToProto(prescription.Energy),
                TotalDose = (float)prescription.TotalDose,
                DailyDose = (float)prescription.DailyDose
            };
            if (!BaseEntry.IsBlankEntry(prescription))
            {
                protoPrescription.Id = prescription.Id;
            }
            return protoPrescription;
        }

        public static ISimulation FromProto(Simulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            return new Models.RDBMS.EMR.Simulation
            {
                CreationDate = FromTimestamp(simulation.CreationDate),
                DiagnosisId = simulation.DiagnosisId,
                Id = simulation.Id,
                LesionDepth = simulation.HasLesionDepth && simulation.LesionDepth > 0 ? simulation.LesionDepth : null,
                LesionSizeL = simulation.LesionSizeL,
                LesionSizeW = simulation.LesionSizeW,
                MarginSizeL = simulation.MarginSizeL,
                MarginSizeW = simulation.MarginSizeW,
                PerformedBy = simulation.PerformedBy,
                SetupNote = simulation.SetupNote,
                ShieldSizeL = simulation.ShieldSizeL,
                ShieldSizeW = simulation.ShieldSizeW,
                Status = FromProtoToSimulationStatus(simulation.Status),
                TargetType = FromProto(simulation.TargetType),
                VisitId = simulation.VisitId
                //ApplicatorSize = simulation.ApplicatorSize,
                //TotalFractions = simulation.TotalFractions,
            };
        }

        public static Simulation ToProto(ISimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var proto = new Simulation()
            {
                DiagnosisId = simulation.DiagnosisId,
                LesionSizeL = simulation.LesionSizeL.Value,
                LesionSizeW = simulation.LesionSizeW.Value,
                MarginSizeL = simulation.MarginSizeL.Value,
                MarginSizeW = simulation.MarginSizeW.Value,
                PerformedBy = simulation.PerformedBy,
                SetupNote = simulation.SetupNote ?? string.Empty,
                ShieldSizeL = simulation.ShieldSizeL.Value,
                ShieldSizeW = simulation.ShieldSizeW.Value,
                Status = ToProto(simulation.Status),
                VisitId = simulation.VisitId,
                TargetType = ToProto(simulation.TargetType)
            };

            //if (simulation.SetupNote != null)
            //{
            //    proto.SetupNote = simulation.SetupNote;
            //}

            if (simulation.LesionDepth != null)
            {
                proto.LesionDepth = simulation.LesionDepth.Value;
            }

            if (!BaseEntry.IsBlankEntry(simulation))
            {
                proto.Id = simulation.Id;
                //proto.VisitId = simulation.VisitId;
            }
            return proto;
        }

        public static IVisit FromProto(Visit visit)
        {
            if (visit == null)
                throw new ArgumentNullException(nameof(visit));

            return new Application.Models.RDBMS.EMR.Visit
            {
                CreationDate = FromTimestamp(visit.CreationDate),
                Id = visit.Id,
                PatientId = visit.PatientId,
                Type = FromProto(visit.Type)
            };
        }

        public static Visit ToProto(IVisit visit)
        {
            if (visit == null)
                throw new ArgumentNullException(nameof(visit));

            var protoVisit = new Visit
            {
                PatientId = visit.PatientId,
                Type = ToProto(visit.Type)
            };

            if (!BaseEntry.IsBlankEntry(visit))
            {
                protoVisit.Id = visit.Id;
            }
            return protoVisit;
        }

        public static IPlan FromProto(Plan plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            return new Application.Models.RDBMS.EMR.Plan
            {
                Id = plan.Id,
                PrescriptionId = plan.PrescriptionId,
                CollimatorType = FromProto(plan.TargetType),
                ApprovedBy = plan.ApprovedBy,
                CreationDate = FromTimestamp(plan.CreationDate),
                Status = FromProtoToPlanStatus(plan.Status),
                TreatmentLoadingState = FromProto(plan.TreatmentLoadingState)
            };
        }

        public static Plan ToProto(IPlan plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            var protoPlan = new Plan
            {
                ApprovedBy = plan.ApprovedBy,
                PrescriptionId = plan.PrescriptionId,
                Status = ToProto(plan.Status),
                TargetType = ToProto(plan.CollimatorType),
                TreatmentLoadingState = ToProto(plan.TreatmentLoadingState)
            };



            if (!BaseEntry.IsBlankEntry(plan))
            {
                protoPlan.Id = plan.Id;
            }
            return protoPlan;
        }

        public static ITreatmentField FromProto(TreatmentField treatmentField)
        {
            if (treatmentField == null)
                throw new ArgumentNullException(nameof(treatmentField));

            return new Application.Models.RDBMS.EMR.TreatmentField
            {
                Id = treatmentField.Id,
                CreationDate = FromTimestamp(treatmentField.CreationDate),
                CalculatedDose = treatmentField.CalculatedDose,
                Current = treatmentField.Current,
                DwellTime = treatmentField.DwellTime,
                Energy = FromProto(treatmentField.Energy),
                Name = FromProto(treatmentField.FieldName),
                PlanId = treatmentField.PlanId
            };
        }

        public static TreatmentField ToProto(ITreatmentField treatmentField)
        {
            if (treatmentField == null)
                throw new ArgumentNullException(nameof(treatmentField));

            var protoTreatmentField = new TreatmentField
            {
                CalculatedDose = treatmentField.CalculatedDose,
                Current = treatmentField.Current,
                DwellTime = treatmentField.DwellTime,
                Energy = ToProto(treatmentField.Energy),
                FieldName = ToProto(treatmentField.Name),
                PlanId = treatmentField.PlanId
            };

            if (!BaseEntry.IsBlankEntry(treatmentField))
            {
                protoTreatmentField.Id = treatmentField.Id;
            }
            return protoTreatmentField;
        }


        public static Treatment ToProto(ITreatment treatment)
        {
            if (treatment == null)
                throw new ArgumentNullException(nameof(treatment));

            var protoTreatment = new Treatment
            {
                PlanId = treatment.PlanId,
                CumulativeDose = Convert.ToDouble(treatment.CumulativeDose),
                DailyDose = Convert.ToDouble(treatment.DailyDose),
                Fraction = treatment.Fraction,
                LesionDepth = Convert.ToDouble(treatment.LesionDepth),
                PerformedBy = treatment.PerformedBy,
                VisitId = treatment.VisitId
            };

            if (!BaseEntry.IsBlankEntry(treatment))
            {
                protoTreatment.Id = treatment.Id;
                //protoTreatment.VisitId = treatment.VisitId;
            }

            return protoTreatment;
        }

        public static ITreatment FromProto(Treatment treatment)
        {
            if (treatment == null)
                throw new ArgumentNullException(nameof(treatment));

            return new Application.Models.RDBMS.EMR.Treatment
            {
                Id = treatment.Id,
                CreationDate = FromTimestamp(treatment.CreationDate),
                PlanId = treatment.PlanId,
                CumulativeDose = treatment.CumulativeDose,
                DailyDose = treatment.DailyDose,
                Fraction = treatment.Fraction,
                LesionDepth = treatment.LesionDepth,
                PerformedBy = treatment.PerformedBy,
                VisitId = treatment.VisitId
            };
        }

        public static IUser FromProto(Com.Empyreanmed.Heracles.Users.V1.User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new Xcc.Core.Domain.DataManagement.Common.Users.User
            {
                Id = user.Id,
                CreationDate = FromTimestamp(user.CreationDate),
                EmailAddress = user.EmailAddress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                LastAccessed = FromTimestamp(user.LastAccessed),
                Role = new UserRole(user.Role),
                Picture = user.Picture,
                Username = user.Username,
                Password = user.Password,
            };
        }
        public static Com.Empyreanmed.Heracles.Users.V1.User ToProto(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var protoUser = new Com.Empyreanmed.Heracles.Users.V1.User
            {
                Username = user.Username,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                LastAccessed = ToTimestamp(user.LastAccessed),
                EmailAddress = user.EmailAddress,
                Password = user.Password,
                Picture = user.Picture,
                Role = user.Role.Name // TODO: mode UserRole to Core and switch to it
            };

            if (!BaseEntry.IsBlankEntry(user))
            {
                protoUser.Id = user.Id;
            }

            return protoUser;
        }


        public static RoleRecord FromProto(Com.Empyreanmed.Heracles.Roles.V1.Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return new RoleRecord()
            {
                Id = role.Id,
                Description = role.Description,
                Name = role.RoleName
            };
        }

        public static Com.Empyreanmed.Heracles.Roles.V1.Role ToProto(RoleRecord role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var proto = new Com.Empyreanmed.Heracles.Roles.V1.Role
            {
                Description = role.Description,
                RoleName = role.Name
            };

            if (!BaseEntry.IsBlankEntry(role))
            {
                proto.Id = role.Id;
            }

            return proto;
        }

        public static UserRoleRecord FromProto(Com.Empyreanmed.Heracles.UserRoles.V1.UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            return new UserRoleRecord()
            {
                Id = userRole.Id,
                RoleId = userRole.RoleId,
                UserEmail = userRole.UserId
            };
        }

        public static Com.Empyreanmed.Heracles.UserRoles.V1.UserRole ToProto(UserRoleRecord userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            var proto = new Com.Empyreanmed.Heracles.UserRoles.V1.UserRole
            {
                RoleId = userRole.RoleId,
                //UserId = userRole.UserId
                UserId = userRole.UserEmail
            };

            if (!BaseEntry.IsBlankEntry(userRole))
            {
                proto.Id = userRole.Id;
            }

            return proto;
        }

        public static PermissionRecord FromProto(Com.Empyreanmed.Heracles.RolesPermissions.V1.RolesPermissions rolePermissions)
        {
            if (rolePermissions == null)
                throw new ArgumentNullException(nameof(rolePermissions));

            return new PermissionRecord()
            {
                Id = rolePermissions.Id,
                RoleId = rolePermissions.RoleId,
                Type = FromProto(rolePermissions.Permission)
            };
        }

        public static Com.Empyreanmed.Heracles.RolesPermissions.V1.RolesPermissions ToProto(PermissionRecord rolePermission)
        {
            if (rolePermission == null)
                throw new ArgumentNullException(nameof(rolePermission));

            var proto = new Com.Empyreanmed.Heracles.RolesPermissions.V1.RolesPermissions
            {
                RoleId = rolePermission.RoleId,
                Permission = ToProto(rolePermission.Type)
            };

            if (!BaseEntry.IsBlankEntry(rolePermission))
            {
                proto.Id = rolePermission.Id;
            }

            return proto;
        }

        public static ITreatmentDevice FromProto(TreatmentDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            return new Application.Models.RDBMS.EMR.TreatmentDevice
            {
                Id = device.Id,
                CreationDate = FromTimestamp(device.CreationDate),
                DeviceName = FromProto(device.DeviceName),
                SimulationId = device.SimulationId
            };
        }

        public static TreatmentDevice ToProto(ITreatmentDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var protoDevice = new TreatmentDevice
            {
                DeviceName = ToProto(device.DeviceName),
                SimulationId = device.SimulationId
            };

            if (!BaseEntry.IsBlankEntry(device))
            {
                protoDevice.Id = device.Id;
            }

            return protoDevice;
        }

        public static IPatientPosition FromProto(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            return new Application.Models.RDBMS.EMR.PatientPositionEntry
            {
                Id = position.Id,
                CreationDate = FromTimestamp(position.CreateDate),
                Position = FromProto(position.PatientPosition),
                SimulationId = position.SimulationId
            };
        }

        public static Position ToProto(IPatientPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            var outValue = new Position
            {
                PatientPosition = ToProto(position.Position),
                SimulationId = position.SimulationId
            };

            if (!BaseEntry.IsBlankEntry(position))
            {
                outValue.Id = position.Id;
            }
            return outValue;
        }

        public static Photo ToProto(IPhotoDescription photoDescription)
        {
            if (photoDescription == null)
                throw new ArgumentNullException(nameof(photoDescription));

            var proto = new Photo
            {
                Description = photoDescription.Description,
                DiagnosisId = photoDescription.DiagnosisId,
                VisitId = photoDescription.VisitId,
                //Path = photo.Path,
                PhotoType = ToProto(photoDescription.Type),
                TemplateType = ToProto(photoDescription.TemplateType)
            };

            if (!BaseEntry.IsBlankEntry(photoDescription))
            {
                proto.Id = photoDescription.Id;
            }

            return proto;
        }

        public static IPhotoDescription FromProto(Photo photo)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            return new Application.Models.RDBMS.EMR.PhotoDescription
            {
                Id = photo.Id,
                DiagnosisId = photo.DiagnosisId,
                CreationDate = FromTimestamp(photo.CreationDate),
                Description = photo.Description,
                //Path = photo.Path,
                Type = FromProto(photo.PhotoType),
                TemplateType = FromProto(photo.TemplateType),
                VisitId = photo.VisitId
            };
        }

        public static ISafetyCheck FromProto(SafetyCheck safetyCheck)
        {
            if (safetyCheck == null)
                throw new ArgumentNullException(nameof(safetyCheck));

            return new Models.QualityCheck.SafetyCheck
            {
                Id = safetyCheck.Id,
                CreationDate = FromTimestamp(safetyCheck.CreateDate),
                Energy = FromProto(safetyCheck.Energy),
                Duration = safetyCheck.Duration,
                PerformedBy = safetyCheck.PerformedBy,
                DoorInterlock = safetyCheck.DoorInterlock,
                Dose = safetyCheck.Dose,
                EStop = safetyCheck.EStop,
                LiveAudio = safetyCheck.LiveAudio,
                LiveVideo = safetyCheck.LiveVideo,
                SStop = safetyCheck.SStop,
                XRayLight = safetyCheck.XRayLight,
                XRaySound = safetyCheck.XRaySound
            };
        }

        public static SafetyCheck ToProto(ISafetyCheck safetyCheck)
        {
            if (safetyCheck == null)
                throw new ArgumentNullException(nameof(safetyCheck));

            var proto = new SafetyCheck
            {
                Duration = safetyCheck.Duration,
                Energy = ToProto(safetyCheck.Energy),
                DoorInterlock = safetyCheck.DoorInterlock,
                Dose = safetyCheck.Dose,
                EStop = safetyCheck.EStop,
                LiveAudio = safetyCheck.LiveAudio,
                LiveVideo = safetyCheck.LiveVideo,
                SStop = safetyCheck.SStop,
                XRayLight = safetyCheck.XRayLight,
                XRaySound = safetyCheck.XRaySound,
                PerformedBy = safetyCheck.PerformedBy
            };

            if (!BaseEntry.IsBlankEntry(safetyCheck))
            {
                proto.Id = safetyCheck.Id;
            }

            return proto;
        }

        public static IIntensity FromProto(Intensity intensity)
        {
            if (intensity == null)
                throw new ArgumentNullException(nameof(intensity));

            return new Xcc.Application.Domain.QualityAssurance.Intensity
            {
                Id = intensity.Id,
                CreationDate = FromTimestamp(intensity.CreateDate),
                DiodeName = intensity.DiodeName,
                IntensityValue = intensity.Intensity_,
                QcSampleFieldId = intensity.QcsampleFieldsId
            };
        }

        public static Intensity ToProto(IIntensity intensity)
        {
            if (intensity == null)
                throw new ArgumentNullException(nameof(intensity));

            var proto = new Intensity
            {
                DiodeName = intensity.DiodeName,
                Intensity_ = intensity.IntensityValue,
                QcsampleFieldsId = intensity.QcSampleFieldId
            };

            if (!BaseEntry.IsBlankEntry(intensity))
            {
                proto.Id = intensity.Id;
            }

            return proto;
        }

        public static IQcSampleField FromProto(QCSampleField qcSampleField)
        {
            if (qcSampleField == null)
                throw new ArgumentNullException(nameof(qcSampleField));

            return new QcSampleField
            {
                Id = qcSampleField.Id,
                CreationDate = FromTimestamp(qcSampleField.CreateDate),
                Name = FromProto(qcSampleField.Field),
                QcSampleId = qcSampleField.QcsampleId
            };
        }
        public static QCSampleField ToProto(IQcSampleField qcSampleField)
        {
            if (qcSampleField == null)
                throw new ArgumentNullException(nameof(qcSampleField));

            var proto = new QCSampleField
            {
                Field = ToProto(qcSampleField.Name),
                QcsampleId = qcSampleField.QcSampleId
            };

            if (!BaseEntry.IsBlankEntry(qcSampleField))
            {
                proto.Id = qcSampleField.Id;
            }

            return proto;
        }

        public static Domain.DataManagement.System.QualityCheck.IQcSampleHeader FromProto(QCSample qcSample)
        {
            if (qcSample == null)
                throw new ArgumentNullException(nameof(qcSample));

            return new QcSampleHeader(qcSample.Id)
            {
                CreationDate = FromTimestamp(qcSample.CreateDate),
                CollimatorConfigurationId = qcSample.CollimatorConfigurationId,
                Duration = qcSample.Duration,
                EmissionCurrent = qcSample.EmissionCurrent,
                HeaterCurrent = qcSample.HeaterCurrent,
                PerformedBy = qcSample.PerformedBy,
                Referenced = qcSample.HasReferenced ? qcSample.Referenced : false,
                ApprovedBy = qcSample.HasApprovedBy ? qcSample.ApprovedBy : string.Empty,
                Notes = qcSample.HasNotes ? qcSample.Notes : string.Empty
            };
        }

        public static QCSample ToProto(Domain.DataManagement.System.QualityCheck.IQcSampleHeader qcSample)
        {
            if (qcSample == null)
                throw new ArgumentNullException(nameof(qcSample));

            var proto = new QCSample
            {
                CollimatorConfigurationId = qcSample.CollimatorConfigurationId,
                Referenced = qcSample.Referenced,
                Duration = qcSample.Duration,
                EmissionCurrent = qcSample.EmissionCurrent,
                HeaterCurrent = qcSample.HeaterCurrent,
                PerformedBy = qcSample.PerformedBy,
                Notes = qcSample.Notes
            };

            if (!BaseEntry.IsBlankEntry(qcSample))
            {
                proto.Id = qcSample.Id;
            }

            return proto;
        }

        public static IHead FromProto(Com.Empyreanmed.Heracles.Head.V1.Head head)
        {
            if (head == null)
                throw new ArgumentNullException(nameof(head));

            return new Domain.DataManagement.System.Collimators.Head
            {
                CreationDate = FromTimestamp(head.CreateDate),
                Id = head.Id,
                IsActive = head.IsActive,
                Serial = head.Serial
            };
        }
        public static Com.Empyreanmed.Heracles.Head.V1.Head ToProto(IHead head)
        {
            if (head == null)
                throw new ArgumentNullException(nameof(head));

            var proto = new Com.Empyreanmed.Heracles.Head.V1.Head
            {
                Serial = head.Serial,
                IsActive = head.IsActive
            };

            if (!BaseEntry.IsBlankEntry(head))
            {
                proto.Id = head.Id;
            }

            return proto;
        }

        public static ICollimator FromProto(Com.Empyreanmed.Heracles.Collimators.V1.Collimator collimator)
        {
            if (collimator == null)
                throw new ArgumentNullException(nameof(collimator));

            return new Collimator
            {
                Id = collimator.Id,
                CreationDate = FromTimestamp(collimator.CreateDate),
                IsActive = collimator.IsActive,
                Serial = collimator.Serial,
                CollimatorConfigurationId = collimator.CollimatorConfigurationId,
                HeadId = collimator.HeadId
            };
        }
        public static Com.Empyreanmed.Heracles.Collimators.V1.Collimator ToProto(ICollimator collimator)
        {
            if (collimator == null)
                throw new ArgumentNullException(nameof(collimator));

            var proto = new Com.Empyreanmed.Heracles.Collimators.V1.Collimator
            {
                IsActive = collimator.IsActive,
                Serial = collimator.Serial,
                CollimatorConfigurationId = collimator.CollimatorConfigurationId,
                HeadId = collimator.HeadId
            };

            if (!BaseEntry.IsBlankEntry(collimator))
            {
                proto.Id = collimator.Id;
            }

            return proto;
        }

        public static ICollimatorConfiguration FromProto(
            Com.Empyreanmed.Heracles.CollimatorConfigurations.V1.CollimatorConfiguration collimatorConfiguration)
        {
            if (collimatorConfiguration == null)
                throw new ArgumentNullException(nameof(collimatorConfiguration));

            return new CollimatorConfiguration
            {
                Id = collimatorConfiguration.Id,
                CreationDate = FromTimestamp(collimatorConfiguration.CreateDate),
                Energy = FromProto(collimatorConfiguration.Energy),
                ReferencedDoseRate = collimatorConfiguration.ReferencedDoseRate,
                SsdType = FromProto(collimatorConfiguration.Ssd),
                Type = FromProto(collimatorConfiguration.Type)
            };
        }
        public static Com.Empyreanmed.Heracles.CollimatorConfigurations.V1.CollimatorConfiguration ToProto(ICollimatorConfiguration collimatorConfiguration)
        {
            if (collimatorConfiguration == null)
                throw new ArgumentNullException(nameof(collimatorConfiguration));

            var proto = new Com.Empyreanmed.Heracles.CollimatorConfigurations.V1.CollimatorConfiguration
            {
                Energy = ToProto(collimatorConfiguration.Energy),
                ReferencedDoseRate = Convert.ToSingle(collimatorConfiguration.ReferencedDoseRate),
                Ssd = ToProto(collimatorConfiguration.SsdType),
                Type = ToProto(collimatorConfiguration.Type)
            };

            if (!BaseEntry.IsBlankEntry(collimatorConfiguration))
            {
                proto.Id = collimatorConfiguration.Id;
            }

            return proto;
        }

        public static IOutputFactor FromProto(Com.Empyreanmed.Heracles.OutputFactors.V1.OutputFactor outputFactor)
        {
            if (outputFactor == null)
                throw new ArgumentNullException(nameof(outputFactor));

            return new OutputFactor
            {
                Id = outputFactor.Id,
                CreationDate = FromTimestamp(outputFactor.CreateDate),
                Factor = outputFactor.Factor,
                FieldName = FromProto(outputFactor.FieldName),
                PresetConfigurationId = outputFactor.PresetConfigurationId
            };
        }

        public static Com.Empyreanmed.Heracles.OutputFactors.V1.OutputFactor ToProto(IOutputFactor outputFactor)
        {
            if (outputFactor == null)
                throw new ArgumentNullException(nameof(outputFactor));

            var proto = new Com.Empyreanmed.Heracles.OutputFactors.V1.OutputFactor
            {
                Factor = (float)outputFactor.Factor.Value,
                FieldName = ToProto(outputFactor.FieldName),
                PresetConfigurationId = outputFactor.PresetConfigurationId
            };

            if (!BaseEntry.IsBlankEntry(outputFactor))
            {
                proto.Id = outputFactor.Id;
            }

            return proto;
        }

        public static IWarmUp FromProto(Warmup warmup)
        {
            if (warmup == null)
                throw new ArgumentNullException(nameof(warmup));

            return new Xcc.Application.Models.RDBMS.WarmUp
            {
                Id = warmup.Id,
                CreationDate = FromTimestamp(warmup.CreateDate),
                Type = FromProto(warmup.WarmupType),
                HeaterCurrent = warmup.HeaterCurrent
            };
        }

        public static Warmup ToProto(IWarmUp warmup)
        {
            if (warmup == null)
                throw new ArgumentNullException(nameof(warmup));

            var protoWarmup = new Warmup
            {
                WarmupType = ToProto(warmup.Type),
                HeaterCurrent = Convert.ToSingle(warmup.HeaterCurrent),
                HeadId = warmup.HeadId
            };

            if (!BaseEntry.IsBlankEntry(warmup))
            {
                protoWarmup.Id = warmup.Id;
            }

            return protoWarmup;
        }

        public static Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration ToProto(IPresetConfiguration presetConfiguration)
        {
            if (presetConfiguration == null)
                throw new ArgumentNullException(nameof(presetConfiguration));

            var proto = new Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration
            {
                CollimatorConfigurationId = presetConfiguration.CollimatorConfigurationId,
                IsActive = presetConfiguration.IsActive,
                IsDefault = presetConfiguration.IsDefault,
                PresetName = presetConfiguration.PresetName
            };

            if (!BaseEntry.IsBlankEntry(presetConfiguration))
            {
                proto.Id = presetConfiguration.Id;
            }

            return proto;
        }

        public static IPresetConfiguration FromProto(Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration presetConfiguration)
        {
            if (presetConfiguration == null)
                throw new ArgumentNullException(nameof(presetConfiguration));

            return new Domain.DataManagement.System.Collimators.PresetConfiguration
            {
                Id = presetConfiguration.Id,
                CollimatorConfigurationId = presetConfiguration.CollimatorConfigurationId,
                CreationDate = FromTimestamp(presetConfiguration.CreateDate),
                IsActive = presetConfiguration.IsActive,
                IsDefault = presetConfiguration.IsDefault,
                PresetName = presetConfiguration.PresetName,
                ApprovedBy = presetConfiguration.HasApprovedBy ? presetConfiguration.ApprovedBy : string.Empty
            };
        }

        public static ActualTreatmentField ToProto(IActualTreatmentField actualTreatmentField)
        {
            if (actualTreatmentField == null)
                throw new ArgumentNullException(nameof(actualTreatmentField));

            var proto = new ActualTreatmentField
            {
                ActualCurrent = actualTreatmentField.ActualCurrent,
                ActualDose = actualTreatmentField.ActualDose,
                ActualDwellTime = actualTreatmentField.ActualDuration,
                ActualEnergy = actualTreatmentField.ActualEnergy,
                Completed = actualTreatmentField.Completed,
                FieldName = ToProto(actualTreatmentField.Name),
                ResumePartial = actualTreatmentField.ResumePartial,
                TreatmentId = actualTreatmentField.TreatmentId,
            };

            if (!BaseEntry.IsBlankEntry(actualTreatmentField))
            {
                proto.Id = actualTreatmentField.Id;
            }

            return proto;
        }

        public static IActualTreatmentField FromProto(ActualTreatmentField actualTreatmentField)
        {
            if (actualTreatmentField == null)
                throw new ArgumentNullException(nameof(actualTreatmentField));

            return new Models.RDBMS.EMR.ActualTreatmentField
            {
                Id = actualTreatmentField.Id,
                CreationDate = FromTimestamp(actualTreatmentField.CreationDate),
                ActualCurrent = actualTreatmentField.ActualCurrent,
                ActualDose = actualTreatmentField.ActualDose,
                ActualDuration = actualTreatmentField.ActualDwellTime,
                ActualEnergy = actualTreatmentField.ActualEnergy,
                Completed = actualTreatmentField.Completed,
                Name = FromProto(actualTreatmentField.FieldName),
                ResumePartial = actualTreatmentField.ResumePartial,
                TreatmentId = actualTreatmentField.TreatmentId
            };
        }

        public static EmissionTreatmentField ToProto(IEmissionTreatmentField emissionTreatmentField)
        {
            if (emissionTreatmentField == null)
                throw new ArgumentNullException(nameof(emissionTreatmentField));

            var proto = new EmissionTreatmentField
            {
                ActualDwellTime = emissionTreatmentField.ActualDwellTime,
                ActualTreatmentFieldId = emissionTreatmentField.ActualTreatmentFieldId
            };

            if (!BaseEntry.IsBlankEntry(emissionTreatmentField))
            {
                proto.Id = emissionTreatmentField.Id;
            }

            return proto;
        }

        public static IEmissionTreatmentField FromProto(EmissionTreatmentField emissionTreatmentField)
        {
            if (emissionTreatmentField == null)
                throw new ArgumentNullException(nameof(emissionTreatmentField));

            return new Models.RDBMS.EMR.EmissionTreatmentField
            {
                Id = emissionTreatmentField.Id,
                CreationDate = FromTimestamp(emissionTreatmentField.CreationDate),
                ActualTreatmentFieldId = emissionTreatmentField.ActualTreatmentFieldId,
                ActualDwellTime = emissionTreatmentField.ActualDwellTime
            };
        }

        public static ICorrectionMatrixEntry FromProto(CorrectionMatrix correctionMatrix)
        {
            if (correctionMatrix == null)
                throw new ArgumentNullException(nameof(correctionMatrix));

            return new Xcc.Application.Domain.System.CorrectionMatrixEntry
            {
                Id = correctionMatrix.Id,
                Cm11 = correctionMatrix.Cm11,
                Cm12 = correctionMatrix.Cm12,
                Cm13 = correctionMatrix.Cm13,
                Cm21 = correctionMatrix.Cm21,
                Cm22 = correctionMatrix.Cm22,
                Cm23 = correctionMatrix.Cm23,
                CreationDate = FromTimestamp(correctionMatrix.CreateDate),
                MagnetometerType = FromProto(correctionMatrix.MagnetometerType),
                PresetConfigurationId = correctionMatrix.PresetConfigurationId
            };
        }

        public static CorrectionMatrix ToProto(ICorrectionMatrixEntry correctionMatrix)
        {
            if (correctionMatrix == null)
                throw new ArgumentNullException(nameof(correctionMatrix));

            var proto = new CorrectionMatrix
            {
                PresetConfigurationId = correctionMatrix.PresetConfigurationId,
                Cm11 = Convert.ToSingle(correctionMatrix.Cm11),
                Cm22 = Convert.ToSingle(correctionMatrix.Cm22),
                Cm23 = Convert.ToSingle(correctionMatrix.Cm23),
                Cm12 = Convert.ToSingle(correctionMatrix.Cm12),
                Cm13 = Convert.ToSingle(correctionMatrix.Cm13),
                Cm21 = Convert.ToSingle(correctionMatrix.Cm21),
                MagnetometerType = ToProto(correctionMatrix.MagnetometerType)
            };

            if (!BaseEntry.IsBlankEntry(correctionMatrix))
            {
                proto.Id = correctionMatrix.Id;
            }

            return proto;
        }

        public static IHeaterCurrentConfig FromProto(HeaterCurrentConfig heaterCurrentConfig)
        {
            if (heaterCurrentConfig == null)
                throw new ArgumentNullException(nameof(heaterCurrentConfig));

            return new Xcc.Application.Domain.System.HeaterCurrentConfig
            {
                Id = heaterCurrentConfig.Id,
                CreationDate = FromTimestamp(heaterCurrentConfig.CreateDate),
                HeaterCurrent = heaterCurrentConfig.HeaterCurrent,
                PresetConfigurationId = heaterCurrentConfig.PresetConfigurationId
            };
        }

        public static HeaterCurrentConfig ToProto(IHeaterCurrentConfig heaterCurrentConfig)
        {
            if (heaterCurrentConfig == null)
                throw new ArgumentNullException(nameof(heaterCurrentConfig));

            var proto = new HeaterCurrentConfig
            {
                PresetConfigurationId = heaterCurrentConfig.PresetConfigurationId,
                HeaterCurrent = Convert.ToSingle(heaterCurrentConfig.HeaterCurrent.Value)
            };

            if (!BaseEntry.IsBlankEntry(heaterCurrentConfig))
            {
                proto.Id = heaterCurrentConfig.Id;
            }

            return proto;
        }

        public static IReferenceFieldEntry FromProto(ReferenceField referenceField)
        {
            if (referenceField == null)
                throw new ArgumentNullException(nameof(referenceField));

            return new ReferenceFieldEntry
            {
                Id = referenceField.Id,
                CreationDate = FromTimestamp(referenceField.CreateDate),
                MagnetometerType = FromProto(referenceField.MagnetometerType),
                PresetConfigurationId = referenceField.PresetConfigurationId,
                Rf11 = referenceField.Rf11,
                Rf21 = referenceField.Rf21,
                Rf31 = referenceField.Rf31
            };
        }

        public static ReferenceField ToProto(IReferenceFieldEntry referenceField)
        {
            if (referenceField == null)
                throw new ArgumentNullException(nameof(referenceField));

            var proto = new ReferenceField
            {
                PresetConfigurationId = referenceField.PresetConfigurationId,
                MagnetometerType = ToProto(referenceField.MagnetometerType),
                Rf11 = Convert.ToSingle(referenceField.Rf11),
                Rf21 = Convert.ToSingle(referenceField.Rf21),
                Rf31 = Convert.ToSingle(referenceField.Rf31)
            };
            if (!BaseEntry.IsBlankEntry(referenceField))
            {
                proto.Id = referenceField.Id;
            }

            return proto;
        }

        public static CoilConfiguration ToProto(ICoilConfigurationEntry coilConfiguration)
        {
            if (coilConfiguration == null)
                throw new ArgumentNullException(nameof(coilConfiguration));

            var proto = new CoilConfiguration
            {
                FieldName = ToProto(coilConfiguration.FieldName),
                FocusCurrent = (float)coilConfiguration.FocusCurrent,
                PresetConfigurationId = coilConfiguration.PresetConfigurationId,
                XDeflectionCurrent = (float)coilConfiguration.XDeflectionCurrent,
                YDeflectionCurrent = (float)coilConfiguration.YDeflectionCurrent
            };

            if (!BaseEntry.IsBlankEntry(coilConfiguration))
            {
                proto.Id = coilConfiguration.Id;
            }

            return proto;
        }

        public static ICoilConfigurationEntry FromProto(CoilConfiguration coilConfiguration)
        {
            if (coilConfiguration == null)
                throw new ArgumentNullException(nameof(coilConfiguration));

            var entry = new Models.CollimatorConfiguration.CoilConfigurationEntry
            {
                Id = coilConfiguration.Id,
                CreationDate = FromTimestamp(coilConfiguration.CreateDate),
                FieldName = FromProto(coilConfiguration.FieldName), 
                PresetConfigurationId = coilConfiguration.PresetConfigurationId,
                XDeflectionCurrent = (double)coilConfiguration.XDeflectionCurrent,
                YDeflectionCurrent = (double)coilConfiguration.YDeflectionCurrent,
                FocusCurrent = (double)coilConfiguration.FocusCurrent
            };

            return entry;
        }

        public static Log ToProto(ILogRecord logRecord)
        {
            if (logRecord == null)
            {
                throw new ArgumentNullException(nameof(logRecord));
            }

            var proto = new Log
            {
                Message = logRecord.Message,
                Timestamp = ToTimestamp(logRecord.TimeStamp),
                Severity = ToProto(logRecord.Severity),
                Type = ToProto(logRecord.Type)
            };

            if (!BaseEntry.IsBlankEntry(logRecord))
            {
                proto.Id = logRecord.Id;
            }

            return proto;
        }

        public static ILogRecord FromProto(Log logRecord)
        {
            if (logRecord == null)
            {
                throw new ArgumentNullException(nameof(logRecord));
            }

            return new LogRecord
            {
                Id = logRecord.Id,
                Message = logRecord.Message,
                Severity = Application.Protos.ProtoTypesConverter.FromProto(logRecord.Severity),
                TimeStamp = FromTimestamp(logRecord.Timestamp),
                Type = Application.Protos.ProtoTypesConverter.FromProto(logRecord.Type)
            };
        }

        public static ISystemSettings FromProto(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException((nameof(settings)));
            }

            return new SystemSettings()
            {
                Id = settings.Id,
                EndPointsConfiguration = new EndPointsConfiguration
                {
                    RecordAndVerifyEndPoint = new SystemEndPoint(settings.RecordAndVerifyIp, settings.RecordAndVerifyPort),
                    DatabaseEndpoint = new SystemEndPoint(settings.DatabaseIp, settings.DatabasePort),
                    ImagingHeadCamEndPoint = new SystemEndPoint(settings.ImagingHeadcamIp, settings.ImagingHeadcamPort),
                    TreatmentHeadCamEndPoint = new SystemEndPoint(settings.TreatmentHeadcamIp, settings.TreatmentHeadcamPort),
                    GCBTelemetryEndPoint = new SystemEndPoint(settings.GcbTelemetryIp, settings.GcbTelemetryPort),
                    GCBCommandsEndPoint = new SystemEndPoint(settings.GcbCommandsIp, settings.GcbCommandsPort),
                    // TODO: we temporarily store qcb in robot's columns
                    QcbCommandsEndPoint = new SystemEndPoint(settings.RoboticRosIp, settings.RoboticRosPort),
                    ImagingServerEndPoint = new SystemEndPoint(settings.DataAcquisitionIp, settings.DataAcquisitionPort),
                    DCDataReconstructionServerEndPoint = new SystemEndPoint(settings.DcDataReconstructionIp, settings.DcDataReconstructionPort),
                    DCDataProgressWebSocketEndPoint = new SystemEndPoint(settings.DcDataProgressWebsocketIp, settings.DcDataProgressWebsocketPort),
                    DCDataReconstructionZmqEndPoint = new SystemEndPoint(settings.DcDataReconstructionZMqIp, settings.DcDataReconstructionZMqPort),
                    DCDatabaseEndPoint = new SystemEndPoint(settings.DcDatabaseIp, settings.DcDatabasePort)
                },
                DeviceSerial = settings.DeviceSerial,
            };
        }

        public static Settings ToProto(ISystemSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException((nameof(settings)));
            }

            var proto = new Settings()
            {
                RecordAndVerifyIp = settings.EndPointsConfiguration.RecordAndVerifyEndPoint.Ip(),
                RecordAndVerifyPort = settings.EndPointsConfiguration.RecordAndVerifyEndPoint.Port.ToString(),
                DatabaseIp = settings.EndPointsConfiguration.DatabaseEndpoint.Ip(),
                DatabasePort = settings.EndPointsConfiguration.DatabaseEndpoint.Port.ToString(),
                ImagingHeadcamIp = settings.EndPointsConfiguration.ImagingHeadCamEndPoint.Ip(),
                ImagingHeadcamPort = settings.EndPointsConfiguration.ImagingHeadCamEndPoint.Port.ToString(),
                TreatmentHeadcamIp = settings.EndPointsConfiguration.TreatmentHeadCamEndPoint.Ip(),
                TreatmentHeadcamPort = settings.EndPointsConfiguration.TreatmentHeadCamEndPoint.Port.ToString(),
                GcbTelemetryIp = settings.EndPointsConfiguration.GCBTelemetryEndPoint.Ip(),
                GcbTelemetryPort = settings.EndPointsConfiguration.GCBTelemetryEndPoint.Port.ToString(),
                GcbCommandsIp = settings.EndPointsConfiguration.GCBCommandsEndPoint.Ip(),
                GcbCommandsPort = settings.EndPointsConfiguration.GCBCommandsEndPoint.Port.ToString(),
                DataAcquisitionIp = settings.EndPointsConfiguration.ImagingServerEndPoint.Ip(),
                DataAcquisitionPort = settings.EndPointsConfiguration.ImagingServerEndPoint.Port.ToString(),

                DcDataReconstructionIp = settings.EndPointsConfiguration.DCDataReconstructionServerEndPoint.Ip(),
                DcDataReconstructionPort = settings.EndPointsConfiguration.DCDataReconstructionServerEndPoint.Port.ToString(),
                DcDataProgressWebsocketIp = settings.EndPointsConfiguration.DCDataProgressWebSocketEndPoint.Ip(),
                DcDataProgressWebsocketPort = settings.EndPointsConfiguration.DCDataProgressWebSocketEndPoint.Port.ToString(),
                DcDataReconstructionZMqIp = settings.EndPointsConfiguration.DCDataReconstructionZmqEndPoint.Ip(),
                DcDataReconstructionZMqPort = settings.EndPointsConfiguration.DCDataReconstructionZmqEndPoint.Port.ToString(),
                DcDatabaseIp = settings.EndPointsConfiguration.DCDatabaseEndPoint.Ip(),
                DcDatabasePort = settings.EndPointsConfiguration.DCDatabaseEndPoint.Port.ToString(),

                DeviceSerial = settings.DeviceSerial
            };

            if (!BaseEntry.IsBlankEntry(settings))
            {
                proto.Id = settings.Id;
            }

            return proto;
        }

        public static MosesSystemInfo FromProto(Com.Empyreanmed.Heracles.System.V1.System system)
        {
            return new MosesSystemInfo(system.Version);
        }
    }
}
