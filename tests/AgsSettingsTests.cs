using System.Globalization;

namespace AGS.Tests;

/// <summary>
///     Covers configuration parsing, persistence, and process-wide state management.
/// </summary>
public sealed class AgsSettingsTests
{
    /// <summary>
    ///     Verifies that timestamps are normalized to UTC and state flags are exposed correctly.
    /// </summary>
    [Fact]
    public void ConstructorNormalizesTimestampsAndFlags()
    {
        var claudeTimestamp = new DateTimeOffset(2026, 3, 27, 12, 30, 0, TimeSpan.FromHours(3));
        var codexTimestamp = new DateTimeOffset(2026, 3, 26, 7, 15, 0, TimeSpan.FromHours(-4));
        var settings = new AgsSettings(true, false, claudeTimestamp, codexTimestamp);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        Assert.Equal(claudeTimestamp.ToUniversalTime(), settings.ClaudeLastUpdateUtc);
        Assert.Equal(codexTimestamp.ToUniversalTime(), settings.CodexLastUpdateUtc);
        Assert.True(settings.HasClaudeLastUpdateUtc);
        Assert.True(settings.HasCodexLastUpdateUtc);
        Assert.False(settings.AreAllModelsDisabled);
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
        var claudeTimestamp = new DateTimeOffset(2026, 3, 23, 8, 45, 0, TimeSpan.Zero);
        var codexTimestamp = new DateTimeOffset(2026, 3, 22, 21, 15, 0, TimeSpan.FromHours(3));
        var legacyContent = string.Join(Environment.NewLine, "use-claude=true", "use-codex=false",
            "ignored-entry=ignored",
            "claude-last-update-utc=" + claudeTimestamp.ToString("O", CultureInfo.InvariantCulture),
            "codex-last-update-utc=" + codexTimestamp.ToString("O", CultureInfo.InvariantCulture));
        File.WriteAllText(configPath, legacyContent);
        var canRead = AgsSettings.TryReadFromConfig(configPath, out var settings);
        Assert.True(canRead);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        Assert.Equal(claudeTimestamp, settings.ClaudeLastUpdateUtc);
        Assert.Equal(codexTimestamp.ToUniversalTime(), settings.CodexLastUpdateUtc);
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
    ///     Verifies that copy helpers update only the requested values.
    /// </summary>
    [Fact]
    public void WithMethodsReturnUpdatedCopies()
    {
        var originalTimestamp = new DateTimeOffset(2026, 3, 27, 12, 30, 0, TimeSpan.Zero);
        var updatedTimestamp = originalTimestamp.AddHours(4);
        var settings = new AgsSettings(false, false, originalTimestamp, originalTimestamp);
        var updatedClaude = settings.WithUseClaude(true);
        var updatedCodex = settings.WithUseCodex(true);
        var updatedClaudeTimestamp = settings.WithClaudeLastUpdateUtc(updatedTimestamp);
        var updatedCodexTimestamp = settings.WithCodexLastUpdateUtc(updatedTimestamp);
        Assert.True(updatedClaude.UseClaude);
        Assert.False(updatedClaude.UseCodex);
        Assert.False(updatedCodex.UseClaude);
        Assert.True(updatedCodex.UseCodex);
        Assert.Equal(updatedTimestamp, updatedClaudeTimestamp.ClaudeLastUpdateUtc);
        Assert.Equal(originalTimestamp, updatedClaudeTimestamp.CodexLastUpdateUtc);
        Assert.Equal(updatedTimestamp, updatedCodexTimestamp.CodexLastUpdateUtc);
        Assert.Equal(originalTimestamp, updatedCodexTimestamp.ClaudeLastUpdateUtc);
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
        var claudeTimestamp = new DateTimeOffset(2026, 3, 25, 10, 0, 0, TimeSpan.FromHours(2));
        var codexTimestamp = new DateTimeOffset(2026, 3, 24, 18, 30, 0, TimeSpan.FromHours(-5));
        var settings = new AgsSettings(true, false, claudeTimestamp, codexTimestamp);
        settings.WriteToConfig(configPath);
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var reloadedSettings));
        Assert.True(reloadedSettings.UseClaude);
        Assert.False(reloadedSettings.UseCodex);
        Assert.Equal(claudeTimestamp.ToUniversalTime(), reloadedSettings.ClaudeLastUpdateUtc);
        Assert.Equal(codexTimestamp.ToUniversalTime(), reloadedSettings.CodexLastUpdateUtc);
    }

}