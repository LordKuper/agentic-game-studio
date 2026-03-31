using System.Globalization;

namespace AGS.subsystems;

/// <summary>
///     Displays and edits persisted application settings.
/// </summary>
internal static class SettingsSubsystem
{
    private const int DefaultModelTimeoutOptionIndex = 2;
    private const int ReturnOptionIndex = 3;
    private const string Title = "Settings";
    private const int UseClaudeOptionIndex = 1;
    private const int UseCodexOptionIndex = 0;

    /// <summary>
    ///     Builds the visible settings option labels for the current application state.
    /// </summary>
    /// <param name="settings">Current persisted application settings.</param>
    /// <returns>Ordered settings option labels.</returns>
    private static string[] BuildOptionLabels(AgsSettings settings)
    {
        return
        [
            $"use-codex: {FormatBooleanValue(settings.UseCodex)}",
            $"use-claude: {FormatBooleanValue(settings.UseClaude)}",
            $"default-model-timeout: {FormatMinutesValue(settings.RateLimitDefaultCooldownMinutes)}",
            "Return to main menu"
        ];
    }

    /// <summary>
    ///     Formats a Boolean setting value for display in the settings screen.
    /// </summary>
    /// <param name="value">Boolean value to format.</param>
    /// <returns><c>yes</c> when <paramref name="value" /> is true; otherwise, <c>no</c>.</returns>
    private static string FormatBooleanValue(bool value)
    {
        return value ? "yes" : "no";
    }

    /// <summary>
    ///     Formats a minute-based setting value for display in the settings screen.
    /// </summary>
    /// <param name="minutes">Minute value to format.</param>
    /// <returns>Human-readable minute text.</returns>
    private static string FormatMinutesValue(int minutes)
    {
        return minutes == 1 ? "1 minute" : $"{minutes} minutes";
    }

    /// <summary>
    ///     Gets the visible display name for the selected settings row.
    /// </summary>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <returns>Visible setting name for the selected row.</returns>
    private static string GetSettingDisplayName(int selectedIndex)
    {
        if (selectedIndex == UseCodexOptionIndex) return "use-codex";
        if (selectedIndex == UseClaudeOptionIndex) return "use-claude";
        if (selectedIndex == DefaultModelTimeoutOptionIndex) return "default model timeout";
        return string.Empty;
    }

    /// <summary>
    ///     Gets the current Boolean value for the selected settings row.
    /// </summary>
    /// <param name="settings">Current persisted application settings.</param>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <returns>
    ///     Current Boolean value for the selected settings row, or
    ///     <see langword="false" /> when the row does not represent a Boolean setting.
    /// </returns>
    private static bool GetSettingValue(AgsSettings settings, int selectedIndex)
    {
        if (selectedIndex == UseCodexOptionIndex) return settings.UseCodex;
        if (selectedIndex == UseClaudeOptionIndex) return settings.UseClaude;
        return false;
    }

    /// <summary>
    ///     Persists the provided settings and updates the process-wide current settings instance on
    ///     success.
    /// </summary>
    /// <param name="updatedSettings">Settings instance to persist.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string PersistSettings(AgsSettings updatedSettings)
    {
        if (!updatedSettings.TryWriteToProjectConfig(Directory.GetCurrentDirectory(),
                out var errorMessage))
            return errorMessage;
        AgsSettings.SetCurrent(updatedSettings);
        return string.Empty;
    }

    /// <summary>
    ///     Prompts for a positive whole-number minute value.
    /// </summary>
    /// <param name="currentValue">Currently configured minute value.</param>
    /// <param name="minutes">Parsed minute value.</param>
    /// <returns>Error message when the input cannot be parsed; otherwise, an empty string.</returns>
    private static string PromptForMinutesValue(int currentValue, out int minutes)
    {
        var rawValue = AgsPrompt.Input("Enter default model timeout in minutes:",
            currentValue.ToString(CultureInfo.InvariantCulture));
        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out minutes) || minutes <= 0)
            return "The default model timeout must be a positive whole number of minutes.";
        return string.Empty;
    }

    /// <summary>
    ///     Shows the settings screen and persists any configuration changes made by the user.
    /// </summary>
    internal static void Run()
    {
        if (!AgsSettings.HasCurrentSettings) return;
        while (true)
        {
            var currentSettings = AgsSettings.Current;
            var selectedIndex = AgsPrompt.Select(Title, BuildOptionLabels(currentSettings));
            if (selectedIndex == ReturnOptionIndex) return;
            if (selectedIndex == DefaultModelTimeoutOptionIndex)
            {
                var inputErrorMessage = PromptForMinutesValue(
                    currentSettings.RateLimitDefaultCooldownMinutes, out var updatedMinutes);
                if (!string.IsNullOrEmpty(inputErrorMessage))
                {
                    Console.WriteLine(inputErrorMessage);
                    continue;
                }
                var timeoutErrorMessage = SetRateLimitDefaultCooldownMinutes(updatedMinutes);
                if (!string.IsNullOrEmpty(timeoutErrorMessage))
                    Console.WriteLine(timeoutErrorMessage);
                continue;
            }
            var settingDisplayName = GetSettingDisplayName(selectedIndex);
            var updatedValue = AgsPrompt.Confirm($"Enable {settingDisplayName}?",
                GetSettingValue(currentSettings, selectedIndex));
            var errorMessage = SetSettingValue(selectedIndex, updatedValue);
            if (!string.IsNullOrEmpty(errorMessage)) Console.WriteLine(errorMessage);
        }
    }

    /// <summary>
    ///     Sets the default timeout in minutes and persists the updated configuration.
    /// </summary>
    /// <param name="minutes">Positive whole-number minute value to persist.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string SetRateLimitDefaultCooldownMinutes(int minutes)
    {
        var currentSettings = AgsSettings.Current;
        if (currentSettings.RateLimitDefaultCooldownMinutes == minutes) return string.Empty;
        return PersistSettings(currentSettings.WithRateLimitDefaultCooldownMinutes(minutes));
    }

    /// <summary>
    ///     Sets the selected Boolean setting to the requested value and persists the updated
    ///     configuration.
    /// </summary>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <param name="value">Boolean value to persist for the selected settings row.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string SetSettingValue(int selectedIndex, bool value)
    {
        var currentSettings = AgsSettings.Current;
        if (selectedIndex == UseCodexOptionIndex)
        {
            if (currentSettings.UseCodex == value) return string.Empty;
            return PersistSettings(currentSettings.WithUseCodex(value));
        }
        if (selectedIndex == UseClaudeOptionIndex)
        {
            if (currentSettings.UseClaude == value) return string.Empty;
            return PersistSettings(currentSettings.WithUseClaude(value));
        }
        return string.Empty;
    }
}
