using System.Globalization;

namespace AGS.subsystems;

/// <summary>
///     Displays and edits persisted application settings.
/// </summary>
internal static class SettingsSubsystem
{
    private static readonly HashSet<string> SupportedDefaultModels =
        new(["chatgpt", "claude-opus", "claude-sonnet", "claude-haiku"],
            StringComparer.OrdinalIgnoreCase);

    private const int DefaultModelTimeoutOptionIndex = 2;
    private const int DefaultModelsOptionIndex = 3;
    private const int ReturnOptionIndex = 4;
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
            $"default-models: {FormatDefaultModelsValue(settings.DefaultModels)}",
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
    ///     Formats the configured default model list for display in the settings screen.
    /// </summary>
    /// <param name="defaultModels">Current default model list.</param>
    /// <returns>Human-readable default model summary.</returns>
    private static string FormatDefaultModelsValue(IReadOnlyList<string> defaultModels)
    {
        return defaultModels == null || defaultModels.Count == 0
            ? "not configured"
            : string.Join(", ", defaultModels);
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
        if (selectedIndex == DefaultModelsOptionIndex) return "default-models";
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
        var validationErrorMessage = ValidateSettings(updatedSettings);
        if (!string.IsNullOrEmpty(validationErrorMessage))
            return validationErrorMessage;
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
    ///     Prompts for a comma-separated default model priority list and validates the input.
    /// </summary>
    /// <param name="currentModels">Currently configured default model list.</param>
    /// <param name="defaultModels">Parsed and normalized default model list.</param>
    /// <returns>Error message when the input is invalid; otherwise, an empty string.</returns>
    private static string PromptForDefaultModels(IReadOnlyList<string> currentModels,
        out IReadOnlyList<string> defaultModels)
    {
        var currentValue = currentModels == null ? string.Empty : string.Join(", ", currentModels);
        var rawValue = AgsPrompt.Input(
            $"Enter default models in priority order (comma-separated). Supported: {string.Join(", ", SupportedDefaultModels.Order())}",
            currentValue);
        return TryParseDefaultModels(rawValue, out defaultModels);
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
            if (selectedIndex == DefaultModelsOptionIndex)
            {
                var inputErrorMessage = PromptForDefaultModels(currentSettings.DefaultModels,
                    out var updatedDefaultModels);
                if (!string.IsNullOrEmpty(inputErrorMessage))
                {
                    Console.WriteLine(inputErrorMessage);
                    continue;
                }
                var defaultModelsErrorMessage = SetDefaultModels(updatedDefaultModels);
                if (!string.IsNullOrEmpty(defaultModelsErrorMessage))
                    Console.WriteLine(defaultModelsErrorMessage);
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
    ///     Sets the default model priority list and persists the updated configuration.
    /// </summary>
    /// <param name="defaultModels">Priority-ordered default model list to persist.</param>
    /// <returns>Error message when persistence fails; otherwise, an empty string.</returns>
    private static string SetDefaultModels(IReadOnlyList<string> defaultModels)
    {
        var currentSettings = AgsSettings.Current;
        if (HaveSameValues(currentSettings.DefaultModels, defaultModels)) return string.Empty;
        return PersistSettings(currentSettings.WithDefaultModels(defaultModels));
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

    /// <summary>
    ///     Parses and validates a comma-separated default model priority list.
    /// </summary>
    /// <param name="rawValue">Raw user input.</param>
    /// <param name="defaultModels">Parsed default model list.</param>
    /// <returns>Error message when the input is invalid; otherwise, an empty string.</returns>
    private static string TryParseDefaultModels(string rawValue,
        out IReadOnlyList<string> defaultModels)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        defaultModels = result;
        if (string.IsNullOrWhiteSpace(rawValue))
            return "The default model list must contain at least one supported model.";

        foreach (var item in rawValue.Split(','))
        {
            var model = item.Trim();
            if (model.Length == 0) continue;
            if (!SupportedDefaultModels.Contains(model, StringComparer.OrdinalIgnoreCase))
                return $"Unsupported default model '{model}'.";
            if (!seen.Add(model))
                return $"Default model '{model}' is listed more than once.";
            result.Add(model.ToLowerInvariant());
        }

        if (result.Count == 0)
            return "The default model list must contain at least one supported model.";
        return string.Empty;
    }

    /// <summary>
    ///     Validates cross-field constraints before persisting settings. Supported-model
    ///     containment is enforced earlier by <see cref="TryParseDefaultModels" />.
    /// </summary>
    /// <param name="settings">Settings to validate before persistence.</param>
    /// <returns>Error message when validation fails; otherwise, an empty string.</returns>
    private static string ValidateSettings(AgsSettings settings)
    {
        if (settings.DefaultModels == null || settings.DefaultModels.Count == 0)
            return "Configure at least one default model before saving settings.";

        foreach (var model in settings.DefaultModels)
        {
            if (model == "chatgpt" && !settings.UseCodex)
                return "Model 'chatgpt' requires Codex to be enabled.";
            if (model.StartsWith("claude-", StringComparison.OrdinalIgnoreCase) &&
                !settings.UseClaude)
                return $"Model '{model}' requires Claude Code to be enabled.";
        }

        return string.Empty;
    }

    /// <summary>
    ///     Returns <see langword="true" /> when both lists contain the same values in the same
    ///     order under a case-insensitive ordinal comparison.
    /// </summary>
    private static bool HaveSameValues(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return false;
        return left.SequenceEqual(right, StringComparer.OrdinalIgnoreCase);
    }
}
