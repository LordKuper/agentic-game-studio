using AGS.ai;
using AGS.prompt;

namespace AGS.orchestration;

/// <summary>
///     Coordinates agent invocations by assembling prompts, selecting an eligible AI provider,
///     and applying rate-limit failover when a provider reports quota exhaustion.
/// </summary>
internal sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly PromptAssembler promptAssembler;
    private readonly AIProviderRegistry providerRegistry;
    private readonly ResourceLoader resourceLoader;
    private readonly TimeProvider timeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentOrchestrator" /> class.
    /// </summary>
    /// <param name="resourceLoader">Resource loader used to resolve agent definitions.</param>
    /// <param name="promptAssembler">Prompt assembler used to build provider requests.</param>
    /// <param name="providerRegistry">Provider registry used for resolution and cooldown tracking.</param>
    /// <param name="timeProvider">Clock used for default cooldown calculations.</param>
    internal AgentOrchestrator(ResourceLoader resourceLoader, PromptAssembler promptAssembler,
        AIProviderRegistry providerRegistry, TimeProvider timeProvider = null)
    {
        this.resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
        this.promptAssembler =
            promptAssembler ?? throw new ArgumentNullException(nameof(promptAssembler));
        this.providerRegistry =
            providerRegistry ?? throw new ArgumentNullException(nameof(providerRegistry));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    ///     Invokes an agent using the first eligible provider from the agent's ordered model list.
    ///     When the provider reports rate limiting, the provider is marked on cooldown and the task
    ///     is restarted from the beginning with the next eligible provider.
    /// </summary>
    /// <param name="request">Agent invocation request.</param>
    /// <returns>The terminal result of the invocation.</returns>
    public AgentInvocationResult InvokeAgent(AgentInvocationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.AgentName))
            throw new ArgumentException("Agent name must be provided.", nameof(request));

        var agentDefinition = LoadAgentDefinition(request.AgentName);
        var providerRequest = promptAssembler.BuildRequest(request.AgentName, request.RuleNames,
            request.Context, request.TaskPrompt, request.WorkingDirectory, request.Timeout,
            request.ProviderArguments);
        return ExecuteWithFailover(providerRequest, agentDefinition.Models,
            $"No enabled AI provider is currently available for agent '{request.AgentName}'.");
    }

    /// <summary>
    ///     Invokes the default AI provider for a general task using the model priority list from
    ///     <see cref="AgsSettings.DefaultModels" />, with the same rate-limit failover logic as
    ///     <see cref="InvokeAgent" />.
    /// </summary>
    /// <param name="systemPrompt">System-level instructions for the AI.</param>
    /// <param name="taskPrompt">Primary task instruction sent to the AI.</param>
    /// <param name="workingDirectory">Working directory for the AI subprocess.</param>
    /// <param name="timeout">Maximum time to wait for a provider response.</param>
    /// <returns>The terminal result of the invocation.</returns>
    public AgentInvocationResult InvokeDefault(string systemPrompt, string taskPrompt,
        string workingDirectory, TimeSpan timeout)
    {
        var models = AgsSettings.HasCurrentSettings
            ? AgsSettings.Current.DefaultModels
            : Array.Empty<string>();

        if (models.Count == 0)
        {
            var noProviderResult = AIProviderResult.Failed(
                "No default AI provider is configured. " +
                "Set 'default-models' in the project settings.", -1);
            return new AgentInvocationResult(string.Empty, noProviderResult, []);
        }

        var request = new AIProviderRequest(
            systemPrompt ?? string.Empty,
            taskPrompt ?? string.Empty,
            workingDirectory ?? string.Empty,
            timeout,
            null);

        return ExecuteWithFailover(request, models,
            "No enabled default AI provider is currently available.");
    }

    /// <summary>
    ///     Runs the provider invocation loop with rate-limit failover for the given model priority
    ///     list, returning the terminal result.
    /// </summary>
    private AgentInvocationResult ExecuteWithFailover(AIProviderRequest providerRequest,
        IReadOnlyList<string> models, string noProviderMessage)
    {
        var attemptedProviderIds = new List<string>();
        var provider = ResolveAvailableProvider(models);

        if (provider == null)
            return new AgentInvocationResult(string.Empty,
                AIProviderResult.Failed(noProviderMessage, -1), attemptedProviderIds);

        AIProviderResult lastProviderResult = null;
        var lastProviderId = string.Empty;

        while (provider != null)
        {
            lastProviderId = provider.ProviderId;
            attemptedProviderIds.Add(provider.ProviderId);

            var providerResult = provider.Invoke(providerRequest);
            lastProviderResult = providerResult;

            if (!providerResult.IsRateLimited)
                return new AgentInvocationResult(provider.ProviderId, providerResult,
                    attemptedProviderIds);

            var cooldownExpiry = GetCooldownExpiry(providerResult);
            providerRegistry.MarkRateLimited(provider.ProviderId, cooldownExpiry);
            provider = ResolveNextAvailableProvider(provider.ProviderId, models);
        }

        return new AgentInvocationResult(lastProviderId,
            lastProviderResult ?? AIProviderResult.Failed("Agent invocation failed.", -1),
            attemptedProviderIds);
    }

    /// <summary>
    ///     Loads and parses the requested agent definition from the resource overlay.
    /// </summary>
    /// <param name="agentName">Logical agent name.</param>
    /// <returns>Parsed agent definition.</returns>
    private AgentDefinition LoadAgentDefinition(string agentName)
    {
        var agentMarkdown = resourceLoader.ReadResource("agents", agentName);
        return AgentDefinitionParser.Parse(agentMarkdown);
    }

    /// <summary>
    ///     Resolves the initial provider to use for an invocation, skipping providers that are not
    ///     installed even if they are enabled in settings.
    /// </summary>
    /// <param name="preferredModels">Priority-ordered model preferences from the agent definition.</param>
    /// <returns>The first installed and eligible provider, or <see langword="null" />.</returns>
    private IAIProvider ResolveAvailableProvider(IReadOnlyList<string> preferredModels)
    {
        var provider = providerRegistry.ResolveProvider(preferredModels);
        return SkipUnavailableProviders(provider, preferredModels);
    }

    /// <summary>
    ///     Resolves the next provider to use after a rate-limit event, skipping providers that are
    ///     not installed even if they are enabled in settings.
    /// </summary>
    /// <param name="currentProviderId">Provider ID that should be skipped.</param>
    /// <param name="preferredModels">Priority-ordered model preferences from the agent definition.</param>
    /// <returns>The next installed and eligible provider, or <see langword="null" />.</returns>
    private IAIProvider ResolveNextAvailableProvider(string currentProviderId,
        IReadOnlyList<string> preferredModels)
    {
        var provider = providerRegistry.GetNextAvailableProvider(currentProviderId,
            preferredModels);
        return SkipUnavailableProviders(provider, preferredModels);
    }

    /// <summary>
    ///     Skips providers that are enabled but not installed on the current machine.
    /// </summary>
    /// <param name="provider">Provider returned by the registry.</param>
    /// <param name="preferredModels">Priority-ordered model preferences from the agent definition.</param>
    /// <returns>The first installed provider in the remaining preference order, or <see langword="null" />.</returns>
    private IAIProvider SkipUnavailableProviders(IAIProvider provider,
        IReadOnlyList<string> preferredModels)
    {
        var visitedProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (provider != null && !provider.IsAvailable)
        {
            if (!visitedProviderIds.Add(provider.ProviderId)) return null;
            provider = providerRegistry.GetNextAvailableProvider(provider.ProviderId,
                preferredModels);
        }

        return provider;
    }

    /// <summary>
    ///     Gets the cooldown expiry for a rate-limited provider result, falling back to the
    ///     configured default cooldown when the provider did not return an explicit reset time.
    /// </summary>
    /// <param name="providerResult">Rate-limited provider result.</param>
    /// <returns>The time at which the provider should be considered available again.</returns>
    private DateTimeOffset GetCooldownExpiry(AIProviderResult providerResult)
    {
        if (providerResult.RateLimitResetsAt.HasValue) return providerResult.RateLimitResetsAt.Value;

        var defaultCooldown = AgsSettings.HasCurrentSettings
            ? AgsSettings.Current.RateLimitDefaultCooldown
            : TimeSpan.FromMinutes(AgsSettings.DefaultRateLimitCooldownMinutes);
        return timeProvider.GetUtcNow().Add(defaultCooldown);
    }
}
