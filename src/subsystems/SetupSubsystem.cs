namespace AGS.subsystems;

/// <summary>
///     Handles interactive initialization for the local <c>.ags</c> configuration.
/// </summary>
internal static class SetupSubsystem
{
    /// <summary>
    ///     Runs interactive setup and writes the generated configuration file.
    /// </summary>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    /// <param name="settings">
    ///     Settings selected during setup when setup succeeds; otherwise disabled defaults.
    /// </param>
    internal static void Run(string agsDirectoryPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        Console.WriteLine("Setup required. Starting setup...");
        settings = new AgsSettings(AgsPrompt.Confirm("Do you want to use Claude Code?", false),
            AgsPrompt.Confirm("Do you want to use Codex?", false));
        Directory.CreateDirectory(agsDirectoryPath);
        var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
        settings.WriteToConfig(configPath);
        Console.WriteLine($"Configuration saved: {configPath}");
    }
}
