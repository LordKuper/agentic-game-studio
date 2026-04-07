namespace AGS.ui;

/// <summary>
///     Presents an interactive selection menu when the AI response offers multiple choices,
///     and falls back to free-form text input when the user wants to provide a custom answer.
/// </summary>
internal static class ChoiceMenu
{
    private const string FreeFormOption = "Enter your own answer...";

    /// <summary>
    ///     Shows a selection menu built from the provided choices.
    ///     The last item is always a free-form entry option.
    ///     Returns the selected choice, or the user's custom input.
    /// </summary>
    /// <param name="choices">Non-empty list of choices from the AI response.</param>
    /// <returns>The selected or entered response string.</returns>
    internal static string Show(IReadOnlyList<string> choices)
    {
        var options = new List<string>(choices) { FreeFormOption };
        var selectedIndex = AgsPrompt.Select("Choose an option", options);

        if (selectedIndex < choices.Count)
            return choices[selectedIndex];

        return AgsPrompt.Input("Your answer");
    }
}
