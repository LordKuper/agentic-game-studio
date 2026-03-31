namespace AGS.ai;

/// <summary>
///     Represents the result of an AI provider invocation.
/// </summary>
internal sealed class AIProviderResult
{
    private AIProviderResult(bool success, string output, int exitCode, string errorMessage,
        IReadOnlyList<string> modifiedFiles)
    {
        Success = success;
        Output = output;
        ExitCode = exitCode;
        ErrorMessage = errorMessage;
        ModifiedFiles = modifiedFiles;
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
}
