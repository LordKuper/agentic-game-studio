namespace AGS.subsystems;

/// <summary>
///     Handles interactive initialization for the local <c>.ags</c> configuration.
/// </summary>
internal static class SetupSubsystem
{
    /// <summary>
    ///     Prompts the user for a yes/no answer until valid input is provided.
    /// </summary>
    /// <param name="question">Question text shown to the user.</param>
    /// <returns><see langword="true" /> for yes answers; otherwise, <see langword="false" />.</returns>
    private static bool AskYesNo(string question)
    {
        while (true)
        {
            Console.Write($"{question} [y/n]: ");
            var rawAnswer = Console.ReadLine();
            var answer = rawAnswer == null ? string.Empty : rawAnswer.Trim().ToLowerInvariant();
            if (answer is "y" or "yes") return true;
            if (answer is "n" or "no") return false;
            Console.WriteLine("Please answer with y/yes or n/no.");
        }
    }

    /// <summary>
    ///     Runs interactive setup, validates that the current directory is the project root,
    ///     and writes the generated configuration file.
    /// </summary>
    /// <param name="currentDirectory">Current working directory where the application is started.</param>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    internal static void Run(string currentDirectory, string agsDirectoryPath)
    {
        Console.WriteLine("Setup required. Starting setup...");
        var isProjectRoot =
            AskYesNo($"Is the current folder the project root? ({currentDirectory})");
        if (!isProjectRoot)
        {
            Console.WriteLine(
                "The application must be started from the project root folder. Exiting.");
            return;
        }
        var settings = new AgsSettings(AskYesNo("Do you want to use Claude Code?"),
            AskYesNo("Do you want to use Codex?"));
        Directory.CreateDirectory(agsDirectoryPath);
        var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
        settings.WriteToConfig(configPath);
        Console.WriteLine($"Configuration saved: {configPath}");
    }
}
