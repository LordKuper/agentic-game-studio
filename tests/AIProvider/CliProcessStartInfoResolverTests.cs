using System.Diagnostics;
using AGS.ai;

namespace AGS.Tests;

/// <summary>
///     Covers Windows command resolution for CLI tools that are installed as command shims.
/// </summary>
public sealed class CliProcessStartInfoResolverTests
{
    /// <summary>
    ///     Verifies that PATHEXT-based command resolution finds a <c>.cmd</c> shim in a searched
    ///     directory.
    /// </summary>
    [Fact]
    public void ResolveWindowsCommandPathFindsCmdShimInSearchDirectory()
    {
        using var workingDirectory = new TemporaryDirectoryScope();
        using var searchDirectory = new TemporaryDirectoryScope();
        var commandPath = CreateCommandShim(searchDirectory.Path, "codex.cmd", "@echo off");

        var resolvedPath = CliProcessStartInfoResolver.ResolveWindowsCommandPath("codex",
            workingDirectory.Path, [searchDirectory.Path], [".EXE", ".CMD"]);

        Assert.NotEmpty(resolvedPath);
        Assert.Equal(commandPath, resolvedPath, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Verifies that the real adapter can execute <c>codex.cmd</c> when it is available on the
    ///     Windows PATH.
    /// </summary>
    [Fact]
    public void TryGetVersionReturnsTrueWhenCodexCmdShimIsOnWindowsPath()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var shimDirectory = new TemporaryDirectoryScope();
        CreateCommandShim(shimDirectory.Path, "codex.cmd", "@echo off\r\necho codex 1.2.3");

        var originalPath = Environment.GetEnvironmentVariable("PATH");
        var originalPathExtensions = Environment.GetEnvironmentVariable("PATHEXT");

        try
        {
            Environment.SetEnvironmentVariable("PATH",
                PrependPathEntry(shimDirectory.Path, originalPath));
            Environment.SetEnvironmentVariable("PATHEXT", ".COM;.EXE;.BAT;.CMD");

            var adapter = new CodexAdapter();
            var isAvailable = adapter.TryGetVersion(out var version);

            Assert.True(isAvailable);
            Assert.Equal("codex 1.2.3", version);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Environment.SetEnvironmentVariable("PATHEXT", originalPathExtensions);
        }
    }

    /// <summary>
    ///     Creates a command shim file in the supplied directory.
    /// </summary>
    /// <param name="directoryPath">Directory that should receive the shim.</param>
    /// <param name="fileName">Shim file name.</param>
    /// <param name="contents">Shim file contents.</param>
    /// <returns>The absolute path to the created shim file.</returns>
    private static string CreateCommandShim(string directoryPath, string fileName,
        string contents)
    {
        var commandPath = Path.Combine(directoryPath, fileName);
        File.WriteAllText(commandPath, contents);
        return commandPath;
    }

    /// <summary>
    ///     Prepends a single directory to a PATH-like environment variable value.
    /// </summary>
    /// <param name="pathEntry">Directory that should be prepended.</param>
    /// <param name="existingValue">Existing PATH-like value.</param>
    /// <returns>The combined PATH-like string.</returns>
    private static string PrependPathEntry(string pathEntry, string existingValue)
    {
        return string.IsNullOrWhiteSpace(existingValue)
            ? pathEntry
            : pathEntry + Path.PathSeparator + existingValue;
    }
}
