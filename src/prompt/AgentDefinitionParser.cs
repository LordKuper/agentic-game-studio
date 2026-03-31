using System.Text;

namespace AGS.prompt;

/// <summary>
///     Parses an agent markdown file into an <see cref="AgentDefinition" />.
/// </summary>
/// <remarks>
///     Expected markdown format:
///     <code>
///         # Agent Title
///
///         | Field | Value |
///         | --- | --- |
///         | `name` | `agent-name` |
///         | `description` | Agent description text. |
///         | `must_not` | - Prohibition one.&lt;br&gt;- Prohibition two. |
///         | `models` | - model-a&lt;br&gt;- model-b |
///         | `max_iterations` | 20 |
///
///         ## Practical Guidance
///
///         - Tactical guidance bullet one.
///     </code>
/// </remarks>
internal static class AgentDefinitionParser
{
    private const string PracticalGuidanceHeader = "## Practical Guidance";
    private const string ListItemSeparator = "<br>";

    /// <summary>
    ///     Parses an agent definition from its markdown content.
    /// </summary>
    /// <param name="markdown">Full markdown text of an agent definition file.</param>
    /// <returns>The parsed <see cref="AgentDefinition" />.</returns>
    internal static AgentDefinition Parse(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            throw new ArgumentException("Agent markdown content must not be empty.", nameof(markdown));

        var lines = markdown.Split('\n');
        var tableFields = ParseTableFields(lines);

        var name = GetFieldValue(tableFields, "name");
        var description = GetFieldValue(tableFields, "description");
        var mustNot = ParseListField(tableFields, "must_not");
        var models = ParseListField(tableFields, "models");
        var maxIterationsText = GetFieldValue(tableFields, "max_iterations");
        var maxIterations = int.TryParse(maxIterationsText, out var parsed) ? parsed : 0;
        var practicalGuidance = ExtractSectionContent(lines, PracticalGuidanceHeader);

        return new AgentDefinition(name, description, mustNot, models, maxIterations,
            practicalGuidance);
    }

    /// <summary>
    ///     Parses all markdown table rows into a field-name-to-raw-value dictionary.
    /// </summary>
    /// <param name="lines">Lines of the agent markdown file.</param>
    /// <returns>Dictionary mapping lowercase field names to their raw cell values.</returns>
    private static Dictionary<string, string> ParseTableFields(string[] lines)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('|') || !trimmed.EndsWith('|')) continue;
            var cells = trimmed.Split('|', StringSplitOptions.None);
            if (cells.Length < 4) continue;
            var fieldCell = cells[1].Trim();
            var valueCell = cells[2].Trim();
            if (IsTableHeaderOrSeparatorCell(fieldCell)) continue;
            var fieldName = StripBackticks(fieldCell).ToLowerInvariant();
            if (string.IsNullOrEmpty(fieldName)) continue;
            fields[fieldName] = valueCell;
        }
        return fields;
    }

    /// <summary>
    ///     Gets a raw string field value from the parsed table, returning an empty string when absent.
    /// </summary>
    /// <param name="fields">Parsed table fields.</param>
    /// <param name="key">Lowercase field name to look up.</param>
    /// <returns>The stripped field value, or an empty string when the field is not present.</returns>
    private static string GetFieldValue(Dictionary<string, string> fields, string key)
    {
        if (!fields.TryGetValue(key, out var value)) return string.Empty;
        return StripBackticks(value);
    }

    /// <summary>
    ///     Parses a <c>&lt;br&gt;</c>-separated list field into individual items.
    /// </summary>
    /// <param name="fields">Parsed table fields.</param>
    /// <param name="key">Lowercase field name to look up.</param>
    /// <returns>Trimmed list items with leading <c>- </c> stripped, or an empty list when absent.</returns>
    private static IReadOnlyList<string> ParseListField(Dictionary<string, string> fields, string key)
    {
        if (!fields.TryGetValue(key, out var value)) return Array.Empty<string>();
        var rawValue = StripBackticks(value);
        if (string.IsNullOrWhiteSpace(rawValue)) return Array.Empty<string>();
        var items = rawValue.Split(ListItemSeparator, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(items.Length);
        foreach (var item in items)
        {
            var trimmedItem = item.Trim();
            if (trimmedItem.StartsWith("- ")) trimmedItem = trimmedItem[2..].Trim();
            else if (trimmedItem.StartsWith('-')) trimmedItem = trimmedItem[1..].Trim();
            if (!string.IsNullOrEmpty(trimmedItem)) result.Add(trimmedItem);
        }
        return result;
    }

    /// <summary>
    ///     Extracts the body text of a markdown section identified by its heading line.
    /// </summary>
    /// <param name="lines">Lines of the agent markdown file.</param>
    /// <param name="sectionHeader">Exact heading line that opens the section (e.g. <c>## Practical Guidance</c>).</param>
    /// <returns>Trimmed section body, or an empty string when the section is absent.</returns>
    private static string ExtractSectionContent(string[] lines, string sectionHeader)
    {
        var inSection = false;
        var content = new StringBuilder();
        foreach (var line in lines)
        {
            var trimmedLine = line.TrimEnd();
            if (!inSection)
            {
                if (string.Equals(trimmedLine, sectionHeader, StringComparison.OrdinalIgnoreCase))
                    inSection = true;
                continue;
            }
            // Stop at the next heading of any level
            if (trimmedLine.StartsWith('#')) break;
            content.AppendLine(trimmedLine);
        }
        return content.ToString().Trim();
    }

    /// <summary>
    ///     Determines whether a table cell is a header label or a separator line rather than data.
    /// </summary>
    /// <param name="cell">Trimmed cell content to inspect.</param>
    /// <returns>
    ///     <see langword="true" /> when the cell is a known header or separator; otherwise
    ///     <see langword="false" />.
    /// </returns>
    private static bool IsTableHeaderOrSeparatorCell(string cell)
    {
        if (string.IsNullOrWhiteSpace(cell)) return true;
        // Separator cells look like "---", ":---", "---:", ":---:"
        var stripped = cell.Trim('-', ':', ' ');
        if (stripped.Length == 0) return true;
        // Header cell is "Field"
        if (string.Equals(cell, "Field", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>
    ///     Removes surrounding backtick characters from a value when present.
    /// </summary>
    /// <param name="value">Raw cell value that may be wrapped in backticks.</param>
    /// <returns>Value with leading and trailing backtick characters removed.</returns>
    private static string StripBackticks(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed[0] == '`' && trimmed[^1] == '`')
            return trimmed[1..^1].Trim();
        return trimmed;
    }
}
