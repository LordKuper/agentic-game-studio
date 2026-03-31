using System.Diagnostics;
using AGS.ai;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="ClaudeCodeAdapter" /> invocation, argument assembly, and error handling.
/// </summary>
public sealed class ClaudeCodeAdapterTests
{
    /// <summary>
    ///     Verifies that the provider ID is "claude-code".
    /// </summary>
    [Fact]
    public void ProviderIdIsClaudeCode()
    {
        var adapter = new ClaudeCodeAdapter(AlwaysSucceedRunner());
        Assert.Equal("claude-code", adapter.ProviderId);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns true when the CLI exits with code 0.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsTrueWhenCliSucceeds()
    {
        var adapter = new ClaudeCodeAdapter(AlwaysSucceedRunner());
        Assert.True(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns false when the CLI exits with a non-zero code.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsFalseWhenCliFailsOrThrows()
    {
        var adapter = new ClaudeCodeAdapter(AlwaysFailRunner());
        Assert.False(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that IsAvailable returns false when the process runner throws.
    /// </summary>
    [Fact]
    public void IsAvailableReturnsFalseWhenRunnerThrows()
    {
        var adapter = new ClaudeCodeAdapter(ThrowingRunner());
        Assert.False(adapter.IsAvailable);
    }

    /// <summary>
    ///     Verifies that Invoke passes the task prompt as a --print argument.
    /// </summary>
    [Fact]
    public void InvokePassesTaskPromptAsPrintArgument()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new ClaudeCodeAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "do the thing", tempDir.Path,
            TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.NotNull(capturedStartInfo);
        Assert.Contains("--print", capturedStartInfo.Arguments);
        Assert.Contains("do the thing", capturedStartInfo.Arguments);
    }

    /// <summary>
    ///     Verifies that Invoke includes --system-prompt when a system prompt is provided.
    /// </summary>
    [Fact]
    public void InvokeIncludesSystemPromptWhenProvided()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new ClaudeCodeAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("you are a designer", "design the level",
            tempDir.Path, TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.Contains("--system-prompt", capturedStartInfo.Arguments);
        Assert.Contains("you are a designer", capturedStartInfo.Arguments);
    }

    /// <summary>
    ///     Verifies that Invoke omits --system-prompt when the system prompt is empty.
    /// </summary>
    [Fact]
    public void InvokeOmitsSystemPromptWhenEmpty()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new ClaudeCodeAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        adapter.Invoke(request);

        Assert.DoesNotContain("--system-prompt", capturedStartInfo.Arguments);
    }

    /// <summary>
    ///     Verifies that Invoke sets the working directory on the process start info.
    /// </summary>
    [Fact]
    public void InvokeSetsWorkingDirectory()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new ClaudeCodeAdapter(si =>
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
    ///     Verifies that a successful invocation returns a succeeded result with the captured output.
    /// </summary>
    [Fact]
    public void InvokeReturnsSucceededResultOnExitCodeZero()
    {
        // The runner returns success for invocation and empty for git diff
        var callCount = 0;
        var adapter = new ClaudeCodeAdapter(si =>
        {
            callCount++;
            if (si.FileName == "git") return (0, string.Empty, string.Empty);
            return (0, "agent produced this", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.True(result.Success);
        Assert.Equal("agent produced this", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    /// <summary>
    ///     Verifies that a failed invocation returns a failed result with the error output.
    /// </summary>
    [Fact]
    public void InvokeReturnsFailedResultOnNonZeroExitCode()
    {
        var adapter = new ClaudeCodeAdapter(si => (1, string.Empty, "rate limit exceeded"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
        Assert.Equal("rate limit exceeded", result.ErrorMessage);
    }

    /// <summary>
    ///     Verifies that a failed invocation uses a default message when stderr is empty.
    /// </summary>
    [Fact]
    public void InvokeProducesDefaultErrorMessageWhenStderrIsEmpty()
    {
        var adapter = new ClaudeCodeAdapter(si => (2, string.Empty, string.Empty));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.Contains("2", result.ErrorMessage);
    }

    /// <summary>
    ///     Verifies that an exception thrown by the process runner is caught and returned as
    ///     a failed result.
    /// </summary>
    [Fact]
    public void InvokeCatchesProcessRunnerException()
    {
        var adapter = new ClaudeCodeAdapter(ThrowingRunner());

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.Equal(-1, result.ExitCode);
        Assert.Contains("simulated failure", result.ErrorMessage);
    }

    /// <summary>
    ///     Verifies that Invoke includes modified files detected via git diff.
    /// </summary>
    [Fact]
    public void InvokeIncludesModifiedFilesFromGitDiff()
    {
        var adapter = new ClaudeCodeAdapter(si =>
        {
            if (si.FileName == "git")
                return (0, "Assets/Scripts/Foo.cs\nAssets/Scripts/Bar.cs", string.Empty);
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.True(result.Success);
        Assert.Contains("Assets/Scripts/Foo.cs", result.ModifiedFiles);
        Assert.Contains("Assets/Scripts/Bar.cs", result.ModifiedFiles);
    }

    /// <summary>
    ///     Verifies that Invoke includes extra provider arguments in the CLI invocation.
    /// </summary>
    [Fact]
    public void InvokePassesProviderArguments()
    {
        ProcessStartInfo capturedStartInfo = null;
        var adapter = new ClaudeCodeAdapter(si =>
        {
            if (si.FileName != "git") capturedStartInfo = si;
            return (0, "output", string.Empty);
        });

        using var tempDir = new TemporaryDirectoryScope();
        var args = new Dictionary<string, string> { ["--model"] = "claude-opus-4-6" };
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1),
            args);
        adapter.Invoke(request);

        Assert.Contains("--model", capturedStartInfo.Arguments);
        Assert.Contains("claude-opus-4-6", capturedStartInfo.Arguments);
    }

    // ── Rate-limit detection ──────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that "rate limit" in stderr produces a rate-limited result.
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultWhenStderrContainsRateLimit()
    {
        var adapter = new ClaudeCodeAdapter(si => (1, string.Empty, "Error: rate limit exceeded"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.True(result.IsRateLimited);
    }

    /// <summary>
    ///     Verifies that "429" in stdout produces a rate-limited result.
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultWhenOutputContains429()
    {
        var adapter = new ClaudeCodeAdapter(si => (1, "HTTP 429 Too Many Requests", string.Empty));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.True(result.IsRateLimited);
    }

    /// <summary>
    ///     Verifies that "overloaded" in stderr triggers rate-limited (Claude-specific signal).
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultWhenStderrContainsOverloaded()
    {
        var adapter = new ClaudeCodeAdapter(si => (1, string.Empty, "API overloaded"));

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
        var adapter = new ClaudeCodeAdapter(si => (1, string.Empty, "fatal: config not found"));

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
        var (isRateLimited, resetsAt) = ClaudeCodeAdapter.DetectRateLimit(
            string.Empty, "rate limit exceeded, retry after 60s");

        Assert.True(isRateLimited);
        Assert.NotNull(resetsAt);
        Assert.True(resetsAt.Value >= before.AddSeconds(55));
        Assert.True(resetsAt.Value <= before.AddSeconds(65));
    }

    /// <summary>
    ///     Verifies that an ISO 8601 reset timestamp after "reset" is parsed correctly.
    /// </summary>
    [Fact]
    public void DetectRateLimitParsesIsoTimestampAfterReset()
    {
        var expected = new DateTimeOffset(2026, 4, 1, 14, 32, 0, TimeSpan.Zero);
        var (isRateLimited, resetsAt) = ClaudeCodeAdapter.DetectRateLimit(
            string.Empty, "rate limit hit, reset at 2026-04-01T14:32:00Z");

        Assert.True(isRateLimited);
        Assert.Equal(expected, resetsAt);
    }

    /// <summary>
    ///     Verifies that Claude-specific limit messages with a localized reset time are detected
    ///     and parsed correctly.
    /// </summary>
    [Fact]
    public void DetectRateLimitParsesLocalizedResetTimeFromClaudeLimitMessage()
    {
        var timeZone = ResolveTimeZone("Europe/Moscow");
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        var (isRateLimited, resetsAt) = ClaudeCodeAdapter.DetectRateLimit(
            string.Empty, "You've hit your limit \u00B7 resets 2pm (Europe/Moscow)");

        Assert.True(isRateLimited);
        Assert.NotNull(resetsAt);

        var localResetTime = TimeZoneInfo.ConvertTime(resetsAt.Value, timeZone);
        Assert.Equal(14, localResetTime.Hour);
        Assert.Equal(0, localResetTime.Minute);
        Assert.Equal(0, localResetTime.Second);
        Assert.True(localResetTime > localNow);
        Assert.True(localResetTime <= localNow.AddHours(25));
    }

    /// <summary>
    ///     Verifies that Claude limit messages are surfaced as rate-limited invocation results.
    /// </summary>
    [Fact]
    public void InvokeReturnsRateLimitedResultForClaudeLimitMessage()
    {
        var adapter = new ClaudeCodeAdapter(si =>
            (1, string.Empty, "You've hit your limit \u00B7 resets 2pm (Europe/Moscow)"));

        using var tempDir = new TemporaryDirectoryScope();
        var request = new AIProviderRequest("", "task", tempDir.Path, TimeSpan.FromMinutes(1));
        var result = adapter.Invoke(request);

        Assert.False(result.Success);
        Assert.True(result.IsRateLimited);
        Assert.NotNull(result.RateLimitResetsAt);
    }

    /// <summary>
    ///     Verifies that rate-limit text with no parsable time returns null for ResetsAt.
    /// </summary>
    [Fact]
    public void DetectRateLimitReturnsNullResetsAtWhenNoParsableTime()
    {
        var (isRateLimited, resetsAt) = ClaudeCodeAdapter.DetectRateLimit(
            string.Empty, "quota exceeded");

        Assert.True(isRateLimited);
        Assert.Null(resetsAt);
    }

    private static Func<ProcessStartInfo, (int, string, string)> AlwaysSucceedRunner()
        => _ => (0, string.Empty, string.Empty);

    private static Func<ProcessStartInfo, (int, string, string)> AlwaysFailRunner()
        => _ => (1, string.Empty, "error");

    private static Func<ProcessStartInfo, (int, string, string)> ThrowingRunner()
        => _ => throw new InvalidOperationException("simulated failure");

    /// <summary>
    ///     Resolves a time zone using either its native identifier or the Windows equivalent for
    ///     IANA IDs.
    /// </summary>
    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            Assert.True(TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsId));
            return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        }
    }
}
