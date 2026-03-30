using Sharprompt;

namespace AGS;

/// <summary>
///     Provides a small testable wrapper around Sharprompt for interactive application prompts.
/// </summary>
internal static class AgsPrompt
{
    private static Func<string, bool, bool> confirmHandler =
        (message, defaultValue) => Prompt.Confirm(message, defaultValue: defaultValue);

    private static Func<string, IReadOnlyList<string>, string> selectHandler =
        (message, options) => Prompt.Select(message, [.. options]);

    /// <summary>
    ///     Shows a confirmation prompt and returns the selected Boolean value.
    /// </summary>
    /// <param name="message">Question text shown to the user.</param>
    /// <param name="defaultValue">Default answer used when the user accepts the default.</param>
    /// <returns>
    ///     <see langword="true" /> when the confirmation is accepted; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    internal static bool Confirm(string message, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Prompt message must be provided.", nameof(message));
        return confirmHandler(message, defaultValue);
    }

    /// <summary>
    ///     Shows a selection prompt and returns the zero-based index of the chosen option.
    /// </summary>
    /// <param name="message">Question text shown to the user.</param>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <returns>Zero-based index of the selected option.</returns>
    internal static int Select(string message, IReadOnlyList<string> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Prompt message must be provided.", nameof(message));
        if (options.Count == 0)
            throw new ArgumentException("At least one option must be provided.", nameof(options));
        var selectedOption = selectHandler(message, options);
        for (var index = 0; index < options.Count; index++)
        {
            if (string.Equals(options[index], selectedOption, StringComparison.Ordinal))
                return index;
        }
        throw new InvalidOperationException(
            $"Prompt returned an unknown option: {selectedOption}");
    }
}
