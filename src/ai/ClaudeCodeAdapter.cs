using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AGS.ai;

/// <summary>
///     Invokes the Claude Code CLI as an AI provider subprocess.
/// </summary>
internal sealed class ClaudeCodeAdapter : IAIProvider
{
    /// <summary>
    ///     Provider ID for Claude Code.
    /// </summary>
    internal const string Id = "claude-code";

    private const string ClaudeExecutable = "claude";
    private const int AvailabilityCheckTimeoutMs = 10_000;
    private const int DefaultInvocationTimeoutMs = 600_000;

    private readonly Func<ProcessStartInfo, (int ExitCode, string StandardOutput,
        string StandardError)> processRunner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaudeCodeAdapter" /> class using the
    ///     default process runner.
    /// </summary>
    internal ClaudeCodeAdapter() : this(null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaudeCodeAdapter" /> class.
    /// </summary>
    /// <param name="processRunner">
    ///     Process runner delegate used for all subprocess invocations. When <see langword="null" />,
    ///     the default runner that spawns a real OS process is used.
    /// </param>
    internal ClaudeCodeAdapter(
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
            var startInfo = BuildStartInfo(ClaudeExecutable, "--version", null);
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
        var startInfo = BuildStartInfo(ClaudeExecutable, arguments, request.WorkingDirectory);

        try
        {
            var (exitCode, output, error) = RunProcess(startInfo, timeoutMs);
            if (exitCode != 0)
            {
                var errorMessage = error.Length > 0
                    ? error
                    : $"Claude Code exited with code {exitCode}.";
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
            !combined.Contains("429") && !combined.Contains("overloaded") &&
            !combined.Contains("hit your limit"))
            return (false, null);
        var resetsAt = TryParseResetTime(output + "\n" + error);
        return (true, resetsAt);
    }

    /// <summary>
    ///     Attempts to parse a reset timestamp from provider error output.
    ///     Looks for "retry after N seconds", an ISO 8601 timestamp following "retry" or
    ///     "reset" keywords, or a localized time such as "resets 2pm (Europe/Moscow)".
    /// </summary>
    private static DateTimeOffset? TryParseResetTime(string text)
    {
        var retryAfterSeconds = TryParseRetryAfterSeconds(text);
        if (retryAfterSeconds.HasValue) return retryAfterSeconds.Value;

        var absoluteResetTime = TryParseAbsoluteResetTime(text);
        if (absoluteResetTime.HasValue) return absoluteResetTime.Value;

        var localizedResetTime = TryParseLocalizedResetTime(text);
        if (localizedResetTime.HasValue) return localizedResetTime.Value;

        return null;
    }

    /// <summary>
    ///     Attempts to parse a relative "retry after N seconds" directive.
    /// </summary>
    private static DateTimeOffset? TryParseRetryAfterSeconds(string text)
    {
        var retryAfterPattern = Regex.Match(text, @"retry.after[:\s]+(\d+)\s*s",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (retryAfterPattern.Success &&
            int.TryParse(retryAfterPattern.Groups[1].Value, out var seconds))
            return DateTimeOffset.UtcNow.AddSeconds(seconds);

        return null;
    }

    /// <summary>
    ///     Attempts to parse an ISO 8601 reset timestamp after "reset", "available", or "retry".
    /// </summary>
    private static DateTimeOffset? TryParseAbsoluteResetTime(string text)
    {
        var isoPattern = Regex.Match(text,
            @"(?:reset|available|retry)[^\d]*(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[^\s]*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (isoPattern.Success && DateTimeOffset.TryParse(isoPattern.Groups[1].Value,
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
            return parsed.ToUniversalTime();

        return null;
    }

    /// <summary>
    ///     Attempts to parse a localized reset time such as "resets 2pm (Europe/Moscow)".
    /// </summary>
    private static DateTimeOffset? TryParseLocalizedResetTime(string text)
    {
        var resetPattern = Regex.Match(text,
            @"resets?\s+(?:at\s+)?(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?)(?:\s*\((?<timezone>[^)]+)\))?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!resetPattern.Success) return null;

        var clockText = resetPattern.Groups["time"].Value;
        var timeZoneId = resetPattern.Groups["timezone"].Success
            ? resetPattern.Groups["timezone"].Value.Trim()
            : string.Empty;

        if (!TryParseClockTime(clockText, out var clockTime)) return null;
        if (!TryResolveTimeZone(timeZoneId, out var timeZone)) return null;

        var currentTimeInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        var resetDateTime = currentTimeInZone.Date.Add(clockTime);
        if (resetDateTime <= currentTimeInZone.DateTime)
            resetDateTime = resetDateTime.AddDays(1);

        var offset = timeZone.GetUtcOffset(resetDateTime);
        return new DateTimeOffset(resetDateTime, offset).ToUniversalTime();
    }

    /// <summary>
    ///     Attempts to resolve the supplied time zone identifier.
    /// </summary>
    private static bool TryResolveTimeZone(string timeZoneId, out TimeZoneInfo timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            timeZone = TimeZoneInfo.Local;
            return true;
        }

        if (TryFindTimeZone(timeZoneId, out timeZone))
            return true;

        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsId) &&
            TryFindTimeZone(windowsId, out timeZone))
            return true;

        timeZone = TimeZoneInfo.Utc;
        return false;
    }

    /// <summary>
    ///     Attempts to find a system time zone by its identifier.
    /// </summary>
    private static bool TryFindTimeZone(string timeZoneId, out TimeZoneInfo timeZone)
    {
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
    }

    /// <summary>
    ///     Attempts to parse a 12-hour or 24-hour clock time.
    /// </summary>
    private static bool TryParseClockTime(string text, out TimeSpan clockTime)
    {
        var timePattern = Regex.Match(text.Trim(),
            @"^(?<hour>\d{1,2})(?::(?<minute>\d{2}))?\s*(?<period>am|pm)?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!timePattern.Success)
        {
            clockTime = TimeSpan.Zero;
            return false;
        }

        if (!int.TryParse(timePattern.Groups["hour"].Value, out var hour))
        {
            clockTime = TimeSpan.Zero;
            return false;
        }

        var minuteText = timePattern.Groups["minute"].Success
            ? timePattern.Groups["minute"].Value
            : "0";
        if (!int.TryParse(minuteText, out var minute) || minute < 0 || minute > 59)
        {
            clockTime = TimeSpan.Zero;
            return false;
        }

        var period = timePattern.Groups["period"].Value;
        if (period.Length > 0)
        {
            if (hour < 1 || hour > 12)
            {
                clockTime = TimeSpan.Zero;
                return false;
            }

            if (hour == 12) hour = 0;
            if (period.Equals("pm", StringComparison.OrdinalIgnoreCase))
                hour += 12;
        }
        else if (hour < 0 || hour > 23)
        {
            clockTime = TimeSpan.Zero;
            return false;
        }

        clockTime = new TimeSpan(hour, minute, 0);
        return true;
    }

    /// <summary>
    ///     Builds the CLI argument string for a Claude Code invocation.
    /// </summary>
    private static string BuildInvocationArguments(AIProviderRequest request)
    {
        var builder = new StringBuilder();
        builder.Append("--print ");
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
