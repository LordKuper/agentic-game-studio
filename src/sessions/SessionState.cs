namespace AGS.sessions;

/// <summary>
///     Represents the persisted metadata and current status of an AGS session,
///     corresponding to the <c>state.md</c> file in the session directory.
/// </summary>
internal sealed class SessionState
{
    /// <summary>Gets the unique session identifier in <c>&lt;yyyy-MM-dd&gt;-&lt;slug&gt;</c> format.</summary>
    internal required string SessionId { get; init; }

    /// <summary>Gets the human-readable session title.</summary>
    internal required string Title { get; init; }

    /// <summary>Gets the current lifecycle status of the session.</summary>
    internal required SessionStatus Status { get; init; }

    /// <summary>Gets the date the session was created.</summary>
    internal required DateOnly Created { get; init; }

    /// <summary>Gets the date the session state was last updated.</summary>
    internal DateOnly LastUpdated { get; init; }

    /// <summary>Gets the names of agents involved in the scoping phase.</summary>
    internal IReadOnlyList<string> ScopingAgents { get; init; } = [];

    /// <summary>Gets the names of agents involved in the planning phase.</summary>
    internal IReadOnlyList<string> PlanningAgents { get; init; } = [];

    /// <summary>
    ///     Gets the relative path to the current task brief file within the session directory,
    ///     or an empty string when no task is active.
    /// </summary>
    internal string CurrentTask { get; init; } = string.Empty;

    /// <summary>Gets the list of relevant file entries for this session.</summary>
    internal IReadOnlyList<string> RelevantFiles { get; init; } = [];

    /// <summary>Gets the list of session-level decisions and their rationale.</summary>
    internal IReadOnlyList<string> Decisions { get; init; } = [];

    /// <summary>Gets the description of what to do when resuming this session.</summary>
    internal string NextStep { get; init; } = string.Empty;
}
