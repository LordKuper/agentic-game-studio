namespace AGS.ai;

/// <summary>
///     Defines the contract for invoking an AI agent through any provider.
/// </summary>
internal interface IAIProvider
{
    /// <summary>
    ///     Gets the unique identifier for this provider.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    ///     Gets a value indicating whether this provider is currently installed and available.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    ///     Invokes an AI agent with the given prompt and returns the result.
    /// </summary>
    /// <param name="request">Request containing prompts, working directory, and timeout.</param>
    /// <returns>The result of the invocation.</returns>
    AIProviderResult Invoke(AIProviderRequest request);
}
