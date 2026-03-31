namespace AGS.sessions;

/// <summary>
///     Represents a single row in the sessions index file (<c>.ags/sessions/index.md</c>).
/// </summary>
internal sealed class SessionIndexEntry
{
    /// <summary>Gets the unique session identifier.</summary>
    internal required string SessionId { get; init; }

    /// <summary>Gets the human-readable session title.</summary>
    internal required string Title { get; init; }

    /// <summary>Gets the current lifecycle status of the session.</summary>
    internal required SessionStatus Status { get; init; }

    /// <summary>Gets the date the session was last updated.</summary>
    internal required DateOnly LastUpdated { get; init; }
}
