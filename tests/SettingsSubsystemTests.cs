using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers fallback settings editing and non-public helpers.
/// </summary>
public sealed class SettingsSubsystemTests
{
    /// <summary>
    ///     Verifies that clearing the console does not throw when output is redirected.
    /// </summary>
    [Fact]
    public void ClearConsoleDoesNotThrowWhenOutputIsRedirected()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var exception = Record.Exception(() =>
            PrivateAccess.InvokeStatic(typeof(SettingsSubsystem), "ClearConsole"));
        Assert.Null(exception);
    }

    /// <summary>
    ///     Verifies that Boolean values are formatted consistently for display.
    /// </summary>
    [Fact]
    public void FormatBooleanValueReturnsYesOrNo()
    {
        var yesValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "FormatBooleanValue", true);
        var noValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "FormatBooleanValue", false);
        Assert.Equal("yes", yesValue);
        Assert.Equal("no", noValue);
    }

    /// <summary>
    ///     Verifies that persisting settings updates the process-wide state and writes configuration.
    /// </summary>
    [Fact]
    public void PersistSettingsWritesConfigAndUpdatesCurrentState()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "PersistSettings", new AgsSettings(true, true));
        Assert.Equal(string.Empty, errorMessage);
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.True(File.Exists(AgsSettings.GetConfigPath(tempDirectory.Path)));
    }

    /// <summary>
    ///     Verifies that interactive rendering prints the title, status message, and option rows.
    /// </summary>
    [Fact]
    public void RenderInteractiveWritesCurrentSettingsState()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        PrivateAccess.InvokeStatic(typeof(SettingsSubsystem), "RenderInteractive",
            new AgsSettings(true, false), 1, "Saved");
        Assert.Contains("Settings", console.Output);
        Assert.Contains("Saved", console.Output);
        Assert.Contains("use-codex: no", console.Output);
        Assert.Contains("> use-claude: yes", console.Output);
    }

    /// <summary>
    ///     Verifies that fallback mode reports unsupported commands.
    /// </summary>
    [Fact]
    public void RunFallbackPrintsValidationMessageForUnknownCommand()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "unknown", "0", string.Empty));
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        SettingsSubsystem.Run();
        Assert.Contains("Please enter use-codex, use-claude, or 0.", console.Output);
    }

    /// <summary>
    ///     Verifies that fallback commands toggle settings and persist the updated configuration.
    /// </summary>
    [Fact]
    public void RunFallbackTogglesSettingsAndPersistsConfig()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "use-codex", "use-claude", "0", string.Empty));
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        SettingsSubsystem.Run();
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.True(AgsSettings.Current.UseClaude);
        var configPath = AgsSettings.GetConfigPath(tempDirectory.Path);
        Assert.True(File.Exists(configPath));
        Assert.True(AgsSettings.TryReadFromConfig(configPath, out var persistedSettings));
        Assert.True(persistedSettings.UseCodex);
        Assert.True(persistedSettings.UseClaude);
        Assert.Contains("Settings", console.Output);
    }

    /// <summary>
    ///     Verifies that selecting the return row exits the interactive loop immediately.
    /// </summary>
    [Fact]
    public void RunInteractiveReturnsWhenReturnRowIsConfirmed()
    {
        var originalReadKeyProvider = PrivateAccess.GetStaticField<Func<ConsoleKey>>(
            typeof(SettingsSubsystem), "readKeyProvider");
        var pressedKeys = new Queue<ConsoleKey>(new[]
        {
            ConsoleKey.UpArrow,
            ConsoleKey.Enter
        });
        try
        {
            using var console = new ConsoleRedirectionScope(string.Empty);
            AgsSettings.SetCurrent(new AgsSettings(false, false));
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "readKeyProvider",
                (Func<ConsoleKey>)(() => pressedKeys.Dequeue()));
            PrivateAccess.InvokeStatic(typeof(SettingsSubsystem), "RunInteractive");
            Assert.Contains("Settings", console.Output);
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "readKeyProvider",
                originalReadKeyProvider);
        }
    }

    /// <summary>
    ///     Verifies that the settings screen does nothing when no current settings exist.
    /// </summary>
    [Fact]
    public void RunReturnsImmediatelyWhenCurrentSettingsAreMissing()
    {
        AgsTestState.ResetCurrentSettings();
        using var console = new ConsoleRedirectionScope(string.Empty);
        SettingsSubsystem.Run();
        Assert.Equal(string.Empty, console.Output);
    }

    /// <summary>
    ///     Verifies that the interactive settings loop processes navigation, value changes, and exit keys.
    /// </summary>
    [Fact]
    public void RunUsesInteractiveLoopWhenConsoleIsAvailable()
    {
        var originalInputProvider =
            PrivateAccess.GetStaticField<Func<bool>>(typeof(SettingsSubsystem),
                "isInputRedirectedProvider");
        var originalOutputProvider =
            PrivateAccess.GetStaticField<Func<bool>>(typeof(SettingsSubsystem),
                "isOutputRedirectedProvider");
        var originalReadKeyProvider = PrivateAccess.GetStaticField<Func<ConsoleKey>>(
            typeof(SettingsSubsystem), "readKeyProvider");
        var pressedKeys = new Queue<ConsoleKey>(new[]
        {
            ConsoleKey.DownArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.UpArrow,
            ConsoleKey.Enter,
            ConsoleKey.D0
        });
        try
        {
            AgsTestState.ResetCurrentSettings();
            using var tempDirectory = new TemporaryDirectoryScope();
            using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
            using var console = new ConsoleRedirectionScope(string.Empty);
            AgsSettings.SetCurrent(new AgsSettings(false, false));
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "isInputRedirectedProvider",
                (Func<bool>)(() => false));
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "isOutputRedirectedProvider",
                (Func<bool>)(() => false));
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "readKeyProvider",
                (Func<ConsoleKey>)(() => pressedKeys.Dequeue()));
            SettingsSubsystem.Run();
            Assert.True(AgsSettings.Current.UseCodex);
            Assert.True(AgsSettings.Current.UseClaude);
            Assert.Contains("Settings", console.Output);
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "isInputRedirectedProvider",
                originalInputProvider);
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "isOutputRedirectedProvider",
                originalOutputProvider);
            PrivateAccess.SetStaticField(typeof(SettingsSubsystem), "readKeyProvider",
                originalReadKeyProvider);
        }
    }

    /// <summary>
    ///     Verifies that changing a selected setting persists the new value and updates current state.
    /// </summary>
    [Fact]
    public void SetSettingValuePersistsChangedValue()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 0, true);
        Assert.Equal(string.Empty, errorMessage);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.True(AgsSettings.TryReadFromConfig(AgsSettings.GetConfigPath(tempDirectory.Path),
            out var persistedSettings));
        Assert.True(persistedSettings.UseCodex);
    }

    /// <summary>
    ///     Verifies that setting helpers return without changes for unsupported or redundant input.
    /// </summary>
    [Fact]
    public void SetSettingValueReturnsEmptyWhenNoStateChangeIsNeeded()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(true, false));
        var unchangedValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 1, true);
        var unsupportedValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 2, true);
        Assert.Equal(string.Empty, unchangedValue);
        Assert.Equal(string.Empty, unsupportedValue);
    }

    /// <summary>
    ///     Verifies that toggling settings updates the requested flag and persists the change.
    /// </summary>
    [Fact]
    public void ToggleSettingUpdatesRequestedFlags()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        var codexResult = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "ToggleSetting", 0);
        var claudeResult = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "ToggleSetting", 1);
        var unsupportedResult = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "ToggleSetting", 2);
        Assert.Equal(string.Empty, codexResult);
        Assert.Equal(string.Empty, claudeResult);
        Assert.Equal(string.Empty, unsupportedResult);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.True(AgsSettings.Current.UseClaude);
    }

    /// <summary>
    ///     Verifies that option rows render the expected selection prefix.
    /// </summary>
    [Fact]
    public void WriteOptionLineWritesSelectionPrefix()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        PrivateAccess.InvokeStatic(typeof(SettingsSubsystem), "WriteOptionLine", true, "selected");
        PrivateAccess.InvokeStatic(typeof(SettingsSubsystem), "WriteOptionLine", false, "plain");
        Assert.Contains("> selected", console.Output);
        Assert.Contains("  plain", console.Output);
    }
}