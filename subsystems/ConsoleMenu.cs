namespace AGS.subsystems;

/// <summary>
///     Provides keyboard-driven console menus for interactive selections.
/// </summary>
internal static class ConsoleMenu
{
    private const string NavigationHint =
        "Use Up/Down to choose, Enter to confirm, or press a digit for options 1-9.";
    private const string SelectedOptionPrefix = "> ";
    private const string UnselectedOptionPrefix = "  ";
    private const int FallbackConsoleWidth = 80;

    /// <summary>
    ///     Shows a Yes/No menu and returns the selected Boolean value.
    /// </summary>
    /// <param name="question">Question text shown above the options.</param>
    /// <returns>
    ///     <see langword="true" /> when the first option is selected; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    internal static bool PromptForBoolean(string question)
    {
        return PromptForSelection(question, ["Yes", "No"]) == 0;
    }

    /// <summary>
    ///     Shows a menu with the provided options and returns the selected index.
    /// </summary>
    /// <param name="question">Question text shown above the options.</param>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <returns>Zero-based index of the selected option.</returns>
    internal static int PromptForSelection(string question, IReadOnlyList<string> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question text must be provided.", nameof(question));
        if (options.Count == 0)
            throw new ArgumentException("At least one option must be provided.", nameof(options));

        if (Console.IsInputRedirected || Console.IsOutputRedirected)
            return PromptForSelectionFallback(question, options);

        return PromptForSelectionInteractive(question, options);
    }

    /// <summary>
    ///     Shows an interactive keyboard-controlled menu in a console that supports cursor input.
    /// </summary>
    /// <param name="question">Question text shown above the options.</param>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <returns>Zero-based index of the selected option.</returns>
    private static int PromptForSelectionInteractive(string question, IReadOnlyList<string> options)
    {
        Console.WriteLine(question);
        Console.WriteLine(NavigationHint);
        var optionsTop = Console.CursorTop;
        for (var index = 0; index < options.Count; index++) Console.WriteLine();

        var selectedIndex = 0;
        RenderOptions(options, optionsTop, selectedIndex);
        while (true)
        {
            var pressedKey = Console.ReadKey(true).Key;
            if (TryGetOptionIndexFromDigitKey(pressedKey, options.Count, out var selectedByDigit))
            {
                Console.SetCursorPosition(0, optionsTop + options.Count);
                Console.WriteLine();
                return selectedByDigit;
            }

            if (pressedKey == ConsoleKey.Enter)
            {
                Console.SetCursorPosition(0, optionsTop + options.Count);
                Console.WriteLine();
                return selectedIndex;
            }

            var nextSelectedIndex =
                GetNextSelectedIndex(pressedKey, selectedIndex, options.Count);
            if (nextSelectedIndex == selectedIndex) continue;

            selectedIndex = nextSelectedIndex;
            RenderOptions(options, optionsTop, selectedIndex);
        }
    }

    /// <summary>
    ///     Shows a fallback numbered menu when interactive keyboard navigation is unavailable.
    /// </summary>
    /// <param name="question">Question text shown above the options.</param>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <returns>Zero-based index of the selected option.</returns>
    private static int PromptForSelectionFallback(string question, IReadOnlyList<string> options)
    {
        while (true)
        {
            Console.WriteLine(question);
            for (var index = 0; index < options.Count; index++)
                Console.WriteLine($"{index + 1}. {options[index]}");

            Console.Write("Enter the option number: ");
            var rawSelection = Console.ReadLine();
            if (int.TryParse(rawSelection, out var selectedNumber) &&
                selectedNumber >= 1 &&
                selectedNumber <= options.Count)
                return selectedNumber - 1;

            Console.WriteLine("Please enter a valid option number.");
        }
    }

    /// <summary>
    ///     Re-renders the visible option list with the selected option highlighted.
    /// </summary>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <param name="optionsTop">Top console row where the first option is rendered.</param>
    /// <param name="selectedIndex">Zero-based index of the currently highlighted option.</param>
    private static void RenderOptions(IReadOnlyList<string> options, int optionsTop,
        int selectedIndex)
    {
        for (var index = 0; index < options.Count; index++)
        {
            Console.SetCursorPosition(0, optionsTop + index);
            var prefix = index == selectedIndex ? SelectedOptionPrefix : UnselectedOptionPrefix;
            WriteMenuLine(prefix + FormatOptionLabel(index, options[index]));
        }
    }

