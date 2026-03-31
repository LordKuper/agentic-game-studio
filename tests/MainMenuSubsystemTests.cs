using System.Reflection;
using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers main menu prompt flow and menu construction helpers.
/// </summary>
public sealed class MainMenuSubsystemTests
{
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
        using var prompts = new PromptStubScope(selectionIndexes: [2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(["Main menu"], prompts.SelectMessages);
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
            using var prompts = new PromptStubScope(selectionIndexes: [0, 3]);
            using var console = new ConsoleRedirectionScope(string.Empty);
            MainMenuSubsystem.Run();
            Assert.Equal(["Main menu", "Main menu"], prompts.SelectMessages);
            Assert.Contains("Application is shutting down.", console.Output);
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
        using var prompts = new PromptStubScope(selectionIndexes: [0, 2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(["Main menu", "Main menu"], prompts.SelectMessages);
        Assert.Contains("Application is shutting down.", console.Output);
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
        using var prompts = new PromptStubScope(selectionIndexes: [1, 3, 2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(["Main menu", "Settings", "Main menu"], prompts.SelectMessages);
        Assert.Contains("Application is shutting down.", console.Output);
    }
}
