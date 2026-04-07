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
            "BuildOptionLabels",
            new AgsSettings(true, false, 45, null, ["claude-sonnet"]));
        Assert.Equal(
        [
            "use-codex: no",
            "use-claude: yes",
            "default-model-timeout: 45 minutes",
            "default-models: claude-sonnet",
            "Return to main menu"
        ], labels);
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
    ///     Verifies that minute values are formatted consistently for display.
    /// </summary>
    [Theory]
    [InlineData(1, "1 minute")]
    [InlineData(30, "30 minutes")]
    public void FormatMinutesValueReturnsExpectedText(int minutes, string expectedValue)
    {
        var formattedValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "FormatMinutesValue", minutes);
        Assert.Equal(expectedValue, formattedValue);
    }

    /// <summary>
    ///     Verifies that the default model list is formatted consistently for display.
    /// </summary>
    [Fact]
    public void FormatDefaultModelsValueReturnsExpectedText()
    {
        var configuredValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "FormatDefaultModelsValue", (object)new[] { "chatgpt", "claude-sonnet" });
        var emptyValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "FormatDefaultModelsValue", (object)Array.Empty<string>());
        Assert.Equal("chatgpt, claude-sonnet", configuredValue);
        Assert.Equal("not configured", emptyValue);
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
        var timeoutName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 2);
        var defaultModelsName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 3);
        var unsupportedName = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "GetSettingDisplayName", 4);
        Assert.Equal("use-codex", codexName);
        Assert.Equal("use-claude", claudeName);
        Assert.Equal("default model timeout", timeoutName);
        Assert.Equal("default-models", defaultModelsName);
        Assert.Equal(string.Empty, unsupportedName);
    }

    /// <summary>
    ///     Verifies that current setting values are exposed for editable Boolean rows.
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
            "PersistSettings", new AgsSettings(true, true, 45, null,
                ["chatgpt", "claude-sonnet"]));
        Assert.Equal(string.Empty, errorMessage);
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.Equal(45, AgsSettings.Current.RateLimitDefaultCooldownMinutes);
        Assert.Equal(["chatgpt", "claude-sonnet"], AgsSettings.Current.DefaultModels);
        Assert.True(File.Exists(AgsSettings.GetConfigPath(tempDirectory.Path)));
    }

    /// <summary>
    ///     Verifies that selecting the return option exits the settings screen immediately.
    /// </summary>
    [Fact]
    public void RunReturnsImmediatelyWhenReturnOptionIsSelected()
    {
        AgsTestState.ResetCurrentSettings();
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        using var prompts = new PromptStubScope(selectionIndexes: [4]);
        SettingsSubsystem.Run();
        Assert.Equal(["Settings"], prompts.SelectMessages);
        Assert.Empty(prompts.ConfirmMessages);
        Assert.Empty(prompts.InputMessages);
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
        Assert.Empty(prompts.InputMessages);
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
        using var prompts = new PromptStubScope(confirmations: [true],
            selectionIndexes: [1, 2, 3, 4], inputs: ["45", "chatgpt, claude-sonnet"]);
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        SettingsSubsystem.Run();
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.Equal(45, AgsSettings.Current.RateLimitDefaultCooldownMinutes);
        Assert.Equal(["chatgpt", "claude-sonnet"], AgsSettings.Current.DefaultModels);
        Assert.Equal(["Settings", "Settings", "Settings", "Settings"],
            prompts.SelectMessages);
        Assert.Equal(["Enable use-claude?"], prompts.ConfirmMessages);
        Assert.Equal(
        [
            "Enter default model timeout in minutes:",
            "Enter default models in priority order (comma-separated). Supported: chatgpt, claude-haiku, claude-opus, claude-sonnet"
        ], prompts.InputMessages);
        Assert.Equal(["30", "chatgpt"], prompts.InputDefaultValues);
    }

    /// <summary>
    ///     Verifies that invalid timeout input prints an error and returns to the menu loop.
    /// </summary>
    [Fact]
    public void RunPrintsErrorWhenTimeoutInputIsInvalid()
    {
        AgsTestState.ResetCurrentSettings();
        using var prompts = new PromptStubScope(selectionIndexes: [2, 4], inputs: ["invalid"]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        SettingsSubsystem.Run();
        Assert.Contains("The default model timeout must be a positive whole number of minutes.",
            console.Output);
        Assert.Equal(AgsSettings.DefaultRateLimitCooldownMinutes,
            AgsSettings.Current.RateLimitDefaultCooldownMinutes);
    }

    /// <summary>
    ///     Verifies that changing a selected Boolean setting persists the new value and updates
    ///     current state.
    /// </summary>
    [Fact]
    public void SetSettingValuePersistsChangedValue()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, false, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["claude-sonnet"]));
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 0, true);
        Assert.Contains("requires Claude Code", errorMessage);
        Assert.False(AgsSettings.Current.UseCodex);
    }

    /// <summary>
    ///     Verifies that changing a selected Boolean setting persists the new value when the
    ///     resulting default model configuration remains valid.
    /// </summary>
    [Fact]
    public void SetSettingValuePersistsChangedValueWhenConfigurationRemainsValid()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 1, true);
        Assert.Equal(string.Empty, errorMessage);
        Assert.True(AgsSettings.Current.UseClaude);
        Assert.True(AgsSettings.Current.UseCodex);
        Assert.True(AgsSettings.TryReadFromConfig(AgsSettings.GetConfigPath(tempDirectory.Path),
            out var persistedSettings));
        Assert.True(persistedSettings.UseClaude);
        Assert.True(persistedSettings.UseCodex);
    }

    /// <summary>
    ///     Verifies that changing the timeout persists the new minute value and updates current
    ///     state.
    /// </summary>
    [Fact]
    public void SetRateLimitDefaultCooldownMinutesPersistsChangedValue()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetRateLimitDefaultCooldownMinutes", 45);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal(45, AgsSettings.Current.RateLimitDefaultCooldownMinutes);
        Assert.True(AgsSettings.TryReadFromConfig(AgsSettings.GetConfigPath(tempDirectory.Path),
            out var persistedSettings));
        Assert.Equal(45, persistedSettings.RateLimitDefaultCooldownMinutes);
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
        AgsSettings.SetCurrent(new AgsSettings(true, false, 45, null, ["claude-sonnet"]));
        var unchangedValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 1, true);
        var unchangedTimeout = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetRateLimitDefaultCooldownMinutes", 45);
        var unsupportedValue = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetSettingValue", 4, true);
        Assert.Equal(string.Empty, unchangedValue);
        Assert.Equal(string.Empty, unchangedTimeout);
        Assert.Equal(string.Empty, unsupportedValue);
    }

    /// <summary>
    ///     Verifies that invalid default model input is rejected.
    /// </summary>
    [Fact]
    public void TryParseDefaultModelsRejectsUnsupportedValue()
    {
        object[] arguments = ["chatgpt, unknown-model", null];
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "TryParseDefaultModels", arguments);
        Assert.Equal("Unsupported default model 'unknown-model'.", errorMessage);
    }

    /// <summary>
    ///     Verifies that valid default model input is parsed in order.
    /// </summary>
    [Fact]
    public void TryParseDefaultModelsParsesOrderedValues()
    {
        object[] arguments = ["chatgpt, claude-sonnet", null];
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "TryParseDefaultModels", arguments);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal(["chatgpt", "claude-sonnet"],
            Assert.IsAssignableFrom<IReadOnlyList<string>>(arguments[1]));
    }

    /// <summary>
    ///     Verifies that default model persistence rejects models whose provider is disabled.
    /// </summary>
    [Fact]
    public void SetDefaultModelsRejectsDisabledProviderModels()
    {
        AgsTestState.ResetCurrentSettings();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        var errorMessage = (string)PrivateAccess.InvokeStatic(typeof(SettingsSubsystem),
            "SetDefaultModels", (object)new[] { "claude-sonnet" });
        Assert.Equal("Model 'claude-sonnet' requires Claude Code to be enabled.", errorMessage);
    }
}
