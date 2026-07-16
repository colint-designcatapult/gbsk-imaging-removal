using System;
using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public interface IMainBoardState
    {
        GcbStateNew? State { get; }
        GcbEmissionPlan CurrentPlan { get; }
        GcbSession? Session { get; }
        bool IsPlanStaged { get; }
        ISystemTelemetry? SystemTelemetry { get; }
        GcbOperationalPoint CurrentPoint();
        // Methods for board state checks
        #region Board command predicates
        bool CanPrepare();
        bool CanStartWarmUp();
        bool CanLoadPlan();
        bool CanBeamOn();
        bool CanStop();
        bool CanClearFaults();
        bool CanClearPlan();
        bool CanResetTimers();
        #endregion Board command predicates
    }

    public interface ISystemTelemetryChanged
    {
        void OnSystemTelemetryChanged(ISystemTelemetry systemTelemetry);
    }

    public interface IMainBoardStateManagement : IMainBoardState, ISystemTelemetryChanged
    {
        void SetSession(GcbSession session);
        void SetCurrentPlan(GcbEmissionPlan plan);
    }

    public interface IMainBoardAPI
    {
        void CancelCurrentTask();

        // State changing commands:
        #region Active commands 
        Task Initialize();
        Task Stop();
        Task ClearFaults();
        Task ClearPlan();

        #region Board command sequences
        /// <summary>
        /// Sequence of warmup and Load calls.
        /// </summary>
        Task<bool> PreparePlan(GcbEmissionPlan plan, bool tryKeepPrevPlan);

        Task<bool> SafeWarmup(WarmupParameters warmupParameters);
        Task BeamOn();
        Task BeamOnOnePoint();

        Task ResumePlan();
        #endregion Board command sequences
        #endregion Active commands 


        // Commands changing onboard data, but not its operational state:
        #region Board setup commands
        Task ResetTimers();
        #endregion Board setup commands

        // Commands that just query the board's state
        #region Board state queries
        Task<VersionInfo> GetVersionInfo();
        Task<FaultEntry> GetFaults();
        Task<GcbOperationalPoint> QueryPointFromGCB(int index);
        Task UpdatePlanPointFromGCB(int index);
        Task<GcbEmissionPlan> QueryPlanFromGCB();

        #endregion Board state queries

        event EventHandler<GcbActionCompletionEventArgs> GcbActionCompletionEvent;
    }

    public interface IMainBoardModel : IMainBoardStateManagement, IMainBoardAPI
    {
    }

    public class GcbActionCompletionEventArgs : EventArgs
    {
        public GcbActionType ActionType { get; set; }
    }

    public enum GcbActionType
    {
        NewSession,
        StagePlan,
        ReleasePlan,
        StartBeamOn,
        OnePointCompleted,
        BeamOnCompleted,
        ClearPlan,
        ClearErrors,
        Stop
    }
}
