using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers interactive setup persistence through the Sharprompt wrapper flow.
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
        using var prompts = new PromptStubScope(confirmations: [true, false]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        var agsDirectoryPath = Path.Combine(tempDirectory.Path, ".ags");
        SetupSubsystem.Run(agsDirectoryPath, out var settings);
        Assert.True(settings.UseClaude);
        Assert.False(settings.UseCodex);
        var configPath = Path.Combine(agsDirectoryPath, "config.json");
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var persistedSettings));
        Assert.True(persistedSettings.UseClaude);
        Assert.False(persistedSettings.UseCodex);
        Assert.Equal(["Do you want to use Claude Code?", "Do you want to use Codex?"],
            prompts.ConfirmMessages);
        Assert.Contains("Setup required. Starting setup...", console.Output);
        Assert.Contains("Configuration saved:", console.Output);
    }
}
