namespace AGS.output;

/// <summary>
///     Holds the extracted result of processing raw AI provider output through
///     <see cref="StructuredOutputProcessor" />.
/// </summary>
internal sealed class StructuredOutput
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StructuredOutput" /> class.
    /// </summary>
    /// <param name="message">The meaningful message to display to the user.</param>
    /// <param name="choices">Optional list of choices for the user to select from.</param>
    internal StructuredOutput(string message, IReadOnlyList<string> choices)
    {
        Message = message ?? string.Empty;
        Choices = choices ?? [];
    }

    /// <summary>
    ///     Gets the meaningful response message to display to the user.
    /// </summary>
    internal string Message { get; }

    /// <summary>
    ///     Gets the optional list of choices offered to the user. Empty when the response
    ///     does not present a decision point.
    /// </summary>
    internal IReadOnlyList<string> Choices { get; }
}
