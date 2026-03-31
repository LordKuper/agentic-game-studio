using AGS.ai;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="AIProviderResult" /> factory methods and property values.
/// </summary>
public sealed class AIProviderResultTests
{
    /// <summary>
    ///     Verifies that a successful result carries output, exit code, and modified files.
    /// </summary>
    [Fact]
    public void SucceededResultHasCorrectProperties()
    {
        var modifiedFiles = new List<string> { "foo.cs", "bar.cs" };
        var result = AIProviderResult.Succeeded("agent output", 0, modifiedFiles);

        Assert.True(result.Success);
        Assert.Equal("agent output", result.Output);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.ErrorMessage);
        Assert.Equal(modifiedFiles, result.ModifiedFiles);
    }

    /// <summary>
    ///     Verifies that a failed result carries the error message and exit code.
    /// </summary>
    [Fact]
    public void FailedResultHasCorrectProperties()
    {
        var result = AIProviderResult.Failed("something went wrong", 1);

        Assert.False(result.Success);
        Assert.Equal("something went wrong", result.ErrorMessage);
        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.ModifiedFiles);
    }

    /// <summary>
    ///     Verifies that a failed result preserves partial output when provided.
    /// </summary>
    [Fact]
    public void FailedResultPreservesPartialOutput()
    {
        var result = AIProviderResult.Failed("error", 2, "partial");

        Assert.False(result.Success);
        Assert.Equal("partial", result.Output);
    }

    /// <summary>
    ///     Verifies that a null error message is normalized to an empty string.
    /// </summary>
    [Fact]
    public void FailedResultNormalizesNullErrorMessage()
    {
        var result = AIProviderResult.Failed(null, -1);

        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    // ── IsRateLimited / RateLimitResetsAt ─────────────────────────────────────

    /// <summary>
    ///     Verifies that Succeeded results are not rate-limited.
    /// </summary>
    [Fact]
    public void SucceededResultIsNotRateLimited()
    {
        var result = AIProviderResult.Succeeded("output", 0, []);

        Assert.False(result.IsRateLimited);
        Assert.Null(result.RateLimitResetsAt);
    }

    /// <summary>
    ///     Verifies that Failed results are not rate-limited.
    /// </summary>
    [Fact]
    public void FailedResultIsNotRateLimited()
    {
        var result = AIProviderResult.Failed("error", 1);

        Assert.False(result.IsRateLimited);
        Assert.Null(result.RateLimitResetsAt);
    }

    /// <summary>
    ///     Verifies that a rate-limited result sets IsRateLimited and carries the reset time.
    /// </summary>
    [Fact]
    public void RateLimitedResultHasCorrectProperties()
    {
        var resetsAt = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
        var result = AIProviderResult.RateLimited("rate limit exceeded", 1, resetsAt, "partial");

        Assert.False(result.Success);
        Assert.True(result.IsRateLimited);
        Assert.Equal(resetsAt, result.RateLimitResetsAt);
        Assert.Equal("rate limit exceeded", result.ErrorMessage);
        Assert.Equal("partial", result.Output);
        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.ModifiedFiles);
    }

    /// <summary>
    ///     Verifies that a rate-limited result accepts a null reset time.
    /// </summary>
    [Fact]
    public void RateLimitedResultAcceptsNullResetTime()
    {
        var result = AIProviderResult.RateLimited("quota exceeded", 1, null);

        Assert.True(result.IsRateLimited);
        Assert.Null(result.RateLimitResetsAt);
    }

    /// <summary>
    ///     Verifies that a null error message in a rate-limited result is normalized to empty.
    /// </summary>
    [Fact]
    public void RateLimitedResultNormalizesNullErrorMessage()
    {
        var result = AIProviderResult.RateLimited(null, 1, null);

        Assert.Equal(string.Empty, result.ErrorMessage);
    }
}
