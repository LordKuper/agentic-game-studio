using AGS.ai;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="AIProviderRegistry" /> registration and provider resolution based on
///     enabled settings.
/// </summary>
public sealed class AIProviderRegistryTests : IDisposable
{
    /// <summary>
    ///     Resets process-wide settings between tests.
    /// </summary>
    public void Dispose()
    {
        AgsTestState.ResetCurrentSettings();
    }

    // ── Registration ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Register throws when a null provider is passed.
    /// </summary>
    [Fact]
    public void RegisterThrowsWhenProviderIsNull()
    {
        var registry = new AIProviderRegistry();
        Assert.Throws<ArgumentNullException>(() => registry.Register(null));
    }

    /// <summary>
    ///     Verifies that Register ignores duplicate provider IDs.
    /// </summary>
    [Fact]
    public void RegisterIgnoresDuplicateProviderIds()
    {
        var registry = new AIProviderRegistry();
        var first = new StubProvider("alpha");
        registry.Register(first);
        registry.Register(new StubProvider("alpha"));

        Assert.Single(registry.GetAllProviders());
        Assert.Same(first, registry.GetAllProviders()[0]);
    }

    /// <summary>
    ///     Verifies that GetAllProviders returns all registered providers in registration order.
    /// </summary>
    [Fact]
    public void GetAllProvidersReturnsAllInRegistrationOrder()
    {
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));

        var all = registry.GetAllProviders();

        Assert.Equal(2, all.Count);
        Assert.Equal(ClaudeCodeAdapter.Id, all[0].ProviderId);
        Assert.Equal(CodexAdapter.Id, all[1].ProviderId);
    }

    // ── GetEnabledProviders ───────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetEnabledProviders returns an empty list when no settings are loaded.
    /// </summary>
    [Fact]
    public void GetEnabledProvidersReturnsEmptyWhenNoSettingsLoaded()
    {
        AgsTestState.ResetCurrentSettings();
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));

        Assert.Empty(registry.GetEnabledProviders());
    }

    /// <summary>
    ///     Verifies that GetEnabledProviders returns only Claude when UseClaude is true.
    /// </summary>
    [Fact]
    public void GetEnabledProvidersReturnsOnlyClaudeWhenClaudeEnabled()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));

        var enabled = registry.GetEnabledProviders();

        Assert.Single(enabled);
        Assert.Equal(ClaudeCodeAdapter.Id, enabled[0].ProviderId);
    }

    /// <summary>
    ///     Verifies that GetEnabledProviders returns only Codex when UseCodex is true.
    /// </summary>
    [Fact]
    public void GetEnabledProvidersReturnsOnlyCodexWhenCodexEnabled()
    {
        AgsSettings.SetCurrent(new AgsSettings(false, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));

        var enabled = registry.GetEnabledProviders();

        Assert.Single(enabled);
        Assert.Equal(CodexAdapter.Id, enabled[0].ProviderId);
    }

    /// <summary>
    ///     Verifies that GetEnabledProviders returns both providers when both are enabled.
    /// </summary>
    [Fact]
    public void GetEnabledProvidersReturnsBothWhenBothEnabled()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));

        var enabled = registry.GetEnabledProviders();

        Assert.Equal(2, enabled.Count);
    }

    /// <summary>
    ///     Verifies that providers with unknown IDs are not returned by GetEnabledProviders even
    ///     when settings are loaded.
    /// </summary>
    [Fact]
    public void GetEnabledProvidersIgnoresUnknownProviderIds()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider("custom-provider"));

        Assert.Empty(registry.GetEnabledProviders());
    }

    // ── ResolveProvider ───────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that ResolveProvider returns null when no providers are enabled.
    /// </summary>
    [Fact]
    public void ResolveProviderReturnsNullWhenNoProvidersEnabled()
    {
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));

        Assert.Null(registry.ResolveProvider());
    }

    /// <summary>
    ///     Verifies that ResolveProvider returns null when no settings have been loaded.
    /// </summary>
    [Fact]
    public void ResolveProviderReturnsNullWhenNoSettingsLoaded()
    {
        AgsTestState.ResetCurrentSettings();
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));

        Assert.Null(registry.ResolveProvider(["claude-sonnet"]));
    }

    /// <summary>
    ///     Verifies that ResolveProvider picks the first enabled model from the preference list
    ///     (claude-sonnet → ClaudeCodeAdapter when Claude is enabled).
    /// </summary>
    [Fact]
    public void ResolveProviderReturnsFirstEnabledModelMatch()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(claude);
        registry.Register(codex);

        // agent prefers claude-sonnet first, chatgpt second
        var resolved = registry.ResolveProvider(["claude-sonnet", "chatgpt"]);

        Assert.Same(claude, resolved);
    }

    /// <summary>
    ///     Verifies that ResolveProvider skips disabled providers and returns the next match.
    /// </summary>
    [Fact]
    public void ResolveProviderSkipsDisabledProviderAndUsesNextMatch()
    {
        // Claude disabled, Codex enabled
        AgsSettings.SetCurrent(new AgsSettings(false, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(codex);

        // agent prefers claude-sonnet first, chatgpt second
        var resolved = registry.ResolveProvider(["claude-sonnet", "chatgpt"]);

        Assert.Same(codex, resolved);
    }

    /// <summary>
    ///     Verifies that ResolveProvider falls back to the first enabled provider when none of the
    ///     preferred models are enabled.
    /// </summary>
    [Fact]
    public void ResolveProviderFallsBackToFirstEnabledWhenNoModelMatches()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        registry.Register(claude);
        registry.Register(new StubProvider(CodexAdapter.Id));

        // agent lists only chatgpt, but Codex is disabled
        var resolved = registry.ResolveProvider(["chatgpt"]);

        Assert.Same(claude, resolved);
    }

    /// <summary>
    ///     Verifies that ResolveProvider falls back to the first enabled provider when called
    ///     with no preference list.
    /// </summary>
    [Fact]
    public void ResolveProviderReturnsFirstEnabledWhenNoPreference()
    {
        AgsSettings.SetCurrent(new AgsSettings(false, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(codex);

        var resolved = registry.ResolveProvider();

        Assert.Same(codex, resolved);
    }

    /// <summary>
    ///     Verifies that all three Claude model names map to ClaudeCodeAdapter.
    /// </summary>
    [Theory]
    [InlineData("claude-opus")]
    [InlineData("claude-sonnet")]
    [InlineData("claude-haiku")]
    public void ResolveProviderRecognisesAllClaudeModelNames(string modelName)
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        registry.Register(claude);

        var resolved = registry.ResolveProvider([modelName]);

        Assert.Same(claude, resolved);
    }

    /// <summary>
    ///     Verifies that the chatgpt model name maps to CodexAdapter.
    /// </summary>
    [Fact]
    public void ResolveProviderRecognisesChatGptModelName()
    {
        AgsSettings.SetCurrent(new AgsSettings(false, true));
        var registry = new AIProviderRegistry();
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(codex);

        var resolved = registry.ResolveProvider(["chatgpt"]);

        Assert.Same(codex, resolved);
    }

    /// <summary>
    ///     Verifies that unknown model names are skipped without throwing.
    /// </summary>
    [Fact]
    public void ResolveProviderSkipsUnrecognisedModelNames()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        registry.Register(claude);

        // "gemini" is not a known model — should fall back to first enabled provider
        var resolved = registry.ResolveProvider(["gemini", "claude-sonnet"]);

        Assert.Same(claude, resolved);
    }

    // ── Cooldown / Rate-limit ─────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that MarkRateLimited throws when provider ID is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MarkRateLimitedThrowsForNullOrEmptyProviderId(string providerId)
    {
        var registry = new AIProviderRegistry();
        Assert.Throws<ArgumentException>(() =>
            registry.MarkRateLimited(providerId, DateTimeOffset.UtcNow.AddMinutes(30)));
    }

    /// <summary>
    ///     Verifies that GetCooldownExpiry returns null for a provider that has not been marked.
    /// </summary>
    [Fact]
    public void GetCooldownExpiryReturnsNullForUnmarkedProvider()
    {
        var registry = new AIProviderRegistry();
        Assert.Null(registry.GetCooldownExpiry(ClaudeCodeAdapter.Id));
    }

    /// <summary>
    ///     Verifies that GetCooldownExpiry returns the expiry time after MarkRateLimited.
    /// </summary>
    [Fact]
    public void GetCooldownExpiryReturnsExpiryAfterMarkRateLimited()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30);
        var registry = new AIProviderRegistry();
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, expiry);

        var result = registry.GetCooldownExpiry(ClaudeCodeAdapter.Id);

        Assert.NotNull(result);
        Assert.Equal(expiry.ToUniversalTime(), result!.Value, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Verifies that GetCooldownExpiry returns null once the cooldown has expired.
    /// </summary>
    [Fact]
    public void GetCooldownExpiryReturnsNullAfterExpiry()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddSeconds(-1));

        Assert.Null(registry.GetCooldownExpiry(ClaudeCodeAdapter.Id));
    }

    /// <summary>
    ///     Verifies that IsInCooldown returns true while within the cooldown window.
    /// </summary>
    [Fact]
    public void IsInCooldownReturnsTrueForActiveCooldown()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(30));

        Assert.True(registry.IsInCooldown(ClaudeCodeAdapter.Id));
    }

    /// <summary>
    ///     Verifies that IsInCooldown returns false once the cooldown has expired.
    /// </summary>
    [Fact]
    public void IsInCooldownReturnsFalseAfterExpiry()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddSeconds(-1));

        Assert.False(registry.IsInCooldown(ClaudeCodeAdapter.Id));
    }

    /// <summary>
    ///     Verifies that ResolveProvider skips a rate-limited provider.
    /// </summary>
    [Fact]
    public void ResolveProviderSkipsRateLimitedProvider()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(claude);
        registry.Register(codex);
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(30));

        var resolved = registry.ResolveProvider(["claude-sonnet", "chatgpt"]);

        Assert.Same(codex, resolved);
    }

    /// <summary>
    ///     Verifies that ResolveProvider returns null when all providers are rate-limited.
    /// </summary>
    [Fact]
    public void ResolveProviderReturnsNullWhenAllProvidersRateLimited()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(30));
        registry.MarkRateLimited(CodexAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(30));

        Assert.Null(registry.ResolveProvider());
    }

    /// <summary>
    ///     Verifies that ResolveProvider uses a provider once its cooldown expires.
    /// </summary>
    [Fact]
    public void ResolveProviderUsesProviderAfterCooldownExpires()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        registry.Register(claude);
        // Expired cooldown — provider should be available
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddSeconds(-1));

        var resolved = registry.ResolveProvider();

        Assert.Same(claude, resolved);
    }

    /// <summary>
    ///     Verifies that GetNextAvailableProvider skips the current provider.
    /// </summary>
    [Fact]
    public void GetNextAvailableProviderSkipsCurrentProvider()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(claude);
        registry.Register(codex);

        var next = registry.GetNextAvailableProvider(ClaudeCodeAdapter.Id,
            ["claude-sonnet", "chatgpt"]);

        Assert.Same(codex, next);
    }

    /// <summary>
    ///     Verifies that GetNextAvailableProvider returns null when the only other provider is
    ///     also in cooldown.
    /// </summary>
    [Fact]
    public void GetNextAvailableProviderReturnsNullWhenAllOthersRateLimited()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(new StubProvider(CodexAdapter.Id));
        registry.MarkRateLimited(CodexAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(30));

        var next = registry.GetNextAvailableProvider(ClaudeCodeAdapter.Id);

        Assert.Null(next);
    }

    /// <summary>
    ///     Verifies that GetEarliestCooldownExpiry returns null when no providers are cooled down.
    /// </summary>
    [Fact]
    public void GetEarliestCooldownExpiryReturnsNullWhenNoCooldowns()
    {
        var registry = new AIProviderRegistry();
        Assert.Null(registry.GetEarliestCooldownExpiry());
    }

    /// <summary>
    ///     Verifies that GetEarliestCooldownExpiry returns null when all cooldowns have expired.
    /// </summary>
    [Fact]
    public void GetEarliestCooldownExpiryReturnsNullWhenAllExpired()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, DateTimeOffset.UtcNow.AddSeconds(-10));
        registry.MarkRateLimited(CodexAdapter.Id, DateTimeOffset.UtcNow.AddSeconds(-5));

        Assert.Null(registry.GetEarliestCooldownExpiry());
    }

    /// <summary>
    ///     Verifies that GetEarliestCooldownExpiry returns the earliest active expiry.
    /// </summary>
    [Fact]
    public void GetEarliestCooldownExpiryReturnsEarliestActiveExpiry()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        var registry = new AIProviderRegistry();
        var earlier = DateTimeOffset.UtcNow.AddMinutes(10);
        var later = DateTimeOffset.UtcNow.AddMinutes(30);
        registry.MarkRateLimited(ClaudeCodeAdapter.Id, earlier);
        registry.MarkRateLimited(CodexAdapter.Id, later);

        var earliest = registry.GetEarliestCooldownExpiry();

        Assert.NotNull(earliest);
        Assert.Equal(earlier.ToUniversalTime(), earliest!.Value, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Verifies that cooldowns loaded from settings at construction are respected by
    ///     ResolveProvider.
    /// </summary>
    [Fact]
    public void RegistryLoadsCooldownsFromSettingsOnConstruction()
    {
        var cooldowns = new Dictionary<string, DateTimeOffset>
        {
            [ClaudeCodeAdapter.Id] = DateTimeOffset.UtcNow.AddMinutes(30)
        };
        AgsSettings.SetCurrent(new AgsSettings(true, true,
            DateTimeOffset.MinValue, DateTimeOffset.MinValue,
            AgsSettings.DefaultRateLimitCooldownSeconds, cooldowns));
        var registry = new AIProviderRegistry();
        var codex = new StubProvider(CodexAdapter.Id);
        registry.Register(new StubProvider(ClaudeCodeAdapter.Id));
        registry.Register(codex);

        // Claude should be skipped (in cooldown loaded from settings)
        var resolved = registry.ResolveProvider(["claude-sonnet", "chatgpt"]);

        Assert.Same(codex, resolved);
    }

    /// <summary>
    ///     Verifies that expired cooldowns loaded from settings are not applied.
    /// </summary>
    [Fact]
    public void RegistryIgnoresExpiredCooldownsFromSettings()
    {
        var cooldowns = new Dictionary<string, DateTimeOffset>
        {
            [ClaudeCodeAdapter.Id] = DateTimeOffset.UtcNow.AddSeconds(-60)  // already expired
        };
        AgsSettings.SetCurrent(new AgsSettings(true, false,
            DateTimeOffset.MinValue, DateTimeOffset.MinValue,
            AgsSettings.DefaultRateLimitCooldownSeconds, cooldowns));
        var claude = new StubProvider(ClaudeCodeAdapter.Id);
        var registry = new AIProviderRegistry();
        registry.Register(claude);

        var resolved = registry.ResolveProvider();

        Assert.Same(claude, resolved);
    }

    /// <summary>
    ///     Verifies that MarkRateLimited persists cooldowns to config.json when a project root is
    ///     supplied.
    /// </summary>
    [Fact]
    public void MarkRateLimitedPersistsCooldownToConfig()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        using var tempDir = new TemporaryDirectoryScope();
        var registry = new AIProviderRegistry(tempDir.Path);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30);

        registry.MarkRateLimited(ClaudeCodeAdapter.Id, expiry);

        var configPath = AgsSettings.GetConfigPath(tempDir.Path);
        Assert.True(File.Exists(configPath));
        var content = File.ReadAllText(configPath);
        Assert.Contains("provider-cooldowns", content);
        Assert.Contains(ClaudeCodeAdapter.Id, content);
    }

    /// <summary>
    ///     Minimal <see cref="IAIProvider" /> stub for registry tests.
    /// </summary>
    private sealed class StubProvider : IAIProvider
    {
        internal StubProvider(string providerId) => ProviderId = providerId;
        public string ProviderId { get; }
        public bool IsAvailable => true;
        public AIProviderResult Invoke(AIProviderRequest request)
            => AIProviderResult.Succeeded(string.Empty, 0, []);
    }
}
