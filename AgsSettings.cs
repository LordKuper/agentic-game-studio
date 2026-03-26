using System.Text.Json;

namespace AGS;

/// <summary>
///     Represents persisted configuration flags for the local <c>.ags/config.json</c> file.
/// </summary>
internal readonly struct AgsSettings
{
    internal const string AgsDirectoryName = ".ags";
    internal const string ConfigFileName = "config.json";

    private const string LegacyConfigFileName = "config";
    private const string UseClaudeSettingName = "use-claude";
    private const string UseCodexSettingName = "use-codex";
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
    {
        UseClaude = useClaude;
        UseCodex = useCodex;
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
            return TryReadFromJson(File.ReadAllText(configPath), out settings);
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
    ///     Attempts to migrate legacy plain-text settings from <c>.ags/config</c> to
    ///     <c>.ags/config.json</c>.
    /// </summary>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    /// <param name="settings">Migrated settings when the legacy file is parsed successfully.</param>
    /// <returns>
    ///     <see langword="true" /> when the legacy file is read and the JSON file is written
    ///     successfully; otherwise, <see langword="false" />.
    /// </returns>
    internal static bool TryMigrateLegacyConfig(string agsDirectoryPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        var legacyConfigPath = Path.Combine(agsDirectoryPath, LegacyConfigFileName);
        if (!TryReadFromLegacyConfig(legacyConfigPath, out settings)) return false;

        try
        {
            var configPath = Path.Combine(agsDirectoryPath, ConfigFileName);
            settings.WriteToConfig(configPath);
            File.Delete(legacyConfigPath);
            return true;
        }
        catch (IOException)
        {
            settings = new AgsSettings(false, false);
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            settings = new AgsSettings(false, false);
            return false;
        }
    }

    /// <summary>
    ///     Writes the current settings to the JSON configuration file.
    /// </summary>
    /// <param name="configPath">Absolute path to the configuration file.</param>
    internal void WriteToConfig(string configPath)
    {
        var serializedSettings = JsonSerializer.Serialize(
            new Dictionary<string, bool>
            {
                [UseClaudeSettingName] = UseClaude,
                [UseCodexSettingName] = UseCodex
            },
            JsonOptions);

        File.WriteAllText(configPath, serializedSettings);
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
            var configValues = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonContent);
            if (configValues == null) return false;
            if (!configValues.TryGetValue(UseClaudeSettingName, out var useClaude)) return false;
            if (!configValues.TryGetValue(UseCodexSettingName, out var useCodex)) return false;

            settings = new AgsSettings(useClaude, useCodex);
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
    ///     Attempts to read settings from the legacy plain-text configuration file.
    /// </summary>
    /// <param name="configPath">Absolute path to the legacy configuration file.</param>
    /// <param name="settings">Parsed settings when read succeeds; otherwise default values.</param>
    /// <returns>
    ///     <see langword="true" /> when both required settings are successfully parsed; otherwise
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryReadFromLegacyConfig(string configPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        if (!File.Exists(configPath)) return false;

        try
        {
            var lines = File.ReadAllLines(configPath);
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
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
