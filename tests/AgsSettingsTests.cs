namespace AGS.Tests;

/// <summary>
///     Covers configuration parsing, persistence, and process-wide state management.
/// </summary>
public sealed class AgsSettingsTests
{
    /// <summary>
    ///     Verifies that flags and defaults are set correctly by the constructor.
    /// </summary>
    [Fact]
    public void ConstructorSetsFlags()
    {
        var settings = new AgsSettings(true, false);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        Assert.False(settings.AreAllModelsDisabled);
        Assert.Equal(AgsSettings.DefaultRateLimitCooldownMinutes,
            settings.RateLimitDefaultCooldownMinutes);
        Assert.Equal(TimeSpan.FromMinutes(AgsSettings.DefaultRateLimitCooldownMinutes),
            settings.RateLimitDefaultCooldown);
    }

    /// <summary>
    ///     Verifies that the configuration path uses the project root and the fixed file names.
    /// </summary>
    [Fact]
    public void GetConfigPathBuildsAgsConfigPath()
    {
        var configPath = AgsSettings.GetConfigPath(@"D:\work\project");
        Assert.Equal(Path.Combine(@"D:\work\project", ".ags", "config.json"), configPath);
    }

    /// <summary>
    ///     Verifies that the process-wide current settings instance can be set and retrieved.
    /// </summary>
    [Fact]
    public void SetCurrentStoresProcessWideSettings()
    {
        AgsTestState.ResetCurrentSettings();
        var settings = new AgsSettings(true, true);
        AgsSettings.SetCurrent(settings);
        Assert.True(AgsSettings.HasCurrentSettings);
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
    }

