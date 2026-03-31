namespace AGS.ai;

/// <summary>
///     Manages available AI providers. All providers whose corresponding setting flag is enabled
///     in <see cref="AgsSettings" /> are considered active. Different agents use different
///     providers based on agent preference and provider availability.
/// </summary>
internal sealed class AIProviderRegistry
{
    private readonly List<IAIProvider> providers = [];
    private readonly Dictionary<string, DateTimeOffset> cooldowns = [];
    private readonly string projectRootPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AIProviderRegistry" /> class.
    ///     Cooldowns are tracked in memory only.
    /// </summary>
    internal AIProviderRegistry() : this(null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AIProviderRegistry" /> class.
    ///     When <paramref name="projectRootPath" /> is provided, cooldown state is loaded from and
    ///     persisted to <c>.ags/config.json</c> inside that directory.
    /// </summary>
    /// <param name="projectRootPath">
    ///     Absolute path to the game project root directory. Pass <see langword="null" /> to
    ///     disable persistence.
    /// </param>
    internal AIProviderRegistry(string projectRootPath)
    {
        this.projectRootPath = projectRootPath;
        LoadCooldownsFromSettings();
    }

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
    ///     <see cref="AgsSettings" />. Does not check <see cref="IAIProvider.IsAvailable" /> or
    ///     provider cooldown state.
    /// </summary>
    internal IReadOnlyList<IAIProvider> GetEnabledProviders()
    {
        return providers.Where(p => IsEnabledInSettings(p.ProviderId)).ToList();
    }

    /// <summary>
    ///     Resolves the provider to use for an agent invocation by walking the agent's ordered
    ///     model preference list and returning the first provider that is registered, enabled, and
    ///     not in cooldown.
    ///     Falls back to the first enabled, non-cooled-down provider when no preferred model
    ///     matches.
    ///     Returns <see langword="null" /> when no eligible providers are available.
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
                    p.ProviderId == providerId &&
                    IsEnabledInSettings(p.ProviderId) &&
                    !IsInCooldown(p.ProviderId));
                if (provider != null) return provider;
            }
        }
        return GetEnabledProviders().FirstOrDefault(p => !IsInCooldown(p.ProviderId));
    }

    /// <summary>
    ///     Resolves the next available provider to use as a failover when
    ///     <paramref name="currentProviderId" /> is rate-limited. Follows the same preference
    ///     order as <see cref="ResolveProvider" /> but skips the current provider.
    ///     Returns <see langword="null" /> when no other eligible providers are available.
    /// </summary>
    /// <param name="currentProviderId">
    ///     ID of the provider that triggered the failover. This provider is skipped even if it is
    ///     not marked as in cooldown.
    /// </param>
    /// <param name="preferredModels">
    ///     Ordered model preference list from the agent definition. Pass <see langword="null" />
    ///     for no preference.
    /// </param>
    internal IAIProvider GetNextAvailableProvider(string currentProviderId,
        IReadOnlyList<string> preferredModels = null)
    {
        if (preferredModels != null)
        {
            foreach (var model in preferredModels)
            {
                var providerId = GetProviderIdForModel(model);
                if (providerId == null || providerId == currentProviderId) continue;
                var provider = providers.FirstOrDefault(p =>
                    p.ProviderId == providerId &&
                    IsEnabledInSettings(p.ProviderId) &&
                    !IsInCooldown(p.ProviderId));
                if (provider != null) return provider;
            }
        }
        return GetEnabledProviders()
            .FirstOrDefault(p => p.ProviderId != currentProviderId && !IsInCooldown(p.ProviderId));
    }

    /// <summary>
    ///     Marks a provider as temporarily unavailable due to rate limiting and persists the
    ///     cooldown expiry to <c>.ags/config.json</c> when a project root path is configured.
    /// </summary>
    /// <param name="providerId">ID of the provider to mark as rate-limited.</param>
    /// <param name="availableAfter">
    ///     The time at which the provider becomes available again.
    /// </param>
    internal void MarkRateLimited(string providerId, DateTimeOffset availableAfter)
    {
        if (string.IsNullOrEmpty(providerId)) throw new ArgumentException(
            "Provider ID must not be null or empty.", nameof(providerId));
        cooldowns[providerId] = availableAfter.ToUniversalTime();
        PersistCooldowns();
    }

    /// <summary>
    ///     Gets the time at which a rate-limited provider becomes available again.
    ///     Returns <see langword="null" /> when the provider is not currently in cooldown.
    /// </summary>
    /// <param name="providerId">ID of the provider to query.</param>
    internal DateTimeOffset? GetCooldownExpiry(string providerId)
    {
        if (!cooldowns.TryGetValue(providerId, out var expiry)) return null;
        if (expiry <= DateTimeOffset.UtcNow) return null;
        return expiry;
    }

    /// <summary>
    ///     Gets a value indicating whether the specified provider is currently in cooldown.
    /// </summary>
    /// <param name="providerId">ID of the provider to check.</param>
    internal bool IsInCooldown(string providerId)
    {
        return cooldowns.TryGetValue(providerId, out var expiry) && expiry > DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Gets the earliest cooldown expiry time among all rate-limited providers. Returns
    ///     <see langword="null" /> when no providers are currently in cooldown.
    /// </summary>
    internal DateTimeOffset? GetEarliestCooldownExpiry()
    {
        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? earliest = null;
        foreach (var expiry in cooldowns.Values)
        {
            if (expiry <= now) continue;
            if (earliest == null || expiry < earliest.Value) earliest = expiry;
        }
        return earliest;
    }

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
    ///     Loads non-expired cooldown entries from the current <see cref="AgsSettings" />.
    /// </summary>
    private void LoadCooldownsFromSettings()
    {
        if (!AgsSettings.HasCurrentSettings) return;
        var now = DateTimeOffset.UtcNow;
        foreach (var (providerId, expiry) in AgsSettings.Current.ProviderCooldowns)
        {
            if (expiry > now)
                cooldowns[providerId] = expiry;
        }
    }

    /// <summary>
    ///     Persists active (non-expired) cooldowns to <c>.ags/config.json</c> and updates the
    ///     current <see cref="AgsSettings" /> instance. Does nothing when no project root path is
    ///     configured or no settings have been loaded.
    /// </summary>
    private void PersistCooldowns()
    {
        if (!AgsSettings.HasCurrentSettings) return;
        var now = DateTimeOffset.UtcNow;
        IReadOnlyDictionary<string, DateTimeOffset> activeCooldowns =
            cooldowns.Where(kvp => kvp.Value > now)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var updated = AgsSettings.Current.WithProviderCooldowns(activeCooldowns);
        AgsSettings.SetCurrent(updated);
        if (!string.IsNullOrEmpty(projectRootPath))
            updated.TryWriteToProjectConfig(projectRootPath, out _);
    }
}
