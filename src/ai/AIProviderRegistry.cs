namespace AGS.ai;

/// <summary>
///     Manages available AI providers. All providers whose corresponding setting flag is enabled
///     in <see cref="AgsSettings" /> are considered active. Different agents use different
///     providers based on agent preference and provider availability.
/// </summary>
internal sealed class AIProviderRegistry
{
    private readonly List<IAIProvider> providers = [];

    /// <summary>
    ///     Registers a provider adapter. Providers with duplicate IDs are ignored.
    /// </summary>
    /// <param name="provider">Provider to register.</param>
    internal void Register(IAIProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (providers.Any(p => p.ProviderId == provider.ProviderId)) return;
        providers.Add(provider);
    }

    /// <summary>
    ///     Gets all registered providers whose corresponding setting flag is enabled in
    ///     <see cref="AgsSettings" />. Does not check <see cref="IAIProvider.IsAvailable" />.
    /// </summary>
    internal IReadOnlyList<IAIProvider> GetEnabledProviders()
    {
        return providers.Where(p => IsEnabledInSettings(p.ProviderId)).ToList();
    }

    /// <summary>
    ///     Resolves the provider to use for an agent invocation by walking the agent's ordered
    ///     model preference list and returning the first provider that is registered and enabled.
    ///     Falls back to the first enabled provider when no preferred model matches.
    ///     Returns <see langword="null" /> when no providers are enabled.
    /// </summary>
    /// <param name="preferredModels">
    ///     Ordered list of model names from the agent definition (e.g. <c>claude-sonnet</c>,
    ///     <c>chatgpt</c>), highest priority first. Pass <see langword="null" /> or an empty list
    ///     when the agent has no preference.
    /// </param>
    internal IAIProvider ResolveProvider(IReadOnlyList<string> preferredModels = null)
    {
        if (preferredModels != null)
        {
            foreach (var model in preferredModels)
            {
                var providerId = GetProviderIdForModel(model);
                if (providerId == null) continue;
                var provider = providers.FirstOrDefault(p =>
                    p.ProviderId == providerId && IsEnabledInSettings(p.ProviderId));
                if (provider != null) return provider;
            }
        }
        return GetEnabledProviders().FirstOrDefault();
    }

    /// <summary>
    ///     Maps a versionless model name from an agent definition to a provider ID.
    ///     Returns <see langword="null" /> for unrecognised model names.
    /// </summary>
    private static string GetProviderIdForModel(string modelName) => modelName switch
    {
        "claude-opus" or "claude-sonnet" or "claude-haiku" => ClaudeCodeAdapter.Id,
        "chatgpt" => CodexAdapter.Id,
        _ => null
    };

    /// <summary>
    ///     Gets all registered providers in registration order.
    /// </summary>
    internal IReadOnlyList<IAIProvider> GetAllProviders() => providers.AsReadOnly();

    /// <summary>
    ///     Determines whether a provider is enabled based on its ID and the current settings.
    /// </summary>
    private static bool IsEnabledInSettings(string providerId)
    {
        if (!AgsSettings.HasCurrentSettings) return false;
        var settings = AgsSettings.Current;
        return providerId switch
        {
            ClaudeCodeAdapter.Id => settings.UseClaude,
            CodexAdapter.Id => settings.UseCodex,
            _ => false
        };
    }
}
