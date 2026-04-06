using System.Globalization;
using System.Text.Json;

namespace AGS;

/// <summary>
///     Represents persisted configuration flags, provider cooldown settings, and last successful
///     update timestamps for the local <c>.ags/config.json</c> file and stores the current
///     process-wide settings instance.
/// </summary>
internal readonly struct AgsSettings
{
    internal const string AgsDirectoryName = ".ags";
    internal const string ConfigFileName = "config.json";
    private const string UseClaudeSettingName = "use-claude";
    private const string UseCodexSettingName = "use-codex";
    private const string LegacyRateLimitDefaultCooldownSecondsSettingName =
        "rate-limit-default-cooldown";
    private const string RateLimitDefaultCooldownMinutesSettingName =
        "rate-limit-default-cooldown-minutes";
    private const string ProviderCooldownsSettingName = "provider-cooldowns";
    private const string DefaultModelsSettingName = "default-models";
    internal const int DefaultRateLimitCooldownMinutes = 30;
    private static AgsSettings currentSettings = new(false, false);
    private static bool hasCurrentSettings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgsSettings" /> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    internal AgsSettings(bool useClaude, bool useCodex)
        : this(useClaude, useCodex, DefaultRateLimitCooldownMinutes, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgsSettings" /> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    /// <param name="rateLimitDefaultCooldownMinutes">
    ///     Cooldown period in minutes applied when the provider response does not include a reset
    ///     time. Defaults to <see cref="DefaultRateLimitCooldownMinutes" />.
    /// </param>
    /// <param name="providerCooldowns">
    ///     Map of provider ID to cooldown expiry timestamp. Pass <see langword="null" /> for an
    ///     empty map.
    /// </param>
    internal AgsSettings(bool useClaude, bool useCodex, int rateLimitDefaultCooldownMinutes,
        IReadOnlyDictionary<string, DateTimeOffset> providerCooldowns)
        : this(useClaude, useCodex, rateLimitDefaultCooldownMinutes, providerCooldowns, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgsSettings" /> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    /// <param name="rateLimitDefaultCooldownMinutes">
    ///     Cooldown period in minutes applied when the provider response does not include a reset
    ///     time. Defaults to <see cref="DefaultRateLimitCooldownMinutes" />.
    /// </param>
    /// <param name="providerCooldowns">
    ///     Map of provider ID to cooldown expiry timestamp. Pass <see langword="null" /> for an
    ///     empty map.
    /// </param>
    /// <param name="defaultModels">
    ///     Priority-ordered list of model names used for general AI tasks (e.g. reading and
    ///     interpreting coordination documents). Pass <see langword="null" /> for an empty list.
    /// </param>
    internal AgsSettings(bool useClaude, bool useCodex, int rateLimitDefaultCooldownMinutes,
        IReadOnlyDictionary<string, DateTimeOffset> providerCooldowns,
        IReadOnlyList<string> defaultModels)
    {
        UseClaude = useClaude;
        UseCodex = useCodex;
        RateLimitDefaultCooldownMinutes = rateLimitDefaultCooldownMinutes > 0
            ? rateLimitDefaultCooldownMinutes
            : DefaultRateLimitCooldownMinutes;
        ProviderCooldowns = providerCooldowns ?? new Dictionary<string, DateTimeOffset>();
        DefaultModels = defaultModels ?? [];
    }

    /// <summary>
    ///     Gets a value indicating whether Claude Code integration is enabled.
    /// </summary>
    internal bool UseClaude { get; }

    /// <summary>
    ///     Gets a value indicating whether Codex integration is enabled.
    /// </summary>
    internal bool UseCodex { get; }

    /// <summary>
    ///     Gets a value indicating whether both integrations are disabled.
    /// </summary>
    internal bool AreAllModelsDisabled => !UseClaude && !UseCodex;

    /// <summary>
    ///     Gets the cooldown period in minutes applied when the provider response does not include
    ///     a reset time. Defaults to <see cref="DefaultRateLimitCooldownMinutes" />.
    /// </summary>
    internal int RateLimitDefaultCooldownMinutes { get; }

    /// <summary>
    ///     Gets the cooldown period as a <see cref="TimeSpan" />.
    /// </summary>
    internal TimeSpan RateLimitDefaultCooldown => TimeSpan.FromMinutes(RateLimitDefaultCooldownMinutes);

    /// <summary>
    ///     Gets the map of provider IDs to their cooldown expiry timestamps.
    /// </summary>
    internal IReadOnlyDictionary<string, DateTimeOffset> ProviderCooldowns { get; }

    /// <summary>
    ///     Gets the priority-ordered list of model names used for general AI tasks such as reading
    ///     and interpreting coordination documents. An empty list means no default AI is configured.
    /// </summary>
    internal IReadOnlyList<string> DefaultModels { get; }

    /// <summary>
    ///     Gets the current application settings for this process.
    /// </summary>
    internal static AgsSettings Current => currentSettings;

    /// <summary>
    ///     Gets a value indicating whether the current application settings have been initialized.
    /// </summary>
    internal static bool HasCurrentSettings => hasCurrentSettings;

    /// <summary>
    ///     Builds the absolute path to the persisted application configuration file.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the project root directory.</param>
    /// <returns>Absolute path to the <c>.ags/config.json</c> file.</returns>
    internal static string GetConfigPath(string projectRootPath)
    {
        return Path.Combine(projectRootPath, AgsDirectoryName, ConfigFileName);
    }

    /// <summary>
    ///     Stores the current application settings for global access within the process.
    /// </summary>
    /// <param name="settings">Settings instance to expose globally.</param>
    internal static void SetCurrent(AgsSettings settings)
    {
        currentSettings = settings;
        hasCurrentSettings = true;
    }

    /// <summary>
    ///     Attempts to read settings from an existing configuration file.
    /// </summary>
    /// <param name="configPath">Absolute path to the configuration file.</param>
    /// <param name="settings">Parsed settings when read succeeds; otherwise default values.</param>
    /// <returns>
    ///     <see langword="true" /> when both required settings are successfully parsed; otherwise
    ///     <see langword="false" />.
    /// </returns>
    internal static bool TryReadFromConfig(string configPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        if (!File.Exists(configPath)) return false;
        try
        {
            var configContent = File.ReadAllText(configPath);
            if (TryReadFromJson(configContent, out settings)) return true;
            return TryReadFromLegacyConfig(configContent, out settings);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Writes the current settings to the JSON configuration file.
    /// </summary>
    /// <param name="configPath">Absolute path to the configuration file.</param>
    internal void WriteToConfig(string configPath)
    {
        var now = DateTimeOffset.UtcNow;
        var activeCooldowns = ProviderCooldowns
            .Where(kvp => kvp.Value > now)
            .ToDictionary(kvp => kvp.Key,
                kvp => (object)kvp.Value.ToString("O", CultureInfo.InvariantCulture));
        var serializedSettings = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [UseClaudeSettingName] = UseClaude,
            [UseCodexSettingName] = UseCodex,
            [RateLimitDefaultCooldownMinutesSettingName] = RateLimitDefaultCooldownMinutes,
            [ProviderCooldownsSettingName] = activeCooldowns,
            [DefaultModelsSettingName] = DefaultModels.ToArray()
        }, JsonOptions);
        File.WriteAllText(configPath, serializedSettings);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated provider cooldowns map.
    ///     Expired entries are removed automatically.
    /// </summary>
    /// <param name="providerCooldowns">New provider cooldowns map.</param>
    /// <returns>A new settings instance with the updated cooldowns.</returns>
    internal AgsSettings WithProviderCooldowns(
        IReadOnlyDictionary<string, DateTimeOffset> providerCooldowns)
    {
        return new AgsSettings(UseClaude, UseCodex, RateLimitDefaultCooldownMinutes,
            providerCooldowns, DefaultModels);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Claude Code enabled flag.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration should be enabled.</param>
    /// <returns>A new settings instance with the updated Claude enabled flag.</returns>
    internal AgsSettings WithUseClaude(bool useClaude)
    {
        return new AgsSettings(useClaude, UseCodex, RateLimitDefaultCooldownMinutes,
            ProviderCooldowns, DefaultModels);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Codex enabled flag.
    /// </summary>
    /// <param name="useCodex">Whether Codex integration should be enabled.</param>
    /// <returns>A new settings instance with the updated Codex enabled flag.</returns>
    internal AgsSettings WithUseCodex(bool useCodex)
    {
        return new AgsSettings(UseClaude, useCodex, RateLimitDefaultCooldownMinutes,
            ProviderCooldowns, DefaultModels);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated default rate-limit cooldown.
    /// </summary>
    /// <param name="rateLimitDefaultCooldownMinutes">
    ///     Cooldown period in minutes applied when the provider response does not include a reset
    ///     time.
    /// </param>
    /// <returns>A new settings instance with the updated default cooldown.</returns>
    internal AgsSettings WithRateLimitDefaultCooldownMinutes(int rateLimitDefaultCooldownMinutes)
    {
        return new AgsSettings(UseClaude, UseCodex, rateLimitDefaultCooldownMinutes,
            ProviderCooldowns, DefaultModels);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated default models list.
    /// </summary>
    /// <param name="defaultModels">
    ///     Priority-ordered list of model names for general AI tasks.
    /// </param>
    /// <returns>A new settings instance with the updated default models.</returns>
    internal AgsSettings WithDefaultModels(IReadOnlyList<string> defaultModels)
    {
        return new AgsSettings(UseClaude, UseCodex, RateLimitDefaultCooldownMinutes,
            ProviderCooldowns, defaultModels);
    }

    /// <summary>
    ///     Writes the current settings to the project configuration file.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the project root directory.</param>
    /// <param name="errorMessage">
    ///     Error message when the write fails; otherwise, an empty string.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the settings are written successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    internal bool TryWriteToProjectConfig(string projectRootPath, out string errorMessage)
    {
        var configPath = GetConfigPath(projectRootPath);
        try
        {
            var configDirectoryPath = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(configDirectoryPath))
                Directory.CreateDirectory(configDirectoryPath);
            WriteToConfig(configPath);
            errorMessage = string.Empty;
            return true;
        }
        catch (IOException exception)
        {
            errorMessage = $"Settings could not be saved to {configPath}: {exception.Message}";
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            errorMessage = $"Settings could not be saved to {configPath}: {exception.Message}";
            return false;
        }
    }

    /// <summary>
    ///     Attempts to read settings from a JSON payload.
    /// </summary>
    /// <param name="jsonContent">JSON content to parse.</param>
    /// <param name="settings">Parsed settings when read succeeds; otherwise default values.</param>
    /// <returns>
    ///     <see langword="true" /> when both required settings are successfully parsed; otherwise
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryReadFromJson(string jsonContent, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var rootElement = jsonDocument.RootElement;
            if (rootElement.ValueKind != JsonValueKind.Object) return false;
            if (!TryReadRequiredBoolean(rootElement, UseClaudeSettingName, out var useClaude))
                return false;
            if (!TryReadRequiredBoolean(rootElement, UseCodexSettingName, out var useCodex))
                return false;
            var rateLimitDefaultCooldownMinutes =
                TryReadRateLimitDefaultCooldownMinutes(rootElement);
            var providerCooldowns = TryReadProviderCooldowns(rootElement);
            var defaultModels = TryReadStringList(rootElement, DefaultModelsSettingName);
            settings = new AgsSettings(useClaude, useCodex, rateLimitDefaultCooldownMinutes,
                providerCooldowns, defaultModels);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Attempts to read an optional integer property from a JSON object.
    /// </summary>
    /// <param name="configElement">JSON object that contains persisted settings.</param>
    /// <param name="propertyName">Property name to read.</param>
    /// <param name="defaultValue">Value returned when the property is absent or invalid.</param>
    /// <returns>Parsed integer value when present and valid; otherwise <paramref name="defaultValue" />.</returns>
    private static int TryReadOptionalInt(JsonElement configElement, string propertyName,
        int defaultValue)
    {
        if (!configElement.TryGetProperty(propertyName, out var propertyElement))
            return defaultValue;
        if (propertyElement.ValueKind != JsonValueKind.Number) return defaultValue;
        return propertyElement.TryGetInt32(out var value) ? value : defaultValue;
    }

    /// <summary>
    ///     Reads the default cooldown value in minutes, falling back to the legacy seconds-based
    ///     property when present.
    /// </summary>
    /// <param name="configElement">JSON object that contains persisted settings.</param>
    /// <returns>Cooldown period in minutes.</returns>
    private static int TryReadRateLimitDefaultCooldownMinutes(JsonElement configElement)
    {
        var configuredMinutes = TryReadOptionalInt(configElement,
            RateLimitDefaultCooldownMinutesSettingName, 0);
        if (configuredMinutes > 0) return configuredMinutes;

        var legacySeconds = TryReadOptionalInt(configElement,
            LegacyRateLimitDefaultCooldownSecondsSettingName, 0);
        if (legacySeconds > 0) return ConvertLegacyCooldownSecondsToMinutes(legacySeconds);

        return DefaultRateLimitCooldownMinutes;
    }

    /// <summary>
    ///     Converts a legacy seconds-based cooldown value to whole minutes.
    /// </summary>
    /// <param name="legacySeconds">Cooldown value persisted in seconds.</param>
    /// <returns>Equivalent cooldown in whole minutes, rounded up.</returns>
    private static int ConvertLegacyCooldownSecondsToMinutes(int legacySeconds)
    {
        return (int)Math.Ceiling(legacySeconds / 60d);
    }

    /// <summary>
    ///     Reads the provider cooldowns map from a JSON object, filtering out expired entries.
    /// </summary>
    private static IReadOnlyDictionary<string, DateTimeOffset> TryReadProviderCooldowns(
        JsonElement configElement)
    {
        var result = new Dictionary<string, DateTimeOffset>();
        if (!configElement.TryGetProperty(ProviderCooldownsSettingName, out var cooldownsElement))
            return result;
        if (cooldownsElement.ValueKind != JsonValueKind.Object) return result;
        var now = DateTimeOffset.UtcNow;
        foreach (var property in cooldownsElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String) continue;
            var rawValue = property.Value.GetString();
            if (string.IsNullOrWhiteSpace(rawValue)) continue;
            if (!DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var expiry)) continue;
            var expiryUtc = expiry.ToUniversalTime();
            if (expiryUtc > now)
                result[property.Name] = expiryUtc;
        }
        return result;
    }

    /// <summary>
    ///     Reads a JSON array of strings from an optional property, returning an empty list when
    ///     the property is absent or not a valid string array.
    /// </summary>
    private static IReadOnlyList<string> TryReadStringList(JsonElement configElement,
        string propertyName)
    {
        var result = new List<string>();
        if (!configElement.TryGetProperty(propertyName, out var arrayElement)) return result;
        if (arrayElement.ValueKind != JsonValueKind.Array) return result;
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String) continue;
            var value = item.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                result.Add(value.Trim());
        }
        return result;
    }

    /// <summary>
    ///     Attempts to read a required Boolean property from a JSON object.
    /// </summary>
    /// <param name="configElement">JSON object that contains persisted settings.</param>
    /// <param name="propertyName">Property name to read.</param>
    /// <param name="value">Parsed Boolean value when available.</param>
    /// <returns>
    ///     <see langword="true" /> when the property exists and contains a Boolean value;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    private static bool TryReadRequiredBoolean(JsonElement configElement, string propertyName,
        out bool value)
    {
        value = false;
        if (!configElement.TryGetProperty(propertyName, out var propertyElement)) return false;
        if (propertyElement.ValueKind != JsonValueKind.True &&
            propertyElement.ValueKind != JsonValueKind.False)
            return false;
        value = propertyElement.GetBoolean();
        return true;
    }

    /// <summary>
    ///     Attempts to read settings from the legacy plain-text configuration file content.
    /// </summary>
    /// <param name="configContent">Raw content of the legacy configuration file.</param>
    /// <param name="settings">Parsed settings when read succeeds; otherwise default values.</param>
    /// <returns>
    ///     <see langword="true" /> when both required settings are successfully parsed; otherwise
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryReadFromLegacyConfig(string configContent, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        try
        {
            var lines = configContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var hasUseClaude = false;
            var hasUseCodex = false;
            var useClaude = false;
            var useCodex = false;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length == 0) continue;
                var separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex == trimmedLine.Length - 1) continue;
                var key = trimmedLine[..separatorIndex].Trim();
                var value = trimmedLine[(separatorIndex + 1)..].Trim();
                if (key == UseClaudeSettingName && bool.TryParse(value, out var parsedUseClaude))
                {
                    useClaude = parsedUseClaude;
                    hasUseClaude = true;
                    continue;
                }
                if (key == UseCodexSettingName && bool.TryParse(value, out var parsedUseCodex))
                {
                    useCodex = parsedUseCodex;
                    hasUseCodex = true;
                }
            }
            if (!hasUseClaude || !hasUseCodex) return false;
            settings = new AgsSettings(useClaude, useCodex);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

}
