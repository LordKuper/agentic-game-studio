using System.Text;
using AGS;
using AGS.git;

namespace AGS.sessions;

/// <summary>
///     Manages AGS session lifecycle: creation, state transitions, pause, resume, and
///     file-based persistence to <c>.ags/sessions/</c>.
/// </summary>
internal sealed class SessionManager
{
    private readonly string projectRootPath;
    private readonly GitManager gitManager;
    private readonly Func<DateOnly> clock;

    private static readonly IReadOnlyDictionary<SessionStatus, IReadOnlySet<SessionStatus>>
        ValidTransitions = new Dictionary<SessionStatus, IReadOnlySet<SessionStatus>>
        {
            [SessionStatus.Scoping] = new HashSet<SessionStatus>
                { SessionStatus.ScopeApproved, SessionStatus.Completed },
            [SessionStatus.ScopeApproved] = new HashSet<SessionStatus>
                { SessionStatus.Planning, SessionStatus.Completed },
            [SessionStatus.Planning] = new HashSet<SessionStatus>
                { SessionStatus.PlanApproved, SessionStatus.Completed },
            [SessionStatus.PlanApproved] = new HashSet<SessionStatus>
                { SessionStatus.InProgress, SessionStatus.Completed },
            [SessionStatus.InProgress] = new HashSet<SessionStatus>
                { SessionStatus.Paused, SessionStatus.Completed },
            [SessionStatus.Paused] = new HashSet<SessionStatus>
                { SessionStatus.InProgress, SessionStatus.Completed },
            [SessionStatus.Completed] = new HashSet<SessionStatus>()
        };

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionManager" /> class using the
    ///     system clock and no git integration.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root directory.</param>
    internal SessionManager(string projectRootPath) : this(projectRootPath, null, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionManager" /> class using the
    ///     system clock.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root directory.</param>
    /// <param name="gitManager">
    ///     Git manager used to create session branches. Pass <see langword="null" /> to skip
    ///     branch creation.
    /// </param>
    internal SessionManager(string projectRootPath, GitManager gitManager)
        : this(projectRootPath, gitManager, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionManager" /> class.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root directory.</param>
    /// <param name="gitManager">
    ///     Git manager used to create session branches. Pass <see langword="null" /> to skip
    ///     branch creation.
    /// </param>
    /// <param name="clock">
    ///     Delegate that returns today's date. Pass <see langword="null" /> to use the system
    ///     clock.
    /// </param>
    internal SessionManager(string projectRootPath, GitManager gitManager, Func<DateOnly> clock)
    {
        if (string.IsNullOrEmpty(projectRootPath))
            throw new ArgumentException("Project root path must not be null or empty.",
                nameof(projectRootPath));
        this.projectRootPath = projectRootPath;
        this.gitManager = gitManager;
        this.clock = clock ?? (() => DateOnly.FromDateTime(DateTime.Today));
    }

    // ── Path Helpers ──────────────────────────────────────────────────────────

    /// <summary>Returns the absolute path to the sessions directory.</summary>
    internal static string GetSessionsDirectoryPath(string projectRootPath) =>
        Path.Combine(projectRootPath, AgsSettings.AgsDirectoryName, "sessions");

    /// <summary>Returns the absolute path to a specific session's directory.</summary>
    internal static string GetSessionDirectoryPath(string projectRootPath, string sessionId) =>
        Path.Combine(GetSessionsDirectoryPath(projectRootPath), sessionId);

    /// <summary>Returns the absolute path to a session's <c>state.md</c> file.</summary>
    internal static string GetStateFilePath(string projectRootPath, string sessionId) =>
        Path.Combine(GetSessionDirectoryPath(projectRootPath, sessionId), "state.md");

    /// <summary>Returns the absolute path to the sessions index file.</summary>
    internal static string GetIndexFilePath(string projectRootPath) =>
        Path.Combine(GetSessionsDirectoryPath(projectRootPath), "index.md");

    /// <summary>Returns the absolute path to a session's <c>tasks/</c> directory.</summary>
    internal static string GetTasksDirectoryPath(string projectRootPath, string sessionId) =>
        Path.Combine(GetSessionDirectoryPath(projectRootPath, sessionId), "tasks");

    /// <summary>Returns the absolute path to a session's <c>archive/</c> directory.</summary>
    internal static string GetArchiveDirectoryPath(string projectRootPath, string sessionId) =>
        Path.Combine(GetSessionDirectoryPath(projectRootPath, sessionId), "archive");

    // ── Session Creation ──────────────────────────────────────────────────────

    /// <summary>
    ///     Creates a new session: directory structure, <c>state.md</c>, <c>index.md</c>
    ///     registration, and a git session branch.
    /// </summary>
    /// <param name="title">Human-readable session title.</param>
    /// <param name="slug">Kebab-case slug used in the session ID and git branch name.</param>
    /// <returns>The newly created <see cref="SessionState" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a session with the generated ID already exists.
    /// </exception>
    internal SessionState CreateSession(string title, string slug)
    {
        var today = clock();
        var sessionId = $"{today:yyyy-MM-dd}-{slug}";
        var sessionDir = GetSessionDirectoryPath(projectRootPath, sessionId);

        if (Directory.Exists(sessionDir))
            throw new InvalidOperationException(
                $"Session '{sessionId}' already exists at '{sessionDir}'.");

        Directory.CreateDirectory(sessionDir);
        Directory.CreateDirectory(GetTasksDirectoryPath(projectRootPath, sessionId));
        Directory.CreateDirectory(GetArchiveDirectoryPath(projectRootPath, sessionId));

        var state = new SessionState
        {
            SessionId = sessionId,
            Title = title,
            Status = SessionStatus.Scoping,
            Created = today,
            LastUpdated = today
        };

        WriteStateFile(state);
        RegisterOrUpdateInIndex(state);
        gitManager?.CreateSessionBranch(sessionId);

        return state;
    }

    // ── Read / Write State ────────────────────────────────────────────────────

    /// <summary>
    ///     Reads and returns the persisted state for the specified session.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>The parsed <see cref="SessionState" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the state file does not exist or cannot be parsed.
    /// </exception>
    internal SessionState ReadSessionState(string sessionId)
    {
        var stateFilePath = GetStateFilePath(projectRootPath, sessionId);
        if (!File.Exists(stateFilePath))
            throw new InvalidOperationException(
                $"State file not found for session '{sessionId}' at '{stateFilePath}'.");

        var content = File.ReadAllText(stateFilePath);
        if (!TryParseStateContent(content, out var state))
            throw new InvalidOperationException(
                $"State file for session '{sessionId}' could not be parsed.");

        return state;
    }

    /// <summary>
    ///     Writes the session state to <c>state.md</c> and updates the sessions index.
    /// </summary>
    /// <param name="state">Session state to persist.</param>
    internal void UpdateSessionState(SessionState state)
    {
        WriteStateFile(state);
        RegisterOrUpdateInIndex(state);
    }

    // ── State Transitions ─────────────────────────────────────────────────────

    /// <summary>
    ///     Transitions the session to a new status, validates the transition, updates
    ///     <c>LastUpdated</c>, and persists the change.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="newStatus">Target status.</param>
    /// <returns>The updated <see cref="SessionState" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the transition is not valid from the current status.
    /// </exception>
    internal SessionState TransitionStatus(string sessionId, SessionStatus newStatus)
    {
        var current = ReadSessionState(sessionId);

        if (!ValidTransitions.TryGetValue(current.Status, out var allowed) ||
            !allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition session '{sessionId}' from " +
                $"'{SerializeStatus(current.Status)}' to '{SerializeStatus(newStatus)}'.");
        }

        var updated = new SessionState
        {
            SessionId = current.SessionId,
            Title = current.Title,
            Status = newStatus,
            Created = current.Created,
            LastUpdated = clock(),
            ScopingAgents = current.ScopingAgents,
            PlanningAgents = current.PlanningAgents,
            CurrentTask = current.CurrentTask,
            RelevantFiles = current.RelevantFiles,
            Decisions = current.Decisions,
            NextStep = current.NextStep
        };

        WriteStateFile(updated);
        RegisterOrUpdateInIndex(updated);
        return updated;
    }

    /// <summary>
    ///     Pauses an in-progress session, recording the next step for resumption.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="nextStep">Description of what to do when resuming.</param>
    /// <returns>The updated <see cref="SessionState" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the session is not currently in-progress.
    /// </exception>
    internal SessionState PauseSession(string sessionId, string nextStep)
    {
        var current = ReadSessionState(sessionId);

        if (current.Status != SessionStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot pause session '{sessionId}': expected status " +
                $"'{SerializeStatus(SessionStatus.InProgress)}' but found " +
                $"'{SerializeStatus(current.Status)}'.");

        var updated = new SessionState
        {
            SessionId = current.SessionId,
            Title = current.Title,
            Status = SessionStatus.Paused,
            Created = current.Created,
            LastUpdated = clock(),
            ScopingAgents = current.ScopingAgents,
            PlanningAgents = current.PlanningAgents,
            CurrentTask = current.CurrentTask,
            RelevantFiles = current.RelevantFiles,
            Decisions = current.Decisions,
            NextStep = nextStep ?? string.Empty
        };

        WriteStateFile(updated);
        RegisterOrUpdateInIndex(updated);
        return updated;
    }

    /// <summary>
    ///     Resumes a paused session, transitioning it back to in-progress.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>The updated <see cref="SessionState" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the session is not currently paused.
    /// </exception>
    internal SessionState ResumeSession(string sessionId)
    {
        var current = ReadSessionState(sessionId);

        if (current.Status != SessionStatus.Paused)
            throw new InvalidOperationException(
                $"Cannot resume session '{sessionId}': expected status " +
                $"'{SerializeStatus(SessionStatus.Paused)}' but found " +
                $"'{SerializeStatus(current.Status)}'.");

        var updated = new SessionState
        {
            SessionId = current.SessionId,
            Title = current.Title,
            Status = SessionStatus.InProgress,
            Created = current.Created,
            LastUpdated = clock(),
            ScopingAgents = current.ScopingAgents,
            PlanningAgents = current.PlanningAgents,
            CurrentTask = current.CurrentTask,
            RelevantFiles = current.RelevantFiles,
            Decisions = current.Decisions,
            NextStep = current.NextStep
        };

        WriteStateFile(updated);
        RegisterOrUpdateInIndex(updated);
        return updated;
    }

    // ── List Sessions ─────────────────────────────────────────────────────────

    /// <summary>
    ///     Returns all sessions registered in the sessions index.
    /// </summary>
    /// <returns>
    ///     A read-only list of index entries, or an empty list when no index file exists.
    /// </returns>
    internal IReadOnlyList<SessionIndexEntry> ListSessions()
    {
        var indexPath = GetIndexFilePath(projectRootPath);
        if (!File.Exists(indexPath)) return [];

        var content = File.ReadAllText(indexPath);
        return ParseIndexContent(content);
    }

    // ── Status Serialization ──────────────────────────────────────────────────

    /// <summary>
    ///     Converts a <see cref="SessionStatus" /> to its kebab-case string representation
    ///     used in markdown files.
    /// </summary>
    internal static string SerializeStatus(SessionStatus status) => status switch
    {
        SessionStatus.Scoping => "scoping",
        SessionStatus.ScopeApproved => "scope-approved",
        SessionStatus.Planning => "planning",
        SessionStatus.PlanApproved => "plan-approved",
        SessionStatus.InProgress => "in-progress",
        SessionStatus.Paused => "paused",
        SessionStatus.Completed => "completed",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    /// <summary>
    ///     Attempts to parse a kebab-case status string into a <see cref="SessionStatus" />.
    /// </summary>
    internal static bool TryParseStatus(string text, out SessionStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        switch (text.Trim().ToLowerInvariant())
        {
            case "scoping": status = SessionStatus.Scoping; return true;
            case "scope-approved": status = SessionStatus.ScopeApproved; return true;
            case "planning": status = SessionStatus.Planning; return true;
            case "plan-approved": status = SessionStatus.PlanApproved; return true;
            case "in-progress": status = SessionStatus.InProgress; return true;
            case "paused": status = SessionStatus.Paused; return true;
            case "completed": status = SessionStatus.Completed; return true;
            default: return false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void WriteStateFile(SessionState state)
    {
        var stateFilePath = GetStateFilePath(projectRootPath, state.SessionId);
        File.WriteAllText(stateFilePath, SerializeStateContent(state));
    }

    private static string SerializeStateContent(SessionState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Session: {state.Title}");
        sb.AppendLine();
        sb.AppendLine("## Metadata");
        sb.AppendLine();
        sb.AppendLine($"- **session-id:** {state.SessionId}");
        sb.AppendLine($"- **title:** {state.Title}");
        sb.AppendLine($"- **status:** {SerializeStatus(state.Status)}");
        sb.AppendLine($"- **created:** {state.Created:yyyy-MM-dd}");
        sb.AppendLine($"- **last-updated:** {state.LastUpdated:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("## Agents");
        sb.AppendLine();
        var scopingStr = state.ScopingAgents.Count > 0
            ? string.Join(", ", state.ScopingAgents) : "(none)";
        var planningStr = state.PlanningAgents.Count > 0
            ? string.Join(", ", state.PlanningAgents) : "(none)";
        sb.AppendLine($"- **scoping:** {scopingStr}");
        sb.AppendLine($"- **planning:** {planningStr}");
        sb.AppendLine();
        sb.AppendLine("## Current Task");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrEmpty(state.CurrentTask)
            ? "(none)"
            : $"→ `{state.CurrentTask}`");
        sb.AppendLine();
        sb.AppendLine("## Relevant Files");
        sb.AppendLine();
        if (state.RelevantFiles.Count > 0)
            foreach (var file in state.RelevantFiles)
                sb.AppendLine($"- {file}");
        else
            sb.AppendLine("(none)");
        sb.AppendLine();
        sb.AppendLine("## Decisions");
        sb.AppendLine();
        if (state.Decisions.Count > 0)
            foreach (var decision in state.Decisions)
                sb.AppendLine($"- {decision}");
        else
            sb.AppendLine("(none)");
        sb.AppendLine();
        sb.AppendLine("## Next Step");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrEmpty(state.NextStep) ? "(none)" : state.NextStep);
        return sb.ToString();
    }

    private static bool TryParseStateContent(string content, out SessionState state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(content)) return false;

        var currentSection = string.Empty;
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var agentFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentTask = string.Empty;
        var relevantFiles = new List<string>();
        var decisions = new List<string>();
        var nextStepLines = new List<string>();
        var inNextStep = false;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd();

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                currentSection = line[3..].Trim().ToLowerInvariant();
                inNextStep = currentSection == "next step";
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                if (inNextStep) nextStepLines.Add(line);
                continue;
            }

            switch (currentSection)
            {
                case "metadata":
                    if (TryParseMetadataLine(line, out var mKey, out var mValue))
                        metadata[mKey] = mValue;
                    break;
                case "agents":
                    if (TryParseMetadataLine(line, out var aKey, out var aValue))
                        agentFields[aKey] = aValue;
                    break;
                case "current task":
                    if (line.StartsWith("→ ", StringComparison.Ordinal))
                        currentTask = line[2..].Trim().Trim('`');
                    break;
                case "relevant files":
                    if (line.StartsWith("- ", StringComparison.Ordinal))
                        relevantFiles.Add(line[2..].Trim());
                    break;
                case "decisions":
                    if (line.StartsWith("- ", StringComparison.Ordinal))
                        decisions.Add(line[2..].Trim());
                    break;
                case "next step":
                    nextStepLines.Add(line);
                    break;
            }
        }

        if (!metadata.TryGetValue("session-id", out var sessionId) ||
            string.IsNullOrWhiteSpace(sessionId)) return false;
        if (!metadata.TryGetValue("title", out var title) ||
            string.IsNullOrWhiteSpace(title)) return false;
        if (!metadata.TryGetValue("status", out var statusStr) ||
            !TryParseStatus(statusStr, out var status)) return false;
        if (!metadata.TryGetValue("created", out var createdStr) ||
            !DateOnly.TryParse(createdStr, out var created)) return false;
        if (!metadata.TryGetValue("last-updated", out var lastUpdatedStr) ||
            !DateOnly.TryParse(lastUpdatedStr, out var lastUpdated)) return false;

        var scopingAgents = ParseAgentList(
            agentFields.TryGetValue("scoping", out var sa) ? sa : string.Empty);
        var planningAgents = ParseAgentList(
            agentFields.TryGetValue("planning", out var pa) ? pa : string.Empty);

        var nextStep = string.Join("\n", nextStepLines).Trim();
        if (nextStep == "(none)") nextStep = string.Empty;

        state = new SessionState
        {
            SessionId = sessionId,
            Title = title,
            Status = status,
            Created = created,
            LastUpdated = lastUpdated,
            ScopingAgents = scopingAgents,
            PlanningAgents = planningAgents,
            CurrentTask = currentTask,
            RelevantFiles = relevantFiles.AsReadOnly(),
            Decisions = decisions.AsReadOnly(),
            NextStep = nextStep
        };
        return true;
    }

