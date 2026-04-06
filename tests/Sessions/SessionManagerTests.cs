using AGS.git;
using AGS.sessions;

namespace AGS.Tests;

/// <summary>
///     Unit tests for <see cref="SessionManager" /> covering session creation, state
///     transitions, pause/resume, directory structure, index persistence, and git
///     branch creation.
/// </summary>
public sealed class SessionManagerTests : IDisposable
{
    private readonly TemporaryDirectoryScope tempDir;

    public SessionManagerTests()
    {
        tempDir = new TemporaryDirectoryScope();
    }

    public void Dispose()
    {
        tempDir.Dispose();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that the constructor throws when projectRootPath is null.
    /// </summary>
    [Fact]
    public void ConstructorThrowsWhenProjectRootPathIsNull()
    {
        Assert.Throws<ArgumentException>(() => new SessionManager(null));
    }

    /// <summary>
    ///     Verifies that the constructor throws when projectRootPath is empty.
    /// </summary>
    [Fact]
    public void ConstructorThrowsWhenProjectRootPathIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new SessionManager(string.Empty));
    }

    // ── Path Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetSessionsDirectoryPath returns .ags/sessions under the project root.
    /// </summary>
    [Fact]
    public void GetSessionsDirectoryPathReturnsCorrectPath()
    {
        var expected = Path.Combine(tempDir.Path, ".ags", "sessions");

        var result = SessionManager.GetSessionsDirectoryPath(tempDir.Path);

        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     Verifies that GetSessionDirectoryPath returns the correct path for a session ID.
    /// </summary>
    [Fact]
    public void GetSessionDirectoryPathReturnsCorrectPath()
    {
        var expected = Path.Combine(tempDir.Path, ".ags", "sessions", "2026-04-01-demo");

        var result = SessionManager.GetSessionDirectoryPath(tempDir.Path, "2026-04-01-demo");

        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     Verifies that GetStateFilePath returns state.md inside the session directory.
    /// </summary>
    [Fact]
    public void GetStateFilePathReturnsCorrectPath()
    {
        var expected = Path.Combine(
            tempDir.Path, ".ags", "sessions", "2026-04-01-demo", "state.md");

        var result = SessionManager.GetStateFilePath(tempDir.Path, "2026-04-01-demo");

        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     Verifies that GetIndexFilePath returns index.md inside the sessions directory.
    /// </summary>
    [Fact]
    public void GetIndexFilePathReturnsCorrectPath()
    {
        var expected = Path.Combine(tempDir.Path, ".ags", "sessions", "index.md");

        var result = SessionManager.GetIndexFilePath(tempDir.Path);

        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     Verifies that GetTasksDirectoryPath returns tasks/ inside the session directory.
    /// </summary>
    [Fact]
    public void GetTasksDirectoryPathReturnsCorrectPath()
    {
        var expected = Path.Combine(
            tempDir.Path, ".ags", "sessions", "2026-04-01-demo", "tasks");

        var result = SessionManager.GetTasksDirectoryPath(tempDir.Path, "2026-04-01-demo");

        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     Verifies that GetArchiveDirectoryPath returns archive/ inside the session directory.
    /// </summary>
    [Fact]
    public void GetArchiveDirectoryPathReturnsCorrectPath()
    {
        var expected = Path.Combine(
            tempDir.Path, ".ags", "sessions", "2026-04-01-demo", "archive");

        var result = SessionManager.GetArchiveDirectoryPath(tempDir.Path, "2026-04-01-demo");

        Assert.Equal(expected, result);
    }

    // ── Session Creation – Directory Structure ────────────────────────────────

    /// <summary>
    ///     Verifies that CreateSession creates the session directory.
    /// </summary>
    [Fact]
    public void CreateSessionCreatesSessionDirectory()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(Directory.Exists(
            SessionManager.GetSessionDirectoryPath(tempDir.Path, "2026-04-01-my-feature")));
    }

    /// <summary>
    ///     Verifies that CreateSession creates the tasks/ subdirectory.
    /// </summary>
    [Fact]
    public void CreateSessionCreatesTasksSubdirectory()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(Directory.Exists(
            SessionManager.GetTasksDirectoryPath(tempDir.Path, "2026-04-01-my-feature")));
    }

    /// <summary>
    ///     Verifies that CreateSession creates the archive/ subdirectory.
    /// </summary>
    [Fact]
    public void CreateSessionCreatesArchiveSubdirectory()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(Directory.Exists(
            SessionManager.GetArchiveDirectoryPath(tempDir.Path, "2026-04-01-my-feature")));
    }

    // ── Session Creation – state.md ───────────────────────────────────────────

    /// <summary>
    ///     Verifies that CreateSession writes a state.md file for the new session.
    /// </summary>
    [Fact]
    public void CreateSessionWritesStateFile()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(File.Exists(
            SessionManager.GetStateFilePath(tempDir.Path, "2026-04-01-my-feature")));
    }

    /// <summary>
    ///     Verifies that CreateSession returns a state with the expected session ID.
    /// </summary>
    [Fact]
    public void CreateSessionReturnsStateWithCorrectSessionId()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        var state = manager.CreateSession("My Feature", "my-feature");

        Assert.Equal("2026-04-01-my-feature", state.SessionId);
    }

    /// <summary>
    ///     Verifies that CreateSession returns a state in the Scoping status.
    /// </summary>
    [Fact]
    public void CreateSessionReturnsStateWithScopingStatus()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        var state = manager.CreateSession("My Feature", "my-feature");

        Assert.Equal(SessionStatus.Scoping, state.Status);
    }

    /// <summary>
    ///     Verifies that CreateSession returns a state with the correct title.
    /// </summary>
    [Fact]
    public void CreateSessionReturnsStateWithCorrectTitle()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        var state = manager.CreateSession("My Feature", "my-feature");

        Assert.Equal("My Feature", state.Title);
    }

    /// <summary>
    ///     Verifies that CreateSession sets the Created date from the injected clock.
    /// </summary>
    [Fact]
    public void CreateSessionSetsCreatedDateFromClock()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        var state = manager.CreateSession("My Feature", "my-feature");

        Assert.Equal(new DateOnly(2026, 4, 1), state.Created);
    }

    /// <summary>
    ///     Verifies that CreateSession throws when a session with the same ID already exists.
    /// </summary>
    [Fact]
    public void CreateSessionThrowsWhenSessionAlreadyExists()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");

        Assert.Throws<InvalidOperationException>(
            () => manager.CreateSession("My Feature Again", "my-feature"));
    }

    // ── Session Creation – index.md ───────────────────────────────────────────

    /// <summary>
    ///     Verifies that CreateSession creates the sessions index file when none exists.
    /// </summary>
    [Fact]
    public void CreateSessionCreatesIndexFile()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(File.Exists(SessionManager.GetIndexFilePath(tempDir.Path)));
    }

    /// <summary>
    ///     Verifies that CreateSession adds a row for the new session to the index.
    /// </summary>
    [Fact]
    public void CreateSessionRegistersSessionInIndex()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        var entries = manager.ListSessions();
        Assert.Single(entries);
        Assert.Equal("2026-04-01-my-feature", entries[0].SessionId);
    }

    // ── Session Creation – Git Branch ─────────────────────────────────────────

    /// <summary>
    ///     Verifies that CreateSession calls GitManager.CreateSessionBranch with the
    ///     session ID when a git manager is provided.
    /// </summary>
    [Fact]
    public void CreateSessionCreatesGitBranch()
    {
        using var repo = new GitRepoScope();
        var gitManager = new GitManager(repo.Path);
        var manager = new SessionManager(repo.Path, gitManager, () => new DateOnly(2026, 4, 1));

        manager.CreateSession("My Feature", "my-feature");

        Assert.True(repo.BranchExists("session/2026-04-01-my-feature"));
    }

    /// <summary>
    ///     Verifies that CreateSession succeeds without creating a git branch when no
    ///     git manager is provided.
    /// </summary>
    [Fact]
    public void CreateSessionSucceedsWithoutGitManager()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));

        var state = manager.CreateSession("My Feature", "my-feature");

        Assert.Equal("2026-04-01-my-feature", state.SessionId);
    }

    // ── ReadSessionState ──────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that ReadSessionState returns state that matches what was written.
    /// </summary>
    [Fact]
    public void ReadSessionStateReturnsPersistedState()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");

        var state = manager.ReadSessionState("2026-04-01-my-feature");

        Assert.Equal("2026-04-01-my-feature", state.SessionId);
        Assert.Equal("My Feature", state.Title);
        Assert.Equal(SessionStatus.Scoping, state.Status);
        Assert.Equal(new DateOnly(2026, 4, 1), state.Created);
    }

    /// <summary>
    ///     Verifies that ReadSessionState throws when the state file does not exist.
    /// </summary>
    [Fact]
    public void ReadSessionStateThrowsWhenStateFileNotFound()
    {
        var manager = new SessionManager(tempDir.Path);

        Assert.Throws<InvalidOperationException>(
            () => manager.ReadSessionState("nonexistent-session"));
    }

    // ── UpdateSessionState ────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that UpdateSessionState persists changes to state.md and the index.
    /// </summary>
    [Fact]
    public void UpdateSessionStatePersistsChanges()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        var original = manager.CreateSession("My Feature", "my-feature");
        var updated = new SessionState
        {
            SessionId = original.SessionId,
            Title = original.Title,
            Status = SessionStatus.ScopeApproved,
            Created = original.Created,
            LastUpdated = new DateOnly(2026, 4, 2),
            NextStep = "Start planning"
        };

        manager.UpdateSessionState(updated);

        var read = manager.ReadSessionState("2026-04-01-my-feature");
        Assert.Equal(SessionStatus.ScopeApproved, read.Status);
        Assert.Equal("Start planning", read.NextStep);
    }

    // ── State Transitions ─────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that all valid forward transitions in the lifecycle succeed.
    /// </summary>
    [Fact]
    public void TransitionStatusFollowsFullLifecyclePath()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("Full Lifecycle", "full");

        var s1 = manager.TransitionStatus("2026-04-01-full", SessionStatus.ScopeApproved);
        Assert.Equal(SessionStatus.ScopeApproved, s1.Status);

        var s2 = manager.TransitionStatus("2026-04-01-full", SessionStatus.Planning);
        Assert.Equal(SessionStatus.Planning, s2.Status);

        var s3 = manager.TransitionStatus("2026-04-01-full", SessionStatus.PlanApproved);
        Assert.Equal(SessionStatus.PlanApproved, s3.Status);

        var s4 = manager.TransitionStatus("2026-04-01-full", SessionStatus.InProgress);
        Assert.Equal(SessionStatus.InProgress, s4.Status);

        var s5 = manager.TransitionStatus("2026-04-01-full", SessionStatus.Completed);
        Assert.Equal(SessionStatus.Completed, s5.Status);
    }

    /// <summary>
    ///     Verifies that any status can transition directly to Completed.
    /// </summary>
    [Theory]
    [InlineData((int)SessionStatus.Scoping)]
    [InlineData((int)SessionStatus.ScopeApproved)]
    [InlineData((int)SessionStatus.Planning)]
    [InlineData((int)SessionStatus.PlanApproved)]
    [InlineData((int)SessionStatus.InProgress)]
    [InlineData((int)SessionStatus.Paused)]
    public void TransitionStatusCanAlwaysReachCompleted(int startingStatusInt)
    {
        var startingStatus = (SessionStatus)startingStatusInt;
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        var slug = startingStatus.ToString().ToLowerInvariant();
        manager.CreateSession($"Session {slug}", slug);
        var sessionId = $"2026-04-01-{slug}";

        // Advance to the starting status through valid transitions
        AdvanceToStatus(manager, sessionId, startingStatus);

        var result = manager.TransitionStatus(sessionId, SessionStatus.Completed);

        Assert.Equal(SessionStatus.Completed, result.Status);
    }

    /// <summary>
    ///     Verifies that an invalid transition throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void TransitionStatusThrowsOnInvalidTransition()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");

        // Scoping -> InProgress is not a valid direct transition
        Assert.Throws<InvalidOperationException>(
            () => manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.InProgress));
    }

    /// <summary>
    ///     Verifies that TransitionStatus throws when trying to transition out of Completed.
    /// </summary>
    [Fact]
    public void TransitionStatusThrowsWhenSessionIsCompleted()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.Completed);

        Assert.Throws<InvalidOperationException>(
            () => manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.Scoping));
    }

    /// <summary>
    ///     Verifies that TransitionStatus updates LastUpdated using the injected clock.
    /// </summary>
    [Fact]
    public void TransitionStatusUpdatesLastUpdatedFromClock()
    {
        var date = new DateOnly(2026, 4, 1);
        var manager = new SessionManager(tempDir.Path, null, () => date);
        manager.CreateSession("My Feature", "my-feature");

        date = new DateOnly(2026, 4, 5);
        var updated = manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.ScopeApproved);

        Assert.Equal(new DateOnly(2026, 4, 5), updated.LastUpdated);
    }

    /// <summary>
    ///     Verifies that TransitionStatus persists the new status to disk.
    /// </summary>
    [Fact]
    public void TransitionStatusPersistsNewStatus()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");

        manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.ScopeApproved);

        var read = manager.ReadSessionState("2026-04-01-my-feature");
        Assert.Equal(SessionStatus.ScopeApproved, read.Status);
    }

    // ── Pause and Resume ──────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that PauseSession transitions an in-progress session to Paused.
    /// </summary>
    [Fact]
    public void PauseSessionTransitionsTopaused()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        AdvanceToStatus(manager, "2026-04-01-my-feature", SessionStatus.InProgress);

        var paused = manager.PauseSession("2026-04-01-my-feature", "Resume from task 3");

        Assert.Equal(SessionStatus.Paused, paused.Status);
    }

    /// <summary>
    ///     Verifies that PauseSession persists the nextStep description.
    /// </summary>
    [Fact]
    public void PauseSessionPersistsNextStep()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        AdvanceToStatus(manager, "2026-04-01-my-feature", SessionStatus.InProgress);

        manager.PauseSession("2026-04-01-my-feature", "Resume from task 3");

        var read = manager.ReadSessionState("2026-04-01-my-feature");
        Assert.Equal("Resume from task 3", read.NextStep);
    }

    /// <summary>
    ///     Verifies that PauseSession throws when the session is not in progress.
    /// </summary>
    [Theory]
    [InlineData((int)SessionStatus.Scoping)]
    [InlineData((int)SessionStatus.ScopeApproved)]
    [InlineData((int)SessionStatus.Planning)]
    [InlineData((int)SessionStatus.PlanApproved)]
    [InlineData((int)SessionStatus.Paused)]
    [InlineData((int)SessionStatus.Completed)]
    public void PauseSessionThrowsWhenNotInProgress(int statusInt)
    {
        var status = (SessionStatus)statusInt;
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        var slug = "pause-" + status.ToString().ToLower();
        manager.CreateSession($"Session {slug}", slug);
        var sessionId = $"2026-04-01-{slug}";
        AdvanceToStatus(manager, sessionId, status);

        Assert.Throws<InvalidOperationException>(
            () => manager.PauseSession(sessionId, "next step"));
    }

    /// <summary>
    ///     Verifies that ResumeSession transitions a paused session back to in-progress.
    /// </summary>
    [Fact]
    public void ResumeSessionTransitionsToInProgress()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        AdvanceToStatus(manager, "2026-04-01-my-feature", SessionStatus.InProgress);
        manager.PauseSession("2026-04-01-my-feature", "Resume from task 3");

        var resumed = manager.ResumeSession("2026-04-01-my-feature");

        Assert.Equal(SessionStatus.InProgress, resumed.Status);
    }

    /// <summary>
    ///     Verifies that ResumeSession persists the in-progress status to disk.
    /// </summary>
    [Fact]
    public void ResumeSessionPersistsInProgressStatus()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        AdvanceToStatus(manager, "2026-04-01-my-feature", SessionStatus.InProgress);
        manager.PauseSession("2026-04-01-my-feature", "Resume from task 3");

        manager.ResumeSession("2026-04-01-my-feature");

        var read = manager.ReadSessionState("2026-04-01-my-feature");
        Assert.Equal(SessionStatus.InProgress, read.Status);
    }

    /// <summary>
    ///     Verifies that ResumeSession throws when the session is not paused.
    /// </summary>
    [Theory]
    [InlineData((int)SessionStatus.Scoping)]
    [InlineData((int)SessionStatus.ScopeApproved)]
    [InlineData((int)SessionStatus.Planning)]
    [InlineData((int)SessionStatus.PlanApproved)]
    [InlineData((int)SessionStatus.InProgress)]
    [InlineData((int)SessionStatus.Completed)]
    public void ResumeSessionThrowsWhenNotPaused(int statusInt)
    {
        var status = (SessionStatus)statusInt;
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        var slug = "resume-" + status.ToString().ToLower();
        manager.CreateSession($"Session {slug}", slug);
        var sessionId = $"2026-04-01-{slug}";
        AdvanceToStatus(manager, sessionId, status);

        Assert.Throws<InvalidOperationException>(() => manager.ResumeSession(sessionId));
    }

    // ── ListSessions ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that ListSessions returns an empty list when no index file exists.
    /// </summary>
    [Fact]
    public void ListSessionsReturnsEmptyListWhenNoIndexExists()
    {
        var manager = new SessionManager(tempDir.Path);

        var sessions = manager.ListSessions();

        Assert.Empty(sessions);
    }

    /// <summary>
    ///     Verifies that ListSessions returns all registered sessions.
    /// </summary>
    [Fact]
    public void ListSessionsReturnsAllRegisteredSessions()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("Feature A", "feature-a");
        manager.CreateSession("Feature B", "feature-b");

        var sessions = manager.ListSessions();

        Assert.Equal(2, sessions.Count);
        Assert.Contains(sessions, s => s.SessionId == "2026-04-01-feature-a");
        Assert.Contains(sessions, s => s.SessionId == "2026-04-01-feature-b");
    }

    /// <summary>
    ///     Verifies that ListSessions reflects updated status after a transition.
    /// </summary>
    [Fact]
    public void ListSessionsReflectsUpdatedStatus()
    {
        var manager = new SessionManager(tempDir.Path, null, () => new DateOnly(2026, 4, 1));
        manager.CreateSession("My Feature", "my-feature");
        manager.TransitionStatus("2026-04-01-my-feature", SessionStatus.ScopeApproved);

        var sessions = manager.ListSessions();

        Assert.Single(sessions);
        Assert.Equal(SessionStatus.ScopeApproved, sessions[0].Status);
    }

    // ── SerializeStatus / TryParseStatus ─────────────────────────────────────

    /// <summary>
    ///     Verifies that SerializeStatus and TryParseStatus round-trip all statuses.
    /// </summary>
    [Theory]
    [InlineData((int)SessionStatus.Scoping, "scoping")]
    [InlineData((int)SessionStatus.ScopeApproved, "scope-approved")]
    [InlineData((int)SessionStatus.Planning, "planning")]
    [InlineData((int)SessionStatus.PlanApproved, "plan-approved")]
    [InlineData((int)SessionStatus.InProgress, "in-progress")]
    [InlineData((int)SessionStatus.Paused, "paused")]
    [InlineData((int)SessionStatus.Completed, "completed")]
    public void StatusRoundTripsCorrectly(int statusInt, string expectedString)
    {
        var status = (SessionStatus)statusInt;
        var serialized = SessionManager.SerializeStatus(status);
        Assert.Equal(expectedString, serialized);

        var parsed = SessionManager.TryParseStatus(serialized, out var result);
        Assert.True(parsed);
        Assert.Equal(status, result);
    }

    /// <summary>
    ///     Verifies that TryParseStatus returns false for an unknown status string.
    /// </summary>
    [Fact]
    public void TryParseStatusReturnsFalseForUnknownString()
    {
        var parsed = SessionManager.TryParseStatus("not-a-status", out _);

        Assert.False(parsed);
    }

    /// <summary>
    ///     Verifies that TryParseStatus returns false for a null or whitespace input.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseStatusReturnsFalseForNullOrWhitespace(string input)
    {
        var parsed = SessionManager.TryParseStatus(input, out _);

        Assert.False(parsed);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    ///     Advances the session to the given status by replaying the minimal valid
    ///     transition sequence from Scoping.
    /// </summary>
    private static void AdvanceToStatus(SessionManager manager, string sessionId,
        SessionStatus target)
    {
        // Scoping is the initial state — no transition needed.
        if (target == SessionStatus.Scoping) return;

        // Completed can be reached directly from any state.
        if (target == SessionStatus.Completed)
        {
            manager.TransitionStatus(sessionId, SessionStatus.Completed);
            return;
        }

        // Linear path: each status must be visited in order to reach the target.
        var path = new[]
        {
            SessionStatus.ScopeApproved,
            SessionStatus.Planning,
            SessionStatus.PlanApproved,
            SessionStatus.InProgress,
            SessionStatus.Paused
        };

        foreach (var status in path)
        {
            manager.TransitionStatus(sessionId, status);
            if (status == target) return;
        }
    }
}
