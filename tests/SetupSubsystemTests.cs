using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers interactive setup persistence through the fallback console flow.
/// </summary>
public sealed class SetupSubsystemTests
{
    /// <summary>
    ///     Verifies that setup creates the configuration directory and writes the selected values.
    /// </summary>
    [Fact]
    public void RunWritesConfigForSelectedIntegrations()
    {
        using var tempDirectory = new TemporaryDirectoryScope();
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "1", "2", string.Empty));
        var agsDirectoryPath = Path.Combine(tempDirectory.Path, ".ags");
        SetupSubsystem.Run(agsDirectoryPath, out var settings);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        var configPath = Path.Combine(agsDirectoryPath, "config.json");
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var persistedSettings));
        Assert.True(persistedSettings.UseClaude);
        Assert.False(persistedSettings.UseCodex);
        Assert.Contains("Setup required. Starting setup...", console.Output);
        Assert.Contains("Configuration saved:", console.Output);
    }
}