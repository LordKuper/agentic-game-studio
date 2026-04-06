using AGS.ai;

namespace AGS.subsystems;

/// <summary>
///     Checks whether supported AI providers are installed and prints their versions.
/// </summary>
internal static class ProviderCheckSubsystem
{
    private static Func<IReadOnlyList<IAIProvider>> providersFactory = GetDefaultProviders;

    /// <summary>
    ///     Returns the default set of supported providers.
    /// </summary>
    private static IReadOnlyList<IAIProvider> GetDefaultProviders()
    {
        return [new ClaudeCodeAdapter(), new CodexAdapter()];
    }

    /// <summary>
    ///     Checks each supported provider, prints its installed version or a "not installed"
    ///     message, and blocks startup when no provider is available.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> when at least one provider is installed;
    ///     <see langword="false" /> when no provider is available and the application should exit.
    /// </returns>
    internal static bool Run()
    {
        var providers = providersFactory();
        var anyAvailable = false;
        foreach (var provider in providers)
        {
            var available = PrintProviderStatus(GetDisplayName(provider.ProviderId), provider);
            anyAvailable = anyAvailable || available;
        }
        if (anyAvailable) return true;
        Console.WriteLine(
            "No supported AI provider is installed. Please install Claude Code or Codex and restart the application.");
        Console.ReadKey(true);
        return false;
    }

    /// <summary>
    ///     Returns the human-readable display name for a provider ID.
    /// </summary>
    private static string GetDisplayName(string providerId) => providerId switch
    {
        ClaudeCodeAdapter.Id => "Claude Code",
        CodexAdapter.Id => "Codex",
        _ => providerId
    };

    /// <summary>
    ///     Attempts to retrieve the provider version and writes the result to the console.
    /// </summary>
    /// <param name="displayName">Human-readable provider name shown in the output.</param>
    /// <param name="provider">Provider to check.</param>
    /// <returns>
    ///     <see langword="true" /> when the provider is installed; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool PrintProviderStatus(string displayName, IAIProvider provider)
    {
        if (provider.TryGetVersion(out var version))
        {
            Console.WriteLine($"{displayName}: {version}");
            return true;
        }
        Console.WriteLine($"{displayName}: not installed");
        return false;
    }
}
