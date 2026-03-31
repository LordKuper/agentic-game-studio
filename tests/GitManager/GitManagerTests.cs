using System.Diagnostics;
using AGS.git;

namespace AGS.Tests;

/// <summary>
///     Integration tests for <see cref="GitManager" /> that use a real git repository.
/// </summary>
public sealed class GitManagerTests : IDisposable
{
    private readonly GitRepoScope repo;

    public GitManagerTests()
    {
        repo = new GitRepoScope();
    }

    public void Dispose()
    {
        repo.Dispose();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that the constructor throws when projectRootPath is null.
    /// </summary>
    [Fact]
    public void ConstructorThrowsWhenProjectRootPathIsNull()
    {
        Assert.Throws<ArgumentException>(() => new GitManager(null));
    }

    /// <summary>
    ///     Verifies that the constructor throws when projectRootPath is empty.
    /// </summary>
    [Fact]
    public void ConstructorThrowsWhenProjectRootPathIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new GitManager(string.Empty));
    }

    // ── CreateSessionBranch ───────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that CreateSessionBranch creates a branch named session/&lt;id&gt;.
    /// </summary>
    [Fact]
    public void CreateSessionBranchCreatesBranchFromMain()
    {
        var manager = new GitManager(repo.Path);

        manager.CreateSessionBranch("2026-03-31-feature");

        Assert.True(repo.BranchExists("session/2026-03-31-feature"));
    }

    /// <summary>
    ///     Verifies that CreateSessionBranch leaves the working directory on the new branch.
    /// </summary>
    [Fact]
    public void CreateSessionBranchCheckoutsBranch()
    {
        var manager = new GitManager(repo.Path);

        manager.CreateSessionBranch("2026-03-31-feature");

        Assert.Equal("session/2026-03-31-feature", repo.CurrentBranch());
    }

    /// <summary>
    ///     Verifies that CreateSessionBranch throws when the branch already exists.
    /// </summary>
    [Fact]
    public void CreateSessionBranchThrowsWhenBranchAlreadyExists()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        repo.Git("checkout main");

        Assert.Throws<InvalidOperationException>(
            () => manager.CreateSessionBranch("2026-03-31-feature"));
    }

    // ── CommitTaskChanges ─────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that CommitTaskChanges stages and commits all untracked files.
    /// </summary>
    [Fact]
    public void CommitTaskChangesCommitsUntrackedFile()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "new-file.txt"), "hello");

        manager.CommitTaskChanges("2026-03-31-feature", "add-greeting", "Add greeting file");

        var log = repo.Git("log --oneline").StandardOutput;
        Assert.Contains("[2026-03-31-feature] add-greeting: Add greeting file", log);
    }

    /// <summary>
    ///     Verifies that CommitTaskChanges stages and commits modified tracked files.
    /// </summary>
    [Fact]
    public void CommitTaskChangesCommitsModifiedFile()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "readme.txt"), "modified content");

        manager.CommitTaskChanges("2026-03-31-feature", "update-readme", "Update readme");

        var log = repo.Git("log --oneline").StandardOutput;
        Assert.Contains("[2026-03-31-feature] update-readme: Update readme", log);
    }

    /// <summary>
    ///     Verifies that CommitTaskChanges throws when there is nothing to commit.
    /// </summary>
    [Fact]
    public void CommitTaskChangesThrowsWhenNothingToCommit()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");

        Assert.Throws<InvalidOperationException>(
            () => manager.CommitTaskChanges("2026-03-31-feature", "no-op", "Nothing changed"));
    }

    // ── HasConflictsWithMain ──────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that HasConflictsWithMain returns false when the session branch is clean.
    /// </summary>
    [Fact]
    public void HasConflictsWithMainReturnsFalseForCleanBranch()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "new feature");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");

        Assert.False(manager.HasConflictsWithMain("2026-03-31-feature"));
    }

    /// <summary>
    ///     Verifies that HasConflictsWithMain returns true when both branches modify the same
    ///     file in conflicting ways.
    /// </summary>
    [Fact]
    public void HasConflictsWithMainReturnsTrueWhenConflictExists()
    {
        var manager = new GitManager(repo.Path);
        // Create session branch from main
        manager.CreateSessionBranch("2026-03-31-conflict");
        // Edit readme on session branch
        File.WriteAllText(Path.Combine(repo.Path, "readme.txt"), "session branch content");
        manager.CommitTaskChanges("2026-03-31-conflict", "edit-readme", "Edit readme on session");

        // Go back to main and make a conflicting change to the same file
        repo.Git("checkout main");
        File.WriteAllText(Path.Combine(repo.Path, "readme.txt"), "main branch content");
        repo.Git("add --all");
        repo.Git("commit -m \"Edit readme on main\"");

        Assert.True(manager.HasConflictsWithMain("2026-03-31-conflict"));
    }

    /// <summary>
    ///     Verifies that HasConflictsWithMain does not modify the working tree.
    /// </summary>
    [Fact]
    public void HasConflictsWithMainDoesNotModifyWorkingTree()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "new feature");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");
        var branchBefore = repo.CurrentBranch();

        manager.HasConflictsWithMain("2026-03-31-feature");

        Assert.Equal(branchBefore, repo.CurrentBranch());
    }

    // ── GeneratePRDescription ─────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GeneratePRDescription includes the session ID header.
    /// </summary>
    [Fact]
    public void GeneratePRDescriptionIncludesSessionIdHeader()
    {
        var manager = new GitManager(repo.Path);

        var description = manager.GeneratePRDescription("2026-03-31-feature");

        Assert.Contains("## Session: 2026-03-31-feature", description);
    }

    /// <summary>
    ///     Verifies that GeneratePRDescription includes scope content when session-scope.md exists.
    /// </summary>
    [Fact]
    public void GeneratePRDescriptionIncludesScopeWhenFileExists()
    {
        var manager = new GitManager(repo.Path);
        var sessionDir = Path.Combine(repo.Path, ".ags", "sessions", "2026-03-31-feature");
        Directory.CreateDirectory(sessionDir);
        File.WriteAllText(Path.Combine(sessionDir, "session-scope.md"), "Implement inventory UI");

        var description = manager.GeneratePRDescription("2026-03-31-feature");

        Assert.Contains("## Scope", description);
        Assert.Contains("Implement inventory UI", description);
    }

    /// <summary>
    ///     Verifies that GeneratePRDescription includes execution plan content when
    ///     execution-plan.md exists.
    /// </summary>
    [Fact]
    public void GeneratePRDescriptionIncludesPlanWhenFileExists()
    {
        var manager = new GitManager(repo.Path);
        var sessionDir = Path.Combine(repo.Path, ".ags", "sessions", "2026-03-31-feature");
        Directory.CreateDirectory(sessionDir);
        File.WriteAllText(Path.Combine(sessionDir, "execution-plan.md"),
            "- [ ] Task 1\n- [x] Task 2");

        var description = manager.GeneratePRDescription("2026-03-31-feature");

        Assert.Contains("## Execution Plan", description);
        Assert.Contains("Task 1", description);
    }

    /// <summary>
    ///     Verifies that GeneratePRDescription silently omits sections when artifact files are
    ///     missing.
    /// </summary>
    [Fact]
    public void GeneratePRDescriptionOmitsMissingSections()
    {
        var manager = new GitManager(repo.Path);

        var description = manager.GeneratePRDescription("2026-03-31-feature");

        Assert.DoesNotContain("## Scope", description);
        Assert.DoesNotContain("## Execution Plan", description);
    }

    // ── MergeToMain ───────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that MergeToMain merges the session branch into main and leaves the
    ///     working directory on main.
    /// </summary>
    [Fact]
    public void MergeToMainMergesBranchIntoMain()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "new feature content");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");

        manager.MergeToMain("2026-03-31-feature");

        Assert.Equal("main", repo.CurrentBranch());
        Assert.True(File.Exists(Path.Combine(repo.Path, "feature.txt")));
    }

    /// <summary>
    ///     Verifies that MergeToMain creates a merge commit (non-fast-forward).
    /// </summary>
    [Fact]
    public void MergeToMainCreatesNonFastForwardMergeCommit()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "new feature content");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");

        manager.MergeToMain("2026-03-31-feature");

        // A no-ff merge always creates a merge commit even when fast-forward would be possible
        var log = repo.Git("log --oneline --merges").StandardOutput;
        Assert.NotEmpty(log.Trim());
    }

    // ── DeleteSessionBranch ───────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that DeleteSessionBranch removes a merged session branch.
    /// </summary>
    [Fact]
    public void DeleteSessionBranchRemovesMergedBranch()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "content");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");
        manager.MergeToMain("2026-03-31-feature");

        manager.DeleteSessionBranch("2026-03-31-feature");

        Assert.False(repo.BranchExists("session/2026-03-31-feature"));
    }

    /// <summary>
    ///     Verifies that DeleteSessionBranch throws when the branch is not merged.
    /// </summary>
    [Fact]
    public void DeleteSessionBranchThrowsWhenBranchNotMerged()
    {
        var manager = new GitManager(repo.Path);
        manager.CreateSessionBranch("2026-03-31-feature");
        File.WriteAllText(Path.Combine(repo.Path, "feature.txt"), "content");
        manager.CommitTaskChanges("2026-03-31-feature", "add-feature", "Add feature");
        repo.Git("checkout main");  // go back to main without merging

        Assert.Throws<InvalidOperationException>(
            () => manager.DeleteSessionBranch("2026-03-31-feature"));
    }

    // ── ProcessRunner injection ───────────────────────────────────────────────

    /// <summary>
    ///     Verifies that the injected processRunner receives a ProcessStartInfo with
    ///     the correct git arguments when CreateSessionBranch is called.
    /// </summary>
    [Fact]
    public void CreateSessionBranchPassesCorrectArgumentsToProcessRunner()
    {
        var captured = new List<IReadOnlyList<string>>();
        (int, string, string) StubRunner(ProcessStartInfo info)
        {
            captured.Add([.. info.ArgumentList]);
            return (0, string.Empty, string.Empty);
        }

        var manager = new GitManager(repo.Path, StubRunner);
        manager.CreateSessionBranch("2026-03-31-feature");

        Assert.Single(captured);
        Assert.Equal(["checkout", "-b", "session/2026-03-31-feature", "main"], captured[0]);
    }

    /// <summary>
    ///     Verifies that CommitTaskChanges passes the correct arguments to the process runner.
    /// </summary>
    [Fact]
    public void CommitTaskChangesPassesCorrectMessageToProcessRunner()
    {
        var captured = new List<IReadOnlyList<string>>();
        int callCount = 0;
        (int, string, string) StubRunner(ProcessStartInfo info)
        {
            captured.Add([.. info.ArgumentList]);
            callCount++;
            // Second call is the commit; return success for both
            return (0, string.Empty, string.Empty);
        }

        var manager = new GitManager(repo.Path, StubRunner);
        manager.CommitTaskChanges("2026-03-31-feature", "my-task", "Do the thing");

        Assert.Equal(2, callCount);
        var commitArgs = captured[1];
        Assert.Equal("commit", commitArgs[0]);
        Assert.Equal("-m", commitArgs[1]);
        Assert.Equal("[2026-03-31-feature] my-task: Do the thing", commitArgs[2]);
    }
}

