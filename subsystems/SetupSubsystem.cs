using System.Text;

/// <summary>
/// Handles interactive initialization for the local <c>.ags</c> configuration.
/// </summary>
internal static class SetupSubsystem
{
    private const string ConfigFileName = "config";

    /// <summary>
    /// Runs interactive setup, validates that the current directory is the project root,
    /// and writes the generated configuration file.
    /// </summary>
    /// <param name="currentDirectory">Current working directory where the application is started.</param>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    internal static void Run(string currentDirectory, string agsDirectoryPath)
    {
        Console.WriteLine(".ags directory not found. Starting setup...");

        var isProjectRoot = AskYesNo($"Is the current folder the project root? ({currentDirectory})");
        if (!isProjectRoot)
        {
            Console.WriteLine("The application must be started from the project root folder. Exiting.");
            return;
        }

        var settings = new AgsSettings(
            useClaude: AskYesNo("Do you want to use Claude Code?"),
            useCodex: AskYesNo("Do you want to use Codex?"));

        Directory.CreateDirectory(agsDirectoryPath);

        var configPath = Path.Combine(agsDirectoryPath, ConfigFileName);
        File.WriteAllText(configPath, BuildConfigContent(settings));

        Console.WriteLine($"Configuration saved: {configPath}");
    }

    /// <summary>
    /// Builds textual configuration content from provided setup settings.
    /// </summary>
    /// <param name="settings">Settings selected during setup.</param>
    /// <returns>Configuration text for the <c>.ags/config</c> file.</returns>
    private static string BuildConfigContent(AgsSettings settings)
    {
        return new StringBuilder()
            .AppendLine($"use-claude={settings.UseClaude.ToString().ToLowerInvariant()}")
            .AppendLine($"use-codex={settings.UseCodex.ToString().ToLowerInvariant()}")
            .ToString();
    }

    /// <summary>
    /// Prompts the user for a yes/no answer until valid input is provided.
    /// </summary>
    /// <param name="question">Question text shown to the user.</param>
    /// <returns><see langword="true"/> for yes answers; otherwise, <see langword="false"/>.</returns>
    private static bool AskYesNo(string question)
    {
        while (true)
        {
            Console.Write($"{question} [y/n]: ");
            var rawAnswer = Console.ReadLine();
            var answer = rawAnswer == null ? string.Empty : rawAnswer.Trim().ToLowerInvariant();

            if (answer is "y" or "yes")
            {
                return true;
            }

            if (answer is "n" or "no")
            {
                return false;
            }

            Console.WriteLine("Please answer with y/yes or n/no.");
        }
    }
}
