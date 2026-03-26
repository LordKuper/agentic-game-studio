using AGS.subsystems;

namespace AGS;

/// <summary>
///     Provides the application entry point and startup validation workflow.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Starts the application, loads settings, runs setup when needed, and updates enabled
    ///     integrations.
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
        AgsSettings settings;
        var currentDirectory = Directory.GetCurrentDirectory();
        var agsDirectoryPath = Path.Combine(currentDirectory, AgsSettings.AgsDirectoryName);
        if (Directory.Exists(agsDirectoryPath))
        {
            var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
            var canReadSettings = AgsSettings.TryReadFromConfig(configPath, out settings);
            if (!canReadSettings || settings.AreAllModelsDisabled)
            {
                Console.WriteLine(
                    "Existing settings are missing/invalid or all AI integrations are disabled. Starting setup...");
                SetupSubsystem.Run(agsDirectoryPath, out settings);
                AgsSettings.SetCurrent(settings);
                return;
            }
            Console.WriteLine(
                "AGS configuration found and loaded. Initialization is not required.");
        }
        else
        {
            if (!ValidateProjectRoot(currentDirectory)) return;
            SetupSubsystem.Run(agsDirectoryPath, out settings);
        }
        AgsSettings.SetCurrent(settings);
        InstallAISubsystem.Run();
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
