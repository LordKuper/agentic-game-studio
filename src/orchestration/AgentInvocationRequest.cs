using AGS.prompt;

namespace AGS.orchestration;

/// <summary>
///     Describes a single agent invocation request handled by the
///     <see cref="AgentOrchestrator" />.
/// </summary>
internal sealed class AgentInvocationRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentInvocationRequest" /> class.
    /// </summary>
    /// <param name="agentName">Logical name of the agent definition to invoke.</param>
    /// <param name="ruleNames">Ordered list of rule names included in the system prompt.</param>
    /// <param name="context">Session and task context for the invocation.</param>
    /// <param name="taskPrompt">User-facing task prompt sent to the provider.</param>
    /// <param name="workingDirectory">Working directory for the provider subprocess.</param>
    /// <param name="timeout">Maximum time to wait for the provider response.</param>
    /// <param name="providerArguments">Additional provider-specific arguments.</param>
    internal AgentInvocationRequest(
        string agentName,
        IReadOnlyList<string> ruleNames,
        PromptContext context,
        string taskPrompt,
        string workingDirectory,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string> providerArguments = null)
    {
        AgentName = agentName ?? string.Empty;
        RuleNames = ruleNames ?? Array.Empty<string>();
        Context = context ?? new PromptContext();
        TaskPrompt = taskPrompt ?? string.Empty;
        WorkingDirectory = workingDirectory ?? string.Empty;
        Timeout = timeout;
        ProviderArguments = providerArguments ?? new Dictionary<string, string>();
    }

    /// <summary>
    ///     Gets the logical name of the agent definition to invoke.
    /// </summary>
    internal string AgentName { get; }

    /// <summary>
    ///     Gets the ordered list of rule names included in the system prompt.
    /// </summary>
    internal IReadOnlyList<string> RuleNames { get; }

    /// <summary>
    ///     Gets the session and task context for the invocation.
    /// </summary>
    internal PromptContext Context { get; }

    /// <summary>
    ///     Gets the user-facing task prompt sent to the provider.
    /// </summary>
    internal string TaskPrompt { get; }

    /// <summary>
    ///     Gets the working directory for the provider subprocess.
    /// </summary>
    internal string WorkingDirectory { get; }

    /// <summary>
    ///     Gets the maximum time to wait for the provider response.
    /// </summary>
    internal TimeSpan Timeout { get; }

    /// <summary>
    ///     Gets additional provider-specific arguments.
    /// </summary>
    internal IReadOnlyDictionary<string, string> ProviderArguments { get; }
}