/// <summary>
///     Creates and manages a temporary git repository for use in tests.
///     Initialises a <c>main</c> branch with one initial commit.
/// </summary>
internal sealed class GitRepoScope : IDisposable
{
    private readonly TemporaryDirectoryScope tempDir;

    internal GitRepoScope()
    {
        tempDir = new TemporaryDirectoryScope();
        Path = tempDir.Path;

        // Initialise repository on 'main'
        Git("init -b main");
        Git("config user.email \"ags-test@example.com\"");
        Git("config user.name \"AGS Test\"");

        // Create an initial commit so 'main' exists as a branch
        File.WriteAllText(System.IO.Path.Combine(Path, "readme.txt"), "initial content");
        Git("add --all");
        Git("commit -m \"Initial commit\"");
    }

    /// <summary>Gets the absolute path to the temporary repository root.</summary>
    internal string Path { get; }

    /// <summary>
    ///     Runs a git command in the repository and returns the result.
    ///     Throws when git is not on PATH or the working directory does not exist.
    /// </summary>
    internal (int ExitCode, string StandardOutput, string StandardError) Git(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = Path,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    /// <summary>
    ///     Returns the name of the currently checked-out branch.
    /// </summary>
    internal string CurrentBranch()
    {
        var (_, output, _) = Git("rev-parse --abbrev-ref HEAD");
        return output.Trim();
    }

    /// <summary>
    ///     Returns <see langword="true" /> when the specified local branch exists.
    /// </summary>
    internal bool BranchExists(string branchName)
    {
        var (exitCode, _, _) = Git($"rev-parse --verify {branchName}");
        return exitCode == 0;
    }

    public void Dispose()
    {
        tempDir.Dispose();
    }
}
