using System.Reflection;
using AGS.subsystems;

namespace AGS.Tests;

/// <summary>
///     Covers main menu prompt flow and menu construction helpers.
/// </summary>
public sealed class MainMenuSubsystemTests
{
    /// <summary>
    ///     Verifies that the menu options are built in the expected order.
    /// </summary>
    [Fact]
    public void BuildOptionsReturnsDefaultOptions()
    {
        var options = (Array)PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "BuildOptions");
        var labelProperty = options.GetType().GetElementType().GetProperty("Label",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(3, options.Length);
        Assert.NotNull(labelProperty);
        Assert.Equal("Start", labelProperty.GetValue(options.GetValue(0)));
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
        Assert.Equal(["Start", "Settings", "Exit"], labels);
    }

    /// <summary>
    ///     Verifies that the main menu exits cleanly when the exit option is selected.
    /// </summary>
    [Fact]
    public void RunExitsWhenExitOptionIsSelected()
    {
        using var startStub = new StartSkillActionStubScope();
        using var prompts = new PromptStubScope(selectionIndexes: [2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(["Main menu"], prompts.SelectMessages);
        Assert.Contains("Application is shutting down.", console.Output);
    }

    /// <summary>
    ///     Verifies that selecting Start invokes the start skill action and returns to the menu.
    /// </summary>
    [Fact]
    public void RunInvokesStartSkillActionWhenStartIsSelected()
    {
        using var startStub = new StartSkillActionStubScope();
        using var prompts = new PromptStubScope(selectionIndexes: [0, 2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(1, startStub.InvocationCount);
    }

    /// <summary>
    ///     Verifies that selecting Start returns to the menu loop until exit is chosen.
    /// </summary>
    [Fact]
    public void RunLoopsAfterStartSelection()
    {
        using var startStub = new StartSkillActionStubScope();
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
        AgsSettings.SetCurrent(new AgsSettings(false, true, AgsSettings.DefaultRateLimitCooldownMinutes,
            null, ["chatgpt"]));
        using var startStub = new StartSkillActionStubScope();
        using var tempDirectory = new TemporaryDirectoryScope();
        using var currentDirectory = new CurrentDirectoryScope(tempDirectory.Path);
        using var prompts = new PromptStubScope(selectionIndexes: [1, 4, 2]);
        using var console = new ConsoleRedirectionScope(string.Empty);
        MainMenuSubsystem.Run();
        Assert.Equal(["Main menu", "Settings", "Main menu"], prompts.SelectMessages);
        Assert.Contains("Application is shutting down.", console.Output);
    }

    /// <summary>
    ///     Verifies that start invocation failures are surfaced to the user.
    /// </summary>
    [Fact]
    public void PrintStartFailureWritesActionableErrorDetails()
    {
        using var console = new ConsoleRedirectionScope(string.Empty);
        var providerResult = AGS.ai.AIProviderResult.Failed("No default AI provider is configured.", 1,
            "Set 'default-models' in the project settings.");
        var invocationResult = new AGS.orchestration.AgentInvocationResult(string.Empty,
            providerResult, []);
        var result = new AGS.skills.SkillInvocationResult("ags-start", invocationResult);

        PrivateAccess.InvokeStatic(typeof(MainMenuSubsystem), "PrintStartFailure", result);

        Assert.Contains("Failed to start AGS workflow.", console.Output);
        Assert.Contains("No default AI provider is configured.", console.Output);
        Assert.Contains("Set 'default-models' in the project settings.", console.Output);
        Assert.Contains("Exit code: 1", console.Output);
    }
}
