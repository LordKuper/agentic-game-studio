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
