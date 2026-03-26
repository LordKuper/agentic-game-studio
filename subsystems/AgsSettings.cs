/// <summary>
/// Represents persisted configuration flags for the local <c>.ags/config</c> file.
/// </summary>
internal readonly struct AgsSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgsSettings"/> struct.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code integration is enabled.</param>
    /// <param name="useCodex">Whether Codex integration is enabled.</param>
    internal AgsSettings(bool useClaude, bool useCodex)
    {
        UseClaude = useClaude;
        UseCodex = useCodex;
    }

    /// <summary>
    /// Gets a value indicating whether Claude Code integration is enabled.
    /// </summary>
    internal bool UseClaude { get; }

    /// <summary>
    /// Gets a value indicating whether Codex integration is enabled.
    /// </summary>
    internal bool UseCodex { get; }

    /// <summary>
    /// Gets a value indicating whether both integrations are disabled.
    /// </summary>
    internal bool AreAllDisabled => !UseClaude && !UseCodex;

    /// <summary>
    /// Attempts to read settings from an existing configuration file.
    /// </summary>
    /// <param name="configPath">Absolute path to the configuration file.</param>
    /// <param name="settings">Parsed settings when read succeeds; otherwise default values.</param>
    /// <returns><see langword="true"/> when both required settings are successfully parsed; otherwise <see langword="false"/>.</returns>
    internal static bool TryReadFromConfig(string configPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);

        if (!File.Exists(configPath))
        {
            return false;
        }

        var lines = File.ReadAllLines(configPath);
        var hasUseClaude = false;
        var hasUseCodex = false;
        var useClaude = false;
        var useCodex = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Length == 0)
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex == trimmedLine.Length - 1)
            {
                continue;
            }

            var key = trimmedLine.Substring(0, separatorIndex).Trim();
            var value = trimmedLine.Substring(separatorIndex + 1).Trim();

            if (key == "use-claude" && bool.TryParse(value, out var parsedUseClaude))
            {
                useClaude = parsedUseClaude;
                hasUseClaude = true;
                continue;
            }

            if (key == "use-codex" && bool.TryParse(value, out var parsedUseCodex))
            {
                useCodex = parsedUseCodex;
                hasUseCodex = true;
            }
        }

        if (!hasUseClaude || !hasUseCodex)
        {
            return false;
        }

        settings = new AgsSettings(useClaude, useCodex);
        return true;
    }
}
