namespace AGS.sessions;

/// <summary>
///     Defines the lifecycle phases a session progresses through from creation to completion.
/// </summary>
internal enum SessionStatus
{
    /// <summary>Agents are asking clarifying questions to form the detailed scope.</summary>
    Scoping,

    /// <summary>The CEO has approved the scope document. Planning may begin.</summary>
    ScopeApproved,

    /// <summary>Agents are collaborating to produce an execution plan.</summary>
    Planning,

    /// <summary>The CEO has approved the execution plan. Task execution may begin.</summary>
    PlanApproved,

    /// <summary>Tasks from the approved plan are being executed sequentially.</summary>
    InProgress,

    /// <summary>Work is suspended; all context is persisted on disk.</summary>
    Paused,

    /// <summary>Every task in the plan is done or explicitly skipped; the session is closed.</summary>
    Completed
}
