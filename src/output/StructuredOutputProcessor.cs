using System.Diagnostics;
using System.Text.RegularExpressions;
using AGS.ai;

namespace AGS.output;

/// <summary>
///     Extracts a meaningful <see cref="StructuredOutput" /> from raw AI provider output using
///     <c>jq</c> when available, or regex-based parsing as a fallback.
/// </summary>
internal sealed class StructuredOutputProcessor
{
    private const int JqTimeoutMs = 10_000;

    private readonly bool jqAvailable;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StructuredOutputProcessor" /> class.
    /// </summary>
    /// <param name="jqAvailable">
    ///     Whether the <c>jq</c> command-line tool is available for JSON extraction.
    ///     When <see langword="false" />, regex-based extraction is used instead.
    /// </param>
    internal StructuredOutputProcessor(bool jqAvailable)
    {
        this.jqAvailable = jqAvailable;
    }

    /// <summary>
    ///     Extracts a <see cref="StructuredOutput" /> from raw provider output.
    ///     For Codex the output conforms directly to the AI output schema; for Claude Code it is
    ///     wrapped in a JSON envelope with a <c>result</c> field.
    /// </summary>
    /// <param name="rawOutput">Raw stdout captured from the AI provider process.</param>
    /// <param name="providerId">Provider ID used to select the correct extraction strategy.</param>
    /// <returns>Extracted <see cref="StructuredOutput" /> with message and optional choices.</returns>
    internal StructuredOutput Process(string rawOutput, string providerId)
    {
        if (string.IsNullOrWhiteSpace(rawOutput))
            return new StructuredOutput(string.Empty, []);

        return IsClaudeCodeProvider(providerId)
            ? ProcessClaudeCodeOutput(rawOutput)
            : ProcessCodexOutput(rawOutput);
    }

    /// <summary>
    ///     Extracts the message and choices from Claude Code's JSON envelope
    ///     (<c>{"result": "..."}</c>). The <c>result</c> string may itself be a
    ///     schema-shaped JSON payload produced when Claude follows the output schema;
    ///     if so, message and choices are parsed from it. Otherwise the plain string
    ///     is used as the message.
    /// </summary>
    private StructuredOutput ProcessClaudeCodeOutput(string rawOutput)
    {
        var resultText = jqAvailable
            ? RunJq(rawOutput, ".result // empty")
            : ExtractStringField(rawOutput, "result");

        if (string.IsNullOrWhiteSpace(resultText))
            return new StructuredOutput(rawOutput.Trim(), []);

        return TryParseSchemaJson(resultText) ?? new StructuredOutput(resultText, []);
    }

    /// <summary>
    ///     Extracts message and choices from a Codex response that conforms directly to the
    ///     AI output schema (<c>{"message": "...", "choices": [...]}</c>).
    /// </summary>
    private StructuredOutput ProcessCodexOutput(string rawOutput)
    {
        return TryParseSchemaJson(rawOutput) ?? new StructuredOutput(rawOutput.Trim(), []);
    }

    /// <summary>
    ///     Attempts to parse a string as schema-shaped JSON containing at least a
    ///     <c>message</c> field. Returns <see langword="null" /> when the string does not
    ///     look like a schema payload or the <c>message</c> field cannot be extracted.
    /// </summary>
    private StructuredOutput TryParseSchemaJson(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("{") || !trimmed.Contains("\"message\""))
            return null;

        string message;
        IReadOnlyList<string> choices;

        if (jqAvailable)
        {
            message = RunJq(trimmed, ".message // empty");
            if (string.IsNullOrWhiteSpace(message)) return null;
            var choicesOutput = RunJq(trimmed, ".choices[]? // empty");
            choices = ParseJqLines(choicesOutput);
        }
        else
        {
            message = ExtractStringField(trimmed, "message");
            if (string.IsNullOrWhiteSpace(message)) return null;
            choices = ExtractStringArray(trimmed, "choices");
        }

        return new StructuredOutput(message, choices);
    }

    /// <summary>
    ///     Runs <c>jq</c> against the given JSON input using the specified filter.
    ///     Returns the trimmed raw output, or <see langword="null" /> on failure.
    /// </summary>
    private static string RunJq(string json, string filter)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, json);
            var startInfo = new ProcessStartInfo
            {
                FileName = "jq",
                Arguments = $"-r {QuoteJqArgument(filter)} {QuoteJqArgument(tempFile)}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            CliProcessStartInfoResolver.PrepareForExecution(startInfo);
            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null) return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(JqTimeoutMs);
            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    /// <summary>
    ///     Splits multi-line <c>jq</c> output (one value per line) into a list of non-empty strings.
    /// </summary>
    private static IReadOnlyList<string> ParseJqLines(string jqOutput)
    {
        if (string.IsNullOrWhiteSpace(jqOutput)) return [];
        return jqOutput
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();
    }

    /// <summary>
    ///     Extracts a JSON string field value using a regex pattern.
    ///     Handles basic JSON string escapes.
    /// </summary>
    private static string ExtractStringField(string json, string fieldName)
    {
        var pattern = $@"""{Regex.Escape(fieldName)}""\s*:\s*""((?:[^""\\]|\\.)*)""";
        var match = Regex.Match(json, pattern, RegexOptions.Singleline);
        if (!match.Success) return null;
        return UnescapeJsonString(match.Groups[1].Value);
    }

    /// <summary>
    ///     Extracts a JSON string array field value using a regex pattern.
    ///     Each element must be a JSON string.
    /// </summary>
    private static IReadOnlyList<string> ExtractStringArray(string json, string fieldName)
    {
        var arrayPattern =
            $@"""{Regex.Escape(fieldName)}""\s*:\s*\[([\s\S]*?)\]";
        var arrayMatch = Regex.Match(json, arrayPattern);
        if (!arrayMatch.Success) return [];

        var itemPattern = @"""((?:[^""\\]|\\.)*)""";
        var items = Regex.Matches(arrayMatch.Groups[1].Value, itemPattern);
        return items
            .Select(m => UnescapeJsonString(m.Groups[1].Value))
            .Where(s => s.Length > 0)
            .ToList();
    }

    /// <summary>
    ///     Unescapes a JSON string value (handles <c>\"</c>, <c>\\</c>, <c>\n</c>, <c>\t</c>).
    /// </summary>
    private static string UnescapeJsonString(string value)
    {
        return value
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t");
    }

    /// <summary>
    ///     Wraps a value in single quotes for use as a shell argument to <c>jq</c> on Unix,
    ///     or double quotes on Windows. Escapes embedded quotes accordingly.
    /// </summary>
    private static string QuoteJqArgument(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    /// <summary>
    ///     Returns <see langword="true" /> when the provider ID corresponds to Claude Code.
    /// </summary>
    private static bool IsClaudeCodeProvider(string providerId) =>
        string.Equals(providerId, "claude-code", StringComparison.OrdinalIgnoreCase);
}
