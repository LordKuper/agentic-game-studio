using System.Diagnostics;
using System.Globalization;

namespace AGS.subsystems;

/// <summary>
///     Executes installation or update scripts for enabled integrations.
/// </summary>
internal static class InstallAISubsystem
{
    private const string ClaudeInstallerFileName = "install-claude-native.ps1";
    private const string CodexInstallerFileName = "install-codex.ps1";
    private const int InstallerSkippedExitCode = 10;
    private const string ScriptsDirectoryName = "scripts";
    private static readonly TimeSpan MinimumUpdateInterval = TimeSpan.FromDays(1);

    /// <summary>
    ///     Formats a UTC timestamp for console output.
    /// </summary>
    /// <param name="timestampUtc">UTC timestamp to format.</param>
    /// <returns>Formatted UTC timestamp string.</returns>
    private static string FormatUtcTimestamp(DateTimeOffset timestampUtc)
    {
        if (timestampUtc == DateTimeOffset.MinValue) return "not recorded";
        return timestampUtc.ToUniversalTime()
            .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Forwards installer process output to the application console.
    /// </summary>
    /// <param name="reader">Process output reader to consume.</param>
    /// <param name="integrationName">Display name of the integration being installed.</param>
    /// <param name="isErrorOutput">
    ///     <see langword="true" /> to write to standard error; otherwise, standard output.
    /// </param>
    private static void ForwardInstallerOutput(StreamReader reader, string integrationName,
        bool isErrorOutput)
    {
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;
            if (isErrorOutput)
                Console.Error.WriteLine($"[{integrationName}] {line}");
            else
                Console.WriteLine($"[{integrationName}] {line}");
        }
    }

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
        var updatedSettings = settings;
        var hasFailures = false;
        var hasSettingsChanges = false;
        if (settings.UseClaude)
        {
            var claudeResult = RunInstallerIfDue("Claude Code", ClaudeInstallerFileName,
                updatedSettings.ClaudeLastUpdateUtc, out var claudeUpdatedAtUtc);
            if (claudeResult == InstallerRunResult.Failed)
            {
                hasFailures = true;
            }
            else if (claudeResult == InstallerRunResult.Succeeded)
            {
                updatedSettings = updatedSettings.WithClaudeLastUpdateUtc(claudeUpdatedAtUtc);
                hasSettingsChanges = true;
            }
        }
        if (settings.UseCodex)
        {
            var codexResult = RunInstallerIfDue("Codex", CodexInstallerFileName,
                updatedSettings.CodexLastUpdateUtc, out var codexUpdatedAtUtc);
            if (codexResult == InstallerRunResult.Failed)
            {
                hasFailures = true;
            }
            else if (codexResult == InstallerRunResult.Succeeded)
            {
                updatedSettings = updatedSettings.WithCodexLastUpdateUtc(codexUpdatedAtUtc);
                hasSettingsChanges = true;
            }
        }
        AgsSettings.SetCurrent(updatedSettings);
        if (hasSettingsChanges && !TryPersistSettings(updatedSettings))
            hasFailures = true;
        if (hasFailures)
            Console.WriteLine("Integration update finished with errors.");
        else
            Console.WriteLine("Integration update completed successfully.");
    }

    /// <summary>
    ///     Runs an installer when no successful update has been recorded in the last 24 hours.
    /// </summary>
    /// <param name="integrationName">Display name of the integration being installed.</param>
    /// <param name="scriptFileName">File name of the PowerShell installer script.</param>
    /// <param name="lastUpdateUtc">
    ///     UTC timestamp of the last successful update, or <see cref="DateTimeOffset.MinValue" />
    ///     when the integration has not been updated yet.
    /// </param>
    /// <param name="updatedAtUtc">
    ///     UTC timestamp of the completed update when the installer succeeds; otherwise,
    ///     <see cref="DateTimeOffset.MinValue" />.
    /// </param>
    /// <returns>The installer outcome for the requested integration.</returns>
    private static InstallerRunResult RunInstallerIfDue(string integrationName,
        string scriptFileName, DateTimeOffset lastUpdateUtc, out DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = DateTimeOffset.MinValue;
        if (!ShouldRunInstaller(lastUpdateUtc, out var nextAllowedUpdateUtc))
        {
            Console.WriteLine(
                $"{integrationName} update skipped because the last successful update was recorded at {FormatUtcTimestamp(lastUpdateUtc)}. The next update is allowed after {FormatUtcTimestamp(nextAllowedUpdateUtc)}.");
            return InstallerRunResult.Skipped;
        }
        var runResult = RunInstallerScript(integrationName, scriptFileName);
        if (runResult != InstallerRunResult.Succeeded) return runResult;
        updatedAtUtc = DateTimeOffset.UtcNow;
        return InstallerRunResult.Succeeded;
    }

    /// <summary>
    ///     Runs a single PowerShell installer script from the application output directory.
    /// </summary>
    /// <param name="integrationName">Display name of the integration being installed.</param>
    /// <param name="scriptFileName">File name of the PowerShell installer script.</param>
    /// <returns>The installer outcome based on the process exit code.</returns>
    private static InstallerRunResult RunInstallerScript(string integrationName,
        string scriptFileName)
    {
        var scriptPath =
            Path.Combine(AppContext.BaseDirectory, ScriptsDirectoryName, scriptFileName);
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine(
                $"Installer script for {integrationName} was not found: {scriptPath}");
            return InstallerRunResult.Failed;
        }
        Console.WriteLine($"Running installer for {integrationName}...");
        try
        {
            var escapedScriptPath = scriptPath.Replace("'", "''");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command " + "\"try { & '" +
                            escapedScriptPath +
                            "' *>&1 } catch { Write-Error $_; exit 1 }; exit $LASTEXITCODE\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = new Process();
            process.StartInfo = processStartInfo;
            if (!process.Start())
            {
                Console.WriteLine($"Installer process for {integrationName} could not be started.");
                return InstallerRunResult.Failed;
            }
            var outputTask = Task.Run(() =>
                ForwardInstallerOutput(process.StandardOutput, integrationName, false));
            var errorTask = Task.Run(() =>
                ForwardInstallerOutput(process.StandardError, integrationName, true));
            process.WaitForExit();
            outputTask.Wait();
            errorTask.Wait();
            if (process.ExitCode == InstallerSkippedExitCode)
            {
                Console.WriteLine($"{integrationName} installer was skipped.");
                return InstallerRunResult.Skipped;
            }
            if (process.ExitCode == 0)
            {
                Console.WriteLine($"{integrationName} installer completed successfully.");
                return InstallerRunResult.Succeeded;
            }
            Console.WriteLine(
                $"{integrationName} installer failed with exit code {process.ExitCode}.");
            return InstallerRunResult.Failed;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"{integrationName} installer failed with an exception: {exception.Message}");
            return InstallerRunResult.Failed;
        }
    }

    /// <summary>
    ///     Determines whether an installer is allowed to run based on the minimum update interval.
    /// </summary>
    /// <param name="lastUpdateUtc">
    ///     UTC timestamp of the last successful update, or <see cref="DateTimeOffset.MinValue" />
    ///     when the integration has not been updated yet.
    /// </param>
    /// <param name="nextAllowedUpdateUtc">
    ///     UTC timestamp when a new update is allowed, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no previous update exists.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the installer may run; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool ShouldRunInstaller(DateTimeOffset lastUpdateUtc,
        out DateTimeOffset nextAllowedUpdateUtc)
    {
        nextAllowedUpdateUtc = DateTimeOffset.MinValue;
        if (lastUpdateUtc == DateTimeOffset.MinValue) return true;
        nextAllowedUpdateUtc = lastUpdateUtc.Add(MinimumUpdateInterval);
        return DateTimeOffset.UtcNow >= nextAllowedUpdateUtc;
    }

    /// <summary>
    ///     Persists updated application settings to the project configuration file.
    /// </summary>
    /// <param name="settings">Settings instance to persist.</param>
    /// <returns>
    ///     <see langword="true" /> when the settings are written successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryPersistSettings(AgsSettings settings)
    {
        var configPath = AgsSettings.GetConfigPath(Directory.GetCurrentDirectory());
        try
        {
            var configDirectoryPath = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(configDirectoryPath))
                Directory.CreateDirectory(configDirectoryPath);
            settings.WriteToConfig(configPath);
            return true;
        }
        catch (IOException exception)
        {
            Console.WriteLine(
                $"Updated settings could not be saved to {configPath}: {exception.Message}");
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.WriteLine(
                $"Updated settings could not be saved to {configPath}: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Represents the outcome of a single installer attempt.
    /// </summary>
    private enum InstallerRunResult
    {
        /// <summary>
        ///     The installer completed successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        ///     The installer did not run or intentionally skipped work.
        /// </summary>
        Skipped,

        /// <summary>
        ///     The installer failed.
        /// </summary>
        Failed
    }
}