    private static bool TryParseMetadataLine(string line, out string key, out string value)
    {
        key = value = string.Empty;
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("- **", StringComparison.Ordinal)) return false;
        var afterPrefix = trimmed[4..]; // after "- **"
        var endMarker = afterPrefix.IndexOf("**:", StringComparison.Ordinal);
        if (endMarker < 0) return false;
        key = afterPrefix[..endMarker].Trim();
        value = afterPrefix[(endMarker + 3)..].Trim();
        return !string.IsNullOrEmpty(key);
    }

    private static IReadOnlyList<string> ParseAgentList(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim() == "(none)") return [];
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries |
                                StringSplitOptions.TrimEntries);
    }

    private void RegisterOrUpdateInIndex(SessionState state)
    {
        var indexPath = GetIndexFilePath(projectRootPath);
        List<string> lines;

        if (!File.Exists(indexPath))
        {
            lines =
            [
                "# Sessions Index",
                string.Empty,
                "| session-id | title | status | last-updated |",
                "|---|---|---|---|"
            ];
        }
        else
        {
            lines = [.. File.ReadAllLines(indexPath)];
        }

        var newRow = BuildIndexRow(state);
        var existingIndex = lines.FindIndex(l =>
            l.StartsWith($"| {state.SessionId} |", StringComparison.Ordinal));

        if (existingIndex >= 0)
            lines[existingIndex] = newRow;
        else
            lines.Add(newRow);

        File.WriteAllText(indexPath,
            string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    private static string BuildIndexRow(SessionState state) =>
        $"| {state.SessionId} | {state.Title} | {SerializeStatus(state.Status)} | {state.LastUpdated:yyyy-MM-dd} |";

    private static IReadOnlyList<SessionIndexEntry> ParseIndexContent(string content)
    {
        var entries = new List<SessionIndexEntry>();
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("|", StringComparison.Ordinal) ||
                !trimmed.EndsWith("|", StringComparison.Ordinal))
                continue;

            var cells = trimmed[1..^1].Split('|').Select(c => c.Trim()).ToArray();
            if (cells.Length != 4) continue;
            if (cells[0] == "session-id" ||
                cells[0].StartsWith("---", StringComparison.Ordinal))
                continue;

            if (!TryParseStatus(cells[2], out var status)) continue;
            if (!DateOnly.TryParse(cells[3], out var lastUpdated)) continue;

            entries.Add(new SessionIndexEntry
            {
                SessionId = cells[0],
                Title = cells[1],
                Status = status,
                LastUpdated = lastUpdated
            });
        }
        return entries.AsReadOnly();
    }
}
