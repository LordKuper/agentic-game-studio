namespace AGS.ai;

/// <summary>
///     Represents the result of an AI provider invocation.
/// </summary>
internal sealed class AIProviderResult
{
    private AIProviderResult(bool success, string output, int exitCode, string errorMessage,
        IReadOnlyList<string> modifiedFiles, bool isRateLimited = false,
        DateTimeOffset? rateLimitResetsAt = null)
    {
        Success = success;
        Output = output;
        ExitCode = exitCode;
        ErrorMessage = errorMessage;
        ModifiedFiles = modifiedFiles;
        IsRateLimited = isRateLimited;
        RateLimitResetsAt = rateLimitResetsAt;
    }

    /// <summary>
    ///     Gets a value indicating whether the invocation succeeded.
    /// </summary>
    internal bool Success { get; }

    /// <summary>
    ///     Gets the text output from the AI agent.
    /// </summary>
    internal string Output { get; }

    /// <summary>
    ///     Gets the exit code of the CLI subprocess.
    /// </summary>
    internal int ExitCode { get; }

    /// <summary>
    ///     Gets the error message when the invocation failed; otherwise, an empty string.
    /// </summary>
    internal string ErrorMessage { get; }

    /// <summary>
    ///     Gets the files modified during the invocation.
    /// </summary>
    internal IReadOnlyList<string> ModifiedFiles { get; }

    /// <summary>
    ///     Gets a value indicating whether the invocation failed due to a rate limit or quota
    ///     exhaustion.
    /// </summary>
    internal bool IsRateLimited { get; }

    /// <summary>
    ///     Gets the time at which the provider is expected to become available again, as parsed
    ///     from the provider's error response. <see langword="null" /> when the reset time could
    ///     not be determined.
    /// </summary>
    internal DateTimeOffset? RateLimitResetsAt { get; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <param name="output">Text output from the AI agent.</param>
    /// <param name="exitCode">Exit code of the CLI subprocess.</param>
    /// <param name="modifiedFiles">Files modified during the invocation.</param>
    internal static AIProviderResult Succeeded(string output, int exitCode,
        IReadOnlyList<string> modifiedFiles)
    {
        return new AIProviderResult(true, output, exitCode, string.Empty, modifiedFiles);
    }

    /// <summary>
    ///     Creates a failed result.
    /// </summary>
    /// <param name="errorMessage">Error message describing the failure.</param>
    /// <param name="exitCode">Exit code of the CLI subprocess.</param>
    /// <param name="output">Any partial output captured before the failure.</param>
    internal static AIProviderResult Failed(string errorMessage, int exitCode,
        string output = "")
    {
        return new AIProviderResult(false, output, exitCode, errorMessage ?? string.Empty, []);
    }

    /// <summary>
    ///     Creates a rate-limited result.
    /// </summary>
    /// <param name="errorMessage">Error message describing the rate limit.</param>
    /// <param name="exitCode">Exit code of the CLI subprocess.</param>
    /// <param name="resetsAt">
    ///     The time at which the provider becomes available again, or <see langword="null" /> when
    ///     the reset time could not be determined.
    /// </param>
    /// <param name="output">Any partial output captured before the failure.</param>
    internal static AIProviderResult RateLimited(string errorMessage, int exitCode,
        DateTimeOffset? resetsAt, string output = "")
    {
        return new AIProviderResult(false, output, exitCode, errorMessage ?? string.Empty, [],
            isRateLimited: true, rateLimitResetsAt: resetsAt);
    }
}
