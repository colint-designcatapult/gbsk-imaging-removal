using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Linq;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Plan : BaseEntry, IPlan
    {
        public Plan(IPlan plan, ICollection<ITreatmentField> treatmentFields = null)
        {
            plan?.CopyProperties(this);
            if (treatmentFields != null)
            {
                TreatmentFields = treatmentFields;
            }
        }
        public Plan()
        {
        }

        public DateTime CreationDate { get; set; }

        public long PrescriptionId { get; set; }

        public PlanStatus Status { get; set; }// = null!;

        public string ApprovedBy { get; set; }

        public TargetType CollimatorType { get; set; }

        public virtual ICollection<ITreatmentField> TreatmentFields { get; } = new List<ITreatmentField>();

        public TreatmentLoadingState TreatmentLoadingState { get; set; }
        
        public ITreatmentField GetField(TreatmentFieldName name)
        {
            return TreatmentFields.FirstOrDefault(f => f.Name == name);
        }
    }
}