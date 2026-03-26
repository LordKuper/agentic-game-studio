using AGS.subsystems;

namespace AGS;

/// <summary>
///     Provides the application entry point and startup validation workflow.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Starts the application, loads settings, runs setup when needed, and updates enabled
    ///     integrations before showing the main menu.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        foreach (var argument in args)
        {
            if (string.Equals(argument, "-version", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(argument, "-v", StringComparison.OrdinalIgnoreCase))
            {
                WriteApplicationVersion();
                return;
            }
        }

        if (!TryInitializeApplication(out var settings)) return;

        AgsSettings.SetCurrent(settings);
        InstallAISubsystem.Run();
        MainMenuSubsystem.Run();
    }

    /// <summary>
    ///     Loads existing settings or runs setup when initialization is required.
    /// </summary>
    /// <param name="settings">
    ///     Loaded or newly created application settings when initialization succeeds.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the application can continue to dependency
    ///     initialization; otherwise, <see langword="false" />.
    /// </returns>
    private static bool TryInitializeApplication(out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        var currentDirectory = Directory.GetCurrentDirectory();
        var agsDirectoryPath = Path.Combine(currentDirectory, AgsSettings.AgsDirectoryName);
        if (Directory.Exists(agsDirectoryPath))
            return TryInitializeFromExistingConfiguration(agsDirectoryPath, out settings);

        if (!ValidateProjectRoot(currentDirectory)) return false;

        SetupSubsystem.Run(agsDirectoryPath, out settings);
        return true;
    }

    /// <summary>
    ///     Loads an existing configuration directory or reruns setup when its contents are not
    ///     usable.
    /// </summary>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    /// <param name="settings">
    ///     Loaded or newly created application settings when initialization succeeds.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when initialization succeeds; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryInitializeFromExistingConfiguration(string agsDirectoryPath,
        out AgsSettings settings)
    {
        var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
        var canReadSettings = AgsSettings.TryReadFromConfig(configPath, out settings);
        if (canReadSettings)
        {
            Console.WriteLine("AGS configuration found and loaded. Initialization is not required.");
            return true;
        }

        Console.WriteLine(
            "Existing settings are missing or invalid. Starting setup...");
        SetupSubsystem.Run(agsDirectoryPath, out settings);
        return true;
    }

    /// <summary>
    ///     Verifies that the current directory is the project root before setup starts.
    /// </summary>
    /// <param name="currentDirectory">Current working directory where the application is started.</param>
    /// <returns>
    ///     <see langword="true" /> when the user confirms the directory is the project root;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    private static bool ValidateProjectRoot(string currentDirectory)
    {
        var isProjectRoot =
            ConsoleMenu.PromptForBoolean($"Is the current folder the project root? ({currentDirectory})");
        if (isProjectRoot) return true;
        Console.WriteLine("The application must be started from the project root folder. Exiting.");
        return false;
    }

    /// <summary>
    ///     Writes the current application version to standard output.
    /// </summary>
    private static void WriteApplicationVersion()
    {
        var assemblyVersion = typeof(AgsSettings).Assembly.GetName().Version;
        var applicationVersion = assemblyVersion == null ? "0.0.0.0" : assemblyVersion.ToString();
        Console.WriteLine(applicationVersion);
    }
}
