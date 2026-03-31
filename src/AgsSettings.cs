using System.Globalization;
using System.Text.Json;

namespace AGS;

/// <summary>
///     Represents persisted configuration flags and last successful update timestamps for the
///     local <c>.ags/config.json</c> file and stores the current process-wide settings instance.
/// </summary>
internal readonly struct AgsSettings
{
    internal const string AgsDirectoryName = ".ags";
    internal const string ConfigFileName = "config.json";
    private const string UseClaudeSettingName = "use-claude";
    private const string UseCodexSettingName = "use-codex";
    private const string ClaudeLastUpdateUtcSettingName = "claude-last-update-utc";
    private const string CodexLastUpdateUtcSettingName = "codex-last-update-utc";
    private const string RateLimitDefaultCooldownSettingName = "rate-limit-default-cooldown";
    private const string ProviderCooldownsSettingName = "provider-cooldowns";
    internal const int DefaultRateLimitCooldownSeconds = 1800;
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
    internal AgsSettings(bool useClaude, bool useCodex) : this(useClaude, useCodex,
        DateTimeOffset.MinValue, DateTimeOffset.MinValue) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgsSettings" /> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    /// <param name="claudeLastUpdateUtc">
    ///     UTC timestamp of the last successful Claude Code update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </param>
    /// <param name="codexLastUpdateUtc">
    ///     UTC timestamp of the last successful Codex update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </param>
    internal AgsSettings(bool useClaude, bool useCodex, DateTimeOffset claudeLastUpdateUtc,
        DateTimeOffset codexLastUpdateUtc) : this(useClaude, useCodex, claudeLastUpdateUtc,
        codexLastUpdateUtc, DefaultRateLimitCooldownSeconds, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgsSettings" /> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    /// <param name="claudeLastUpdateUtc">
    ///     UTC timestamp of the last successful Claude Code update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </param>
    /// <param name="codexLastUpdateUtc">
    ///     UTC timestamp of the last successful Codex update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </param>
    /// <param name="rateLimitDefaultCooldown">
    ///     Cooldown period in seconds applied when the provider response does not include a reset
    ///     time. Defaults to <see cref="DefaultRateLimitCooldownSeconds" />.
    /// </param>
    /// <param name="providerCooldowns">
    ///     Map of provider ID to cooldown expiry timestamp. Pass <see langword="null" /> for an
    ///     empty map.
    /// </param>
    internal AgsSettings(bool useClaude, bool useCodex, DateTimeOffset claudeLastUpdateUtc,
        DateTimeOffset codexLastUpdateUtc, int rateLimitDefaultCooldown,
        IReadOnlyDictionary<string, DateTimeOffset> providerCooldowns)
    {
        UseClaude = useClaude;
        UseCodex = useCodex;
        ClaudeLastUpdateUtc = NormalizeTimestamp(claudeLastUpdateUtc);
        CodexLastUpdateUtc = NormalizeTimestamp(codexLastUpdateUtc);
        RateLimitDefaultCooldown = rateLimitDefaultCooldown > 0
            ? rateLimitDefaultCooldown
            : DefaultRateLimitCooldownSeconds;
        ProviderCooldowns = providerCooldowns ?? new Dictionary<string, DateTimeOffset>();
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
    ///     Gets the UTC timestamp of the last successful Claude Code update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </summary>
    internal DateTimeOffset ClaudeLastUpdateUtc { get; }

    /// <summary>
    ///     Gets the UTC timestamp of the last successful Codex update, or
    ///     <see cref="DateTimeOffset.MinValue" /> when no update has been recorded.
    /// </summary>
    internal DateTimeOffset CodexLastUpdateUtc { get; }

    /// <summary>
    ///     Gets a value indicating whether a successful Claude Code update timestamp is stored.
    /// </summary>
    internal bool HasClaudeLastUpdateUtc => ClaudeLastUpdateUtc != DateTimeOffset.MinValue;

    /// <summary>
    ///     Gets a value indicating whether a successful Codex update timestamp is stored.
    /// </summary>
    internal bool HasCodexLastUpdateUtc => CodexLastUpdateUtc != DateTimeOffset.MinValue;

    /// <summary>
    ///     Gets a value indicating whether both integrations are disabled.
    /// </summary>
    internal bool AreAllModelsDisabled => !UseClaude && !UseCodex;

    /// <summary>
    ///     Gets the cooldown period in seconds applied when the provider response does not include
    ///     a reset time. Defaults to <see cref="DefaultRateLimitCooldownSeconds" />.
    /// </summary>
    internal int RateLimitDefaultCooldown { get; }

    /// <summary>
    ///     Gets the map of provider IDs to their cooldown expiry timestamps.
    /// </summary>
    internal IReadOnlyDictionary<string, DateTimeOffset> ProviderCooldowns { get; }

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
            [ClaudeLastUpdateUtcSettingName] = HasClaudeLastUpdateUtc
                ? ClaudeLastUpdateUtc.ToString("O", CultureInfo.InvariantCulture)
                : null,
            [CodexLastUpdateUtcSettingName] = HasCodexLastUpdateUtc
                ? CodexLastUpdateUtc.ToString("O", CultureInfo.InvariantCulture)
                : null,
            [RateLimitDefaultCooldownSettingName] = RateLimitDefaultCooldown,
            [ProviderCooldownsSettingName] = activeCooldowns
        }, JsonOptions);
        File.WriteAllText(configPath, serializedSettings);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated provider cooldowns map.
    ///     Expired entries are removed automatically.
    /// </summary>
    /// <param name="providerCooldowns">New provider cooldowns map.</param>
    /// <returns>A new settings instance with the updated cooldowns.</returns>
    internal AgsSettings WithProviderCooldowns(IReadOnlyDictionary<string, DateTimeOffset> providerCooldowns)
    {
        return new AgsSettings(UseClaude, UseCodex, ClaudeLastUpdateUtc, CodexLastUpdateUtc,
            RateLimitDefaultCooldown, providerCooldowns);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Claude Code timestamp.
    /// </summary>
    /// <param name="claudeLastUpdateUtc">UTC timestamp of the last successful Claude update.</param>
    /// <returns>A new settings instance with the updated Claude timestamp.</returns>
    internal AgsSettings WithClaudeLastUpdateUtc(DateTimeOffset claudeLastUpdateUtc)
    {
        return new AgsSettings(UseClaude, UseCodex, claudeLastUpdateUtc, CodexLastUpdateUtc);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Claude Code enabled flag.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration should be enabled.</param>
    /// <returns>A new settings instance with the updated Claude enabled flag.</returns>
    internal AgsSettings WithUseClaude(bool useClaude)
    {
        return new AgsSettings(useClaude, UseCodex, ClaudeLastUpdateUtc, CodexLastUpdateUtc);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Codex timestamp.
    /// </summary>
    /// <param name="codexLastUpdateUtc">UTC timestamp of the last successful Codex update.</param>
    /// <returns>A new settings instance with the updated Codex timestamp.</returns>
    internal AgsSettings WithCodexLastUpdateUtc(DateTimeOffset codexLastUpdateUtc)
    {
        return new AgsSettings(UseClaude, UseCodex, ClaudeLastUpdateUtc, codexLastUpdateUtc);
    }

    /// <summary>
    ///     Creates a copy of the current settings with an updated Codex enabled flag.
    /// </summary>
    /// <param name="useCodex">Whether Codex integration should be enabled.</param>
    /// <returns>A new settings instance with the updated Codex enabled flag.</returns>
    internal AgsSettings WithUseCodex(bool useCodex)
    {
        return new AgsSettings(UseClaude, useCodex, ClaudeLastUpdateUtc, CodexLastUpdateUtc);
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
            var claudeLastUpdateUtc =
                TryReadOptionalTimestamp(rootElement, ClaudeLastUpdateUtcSettingName);
            var codexLastUpdateUtc =
                TryReadOptionalTimestamp(rootElement, CodexLastUpdateUtcSettingName);
            var rateLimitDefaultCooldown = TryReadOptionalInt(
                rootElement, RateLimitDefaultCooldownSettingName, DefaultRateLimitCooldownSeconds);
            var providerCooldowns = TryReadProviderCooldowns(rootElement);
            settings = new AgsSettings(useClaude, useCodex, claudeLastUpdateUtc, codexLastUpdateUtc,
                rateLimitDefaultCooldown, providerCooldowns);
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
    ///     Attempts to read an optional UTC timestamp property from a JSON object.
    /// </summary>
    /// <param name="configElement">JSON object that contains persisted settings.</param>
    /// <param name="propertyName">Property name to read.</param>
    /// <returns>
    ///     Parsed UTC timestamp when a valid value is present; otherwise,
    ///     <see cref="DateTimeOffset.MinValue" />.
    /// </returns>
    private static DateTimeOffset TryReadOptionalTimestamp(JsonElement configElement,
        string propertyName)
    {
        if (!configElement.TryGetProperty(propertyName, out var propertyElement))
            return DateTimeOffset.MinValue;
        if (propertyElement.ValueKind == JsonValueKind.Null) return DateTimeOffset.MinValue;
        if (propertyElement.ValueKind != JsonValueKind.String) return DateTimeOffset.MinValue;
        var rawTimestamp = propertyElement.GetString();
        if (string.IsNullOrWhiteSpace(rawTimestamp)) return DateTimeOffset.MinValue;
        return TryParseTimestamp(rawTimestamp);
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
            var claudeLastUpdateUtc = DateTimeOffset.MinValue;
            var codexLastUpdateUtc = DateTimeOffset.MinValue;
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
                    continue;
                }
                if (key == ClaudeLastUpdateUtcSettingName)
                {
                    claudeLastUpdateUtc = TryParseTimestamp(value);
                    continue;
                }
                if (key == CodexLastUpdateUtcSettingName)
                    codexLastUpdateUtc = TryParseTimestamp(value);
            }
            if (!hasUseClaude || !hasUseCodex) return false;
            settings = new AgsSettings(useClaude, useCodex, claudeLastUpdateUtc, codexLastUpdateUtc);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Normalizes a persisted timestamp to UTC.
    /// </summary>
    /// <param name="timestamp">Timestamp to normalize.</param>
    /// <returns>
    ///     <paramref name="timestamp" /> converted to UTC, or
    ///     <see cref="DateTimeOffset.MinValue" /> when the value is not set.
    /// </returns>
    private static DateTimeOffset NormalizeTimestamp(DateTimeOffset timestamp)
    {
        if (timestamp == DateTimeOffset.MinValue) return DateTimeOffset.MinValue;
        return timestamp.ToUniversalTime();
    }

    /// <summary>
    ///     Parses a persisted timestamp string into a UTC value.
    /// </summary>
    /// <param name="rawTimestamp">Timestamp string read from configuration.</param>
    /// <returns>
    ///     Parsed UTC timestamp when the input is valid; otherwise,
    ///     <see cref="DateTimeOffset.MinValue" />.
    /// </returns>
    private static DateTimeOffset TryParseTimestamp(string rawTimestamp)
    {
        if (!DateTimeOffset.TryParse(rawTimestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsedTimestamp))
            return DateTimeOffset.MinValue;
        return NormalizeTimestamp(parsedTimestamp);
    }
}
