using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers fallback menu interaction and non-public selection helpers.
/// </summary>
public sealed class ConsoleMenuTests
{
    /// <summary>
    ///     Verifies that keyboard navigation wraps around the available options.
    /// </summary>
    [Fact]
    public void GetNextSelectedIndexWrapsAroundOptions()
    {
        var moveUpResult = (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "GetNextSelectedIndex", ConsoleKey.UpArrow, 0, 3);
        var moveSingleOptionResult = (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "GetNextSelectedIndex", ConsoleKey.DownArrow, 0, 1);
        var moveDownResult = (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "GetNextSelectedIndex", ConsoleKey.DownArrow, 2, 3);
        var unchangedResult = (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "GetNextSelectedIndex", ConsoleKey.LeftArrow, 1, 3);
        Assert.Equal(2, moveUpResult);
        Assert.Equal(0, moveSingleOptionResult);
        Assert.Equal(0, moveDownResult);
        Assert.Equal(1, unchangedResult);
    }

    /// <summary>
    ///     Verifies that menu labels include shortcuts and selection markers as expected.
    /// </summary>
    [Fact]
    public void MenuLineHelpersFormatLabelsCorrectly()
    {
        var numberedLabel = (string)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "FormatOptionLabel", 0, "First");
        var plainLabel = (string)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "FormatOptionLabel", 9, "Tenth");
        var selectedLine = (string)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "BuildMenuLine", 0, "First", true);
        var unselectedLine = (string)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "BuildMenuLine", 9, "Tenth", false);
        Assert.Equal("1. First", numberedLabel);
        Assert.Equal("Tenth", plainLabel);
        Assert.Equal("> 1. First", selectedLine);
        Assert.Equal("  Tenth", unselectedLine);
    }

    /// <summary>
    ///     Verifies that the Boolean prompt maps the second option to <see langword="false" />.
    /// </summary>
    [Fact]
    public void PromptForBooleanReturnsFalseWhenNoIsSelected()
    {
        using var console = new ConsoleRedirectionScope("2" + Environment.NewLine);
        var answer = ConsoleMenu.PromptForBoolean("Continue?");
        Assert.False(answer);
        Assert.Contains("Continue?", console.Output);
    }

    /// <summary>
    ///     Verifies that interactive failures fall back to the numbered prompt for all supported exception types.
    /// </summary>
    [Fact]
    public void PromptForSelectionFallsBackWhenInteractiveHandlerThrows()
    {
        var originalInputProvider = PrivateAccess.GetStaticField<Func<bool>>(typeof(ConsoleMenu),
            "isInputRedirectedProvider");
        var originalOutputProvider = PrivateAccess.GetStaticField<Func<bool>>(typeof(ConsoleMenu),
            "isOutputRedirectedProvider");
        var originalInteractiveHandler =
            PrivateAccess.GetStaticField<Func<string, IReadOnlyList<string>, int>>(
                typeof(ConsoleMenu), "interactivePromptHandler");
        var exceptionFactories = new Func<Exception>[]
        {
            () => new IOException("io"),
            () => new ArgumentOutOfRangeException("value"),
            () => new InvalidOperationException("invalid"),
            () => new PlatformNotSupportedException("unsupported")
        };
        try
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isInputRedirectedProvider",
                (Func<bool>)(() => false));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isOutputRedirectedProvider",
                (Func<bool>)(() => false));
            foreach (var exceptionFactory in exceptionFactories)
            {
                using var console = new ConsoleRedirectionScope("2" + Environment.NewLine);
                PrivateAccess.SetStaticField(typeof(ConsoleMenu), "interactivePromptHandler",
                    (Func<string, IReadOnlyList<string>, int>)((question, options) =>
                        throw exceptionFactory()));
                var selectedIndex = ConsoleMenu.PromptForSelection("Question", ["A", "B"]);
                Assert.Equal(1, selectedIndex);
            }
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isInputRedirectedProvider",
                originalInputProvider);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isOutputRedirectedProvider",
                originalOutputProvider);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "interactivePromptHandler",
                originalInteractiveHandler);
        }
    }

    /// <summary>
    ///     Verifies that the interactive prompt loop handles navigation and confirmation keys.
    /// </summary>
    [Fact]
    public void PromptForSelectionInteractiveHandlesNavigationAndConfirmation()
    {
        var originalReadKeyProvider =
            PrivateAccess.GetStaticField<Func<ConsoleKey>>(typeof(ConsoleMenu), "readKeyProvider");
        var originalWriteInitialOptionsHandler =
            PrivateAccess.GetStaticField<Func<IReadOnlyList<string>, int, int>>(typeof(ConsoleMenu),
                "writeInitialOptionsHandler");
        var originalRenderOptionsHandler =
            PrivateAccess.GetStaticField<Action<IReadOnlyList<string>, int, int>>(
                typeof(ConsoleMenu), "renderOptionsHandler");
        var originalMoveCursorBelowMenuHandler =
            PrivateAccess.GetStaticField<Action<int, int>>(typeof(ConsoleMenu),
                "moveCursorBelowMenuHandler");
        var renderedSelectionIndexes = new List<int>();
        var moveCursorCalls = new List<(int OptionsTop, int OptionCount)>();
        var pressedKeys = new Queue<ConsoleKey>(new[]
        {
            ConsoleKey.DownArrow,
            ConsoleKey.Enter
        });
        try
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "readKeyProvider",
                (Func<ConsoleKey>)(() => pressedKeys.Dequeue()));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "writeInitialOptionsHandler",
                (Func<IReadOnlyList<string>, int, int>)((options, selectedIndex) => 4));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "renderOptionsHandler",
                (Action<IReadOnlyList<string>, int, int>)((options, optionsTop, selectedIndex) =>
                    renderedSelectionIndexes.Add(selectedIndex)));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "moveCursorBelowMenuHandler",
                (Action<int, int>)((optionsTop, optionCount) =>
                    moveCursorCalls.Add((optionsTop, optionCount))));
            using var console = new ConsoleRedirectionScope(string.Empty);
            var selectedIndex = (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
                "PromptForSelectionInteractive", "Question", new[] { "A", "B" });
            Assert.Equal(1, selectedIndex);
            Assert.Equal([1], renderedSelectionIndexes);
            Assert.Equal([(4, 2)], moveCursorCalls);
            Assert.Contains("Question", console.Output);
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "readKeyProvider",
                originalReadKeyProvider);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "writeInitialOptionsHandler",
                originalWriteInitialOptionsHandler);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "renderOptionsHandler",
                originalRenderOptionsHandler);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "moveCursorBelowMenuHandler",
                originalMoveCursorBelowMenuHandler);
        }
    }

    /// <summary>
    ///     Verifies that fallback selection retries after invalid input.
    /// </summary>
    [Fact]
    public void PromptForSelectionRetriesUntilInputIsValid()
    {
        using var console = new ConsoleRedirectionScope(
            "9" + Environment.NewLine + "1" + Environment.NewLine);
        var selectedIndex = ConsoleMenu.PromptForSelection("Pick an option", ["Alpha", "Beta"]);
        Assert.Equal(0, selectedIndex);
        Assert.Contains("Please enter a valid option number.", console.Output);
    }

    /// <summary>
    ///     Verifies that fallback selection returns the chosen option index.
    /// </summary>
    [Fact]
    public void PromptForSelectionReturnsChosenIndexInFallbackMode()
    {
        using var console = new ConsoleRedirectionScope("2" + Environment.NewLine);
        var selectedIndex = ConsoleMenu.PromptForSelection("Pick an option", ["Alpha", "Beta"]);
        Assert.Equal(1, selectedIndex);
        Assert.Contains("Pick an option", console.Output);
        Assert.Contains("1. Alpha", console.Output);
        Assert.Contains("2. Beta", console.Output);
    }

    /// <summary>
    ///     Verifies that the public prompt uses the interactive handler when the console is available.
    /// </summary>
    [Fact]
    public void PromptForSelectionUsesInteractiveHandlerWhenConsoleIsAvailable()
    {
        var originalInputProvider = PrivateAccess.GetStaticField<Func<bool>>(typeof(ConsoleMenu),
            "isInputRedirectedProvider");
        var originalOutputProvider = PrivateAccess.GetStaticField<Func<bool>>(typeof(ConsoleMenu),
            "isOutputRedirectedProvider");
        var originalInteractiveHandler =
            PrivateAccess.GetStaticField<Func<string, IReadOnlyList<string>, int>>(
                typeof(ConsoleMenu), "interactivePromptHandler");
        try
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isInputRedirectedProvider",
                (Func<bool>)(() => false));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isOutputRedirectedProvider",
                (Func<bool>)(() => false));
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "interactivePromptHandler",
                (Func<string, IReadOnlyList<string>, int>)((question, options) => 1));
            var selectedIndex = ConsoleMenu.PromptForSelection("Question", ["A", "B"]);
            Assert.Equal(1, selectedIndex);
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isInputRedirectedProvider",
                originalInputProvider);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "isOutputRedirectedProvider",
                originalOutputProvider);
            PrivateAccess.SetStaticField(typeof(ConsoleMenu), "interactivePromptHandler",
                originalInteractiveHandler);
        }
    }

    /// <summary>
    ///     Verifies that invalid prompt arguments produce the expected exceptions.
    /// </summary>
    [Fact]
    public void PromptForSelectionValidatesArguments()
    {
        IReadOnlyList<string> missingOptions = null;
        Assert.Throws<ArgumentNullException>(() =>
            ConsoleMenu.PromptForSelection("Question", missingOptions));
        Assert.Throws<ArgumentException>(() =>
            ConsoleMenu.PromptForSelection(string.Empty, ["Only option"]));
        Assert.Throws<ArgumentException>(() =>
            ConsoleMenu.PromptForSelection("Question", Array.Empty<string>()));
    }

    /// <summary>
    ///     Verifies that digit shortcuts resolve to the matching option index.
    /// </summary>
    [Fact]
    public void TryGetOptionIndexFromDigitKeyResolvesSupportedDigits()
    {
        var arguments = new object[] { ConsoleKey.D2, 3, -1 };
        var resolved = (bool)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "TryGetOptionIndexFromDigitKey", arguments);
        var selectedIndex = (int)arguments[2];
        var invalidArguments = new object[] { ConsoleKey.D9, 3, -1 };
        var invalidResolved = (bool)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
            "TryGetOptionIndexFromDigitKey", invalidArguments);
        Assert.True(resolved);
        Assert.Equal(1, selectedIndex);
        Assert.False(invalidResolved);
        Assert.Equal(-1, (int)invalidArguments[2]);
    }

    /// <summary>
    ///     Verifies that every supported digit shortcut maps to the expected zero-based option.
    /// </summary>
    [Fact]
    public void TryGetOptionIndexFromDigitKeySupportsAllVisibleShortcuts()
    {
        var supportedKeys = new[]
        {
            ConsoleKey.D1,
            ConsoleKey.D2,
            ConsoleKey.D3,
            ConsoleKey.D4,
            ConsoleKey.D5,
            ConsoleKey.D6,
            ConsoleKey.D7,
            ConsoleKey.D8,
            ConsoleKey.D9,
            ConsoleKey.NumPad1,
            ConsoleKey.NumPad2,
            ConsoleKey.NumPad3,
            ConsoleKey.NumPad4,
            ConsoleKey.NumPad5,
            ConsoleKey.NumPad6,
            ConsoleKey.NumPad7,
            ConsoleKey.NumPad8,
            ConsoleKey.NumPad9
        };
        for (var index = 0; index < supportedKeys.Length; index++)
        {
            var expectedSelection = index % 9;
            var arguments = new object[] { supportedKeys[index], 9, -1 };
            var resolved = (bool)PrivateAccess.InvokeStatic(typeof(ConsoleMenu),
                "TryGetOptionIndexFromDigitKey", arguments);
            Assert.True(resolved);
            Assert.Equal(expectedSelection, (int)arguments[2]);
        }
    }

    /// <summary>
    ///     Verifies that interactive option rendering fails with an I/O error when cursor APIs are unavailable.
    /// </summary>
    [Fact]
    public void WriteInitialOptionsThrowsWhenCursorApisAreUnavailable()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var exception = Assert.Throws<IOException>(() =>
            PrivateAccess.InvokeStatic(typeof(ConsoleMenu), "WriteInitialOptions",
                new[] { "First", "Second" }, 1));
        Assert.NotNull(exception);
    }

    /// <summary>
    ///     Verifies that menu lines are padded or truncated to the available line width.
    /// </summary>
    [Fact]
    public void WriteMenuLinePadsAndTruncatesOutput()
    {
        using var paddedConsole = new ConsoleRedirectionScope(string.Empty);
        var lineWidth =
            (int)PrivateAccess.InvokeStatic(typeof(ConsoleMenu), "GetWritableLineWidth");
        PrivateAccess.InvokeStatic(typeof(ConsoleMenu), "WriteMenuLine", "short");
        Assert.Equal(lineWidth, paddedConsole.Output.Length);
        Assert.StartsWith("short", paddedConsole.Output, StringComparison.Ordinal);
        using var truncatedConsole = new ConsoleRedirectionScope(string.Empty);
        PrivateAccess.InvokeStatic(typeof(ConsoleMenu), "WriteMenuLine",
            new string('x', lineWidth + 5));
        Assert.Equal(lineWidth, truncatedConsole.Output.Length);
    }
}