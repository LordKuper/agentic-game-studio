namespace AGS.ai;

/// <summary>
///     Represents a request to an AI provider.
/// </summary>
internal sealed class AIProviderRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AIProviderRequest" /> class.
    /// </summary>
    /// <param name="systemPrompt">The assembled system prompt.</param>
    /// <param name="taskPrompt">The user-facing task prompt.</param>
    /// <param name="workingDirectory">Working directory for the AI subprocess.</param>
    /// <param name="timeout">Maximum time to wait for a response.</param>
    /// <param name="providerArguments">Additional provider-specific arguments.</param>
    internal AIProviderRequest(string systemPrompt, string taskPrompt, string workingDirectory,
        TimeSpan timeout, IReadOnlyDictionary<string, string> providerArguments = null,
        string outputSchemaPath = null)
    {
        SystemPrompt = systemPrompt ?? string.Empty;
        TaskPrompt = taskPrompt ?? string.Empty;
        WorkingDirectory = workingDirectory ?? string.Empty;
        Timeout = timeout;
        ProviderArguments = providerArguments ?? new Dictionary<string, string>();
        OutputSchemaPath = outputSchemaPath ?? string.Empty;
    }

    /// <summary>
    ///     Gets the assembled system prompt (agent definition + rules + context).
    /// </summary>
    internal string SystemPrompt { get; }

    /// <summary>
    ///     Gets the user-facing task prompt.
    /// </summary>
    internal string TaskPrompt { get; }

    /// <summary>
    ///     Gets the working directory for the AI subprocess.
    /// </summary>
    internal string WorkingDirectory { get; }

    /// <summary>
    ///     Gets the maximum time to wait for a response.
    /// </summary>
    internal TimeSpan Timeout { get; }

    /// <summary>
    ///     Gets additional provider-specific arguments.
    /// </summary>
    internal IReadOnlyDictionary<string, string> ProviderArguments { get; }

    /// <summary>
    ///     Gets the absolute path to the JSON Schema file that describes the expected output
    ///     structure. Each provider adapter interprets this path according to its own CLI:
    ///     Codex uses <c>--output-schema</c>; Claude Code uses <c>--output-format json</c>
    ///     (the schema drives jq/regex extraction on the AGS side). Empty when no schema is
    ///     configured.
    /// </summary>
    internal string OutputSchemaPath { get; }
}
