namespace AGS.orchestration;

/// <summary>
///     Defines the contract for invoking agents via the orchestration layer.
/// </summary>
internal interface IAgentOrchestrator
{
    /// <summary>
    ///     Invokes a named agent and returns the terminal result, applying provider failover when
    ///     the primary provider is rate-limited.
    /// </summary>
    /// <param name="request">Agent invocation request.</param>
    /// <returns>The terminal result of the invocation.</returns>
    AgentInvocationResult InvokeAgent(AgentInvocationRequest request);

    /// <summary>
    ///     Invokes the default AI provider for a general task (not tied to a named agent
    ///     definition). The provider is selected from
    ///     <see cref="AgsSettings.DefaultModels" /> with the same rate-limit failover logic
    ///     as <see cref="InvokeAgent" />.
    /// </summary>
    /// <param name="systemPrompt">System-level instructions for the AI.</param>
    /// <param name="taskPrompt">Primary task instruction sent to the AI.</param>
    /// <param name="workingDirectory">Working directory for the AI subprocess.</param>
    /// <param name="timeout">Maximum time to wait for a provider response.</param>
    /// <returns>The terminal result of the invocation.</returns>
    AgentInvocationResult InvokeDefault(string systemPrompt, string taskPrompt,
        string workingDirectory, TimeSpan timeout);
}
