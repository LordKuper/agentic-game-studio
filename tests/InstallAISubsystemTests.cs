using System.Text;
using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers installer scheduling, script execution, and settings persistence flows.
/// </summary>
public sealed class InstallAISubsystemTests
{
    /// <summary>
    ///     Verifies that UTC timestamps are formatted for display and that missing timestamps are handled.
    /// </summary>
    [Fact]
    public void FormatUtcTimestampFormatsKnownAndMissingValues()
    {
        var missingValue = (string)PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "FormatUtcTimestamp", DateTimeOffset.MinValue);
        var knownValue = (string)PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "FormatUtcTimestamp",
            new DateTimeOffset(2026, 3, 27, 10, 15, 0, TimeSpan.FromHours(3)));
        Assert.Equal("not recorded", missingValue);
        Assert.Equal("2026-03-27 07:15:00 UTC", knownValue);
    }

    /// <summary>
    ///     Verifies that installer script output is prefixed and forwarded to standard error.
    /// </summary>
    [Fact]
    public void ForwardInstallerOutputWritesPrefixedStandardError()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("error line" +
            Environment.NewLine));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        PrivateAccess.InvokeStatic(typeof(InstallAISubsystem), "ForwardInstallerOutput", reader,
            "Claude Code", true);
        Assert.Contains("[Claude Code] error line", console.Error);
    }

    /// <summary>
    ///     Verifies that installer script output is prefixed and forwarded to standard output.
    /// </summary>
    [Fact]
    public void ForwardInstallerOutputWritesPrefixedStandardOutput()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("line one" +
            Environment.NewLine + Environment.NewLine + "line two" + Environment.NewLine));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        PrivateAccess.InvokeStatic(typeof(InstallAISubsystem), "ForwardInstallerOutput", reader,
            "Codex", false);
        Assert.Contains("[Codex] line one", console.Output);
        Assert.Contains("[Codex] line two", console.Output);
    }

    /// <summary>
    ///     Verifies that due installers run and report an updated timestamp on success.
    /// </summary>
    [Fact]
    public void RunInstallerIfDueReturnsTimestampWhenInstallerSucceeds()
    {
        using var scripts = new InstallerScriptsScope(
            new InstallerScriptDefinition("install-success.ps1", "Write-Output 'success'; exit 0"));
        using var console = new ConsoleRedirectionScope(string.Empty);
        var arguments = new object[]
        {
            "Codex",
            "install-success.ps1",
            DateTimeOffset.MinValue,
            DateTimeOffset.MinValue
        };
        var result = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem), "RunInstallerIfDue",
            arguments);
        var updatedAtUtc = (DateTimeOffset)arguments[3];
        Assert.Equal("Succeeded", result.ToString());
        Assert.NotEqual(DateTimeOffset.MinValue, updatedAtUtc);
        Assert.Contains("Running installer for Codex...", console.Output);
    }

    /// <summary>
    ///     Verifies that a recent successful update prevents the installer from running again.
    /// </summary>
    [Fact]
    public void RunInstallerIfDueSkipsWhenUpdateIsNotDue()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var arguments = new object[]
        {
            "Codex",
            "install-codex.ps1",
            DateTimeOffset.UtcNow.AddHours(-2),
            DateTimeOffset.MinValue
        };
        var result = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem), "RunInstallerIfDue",
            arguments);
        Assert.Equal("Skipped", result.ToString());
        Assert.Equal(DateTimeOffset.MinValue, (DateTimeOffset)arguments[3]);
        Assert.Contains("Codex update skipped because the last successful update was recorded at",
            console.Output);
    }

    /// <summary>
    ///     Verifies that installer scripts report success, skip, and failure according to exit codes.
    /// </summary>
    [Fact]
    public void RunInstallerScriptMapsProcessExitCodesToResults()
    {
        using var successScripts = new InstallerScriptsScope(
            new InstallerScriptDefinition("install-success.ps1", "Write-Output 'success'; exit 0"),
            new InstallerScriptDefinition("install-skip.ps1", "Write-Output 'skip'; exit 10"),
            new InstallerScriptDefinition("install-fail.ps1", "Write-Output 'fail'; exit 1"));
        using var console = new ConsoleRedirectionScope(string.Empty);
        var successResult = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "RunInstallerScript", "Codex", "install-success.ps1");
        var skippedResult = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "RunInstallerScript", "Codex", "install-skip.ps1");
        var failedResult = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "RunInstallerScript", "Codex", "install-fail.ps1");
        Assert.Equal("Succeeded", successResult.ToString());
        Assert.Equal("Skipped", skippedResult.ToString());
        Assert.Equal("Failed", failedResult.ToString());
        Assert.Contains("Codex installer completed successfully.", console.Output);
        Assert.Contains("Codex installer was skipped.", console.Output);
        Assert.Contains("Codex installer failed with exit code 1.", console.Output);
    }

    /// <summary>
    ///     Verifies that missing installer scripts are reported as failures.
    /// </summary>
    [Fact]
    public void RunInstallerScriptReturnsFailedWhenScriptIsMissing()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var result = PrivateAccess.InvokeStatic(typeof(InstallAISubsystem), "RunInstallerScript",
            "Claude Code", "missing-installer.ps1");
        Assert.Equal("Failed", result.ToString());
        Assert.Contains("Installer script for Claude Code was not found", console.Output);
    }

    /// <summary>
    ///     Verifies that enabled integrations update timestamps and persist them to the project configuration.
    /// </summary>
    [Fact]
    public void RunPersistsUpdatedSettingsForEnabledIntegrations()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var scripts = new InstallerScriptsScope(
            new InstallerScriptDefinition("install-claude.ps1", "Write-Output 'claude ok'; exit 0"),
            new InstallerScriptDefinition("install-codex.ps1", "Write-Output 'codex ok'; exit 0"));
        using var console = new ConsoleRedirectionScope(string.Empty);
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        InstallAISubsystem.Run();
        Assert.True(AgsSettings.Current.HasClaudeLastUpdateUtc);
        Assert.True(AgsSettings.Current.HasCodexLastUpdateUtc);
        var configPath = AgsSettings.GetConfigPath(tempDirectory.Path);
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var persistedSettings));
        Assert.True(persistedSettings.HasClaudeLastUpdateUtc);
        Assert.True(persistedSettings.HasCodexLastUpdateUtc);
        Assert.Contains("Integration update completed successfully.", console.Output);
    }

    /// <summary>
    ///     Verifies that installer failures are reported and do not produce update timestamps.
    /// </summary>
    [Fact]
    public void RunReportsErrorsWhenInstallerFails()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var scripts = new InstallerScriptsScope(
            new InstallerScriptDefinition("install-claude.ps1",
                "Write-Output 'claude failed'; exit 1"));
        using var console = new ConsoleRedirectionScope(string.Empty);
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        InstallAISubsystem.Run();
        Assert.False(AgsSettings.Current.HasClaudeLastUpdateUtc);
        Assert.Contains("Integration update finished with errors.", console.Output);
    }

    /// <summary>
    ///     Verifies that recent update timestamps skip installer execution during the full run flow.
    /// </summary>
    [Fact]
    public void RunSkipsInstallersThatWereUpdatedRecently()
    {
        AgsTestState.ResetCurrentSettings();
        var recentUpdateTimestamp = DateTimeOffset.UtcNow.AddHours(-1);
        using var console = new ConsoleRedirectionScope(string.Empty);
        AgsSettings.SetCurrent(new AgsSettings(true, false, recentUpdateTimestamp,
            DateTimeOffset.MinValue));
        InstallAISubsystem.Run();
        Assert.Equal(recentUpdateTimestamp.ToUniversalTime(),
            AgsSettings.Current.ClaudeLastUpdateUtc);
        Assert.Contains(
            "Claude Code update skipped because the last successful update was recorded at",
            console.Output);
    }

    /// <summary>
    ///     Verifies that updates are skipped when all integrations are disabled.
    /// </summary>
    [Fact]
    public void RunSkipsWhenAllIntegrationsAreDisabled()
    {
        AgsTestState.ResetCurrentSettings();
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        using var console = new ConsoleRedirectionScope(string.Empty);
        InstallAISubsystem.Run();
        Assert.Contains("No enabled integrations were selected. Update is skipped.",
            console.Output);
    }

    /// <summary>
    ///     Verifies that updates are skipped when current settings are unavailable.
    /// </summary>
    [Fact]
    public void RunSkipsWhenSettingsAreUnavailable()
    {
        AgsTestState.ResetCurrentSettings();
        using var console = new ConsoleRedirectionScope(string.Empty);
        InstallAISubsystem.Run();
        Assert.Contains("Application settings are not available. Update is skipped.",
            console.Output);
    }

    /// <summary>
    ///     Verifies the scheduler behavior for never-run, recently-run, and overdue installers.
    /// </summary>
    [Fact]
    public void ShouldRunInstallerEvaluatesUpdateIntervals()
    {
        var firstRunArguments = new object[] { DateTimeOffset.MinValue, DateTimeOffset.MinValue };
        var canRunFirstTime = (bool)PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "ShouldRunInstaller", firstRunArguments);
        var recentTimestamp = DateTimeOffset.UtcNow.AddHours(-1);
        var recentArguments = new object[] { recentTimestamp, DateTimeOffset.MinValue };
        var canRunRecent = (bool)PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "ShouldRunInstaller", recentArguments);
        var oldTimestamp = DateTimeOffset.UtcNow.AddDays(-2);
        var oldArguments = new object[] { oldTimestamp, DateTimeOffset.MinValue };
        var canRunOld = (bool)PrivateAccess.InvokeStatic(typeof(InstallAISubsystem),
            "ShouldRunInstaller", oldArguments);
        Assert.True(canRunFirstTime);
        Assert.Equal(DateTimeOffset.MinValue, (DateTimeOffset)firstRunArguments[1]);
        Assert.False(canRunRecent);
        Assert.Equal(recentTimestamp.AddDays(1), (DateTimeOffset)recentArguments[1]);
        Assert.True(canRunOld);
        Assert.Equal(oldTimestamp.AddDays(1), (DateTimeOffset)oldArguments[1]);
    }
}