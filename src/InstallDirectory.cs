namespace AGS;

/// <summary>
///     Resolves the AGS install directory where the binary and standard resources are located.
/// </summary>
internal static class InstallDirectory
{
    internal const string AgentsDirectoryName = "agents";
    internal const string RulesDirectoryName = "rules";
    internal const string SkillsDirectoryName = "skills";
    internal const string TemplatesDirectoryName = "templates";
    private static Func<string> _baseDirectoryProvider = () => AppContext.BaseDirectory;

    /// <summary>
    ///     Gets the absolute path to the AGS install directory (where <c>ags.exe</c> and standard
    ///     resources are located).
    /// </summary>
    /// <returns>Absolute path to the install directory.</returns>
    internal static string GetInstallPath()
    {
        return _baseDirectoryProvider();
    }

    /// <summary>
    ///     Gets the absolute path to a specific standard resource file within the install directory.
    /// </summary>
    /// <param name="resourceDirectoryName">
    ///     Name of the resource directory (e.g. <c>agents</c>, <c>rules</c>).
    /// </param>
    /// <param name="resourceFileName">Name of the resource file within the directory.</param>
    /// <returns>Absolute path to the standard resource file.</returns>
    internal static string GetStandardResourceFilePath(string resourceDirectoryName,
        string resourceFileName)
    {
        return Path.Combine(_baseDirectoryProvider(), resourceDirectoryName, resourceFileName);
    }

    /// <summary>
    ///     Gets the absolute path to a standard resource directory within the install directory.
    /// </summary>
    /// <param name="resourceDirectoryName">
    ///     Name of the resource directory (e.g. <c>agents</c>, <c>rules</c>, <c>skills</c>,
    ///     <c>templates</c>).
    /// </param>
    /// <returns>Absolute path to the standard resource directory.</returns>
    internal static string GetStandardResourcePath(string resourceDirectoryName)
    {
        return Path.Combine(_baseDirectoryProvider(), resourceDirectoryName);
    }

    /// <summary>
    ///     Resets the base directory provider to the default (<see cref="AppContext.BaseDirectory" />).
    /// </summary>
    internal static void ResetBaseDirectoryProvider()
    {
        _baseDirectoryProvider = () => AppContext.BaseDirectory;
    }

    /// <summary>
    ///     Overrides the base directory provider for testing purposes.
    /// </summary>
    /// <param name="provider">Custom base directory provider.</param>
    internal static void SetBaseDirectoryProvider(Func<string> provider)
    {
        _baseDirectoryProvider = provider;
    }

    /// <summary>
    ///     Checks whether the standard resource directory exists at the install location.
    /// </summary>
    /// <param name="resourceDirectoryName">
    ///     Name of the resource directory to check.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the directory exists; otherwise, <see langword="false" />.
    /// </returns>
    internal static bool StandardResourceDirectoryExists(string resourceDirectoryName)
    {
        return Directory.Exists(GetStandardResourcePath(resourceDirectoryName));
    }
}