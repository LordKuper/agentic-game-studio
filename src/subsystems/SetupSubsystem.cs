namespace AGS.subsystems;

/// <summary>
///     Handles interactive initialization for the local <c>.ags</c> configuration.
/// </summary>
internal static class SetupSubsystem
{
    private static readonly string[] DefaultModels =
    [
        "chatgpt",
        "claude-sonnet"
    ];

    /// <summary>
    ///     Builds the default model priority list for a new project configuration.
    /// </summary>
    /// <param name="useClaude">Whether Claude Code is enabled.</param>
    /// <param name="useCodex">Whether Codex is enabled.</param>
    /// <returns>Priority-ordered default model list for the initial configuration.</returns>
    private static IReadOnlyList<string> BuildDefaultModels(bool useClaude, bool useCodex)
    {
        var models = new List<string>();
        foreach (var model in DefaultModels)
        {
            switch (model)
            {
                case "chatgpt" when useCodex:
                case "claude-sonnet" when useClaude:
                    models.Add(model);
                    break;
            }
        }
        return models;
    }

    /// <summary>
    ///     Runs interactive setup and writes the generated configuration file.
    /// </summary>
    /// <param name="agsDirectoryPath">Absolute path to the <c>.ags</c> directory.</param>
    /// <param name="settings">
    ///     Settings selected during setup when setup succeeds; otherwise disabled defaults.
    /// </param>
    internal static void Run(string agsDirectoryPath, out AgsSettings settings)
    {
        settings = new AgsSettings(false, false);
        Console.WriteLine("Setup required. Starting setup...");
        var useClaude = AgsPrompt.Confirm("Do you want to use Claude Code?");
        var useCodex = AgsPrompt.Confirm("Do you want to use Codex?");
        settings = new AgsSettings(useClaude, useCodex, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, BuildDefaultModels(useClaude, useCodex));
        Directory.CreateDirectory(agsDirectoryPath);
        var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
        settings.WriteToConfig(configPath);
        Console.WriteLine($"Configuration saved: {configPath}");
    }
}