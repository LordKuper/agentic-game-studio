namespace AGS.ai;

/// <summary>
///     Defines the contract for invoking an AI agent through any provider.
/// </summary>
internal interface IAIProvider
{
    /// <summary>
    ///     Gets the unique identifier for this provider.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    ///     Gets a value indicating whether this provider is currently installed and available.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    ///     Attempts to retrieve the installed version string for this provider.
    /// </summary>
    /// <param name="version">
    ///     The version string reported by the provider CLI when the call succeeds;
    ///     otherwise, an empty string.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the provider is installed and returned a version;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetVersion(out string version);

    /// <summary>
    ///     Invokes an AI agent with the given prompt and returns the result.
    /// </summary>
    /// <param name="request">Request containing prompts, working directory, and timeout.</param>
    /// <returns>The result of the invocation.</returns>
    AIProviderResult Invoke(AIProviderRequest request);

    /// <summary>
    ///     Gets the absolute path to the directory where AGS skills should be placed for this
    ///     provider to discover them natively.
    /// </summary>
    /// <param name="projectRootPath">Absolute path to the game project root.</param>
    /// <returns>Absolute path to the provider's native skill directory.</returns>
    string GetSkillDirectory(string projectRootPath);
}
