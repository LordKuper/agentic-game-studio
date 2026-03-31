using AGS.ai;

namespace AGS.orchestration;

/// <summary>
///     Represents the final outcome of an agent invocation, including which provider handled the
///     successful or terminal attempt.
/// </summary>
internal sealed class AgentInvocationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentInvocationResult" /> class.
    /// </summary>
    /// <param name="providerId">
    ///     ID of the provider that produced the terminal result. Empty when no provider could be
    ///     selected.
    /// </param>
    /// <param name="providerResult">Terminal provider result returned to the caller.</param>
    /// <param name="attemptedProviderIds">Ordered list of provider IDs attempted during the invocation.</param>
    internal AgentInvocationResult(string providerId, AIProviderResult providerResult,
        IReadOnlyList<string> attemptedProviderIds)
    {
        ProviderId = providerId ?? string.Empty;
        ProviderResult = providerResult ?? throw new ArgumentNullException(nameof(providerResult));
        AttemptedProviderIds = attemptedProviderIds == null
            ? Array.Empty<string>()
            : attemptedProviderIds.ToArray();
    }

    /// <summary>
    ///     Gets the ID of the provider that produced the terminal result.
    /// </summary>
    internal string ProviderId { get; }

    /// <summary>
    ///     Gets the terminal provider result returned to the caller.
    /// </summary>
    internal AIProviderResult ProviderResult { get; }

    /// <summary>
    ///     Gets the ordered list of provider IDs attempted during the invocation.
    /// </summary>
    internal IReadOnlyList<string> AttemptedProviderIds { get; }
}
