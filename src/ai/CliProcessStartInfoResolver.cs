using System.Diagnostics;

namespace AGS.ai;

/// <summary>
///     Resolves CLI command names to concrete Windows paths before a subprocess is started.
/// </summary>
internal static class CliProcessStartInfoResolver
{
    private static readonly string[] DefaultWindowsExecutableExtensions =
        [".com", ".exe", ".bat", ".cmd"];

    /// <summary>
    ///     Resolves the executable path for the supplied start info when running on Windows.
    /// </summary>
    /// <param name="startInfo">Start info to normalize before process startup.</param>
    internal static void PrepareForExecution(ProcessStartInfo startInfo)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var resolvedPath = ResolveWindowsCommandPath(startInfo.FileName,
            startInfo.WorkingDirectory, GetPathDirectories(), GetExecutableExtensions());
        if (resolvedPath.Length > 0)
            startInfo.FileName = resolvedPath;
    }

    /// <summary>
    ///     Resolves a Windows command using the working directory, PATH, and PATHEXT values.
    /// </summary>
    /// <param name="fileName">
    ///     Command name or relative path supplied to <see cref="ProcessStartInfo" />.
    /// </param>
    /// <param name="workingDirectory">Working directory used for relative path resolution.</param>
    /// <param name="searchDirectories">Directories searched after the working directory.</param>
    /// <param name="executableExtensions">
    ///     Executable extensions considered when the command has no extension.
    /// </param>
    /// <returns>
    ///     The resolved absolute path when a matching file exists; otherwise an empty string.
    /// </returns>
    internal static string ResolveWindowsCommandPath(string fileName, string workingDirectory,
        IReadOnlyList<string> searchDirectories, IReadOnlyList<string> executableExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var baseDirectory = string.IsNullOrWhiteSpace(workingDirectory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(workingDirectory);

        if (HasDirectoryComponents(fileName) || Path.IsPathRooted(fileName))
            return ResolveCandidatePath(fileName, baseDirectory, executableExtensions);

        var resolvedFromWorkingDirectory =
            ResolveCandidatePath(fileName, baseDirectory, executableExtensions);
        if (resolvedFromWorkingDirectory.Length > 0)
            return resolvedFromWorkingDirectory;

        foreach (var searchDirectory in searchDirectories)
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                continue;

            var resolvedPath = ResolveCandidatePath(fileName, searchDirectory,
                executableExtensions);
            if (resolvedPath.Length > 0)
                return resolvedPath;
        }

        return string.Empty;
    }

    /// <summary>
    ///     Reads the current PATH directories from the process environment.
    /// </summary>
    /// <returns>Normalized PATH directory entries.</returns>
    private static IReadOnlyList<string> GetPathDirectories()
    {
        var rawPath = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(rawPath))
            return [];

        return rawPath
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>
    ///     Reads the current PATHEXT values from the process environment.
    /// </summary>
    /// <returns>Executable extensions used for Windows command lookup.</returns>
    private static IReadOnlyList<string> GetExecutableExtensions()
    {
        var rawPathExtensions = Environment.GetEnvironmentVariable("PATHEXT");
        if (string.IsNullOrWhiteSpace(rawPathExtensions))
            return DefaultWindowsExecutableExtensions;

        return rawPathExtensions
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeExecutableExtension)
            .ToList();
    }

    /// <summary>
    ///     Resolves a candidate path against a base directory and known executable extensions.
    /// </summary>
    /// <param name="fileName">Command name or relative path to resolve.</param>
    /// <param name="baseDirectory">Base directory used for relative paths.</param>
    /// <param name="executableExtensions">
    ///     Executable extensions considered when the file name has no extension.
    /// </param>
    /// <returns>The absolute path of the first existing match; otherwise an empty string.</returns>
    private static string ResolveCandidatePath(string fileName, string baseDirectory,
        IReadOnlyList<string> executableExtensions)
    {
        foreach (var candidatePath in EnumerateCandidatePaths(fileName, baseDirectory,
                     executableExtensions))
        {
            if (File.Exists(candidatePath))
                return Path.GetFullPath(candidatePath);
        }

        return string.Empty;
    }

    /// <summary>
    ///     Enumerates the absolute candidate paths that should be checked for a command.
    /// </summary>
    /// <param name="fileName">Command name or relative path to resolve.</param>
    /// <param name="baseDirectory">Base directory used for relative paths.</param>
    /// <param name="executableExtensions">
    ///     Executable extensions considered when the file name has no extension.
    /// </param>
    /// <returns>Absolute candidate paths in resolution order.</returns>
    private static IEnumerable<string> EnumerateCandidatePaths(string fileName,
        string baseDirectory, IReadOnlyList<string> executableExtensions)
    {
        var absolutePath = Path.GetFullPath(fileName, baseDirectory);
        var extension = Path.GetExtension(absolutePath);
        if (extension.Length > 0)
        {
            yield return absolutePath;
            yield break;
        }

        foreach (var executableExtension in executableExtensions)
            yield return absolutePath + executableExtension;

        yield return absolutePath;
    }

    /// <summary>
    ///     Normalizes a PATHEXT entry to the dotted form used by
    ///     <see cref="Path.GetExtension(string)" />.
    /// </summary>
    /// <param name="executableExtension">PATHEXT entry to normalize.</param>
    /// <returns>The normalized extension string.</returns>
    private static string NormalizeExecutableExtension(string executableExtension)
    {
        return executableExtension.StartsWith('.')
            ? executableExtension
            : "." + executableExtension;
    }

    /// <summary>
    ///     Determines whether the supplied command name already contains a directory component.
    /// </summary>
    /// <param name="fileName">Command name to inspect.</param>
    /// <returns>
    ///     <see langword="true" /> when the command contains a directory separator.
    /// </returns>
    private static bool HasDirectoryComponents(string fileName)
    {
        return fileName.Contains(Path.DirectorySeparatorChar) ||
               fileName.Contains(Path.AltDirectorySeparatorChar);
    }
}
