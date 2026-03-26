using System.Diagnostics;

namespace AGS.subsystems;

/// <summary>
///     Executes installation or update scripts for enabled integrations.
/// </summary>
internal static class InstallAISubsystem
{
    private const string ClaudeInstallerFileName = "install-claude.ps1";
    private const string CodexInstallerFileName = "install-codex.ps1";
    private const string ScriptsDirectoryName = "scripts";

    /// <summary>
    ///     Runs installer scripts for all integrations enabled in the current application
    ///     settings.
    /// </summary>
    internal static void Run()
    {
        if (!AgsSettings.HasCurrentSettings)
        {
            Console.WriteLine("Application settings are not available. Update is skipped.");
            return;
        }
        var settings = AgsSettings.Current;
        if (settings.AreAllModelsDisabled)
        {
            Console.WriteLine("No enabled integrations were selected. Update is skipped.");
            return;
        }
        Console.WriteLine("Updating enabled integrations...");
        var hasFailures = false;
        if (settings.UseClaude && !RunInstallerScript("Claude Code", ClaudeInstallerFileName))
            hasFailures = true;
        if (settings.UseCodex && !RunInstallerScript("Codex", CodexInstallerFileName))
            hasFailures = true;
        if (hasFailures)
            Console.WriteLine("Integration update finished with errors.");
        else
            Console.WriteLine("Integration update completed successfully.");
    }

    /// <summary>
    ///     Runs a single PowerShell installer script from the application output directory.
    /// </summary>
    /// <param name="integrationName">Display name of the integration being installed.</param>
    /// <param name="scriptFileName">File name of the PowerShell installer script.</param>
    /// <returns>
    ///     <see langword="true" /> when the script finishes with exit code <c>0</c>; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool RunInstallerScript(string integrationName, string scriptFileName)
    {
        var scriptPath =
            Path.Combine(AppContext.BaseDirectory, ScriptsDirectoryName, scriptFileName);
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine(
                $"Installer script for {integrationName} was not found: {scriptPath}");
            return false;
        }
        Console.WriteLine($"Running installer for {integrationName}...");
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false
            };
            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine($"Installer process for {integrationName} could not be started.");
                return false;
            }
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                Console.WriteLine($"{integrationName} installer completed successfully.");
                return true;
            }
            Console.WriteLine(
                $"{integrationName} installer failed with exit code {process.ExitCode}.");
            return false;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"{integrationName} installer failed with an exception: {exception.Message}");
            return false;
        }
    }
}
