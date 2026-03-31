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
}
