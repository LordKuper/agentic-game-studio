using System.Reflection;

namespace AGS.Tests;

/// <summary>
///     Covers startup, setup, configuration loading, and version output flows.
/// </summary>
public sealed class ProgramTests
{
    /// <summary>
    ///     Invokes the private application entry point through reflection.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void InvokeProgramMain(string[] args)
    {
        var mainMethod = typeof(Program).GetMethod("Main",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mainMethod);
        mainMethod.Invoke(null, [args]);
    }

    /// <summary>
    ///     Verifies that a valid existing configuration is loaded without rerunning setup.
    /// </summary>
    [Fact]
    public void MainLoadsExistingConfiguration()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope("3" + Environment.NewLine);
        var settings = new AgsSettings(false, false);
        settings.TryWriteToProjectConfig(tempDirectory.Path, out _);
        InvokeProgramMain(Array.Empty<string>());
        Assert.Contains("AGS configuration found and loaded. Initialization is not required.",
            console.Output);
        Assert.Contains("No enabled integrations were selected. Update is skipped.",
            console.Output);
    }

    /// <summary>
    ///     Verifies that an invalid existing configuration causes setup to run again.
    /// </summary>
    [Fact]
    public void MainRerunsSetupWhenExistingConfigurationIsInvalid()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "2", "2", "3", string.Empty));
        var agsDirectoryPath = Path.Combine(tempDirectory.Path, ".ags");
        Directory.CreateDirectory(agsDirectoryPath);
        File.WriteAllText(Path.Combine(agsDirectoryPath, "config.json"), "invalid configuration");
        InvokeProgramMain(Array.Empty<string>());
        Assert.Contains("Existing settings are missing or invalid. Starting setup...",
            console.Output);
        Assert.True(AgsSettings.TryReadFromConfig(Path.Combine(agsDirectoryPath, "config.json"),
            out var settings));
        Assert.False(settings.UseClaude);
        Assert.False(settings.UseCodex);
    }

    /// <summary>
    ///     Verifies that missing configuration triggers setup, installer checks, and the main menu.
    /// </summary>
    [Fact]
    public void MainRunsSetupWhenConfigurationIsMissing()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "1", "2", "2", "3", string.Empty));
        InvokeProgramMain(Array.Empty<string>());
        var configPath = AgsSettings.GetConfigPath(tempDirectory.Path);
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var persistedSettings));
        Assert.False(persistedSettings.UseClaude);
        Assert.False(persistedSettings.UseCodex);
        Assert.Contains("Setup required. Starting setup...", console.Output);
        Assert.Contains("No enabled integrations were selected. Update is skipped.",
            console.Output);
        Assert.Contains("Application is shutting down.", console.Output);
    }

    /// <summary>
    ///     Verifies that initialization stops when the current folder is not confirmed as the project root.
    /// </summary>
    [Fact]
    public void MainStopsWhenProjectRootValidationFails()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope("2" + Environment.NewLine);
        InvokeProgramMain(Array.Empty<string>());
        Assert.Contains("The application must be started from the project root folder. Exiting.",
            console.Output);
    }

    /// <summary>
    ///     Verifies that the version flag writes the assembly version and exits immediately.
    /// </summary>
    [Fact]
    public void MainWritesVersionForVersionArgument()
    {
        AgsTestState.ResetCurrentSettings();
        using var console = new ConsoleRedirectionScope(string.Empty);
        var expectedVersion = typeof(AgsSettings).Assembly.GetName().Version.ToString();
        InvokeProgramMain(["-version"]);
        Assert.Contains(expectedVersion, console.Output);
    }
}