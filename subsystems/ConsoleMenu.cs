namespace AGS.subsystems;

/// <summary>
///     Provides keyboard-driven console menus for interactive selections.
/// </summary>
internal static class ConsoleMenu
{
    private const int FallbackConsoleWidth = 80;

    private const string NavigationHint =
        "Use Up/Down to choose, Enter to confirm, or press a digit for options 1-9.";

    private const string SelectedOptionPrefix = "> ";
    private const string UnselectedOptionPrefix = "  ";
    private static Func<bool> isInputRedirectedProvider = () => Console.IsInputRedirected;

    private static Func<bool>
        isOutputRedirectedProvider = () => Console.IsOutputRedirected;

    private static Func<string, IReadOnlyList<string>, int> interactivePromptHandler =
        PromptForSelectionInteractive;

    private static Func<ConsoleKey> readKeyProvider = () => Console.ReadKey(true).Key;

    private static Func<IReadOnlyList<string>, int, int> writeInitialOptionsHandler =
        WriteInitialOptions;

    private static Action<IReadOnlyList<string>, int, int> renderOptionsHandler =
        RenderOptions;

    private static Action<int, int> moveCursorBelowMenuHandler = MoveCursorBelowMenu;

    /// <summary>
    ///     Builds a rendered menu line for the specified option state.
    /// </summary>
    /// <param name="optionIndex">Zero-based index of the menu option.</param>
    /// <param name="optionText">Display text of the menu option.</param>
    /// <param name="isSelected">
    ///     <see langword="true" /> when the option should be rendered as selected; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>Rendered menu line for the requested option.</returns>
    private static string BuildMenuLine(int optionIndex, string optionText, bool isSelected)
    {
        var prefix = isSelected ? SelectedOptionPrefix : UnselectedOptionPrefix;
        return prefix + FormatOptionLabel(optionIndex, optionText);
    }

    /// <summary>
    ///     Ensures that the console buffer is tall enough to address the specified row index.
    /// </summary>
    /// <param name="requiredHeight">
    ///     Minimum buffer height required for upcoming cursor operations.
    /// </param>
    private static void EnsureBufferHeight(int requiredHeight)
    {
        if (requiredHeight <= 0) return;
        if (!OperatingSystem.IsWindows()) return;
        if (Console.BufferHeight >= requiredHeight) return;
        Console.BufferHeight = requiredHeight;
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
    ///     Gets the current cursor row and expands the buffer when the cursor is already positioned
    ///     at the current buffer boundary.
    /// </summary>
    /// <returns>A cursor row that is safe to use for subsequent rendering.</returns>
    private static int GetSafeCursorTop()
    {
        var cursorTop = Console.CursorTop;
        if (cursorTop < 0) return 0;
        EnsureBufferHeight(cursorTop + 1);
        return cursorTop;
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

    /// <summary>
    ///     Moves the cursor to the first row below the current menu.
    /// </summary>
    /// <param name="optionsTop">Top console row where the first option is rendered.</param>
    /// <param name="optionCount">Total number of available options.</param>
    private static void MoveCursorBelowMenu(int optionsTop, int optionCount)
    {
        var targetTop = optionsTop + optionCount;
        EnsureBufferHeight(targetTop + 1);
        Console.SetCursorPosition(0, targetTop);
    }

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
        if (isInputRedirectedProvider() || isOutputRedirectedProvider())
            return PromptForSelectionFallback(question, options);
        try
        {
            return interactivePromptHandler(question, options);
        }
        catch (IOException)
        {
            return PromptForSelectionFallback(question, options);
        }
        catch (ArgumentOutOfRangeException)
        {
            return PromptForSelectionFallback(question, options);
        }
        catch (InvalidOperationException)
        {
            return PromptForSelectionFallback(question, options);
        }
        catch (PlatformNotSupportedException)
        {
            return PromptForSelectionFallback(question, options);
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
            if (int.TryParse(rawSelection, out var selectedNumber) && selectedNumber >= 1 &&
                selectedNumber <= options.Count)
                return selectedNumber - 1;
            Console.WriteLine("Please enter a valid option number.");
        }
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
        var selectedIndex = 0;
        var optionsTop = writeInitialOptionsHandler(options, selectedIndex);
        while (true)
        {
            var pressedKey = readKeyProvider();
            if (TryGetOptionIndexFromDigitKey(pressedKey, options.Count, out var selectedByDigit))
            {
                moveCursorBelowMenuHandler(optionsTop, options.Count);
                Console.WriteLine();
                return selectedByDigit;
            }
            if (pressedKey == ConsoleKey.Enter)
            {
                moveCursorBelowMenuHandler(optionsTop, options.Count);
                Console.WriteLine();
                return selectedIndex;
            }
            var nextSelectedIndex = GetNextSelectedIndex(pressedKey, selectedIndex, options.Count);
            if (nextSelectedIndex == selectedIndex) continue;
            selectedIndex = nextSelectedIndex;
            renderOptionsHandler(options, optionsTop, selectedIndex);
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
        EnsureBufferHeight(optionsTop + options.Count + 1);
        for (var index = 0; index < options.Count; index++)
        {
            Console.SetCursorPosition(0, optionsTop + index);
            WriteMenuLine(BuildMenuLine(index, options[index], index == selectedIndex));
        }
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
    ///     Writes the initial visible option list and returns the row where the first option is
    ///     rendered.
    /// </summary>
    /// <param name="options">Ordered option labels that the user can choose from.</param>
    /// <param name="selectedIndex">Zero-based index of the initially highlighted option.</param>
    /// <returns>Top console row where the first option should be rendered.</returns>
    private static int WriteInitialOptions(IReadOnlyList<string> options, int selectedIndex)
    {
        var optionsTop = GetSafeCursorTop();
        EnsureBufferHeight(optionsTop + options.Count + 1);
        for (var index = 0; index < options.Count; index++)
        {
            WriteMenuLine(BuildMenuLine(index, options[index], index == selectedIndex));
            Console.WriteLine();
        }
        return optionsTop;
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
}