    /// <summary>
    ///     Verifies that invalid JSON and invalid legacy content are rejected.
    /// </summary>
    [Fact]
    public void TryReadFromConfigReturnsFalseForInvalidContent()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(configPath, "{ \"use-claude\": true }");
        var canRead = AgsSettings.TryReadFromConfig(configPath, out var settings);
        Assert.False(canRead);
        Assert.False(settings.UseClaude);
        Assert.False(settings.UseCodex);
    }

    /// <summary>
    ///     Verifies that the reader rejects a missing configuration file.
    /// </summary>
    [Fact]
    public void TryReadFromConfigReturnsFalseForMissingFile()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "missing.json");
        var canRead = AgsSettings.TryReadFromConfig(configPath, out var settings);
        Assert.False(canRead);
        Assert.False(settings.UseClaude);
        Assert.False(settings.UseCodex);
    }

    /// <summary>
    ///     Verifies that legacy key-value configuration content is still supported.
    /// </summary>
    [Fact]
    public void TryReadFromConfigSupportsLegacyFormat()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var legacyContent = string.Join(Environment.NewLine,
            "use-claude=true", "use-codex=false", "ignored-entry=ignored");
        File.WriteAllText(configPath, legacyContent);
        var canRead = AgsSettings.TryReadFromConfig(configPath, out var settings);
        Assert.True(canRead);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        Assert.Equal(AgsSettings.DefaultRateLimitCooldownMinutes,
            settings.RateLimitDefaultCooldownMinutes);
    }

    /// <summary>
    ///     Verifies that project configuration writing creates the expected directory and file.
    /// </summary>
    [Fact]
    public void TryWriteToProjectConfigCreatesAgsDirectory()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var settings = new AgsSettings(true, true);
        var didWrite = settings.TryWriteToProjectConfig(tempDirectory.Path, out var errorMessage);
        Assert.True(didWrite);
        Assert.Equal(string.Empty, errorMessage);
        var configPath = Path.Combine(tempDirectory.Path, ".ags", "config.json");
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloadedSettings));
        Assert.True(reloadedSettings.UseClaude);
        Assert.True(reloadedSettings.UseCodex);
    }

    /// <summary>
    ///     Verifies that copy helpers update only the requested values and preserve cooldown
    ///     configuration.
    /// </summary>
    [Fact]
    public void WithMethodsReturnUpdatedCopies()
    {
        var cooldowns = new Dictionary<string, DateTimeOffset>
        {
            ["claude-code"] = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        var settings = new AgsSettings(false, false, 45, cooldowns);
        var updatedClaude = settings.WithUseClaude(true);
        var updatedCodex = settings.WithUseCodex(true);
        var updatedMinutes = settings.WithRateLimitDefaultCooldownMinutes(60);

        Assert.True(updatedClaude.UseClaude);
        Assert.False(updatedClaude.UseCodex);
        Assert.Equal(45, updatedClaude.RateLimitDefaultCooldownMinutes);
        Assert.Single(updatedClaude.ProviderCooldowns);

        Assert.False(updatedCodex.UseClaude);
        Assert.True(updatedCodex.UseCodex);
        Assert.Equal(45, updatedCodex.RateLimitDefaultCooldownMinutes);
        Assert.Single(updatedCodex.ProviderCooldowns);

        Assert.Equal(60, updatedMinutes.RateLimitDefaultCooldownMinutes);
        Assert.Single(updatedMinutes.ProviderCooldowns);
        Assert.True(new AgsSettings(false, false).AreAllModelsDisabled);
    }

    /// <summary>
    ///     Verifies that JSON configuration is written and read back without losing values.
    /// </summary>
    [Fact]
    public void WriteToConfigAndTryReadFromConfigRoundTripJson()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var settings = new AgsSettings(true, false, 45, null);
        settings.WriteToConfig(configPath);
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloadedSettings));
        Assert.True(reloadedSettings.UseClaude);
        Assert.False(reloadedSettings.UseCodex);
        Assert.Equal(45, reloadedSettings.RateLimitDefaultCooldownMinutes);
    }

    /// <summary>
    ///     Verifies that default values for the cooldown properties use minutes.
    /// </summary>
    [Fact]
    public void DefaultSettingsHaveEmptyCooldownsAndDefaultCooldownMinutes()
    {
        var settings = new AgsSettings(true, false);

        Assert.Equal(AgsSettings.DefaultRateLimitCooldownMinutes,
            settings.RateLimitDefaultCooldownMinutes);
        Assert.Empty(settings.ProviderCooldowns);
    }

    /// <summary>
    ///     Verifies that provider cooldowns round-trip through JSON correctly, and that expired
    ///     entries are dropped on write.
    /// </summary>
    [Fact]
    public void WriteToConfigRoundTripsProviderCooldownsAndDropsExpired()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var futureExpiry = DateTimeOffset.UtcNow.AddMinutes(30);
        var pastExpiry = DateTimeOffset.UtcNow.AddMinutes(-5);
        var cooldowns = new Dictionary<string, DateTimeOffset>
        {
            ["claude-code"] = futureExpiry,
            ["codex"] = pastExpiry
        };
        var settings = new AgsSettings(true, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            cooldowns);

        settings.WriteToConfig(configPath);
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloaded));

        Assert.True(reloaded.ProviderCooldowns.ContainsKey("claude-code"));
        Assert.Equal(futureExpiry.ToUniversalTime(), reloaded.ProviderCooldowns["claude-code"],
            TimeSpan.FromSeconds(1));
        Assert.False(reloaded.ProviderCooldowns.ContainsKey("codex"));
    }

    /// <summary>
    ///     Verifies that the default cooldown in minutes round-trips through JSON.
    /// </summary>
    [Fact]
    public void WriteToConfigRoundTripsRateLimitDefaultCooldownMinutes()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var settings = new AgsSettings(true, false, 60, null);

        settings.WriteToConfig(configPath);
        var content = File.ReadAllText(configPath);
        Assert.Contains("rate-limit-default-cooldown-minutes", content);
        Assert.DoesNotContain("\"rate-limit-default-cooldown\":", content);
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloaded));

        Assert.Equal(60, reloaded.RateLimitDefaultCooldownMinutes);
    }

    /// <summary>
    ///     Verifies that WithProviderCooldowns returns a copy with the updated map.
    /// </summary>
    [Fact]
    public void WithProviderCooldownsReturnsUpdatedCopy()
    {
        var settings = new AgsSettings(true, false);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(15);
        var cooldowns = new Dictionary<string, DateTimeOffset> { ["claude-code"] = expiry };

        var updated = settings.WithProviderCooldowns(cooldowns);

        Assert.True(updated.ProviderCooldowns.ContainsKey("claude-code"));
        Assert.Equal(expiry.ToUniversalTime(), updated.ProviderCooldowns["claude-code"],
            TimeSpan.FromSeconds(1));
        Assert.Empty(settings.ProviderCooldowns);
    }

    /// <summary>
    ///     Verifies that a JSON config without provider-cooldowns key parses successfully with an
    ///     empty cooldown map.
    /// </summary>
    [Fact]
    public void TryReadFromConfigReturnsEmptyCooldownsWhenKeyAbsent()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(configPath, "{\"use-claude\":true,\"use-codex\":false}");

        var canRead = AgsSettings.TryReadFromConfig(configPath, out var settings);

        Assert.True(canRead);
        Assert.Empty(settings.ProviderCooldowns);
        Assert.Equal(AgsSettings.DefaultRateLimitCooldownMinutes,
            settings.RateLimitDefaultCooldownMinutes);
    }

    /// <summary>
    ///     Verifies that a JSON config with provider-cooldowns present is fully parsed.
    /// </summary>
    [Fact]
    public void TryReadFromConfigParsesProviderCooldowns()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var expiry = DateTimeOffset.UtcNow.AddMinutes(20);
        var json = $@"{{
  ""use-claude"": true,
  ""use-codex"": false,
  ""rate-limit-default-cooldown-minutes"": 15,
  ""provider-cooldowns"": {{
    ""claude-code"": ""{expiry:O}""
  }}
}}";
        File.WriteAllText(configPath, json);

        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var settings));
        Assert.Equal(15, settings.RateLimitDefaultCooldownMinutes);
        Assert.True(settings.ProviderCooldowns.ContainsKey("claude-code"));
        Assert.Equal(expiry.ToUniversalTime(), settings.ProviderCooldowns["claude-code"],
            TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Verifies that legacy seconds-based cooldown values are migrated to whole minutes.
    /// </summary>
    [Fact]
    public void TryReadFromConfigMigratesLegacySecondsCooldownToMinutes()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(configPath, @"{
  ""use-claude"": true,
  ""use-codex"": true,
  ""rate-limit-default-cooldown"": 1800
}");

        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var settings));
        Assert.Equal(30, settings.RateLimitDefaultCooldownMinutes);
    }

    // ── DefaultModels ─────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that DefaultModels defaults to an empty list when not supplied.
    /// </summary>
    [Fact]
    public void DefaultModelsIsEmptyWhenNotConfigured()
    {
        var settings = new AgsSettings(true, true);
        Assert.Empty(settings.DefaultModels);
    }

    /// <summary>
    ///     Verifies that WithDefaultModels returns a copy with the specified model list and
    ///     preserves all other fields.
    /// </summary>
    [Fact]
    public void WithDefaultModelsReturnsUpdatedCopy()
    {
        var settings = new AgsSettings(true, false);
        var updated = settings.WithDefaultModels(["claude-sonnet", "chatgpt"]);

        Assert.Equal(["claude-sonnet", "chatgpt"], updated.DefaultModels);
        Assert.True(updated.UseClaude);
        Assert.False(updated.UseCodex);
        Assert.Empty(settings.DefaultModels);
    }

    /// <summary>
    ///     Verifies that other With* methods preserve the DefaultModels list.
    /// </summary>
    [Fact]
    public void WithMethodsPreserveDefaultModels()
    {
        var settings = new AgsSettings(true, false)
            .WithDefaultModels(["claude-opus"]);

        Assert.Equal(["claude-opus"], settings.WithUseClaude(false).DefaultModels);
        Assert.Equal(["claude-opus"], settings.WithUseCodex(true).DefaultModels);
        Assert.Equal(["claude-opus"],
            settings.WithRateLimitDefaultCooldownMinutes(60).DefaultModels);
        Assert.Equal(["claude-opus"],
            settings.WithProviderCooldowns(new Dictionary<string, DateTimeOffset>()).DefaultModels);
    }

    /// <summary>
    ///     Verifies that DefaultModels round-trips through JSON serialization.
    /// </summary>
    [Fact]
    public void WriteToConfigRoundTripsDefaultModels()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        var settings = new AgsSettings(true, false, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["claude-sonnet", "chatgpt"]);

        settings.WriteToConfig(configPath);
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloaded));

        Assert.Equal(["claude-sonnet", "chatgpt"], reloaded.DefaultModels);
    }

    /// <summary>
    ///     Verifies that a JSON config without default-models key parses with an empty list.
    /// </summary>
    [Fact]
    public void TryReadFromConfigReturnsEmptyDefaultModelsWhenKeyAbsent()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        var configPath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(configPath, "{\"use-claude\":true,\"use-codex\":false}");

        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var settings));
        Assert.Empty(settings.DefaultModels);
    }
}