    /// <summary>
    ///     Formats a visible menu option label, including a numeric shortcut for the first nine
    ///     options.
    /// </summary>
    /// <param name="optionIndex">Zero-based index of the menu option.</param>
    /// <param name="optionText">Display text of the menu option.</param>
    /// <returns>Formatted option label for console rendering.</returns>
    private static string FormatOptionLabel(int optionIndex, string optionText)
    {
        if (optionIndex < 9) return $"{optionIndex + 1}. {optionText}";
        return optionText;
    }

    /// <summary>
    ///     Calculates the next selected option based on the pressed navigation key.
    /// </summary>
    /// <param name="pressedKey">Keyboard key pressed by the user.</param>
    /// <param name="currentIndex">Zero-based index of the currently highlighted option.</param>
    /// <param name="optionCount">Total number of available options.</param>
    /// <returns>Zero-based index of the option that should be highlighted next.</returns>
    private static int GetNextSelectedIndex(ConsoleKey pressedKey, int currentIndex,
        int optionCount)
    {
        if (optionCount <= 1) return currentIndex;
        if (pressedKey == ConsoleKey.UpArrow)
            return currentIndex == 0 ? optionCount - 1 : currentIndex - 1;
        if (pressedKey == ConsoleKey.DownArrow)
            return currentIndex == optionCount - 1 ? 0 : currentIndex + 1;

        return currentIndex;
    }

    /// <summary>
    ///     Attempts to map a digit key to one of the first nine visible menu options.
    /// </summary>
    /// <param name="pressedKey">Keyboard key pressed by the user.</param>
    /// <param name="optionCount">Total number of available options.</param>
    /// <param name="selectedIndex">
    ///     Zero-based index selected by the digit shortcut when the mapping succeeds.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the pressed key maps to an available option; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool TryGetOptionIndexFromDigitKey(ConsoleKey pressedKey, int optionCount,
        out int selectedIndex)
    {
        selectedIndex = -1;
        var digit = pressedKey switch
        {
            ConsoleKey.D1 or ConsoleKey.NumPad1 => 1,
            ConsoleKey.D2 or ConsoleKey.NumPad2 => 2,
            ConsoleKey.D3 or ConsoleKey.NumPad3 => 3,
            ConsoleKey.D4 or ConsoleKey.NumPad4 => 4,
            ConsoleKey.D5 or ConsoleKey.NumPad5 => 5,
            ConsoleKey.D6 or ConsoleKey.NumPad6 => 6,
            ConsoleKey.D7 or ConsoleKey.NumPad7 => 7,
            ConsoleKey.D8 or ConsoleKey.NumPad8 => 8,
            ConsoleKey.D9 or ConsoleKey.NumPad9 => 9,
            _ => 0
        };
        if (digit == 0 || digit > optionCount) return false;

        selectedIndex = digit - 1;
        return true;
    }

    /// <summary>
    ///     Writes a single menu line and clears any leftover characters from previous renders.
    /// </summary>
    /// <param name="text">Rendered menu text for the current option line.</param>
    private static void WriteMenuLine(string text)
    {
        var writableLineWidth = GetWritableLineWidth();
        if (text.Length >= writableLineWidth)
        {
            Console.Write(text[..writableLineWidth]);
            return;
        }

        Console.Write(text.PadRight(writableLineWidth));
    }

    /// <summary>
    ///     Gets a safe line width that can be written without forcing an automatic line wrap.
    /// </summary>
    /// <returns>Maximum number of characters that can be written on one menu line.</returns>
    private static int GetWritableLineWidth()
    {
        try
        {
            return Math.Max(1, Console.BufferWidth - 1);
        }
        catch (IOException)
        {
            return FallbackConsoleWidth;
        }
        catch (ArgumentOutOfRangeException)
        {
            return FallbackConsoleWidth;
        }
    }
}
