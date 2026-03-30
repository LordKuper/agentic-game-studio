using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers settings prompt flow and helper methods.
/// </summary>
public sealed class SettingsSubsystemTests
{
    /// <summary>
    ///     Verifies that settings option labels reflect the current persisted values.
    /// </summary>
    [Fact]
    public void BuildOptionLabelsReflectCurrentSettings()
    {
        var labels = (string[])PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "BuildOptionLabels", new AgsSettings(true, false));
        Assert.Equal(["use-codex: no", "use-claude: yes", "Return to main menu"], labels);
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
    ///     Verifies that setting display names match the editable rows.
    /// </summary>
    [Fact]
    public void GetSettingDisplayNameReturnsExpectedNames()
    {
        var codexName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 0);
        var claudeName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 1);
        var unsupportedName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 2);
        Assert.Equal("use-codex", codexName);
        Assert.Equal("use-claude", claudeName);
        Assert.Equal(string.Empty, unsupportedName);
    }

    /// <summary>
    ///     Verifies that current setting values are exposed for editable rows.
    /// </summary>
    [Fact]
    public void GetSettingValueReturnsCurrentFlagValues()
    {
        var settings = new AgsSettings(true, false);
        var codexValue = (bool)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingValue", settings, 0);
        var claudeValue = (bool)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingValue", settings, 1);
        var unsupportedValue = (bool)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingValue", settings, 2);
        Assert.False(codexValue);
        Assert.True(claudeValue);
        Assert.False(unsupportedValue);
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
    ///     Verifies that selecting the return option exits the settings screen immediately.
    /// </summary>
    [Fact]
    public void RunReturnsImmediatelyWhenReturnOptionIsSelected()
    {
        AgsTestState.ResetCurrentSettings();
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        using var prompts = new PromptStubScope(selectionIndexes: [2]);
        SettingsSubsystem.Run();
        Assert.Equal(["Settings"], prompts.SelectMessages);
        Assert.Empty(prompts.ConfirmMessages);
    }

    /// <summary>
    ///     Verifies that the settings screen does nothing when no current settings exist.
    /// </summary>
    [Fact]
    public void RunReturnsImmediatelyWhenCurrentSettingsAreMissing()
    {
        AgsTestState.ResetCurrentSettings();
        using var prompts = new PromptStubScope();
        using var console = new ConsoleRedirectionScope(string.Empty);
        SettingsSubsystem.Run();
        Assert.Equal(string.Empty, console.Output);
        Assert.Empty(prompts.SelectMessages);
        Assert.Empty(prompts.ConfirmMessages);
    }

    /// <summary>
    ///     Verifies that changing settings through prompts persists the updated values.
    /// </summary>
    [Fact]
    public void RunUsesPromptFlowToUpdateSettings()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var prompts = new PromptStubScope(confirmations: [true, true],
            selectionIndexes: [1, 0, 2]);
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        SettingsSubsystem.Run();
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.Equal(["Settings", "Settings", "Settings"], prompts.SelectMessages);
        Assert.Equal(["Enable use-claude?", "Enable use-codex?"], prompts.ConfirmMessages);
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
}
