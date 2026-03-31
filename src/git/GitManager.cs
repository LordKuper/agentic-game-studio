using System.Diagnostics;
using System.Text;

namespace AGS.git;

/// <summary>
///     Manages git operations for AGS sessions.
/// </summary>
internal sealed class GitManager
{
    private readonly string projectRootPath;
    private readonly Func<ProcessStartInfo, (int ExitCode, string StandardOutput,
        string StandardError)> processRunner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitManager" /> class using the
    ///     default process runner.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root directory.</param>
    internal GitManager(string projectRootPath) : this(projectRootPath, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitManager" /> class.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root directory.</param>
    /// <param name="processRunner">
    ///     Process runner delegate used for all subprocess invocations. When <see langword="null" />,
    ///     the default runner that spawns a real OS process is used.
    /// </param>
    internal GitManager(string projectRootPath,
        Func<ProcessStartInfo, (int ExitCode, string StandardOutput, string StandardError)>
            processRunner)
    {
        if (string.IsNullOrEmpty(projectRootPath))
            throw new ArgumentException("Project root path must not be null or empty.",
                nameof(projectRootPath));
        this.projectRootPath = projectRootPath;
        this.processRunner = processRunner;
    }

    /// <summary>
    ///     Creates a new branch for a session branched off <c>main</c>.
    /// </summary>
    /// <param name="sessionId">
    ///     Session identifier used to name the branch <c>session/&lt;sessionId&gt;</c>.
    /// </param>
    /// <exception cref="InvalidOperationException">Thrown when git reports an error.</exception>
    internal void CreateSessionBranch(string sessionId)
    {
        var branchName = GetBranchName(sessionId);
        var (exitCode, _, stderr) = RunGit("checkout", "-b", branchName, "main");
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"Failed to create session branch '{branchName}': {stderr.Trim()}");
    }

    /// <summary>
    ///     Stages all current changes and commits them with a task-scoped message in the format
    ///     <c>[&lt;sessionId&gt;] &lt;taskSlug&gt;: &lt;description&gt;</c>.
    /// </summary>
    /// <param name="sessionId">Session identifier included in the commit message prefix.</param>
    /// <param name="taskSlug">Task slug included in the commit message.</param>
    /// <param name="description">Human-readable description of the task work.</param>
    /// <exception cref="InvalidOperationException">Thrown when staging or committing fails.</exception>
    internal void CommitTaskChanges(string sessionId, string taskSlug, string description)
    {
        var (addExitCode, _, addStderr) = RunGit("add", "--all");
        if (addExitCode != 0)
            throw new InvalidOperationException(
                $"Failed to stage changes: {addStderr.Trim()}");

        var message = $"[{sessionId}] {taskSlug}: {description}";
        var (commitExitCode, _, commitStderr) = RunGit("commit", "-m", message);
        if (commitExitCode != 0)
            throw new InvalidOperationException(
                $"Failed to commit task changes: {commitStderr.Trim()}");
    }

    /// <summary>
    ///     Checks whether the session branch has merge conflicts with <c>main</c>.
    ///     Uses <c>git merge-tree</c> for a read-only, working-tree-safe check.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>
    ///     <see langword="true" /> if a merge would produce conflicts; otherwise
    ///     <see langword="false" />.
    /// </returns>
    internal bool HasConflictsWithMain(string sessionId)
    {
        var branchName = GetBranchName(sessionId);

        var (baseExitCode, mergeBase, _) = RunGit("merge-base", branchName, "main");
        if (baseExitCode != 0 || string.IsNullOrWhiteSpace(mergeBase))
            return false;

        var (treeExitCode, treeOutput, _) = RunGit(
            "merge-tree", mergeBase.Trim(), branchName, "main");
        if (treeExitCode != 0)
            return false;

        return treeOutput.Contains("<<<<<<<");
    }

    /// <summary>
    ///     Generates a PR description from the session's scope and execution plan artifacts.
    ///     Missing artifact files are silently skipped.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>Formatted PR description as a Markdown string.</returns>
    internal string GeneratePRDescription(string sessionId)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## Session: {sessionId}");

        var sessionDir = Path.Combine(projectRootPath, ".ags", "sessions", sessionId);

        var scopePath = Path.Combine(sessionDir, "session-scope.md");
        if (File.Exists(scopePath))
        {
            sb.AppendLine();
            sb.AppendLine("## Scope");
            sb.AppendLine(File.ReadAllText(scopePath).Trim());
        }

        var planPath = Path.Combine(sessionDir, "execution-plan.md");
        if (File.Exists(planPath))
        {
            sb.AppendLine();
            sb.AppendLine("## Execution Plan");
            sb.AppendLine(File.ReadAllText(planPath).Trim());
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Merges the session branch into <c>main</c> using a non-fast-forward merge.
    ///     Checks out <c>main</c> first.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the checkout or merge operation fails.
    /// </exception>
    internal void MergeToMain(string sessionId)
    {
        var branchName = GetBranchName(sessionId);

        var (checkoutExitCode, _, checkoutStderr) = RunGit("checkout", "main");
        if (checkoutExitCode != 0)
            throw new InvalidOperationException(
                $"Failed to checkout main: {checkoutStderr.Trim()}");

        var (mergeExitCode, _, mergeStderr) = RunGit("merge", "--no-ff", branchName);
        if (mergeExitCode != 0)
            throw new InvalidOperationException(
                $"Failed to merge '{branchName}' into main: {mergeStderr.Trim()}");
    }

    /// <summary>
    ///     Deletes the session branch. The branch must already be merged (uses <c>-d</c>, not
    ///     <c>-D</c>).
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the branch cannot be deleted (e.g. it is not fully merged).
    /// </exception>
    internal void DeleteSessionBranch(string sessionId)
    {
        var branchName = GetBranchName(sessionId);
        var (exitCode, _, stderr) = RunGit("branch", "-d", branchName);
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"Failed to delete session branch '{branchName}': {stderr.Trim()}");
    }

    private (int ExitCode, string StandardOutput, string StandardError) RunGit(
        params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = projectRootPath
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        if (processRunner != null) return processRunner(startInfo);
        return DefaultRunProcess(startInfo);
    }

    private static (int ExitCode, string StandardOutput, string StandardError) DefaultRunProcess(
        ProcessStartInfo startInfo)
    {
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    private static string GetBranchName(string sessionId) => $"session/{sessionId}";
}
