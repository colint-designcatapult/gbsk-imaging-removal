using Heracles.Core.Models.EMR;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Core.Commands
{
    public interface IEmrPatientCommands : IAsyncRootEntryCommands<IPatient>
    {
    }

    public interface IEmrDiagnosisCommands : IAsyncChildEntryCommands<IDiagnosis>
    {
    }

    public interface IEmrSimulationCommands : IAsyncChildEntryCommands<ISimulation>
    {
    }

    public interface IEmrPrescriptionCommands : IAsyncChildEntryCommands<IPrescription>
    {
    }
    
    public interface IEmrVisitCommands : IAsyncChildEntryCommands<IVisit>
    {
    }

    public interface IEmrTreatmentDeviceCommands : IAsyncChildEntryCommands<ITreatmentDevice>
    {
    }

    public interface IEmrPatientPositionCommands : IAsyncChildEntryCommands<IPatientPosition>
    {
    }

    public interface IEmrTreatmentFieldCommands : IAsyncChildEntryCommands<ITreatmentField>
    {
        Task<ICollection<ITreatmentField>> CreateBunchAsync(ICollection<ITreatmentField> fields);
    }

    public interface IEmrActualTreatmentFieldCommands : IAsyncChildEntryCommands<IActualTreatmentField>
    {
    }

    public interface IEmrEmissionTreatmentFieldCommands : IAsyncChildEntryCommands<IEmissionTreatmentField>
    {
    }
    
    public interface IEmrTreatmentCommands : IAsyncChildEntryCommands<ITreatment>
    {
    }

    public interface IEmrPhotoCommands : IAsyncChildEntryCommands<IPhotoDescription>
    {
        Task SendPhotoAsync(IPhoto photo, int chunkSize, CancellationToken token);
        Task<IPhoto?> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token);
    }

}
