using System.Diagnostics;
using AGS.ai;

namespace AGS.subsystems;

/// <summary>
///     Checks all external dependencies required by AGS: AI providers and supporting tools.
/// </summary>
internal static class DependencyCheckSubsystem
{
    private const string JqExecutable = "jq";
    private const int CheckTimeoutMs = 10_000;

    /// <summary>
    ///     Checks each dependency, prints its status, and blocks startup when no AI provider is
    ///     available.
    /// </summary>
    /// <returns>
    ///     A <see cref="DependencyCheckResult" /> containing availability flags for each
    ///     dependency. When no AI provider is available the method returns
    ///     <see langword="null" /> to signal that the application should exit.
    /// </returns>
    internal static DependencyCheckResult Run()
    {
        var anyProviderAvailable = ProviderCheckSubsystem.Run();
        if (!anyProviderAvailable) return null;

        var jqAvailable = CheckJq();
        return new DependencyCheckResult(jqAvailable);
    }

    /// <summary>
    ///     Checks whether <c>jq</c> is installed and prints its version or a fallback notice.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> when <c>jq</c> is installed; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool CheckJq()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = JqExecutable,
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            CliProcessStartInfoResolver.PrepareForExecution(startInfo);
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                PrintJqNotInstalled();
                return false;
            }
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(CheckTimeoutMs);
            if (process.ExitCode != 0)
            {
                PrintJqNotInstalled();
                return false;
            }
            Console.WriteLine($"jq: {output}");
            return true;
        }
        catch
        {
            PrintJqNotInstalled();
            return false;
        }
    }

    /// <summary>
    ///     Writes the jq fallback notice to the console.
    /// </summary>
    private static void PrintJqNotInstalled()
    {
        Console.WriteLine("jq: not installed (regex fallback will be used)");
    }
}
