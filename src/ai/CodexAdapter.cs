using System.Diagnostics;
using System.Text;

namespace AGS.ai;

/// <summary>
///     Invokes the Codex CLI as an AI provider subprocess.
/// </summary>
internal sealed class CodexAdapter : IAIProvider
{
    /// <summary>
    ///     Provider ID for Codex.
    /// </summary>
    internal const string Id = "codex";

    private const string CodexExecutable = "codex";
    private const int AvailabilityCheckTimeoutMs = 10_000;
    private const int DefaultInvocationTimeoutMs = 600_000;

    private readonly Func<ProcessStartInfo, (int ExitCode, string StandardOutput,
        string StandardError)> processRunner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodexAdapter" /> class using the default
    ///     process runner.
    /// </summary>
    internal CodexAdapter() : this(null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodexAdapter" /> class.
    /// </summary>
    /// <param name="processRunner">
    ///     Process runner delegate used for all subprocess invocations. When <see langword="null" />,
    ///     the default runner that spawns a real OS process is used.
    /// </param>
    internal CodexAdapter(
        Func<ProcessStartInfo, (int ExitCode, string StandardOutput, string StandardError)>
            processRunner)
    {
        this.processRunner = processRunner;
    }

    /// <inheritdoc />
    public string ProviderId => Id;

    /// <inheritdoc />
    public bool IsAvailable => TryGetVersion(out _);

    /// <inheritdoc />
    public bool TryGetVersion(out string version)
    {
        version = string.Empty;
        try
        {
            var startInfo = BuildStartInfo(CodexExecutable, "--version", null);
            var (exitCode, output, _) = RunProcess(startInfo, AvailabilityCheckTimeoutMs);
            if (exitCode != 0) return false;
            version = output.Trim();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public AIProviderResult Invoke(AIProviderRequest request)
    {
        var timeoutMs = request.Timeout > TimeSpan.Zero
            ? (int)Math.Min(request.Timeout.TotalMilliseconds, int.MaxValue)
            : DefaultInvocationTimeoutMs;

        var arguments = BuildInvocationArguments(request);
        var startInfo = BuildStartInfo(CodexExecutable, arguments, request.WorkingDirectory);

        try
        {
            var (exitCode, output, error) = RunProcess(startInfo, timeoutMs);
            if (exitCode != 0)
            {
                var errorMessage = error.Length > 0
                    ? error
                    : $"Codex exited with code {exitCode}.";
                var (isRateLimited, resetsAt) = DetectRateLimit(output, error);
                if (isRateLimited)
                    return AIProviderResult.RateLimited(errorMessage, exitCode, resetsAt, output);
                return AIProviderResult.Failed(errorMessage, exitCode, output);
            }

            var modifiedFiles = DetectModifiedFiles(request.WorkingDirectory);
            return AIProviderResult.Succeeded(output, exitCode, modifiedFiles);
        }
        catch (Exception exception)
        {
            return AIProviderResult.Failed(exception.Message, -1);
        }
    }

    /// <summary>
    ///     Detects whether the CLI output signals a rate-limit or quota-exhaustion error and
    ///     attempts to parse the reset time.
    /// </summary>
    internal static (bool IsRateLimited, DateTimeOffset? ResetsAt) DetectRateLimit(
        string output, string error)
    {
        var combined = (output + "\n" + error).ToLowerInvariant();
        if (!combined.Contains("rate limit") && !combined.Contains("ratelimit") &&
            !combined.Contains("rate_limit") && !combined.Contains("quota exceeded") &&
            !combined.Contains("quota_exceeded") && !combined.Contains("too many requests") &&
            !combined.Contains("429"))
            return (false, null);
        var resetsAt = TryParseResetTime(output + "\n" + error);
        return (true, resetsAt);
    }

    /// <summary>
    ///     Attempts to parse a reset timestamp from provider error output.
    ///     Looks for "retry after N seconds", "retry after HH:MM:SS", or an ISO 8601 timestamp
    ///     following "retry" or "reset" keywords.
    /// </summary>
    private static DateTimeOffset? TryParseResetTime(string text)
    {
        // "retry after N seconds" / "retry-after: N"
        var retryAfterPattern = System.Text.RegularExpressions.Regex.Match(
            text, @"retry.after[:\s]+(\d+)\s*s", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (retryAfterPattern.Success && int.TryParse(retryAfterPattern.Groups[1].Value, out var seconds))
            return DateTimeOffset.UtcNow.AddSeconds(seconds);

        // ISO 8601 timestamp after "reset" or "available" or "retry"
        var isoPattern = System.Text.RegularExpressions.Regex.Match(
            text, @"(?:reset|available|retry)[^\d]*(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[^\s]*)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (isoPattern.Success && DateTimeOffset.TryParse(isoPattern.Groups[1].Value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            return parsed.ToUniversalTime();

        return null;
    }

    /// <summary>
    ///     Builds the CLI argument string for a Codex invocation.
    /// </summary>
    private static string BuildInvocationArguments(AIProviderRequest request)
    {
        var builder = new StringBuilder();
        builder.Append("--approval-mode full-auto ");
        builder.Append(QuoteArgument(request.TaskPrompt));
        if (request.SystemPrompt.Length > 0)
        {
            builder.Append(" --system-prompt ");
            builder.Append(QuoteArgument(request.SystemPrompt));
        }
        foreach (var (key, value) in request.ProviderArguments)
        {
            builder.Append(' ');
            builder.Append(key);
            if (value.Length > 0)
            {
                builder.Append(' ');
                builder.Append(QuoteArgument(value));
            }
        }
        return builder.ToString();
    }

    /// <summary>
    ///     Builds a <see cref="ProcessStartInfo" /> for a subprocess invocation.
    /// </summary>
    private static ProcessStartInfo BuildStartInfo(string fileName, string arguments,
        string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        if (!string.IsNullOrEmpty(workingDirectory))
            startInfo.WorkingDirectory = workingDirectory;
        return startInfo;
    }

    /// <summary>
    ///     Detects files modified since the last commit in the given working directory.
    /// </summary>
    private IReadOnlyList<string> DetectModifiedFiles(string workingDirectory)
    {
        if (string.IsNullOrEmpty(workingDirectory)) return [];
        try
        {
            var startInfo = BuildStartInfo("git", "diff --name-only HEAD", workingDirectory);
            var (exitCode, output, _) = RunProcess(startInfo, AvailabilityCheckTimeoutMs);
            if (exitCode != 0) return [];
            return output
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Runs a subprocess using the injected or default process runner.
    /// </summary>
    private (int ExitCode, string StandardOutput, string StandardError) RunProcess(
        ProcessStartInfo startInfo, int timeoutMs)
    {
        if (processRunner != null) return processRunner(startInfo);
        return DefaultRunProcess(startInfo, timeoutMs);
    }

    /// <summary>
    ///     Default process runner that spawns a real OS process.
    /// </summary>
    private static (int ExitCode, string StandardOutput, string StandardError) DefaultRunProcess(
        ProcessStartInfo startInfo, int timeoutMs)
    {
        CliProcessStartInfoResolver.PrepareForExecution(startInfo);
        using var process = new Process();
        process.StartInfo = startInfo;
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        process.Start();
        var outputTask = Task.Run(() =>
        {
            while (!process.StandardOutput.EndOfStream)
                outputBuilder.AppendLine(process.StandardOutput.ReadLine());
        });
        var errorTask = Task.Run(() =>
        {
            while (!process.StandardError.EndOfStream)
                errorBuilder.AppendLine(process.StandardError.ReadLine());
        });
        var exited = process.WaitForExit(timeoutMs);
        if (!exited)
        {
            try { process.Kill(true); } catch { }
            return (-1, outputBuilder.ToString(), $"Process timed out after {timeoutMs} ms.");
        }
        outputTask.Wait();
        errorTask.Wait();
        return (process.ExitCode, outputBuilder.ToString().TrimEnd(),
            errorBuilder.ToString().TrimEnd());
    }

    /// <summary>
    ///     Wraps a value in double quotes for use as a CLI argument, escaping embedded quotes.
    /// </summary>
    private static string QuoteArgument(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
