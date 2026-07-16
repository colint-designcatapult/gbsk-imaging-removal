using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IPlan : IEntry
    {
        DateTime CreationDate { get; set; }

        long PrescriptionId { get; set; }


        PlanStatus Status { get; set; }

        string ApprovedBy { get; set; }

        TargetType CollimatorType { get; set; }

        ICollection<ITreatmentField> TreatmentFields { get; }

        TreatmentLoadingState TreatmentLoadingState { get; set; }

        ITreatmentField GetField(TreatmentFieldName name);
    }
}