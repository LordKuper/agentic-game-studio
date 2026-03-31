using System.Diagnostics;
using AGS.ai;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="CodexAdapter" /> invocation, argument assembly, and error handling.
/// </summary>
public sealed class CodexAdapterTests
{
    /// <summary>
    ///     Verifies that the provider ID is "codex".
    /// </summary>
    [Fact]
    public void ProviderIdIsCodex()
    {
        var adapter = new CodexAdapter(AlwaysSucceedRunner());
        Assert.Equal("codex", adapter.ProviderId);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns true when the CLI exits with code 0.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsTrueWhenCliSucceeds()
    {
        var adapter = new CodexAdapter(AlwaysSucceedRunner());
        Assert.True(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns false when the CLI exits with a non-zero code.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsFalseWhenCliFails()
    {
        var adapter = new CodexAdapter(AlwaysFailRunner());
        Assert.False(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns false when the process runner throws.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsFalseWhenRunnerThrows()
    {
        var adapter = new CodexAdapter(ThrowingRunner());
        Assert.False(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that Invoke passes the task prompt as an argument.
    /// </summary>
    [Fact]
    public void InvokePassesTaskPromptAsArgument()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new CodexAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "write the shader", tempDir.Path,
            TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.NotNull(capturedStartInfo);
        Assert.Contains("write the shader", capturedStartInfo.Arguments);
    }

    /// <summary>
    ///     Verifies that Invoke includes --system-prompt when a system prompt is provided.
    /// </summary>
    [Fact]
    public void InvokeIncludesSystemPromptWhenProvided()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new CodexAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("you are a programmer", "write the shader",
            tempDir.Path, TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.Contains("--system-prompt", capturedStartInfo.Arguments);
        Assert.Contains("you are a programmer", capturedStartInfo.Arguments);
    }

    /// <summary>
    ///     Verifies that Invoke sets the working directory on the process start info.
    /// </summary>
    [Fact]
    public void InvokeSetsWorkingDirectory()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new CodexAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.Equal(tempDir.Path, capturedStartInfo.WorkingDirectory);
    }

    /// <summary>
    ///     Verifies that a successful invocation returns a succeeded result.
    /// </summary>
    [Fact]
    public void InvokeReturnsSucceededResultOnExitCodeZero()
    {
        var adapter = new CodexAdapter(si =>
        {
            if (si.FileName == "git") return (0, string.Empty, string.Empty);
            return (0, "codex output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.True(result.Success);
        Assert.Equal("codex output", result.Output);
    }

    /// <summary>
    ///     Verifies that a failed invocation returns a failed result with the error output.
    /// </summary>
    [Fact]
    public void InvokeReturnsFailedResultOnNonZeroExitCode()
    {
        var adapter = new CodexAdapter(si => (1, string.Empty, "codex error"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.Equal("codex error", result.ErrorMessage);
    }

    /// <summary>
    ///     Verifies that an exception thrown by the process runner is caught and returned as a
    ///     failed result.
    /// </summary>
    [Fact]
    public void InvokeCatchesProcessRunnerException()
    {
        var adapter = new CodexAdapter(ThrowingRunner());

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.Equal(-1, result.ExitCode);
    }

    // ── Rate-limit detection ──────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that "rate limit" in stderr produces a rate-limited result.
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultWhenStderrContainsRateLimit()
    {
        var adapter = new CodexAdapter(si => (1, string.Empty, "Error: rate limit exceeded"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.True(result.IsRateLimited);
    }

    /// <summary>
    ///     Verifies that "too many requests" in output triggers rate-limited.
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultWhenOutputContainsTooManyRequests()
    {
        var adapter = new CodexAdapter(si => (1, "Too Many Requests", string.Empty));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.True(result.IsRateLimited);
    }

    /// <summary>
    ///     Verifies that a generic non-zero exit with no rate-limit keywords is NOT flagged.
    /// </summary>
    [Fact]
    public void InvokeDoesNotFlagRateLimitOnUnrelatedError()
    {
        var adapter = new CodexAdapter(si => (1, string.Empty, "unknown command"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.False(result.IsRateLimited);
    }

    /// <summary>
    ///     Verifies that "retry after N seconds" sets RateLimitResetsAt.
    /// </summary>
    [Fact]
    public void DetectRateLimitParsesRetryAfterSeconds()
    {
        var before = DateTimeOffset.UtcNow;
        var (isRateLimited, resetsAt) = CodexAdapter.DetectRateLimit(
            string.Empty, "quota exceeded, retry after 120s");

        Assert.True(isRateLimited);
        Assert.NotNull(resetsAt);
        Assert.True(resetsAt.Value >= before.AddSeconds(115));
        Assert.True(resetsAt.Value <= before.AddSeconds(125));
    }

    /// <summary>
    ///     Verifies that rate-limit text with no parsable time returns null for ResetsAt.
    /// </summary>
    [Fact]
    public void DetectRateLimitReturnsNullResetsAtWhenNoParsableTime()
    {
        var (isRateLimited, resetsAt) = CodexAdapter.DetectRateLimit(
            string.Empty, "rate limit hit");

        Assert.True(isRateLimited);
        Assert.Null(resetsAt);
    }

    private static Func<ProcessStartInfo, (int, string, string)> AlwaysSucceedRunner()
        => _ => (0, string.Empty, string.Empty);

    private static Func<ProcessStartInfo, (int, string, string)> AlwaysFailRunner()
        => _ => (1, string.Empty, "error");

    private static Func<ProcessStartInfo, (int, string, string)> ThrowingRunner()
        => _ => throw new InvalidOperationException("simulated failure");
}
