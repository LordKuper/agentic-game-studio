using System.Reflection;
using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers the main menu fallback loop and menu construction helpers.
/// </summary>
public sealed class MainMenuSubsystemTests
{
    /// <summary>
    ///     Counts non-overlapping occurrences of a substring in a source string.
    /// </summary>
    /// <param name="source">Source string to inspect.</param>
    /// <param name="value">Substring to count.</param>
    /// <returns>Number of non-overlapping occurrences found in <paramref name="source" />.</returns>
    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while (true)
        {
            index = source.IndexOf(value, index, StringComparison.Ordinal);
            if (index < 0) return count;
            count++;
            index += value.Length;
        }
    }

    /// <summary>
    ///     Verifies that unfinished sessions are converted into continue-session menu options.
    /// </summary>
    [Fact]
    public void BuildOptionsIncludesUnfinishedSessions()
    {
        var originalProvider = PrivateAccess.GetStaticField<Func<IReadOnlyList<string>>>(
            typeof(MainMenuSubsystem), "unfinishedSessionNamesProvider");
        try
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem),
                "unfinishedSessionNamesProvider",
                (Func<IReadOnlyList<string>>)(() => new[] { "alpha", "beta" }));
            var options =
                (Array)PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "BuildOptions");
            var labelProperty = options.GetType().GetElementType().GetProperty("Label",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(labelProperty);
            Assert.Equal("Continue session alpha", labelProperty.GetValue(options.GetValue(0)));
            Assert.Equal("Continue session beta", labelProperty.GetValue(options.GetValue(1)));
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem),
                "unfinishedSessionNamesProvider", originalProvider);
        }
    }

    /// <summary>
    ///     Verifies that the default menu options are built in the expected order.
    /// </summary>
    [Fact]
    public void BuildOptionsReturnsDefaultOptions()
    {
        var options = (Array)PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "BuildOptions");
        var labelProperty = options.GetType().GetElementType().GetProperty("Label",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(3, options.Length);
        Assert.NotNull(labelProperty);
        Assert.Equal("Start a new session", labelProperty.GetValue(options.GetValue(0)));
        Assert.Equal("Settings", labelProperty.GetValue(options.GetValue(1)));
        Assert.Equal("Exit", labelProperty.GetValue(options.GetValue(2)));
    }

    /// <summary>
    ///     Verifies that clearing the main menu console does not throw when output is redirected.
    /// </summary>
    [Fact]
    public void ClearConsoleForMainMenuDoesNotThrowWhenOutputIsRedirected()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var exception = Record.Exception(() =>
            PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "ClearConsoleForMainMenu"));
        Assert.Null(exception);
    }

    /// <summary>
    ///     Verifies that console clear failures are swallowed when the output stream supports interactive mode.
    /// </summary>
    [Fact]
    public void ClearConsoleForMainMenuSwallowsClearExceptions()
    {
        var originalOutputProvider =
            PrivateAccess.GetStaticField<Func<bool>>(typeof(MainMenuSubsystem),
                "isOutputRedirectedProvider");
        var originalClearHandler = PrivateAccess.GetStaticField<Action>(typeof(MainMenuSubsystem),
            "clearConsoleHandler");
        var exceptions = new Exception[]
        {
            new IOException("io"),
            new ArgumentOutOfRangeException("value"),
            new InvalidOperationException("invalid"),
            new PlatformNotSupportedException("unsupported")
        };
        try
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem), "isOutputRedirectedProvider",
                (Func<bool>)(() => false));
            foreach (var exception in exceptions)
            {
                PrivateAccess.SetStaticField(typeof(MainMenuSubsystem), "clearConsoleHandler",
                    (Action)(() => throw exception));
                var thrownException = Record.Exception(() =>
                    PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem),
                        "ClearConsoleForMainMenu"));
                Assert.Null(thrownException);
            }
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem), "isOutputRedirectedProvider",
                originalOutputProvider);
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem), "clearConsoleHandler",
                originalClearHandler);
        }
    }

    /// <summary>
    ///     Verifies that menu labels are extracted from the option objects.
    /// </summary>
    [Fact]
    public void GetOptionLabelsReturnsVisibleOptionLabels()
    {
        var options = (Array)PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "BuildOptions");
        var labels = (string[])PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem),
            "GetOptionLabels", options);
        Assert.Equal(["Start a new session", "Settings", "Exit"], labels);
    }

    /// <summary>
    ///     Verifies that unfinished sessions are currently not exposed.
    /// </summary>
    [Fact]
    public void GetUnfinishedSessionNamesReturnsEmptyList()
    {
        var sessionNames = (IReadOnlyList<string>)PrivateAccess.InvokeStatic(
            typeof(MainMenuSubsystem), "GetUnfinishedSessionNames");
        Assert.Empty(sessionNames);
    }

    /// <summary>
    ///     Verifies that the main menu exits cleanly when the exit option is selected.
    /// </summary>
    [Fact]
    public void RunExitsWhenExitOptionIsSelected()
    {
        using var console = new ConsoleRedirectionScope("3" + Environment.NewLine);
        MainMenuSubsystem.Run();
        Assert.Contains("Application is shutting down.", console.Output);
    }

    /// <summary>
    ///     Verifies that selecting an unfinished session returns to the menu loop until exit is chosen.
    /// </summary>
    [Fact]
    public void RunLoopsAfterContinueSessionSelection()
    {
        var originalProvider = PrivateAccess.GetStaticField<Func<IReadOnlyList<string>>>(
            typeof(MainMenuSubsystem), "unfinishedSessionNamesProvider");
        try
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem),
                "unfinishedSessionNamesProvider",
                (Func<IReadOnlyList<string>>)(() => new[] { "alpha" }));
            using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
                "1", "4", string.Empty));
            MainMenuSubsystem.Run();
            Assert.True(CountOccurrences(console.Output, "Main menu") >= 2);
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(MainMenuSubsystem),
                "unfinishedSessionNamesProvider", originalProvider);
        }
    }

    /// <summary>
    ///     Verifies that starting a new session returns to the menu loop until exit is selected.
    /// </summary>
    [Fact]
    public void RunLoopsAfterStartNewSession()
    {
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "1", "3", string.Empty));
        MainMenuSubsystem.Run();
        Assert.True(CountOccurrences(console.Output, "Main menu") >= 2);
    }

    /// <summary>
    ///     Verifies that the settings option opens the settings subsystem and then returns.
    /// </summary>
    [Fact]
    public void RunOpensSettingsAndReturnsToMainMenu()
    {
        AgsTestState.ResetCurrentSettings();
        AgsSettings.SetCurrent(new AgsSettings(false, false));
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var console = new ConsoleRedirectionScope(string.Join(Environment.NewLine,
            "2", "0", "3", string.Empty));
        MainMenuSubsystem.Run();
        Assert.Contains("Settings", console.Output);
        Assert.Contains("Application is shutting down.", console.Output);
    }
}