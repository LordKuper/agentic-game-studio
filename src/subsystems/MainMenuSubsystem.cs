using AGS.ai;
using AGS.orchestration;
using AGS.output;
using AGS.prompt;
using AGS.skills;
using AGS.ui;

namespace AGS.subsystems;

/// <summary>
///     Displays the main application menu after initialization completes.
/// </summary>
internal static class MainMenuSubsystem
{
    /// <summary>
    ///     Replaceable action invoked when the user selects "Start". Defaults to
    ///     <see cref="DefaultStartSkillAction" />. Tests replace this with a stub via reflection.
    /// </summary>
    private static Action<DependencyCheckResult> startSkillAction = DefaultStartSkillAction;

    /// <summary>
    ///     Builds the main menu options.
    /// </summary>
    /// <returns>Ordered menu options to display.</returns>
    private static MainMenuOption[] BuildOptions()
    {
        return
        [
            new MainMenuOption("Start", MainMenuOptionKind.Start),
            new MainMenuOption("Settings", MainMenuOptionKind.Settings),
            new MainMenuOption("Exit", MainMenuOptionKind.Exit)
        ];
    }

    /// <summary>
    ///     Extracts display labels from the provided menu options.
    /// </summary>
    /// <param name="options">Ordered menu options to display.</param>
    /// <returns>Visible labels for the console menu.</returns>
    private static string[] GetOptionLabels(IReadOnlyList<MainMenuOption> options)
    {
        var labels = new string[options.Count];
        for (var index = 0; index < options.Count; index++) labels[index] = options[index].Label;
        return labels;
    }

    /// <summary>
    ///     Shows the main menu until the user chooses to exit the application.
    /// </summary>
    /// <param name="dependencies">Dependency availability flags from the startup check.</param>
    internal static void Run(DependencyCheckResult dependencies)
    {
        while (true)
        {
            var options = BuildOptions();
            var selectedIndex = AgsPrompt.Select("Main menu", GetOptionLabels(options));
            var selectedOption = options[selectedIndex];
            if (selectedOption.Kind == MainMenuOptionKind.Exit)
            {
                Console.WriteLine("Application is shutting down.");
                return;
            }
            if (selectedOption.Kind == MainMenuOptionKind.Settings)
            {
                SettingsSubsystem.Run();
                continue;
            }
            if (selectedOption.Kind == MainMenuOptionKind.Start)
            {
                startSkillAction(dependencies);
                continue;
            }
        }
    }

    /// <summary>
    ///     Default start action: synchronizes AGS skills into every enabled provider's native
    ///     directory, then invokes the <c>ags-start</c> skill via the skill runner, displays the
    ///     model response, and shows a choice menu when the response offers multiple options.
    /// </summary>
    private static void DefaultStartSkillAction(DependencyCheckResult dependencies)
    {
        var projectRoot = Directory.GetCurrentDirectory();
        var resourceLoader = new ResourceLoader(projectRoot);

        var registry = new AIProviderRegistry(projectRoot);
        registry.Register(new ClaudeCodeAdapter());
        registry.Register(new CodexAdapter());

        var synchronizer = new SkillSynchronizer(resourceLoader, registry.GetAllProviders());
        synchronizer.Synchronize(registry.GetEnabledProviders());

        var promptAssembler = new PromptAssembler(resourceLoader);
        var orchestrator = new AgentOrchestrator(resourceLoader, promptAssembler, registry);
        var runner = new SkillRunner(orchestrator, resourceLoader);
        var processor = new StructuredOutputProcessor(dependencies.JqAvailable);

        var context = string.Empty;
        while (true)
        {
            var request = new SkillInvocationRequest("ags-start", projectRoot, TimeSpan.Zero,
                context.Length > 0 ? context : null);
            var result = runner.InvokeSkill(request);

            if (!result.Success)
            {
                PrintStartFailure(result);
                return;
            }

            var structured = processor.Process(
                result.InvocationResult.ProviderResult.Output,
                result.InvocationResult.ProviderId);

            if (!string.IsNullOrWhiteSpace(structured.Message))
                Console.WriteLine(structured.Message);

            if (structured.Choices.Count > 0)
                context = ChoiceMenu.Show(structured.Choices);
            else
                break;
        }
    }

    /// <summary>
    ///     Writes a user-visible error message when the startup skill cannot be invoked
    ///     successfully.
    /// </summary>
    /// <param name="result">Failed skill invocation result.</param>
    private static void PrintStartFailure(SkillInvocationResult result)
    {
        var providerResult = result.InvocationResult.ProviderResult;
        Console.WriteLine("Failed to start AGS workflow.");
        if (!string.IsNullOrWhiteSpace(providerResult.ErrorMessage))
            Console.WriteLine(providerResult.ErrorMessage);
        if (!string.IsNullOrWhiteSpace(providerResult.Output))
            Console.WriteLine(providerResult.Output);
        if (providerResult.ExitCode != 0)
            Console.WriteLine($"Exit code: {providerResult.ExitCode}");
        Console.WriteLine("Update Settings and try again.");
    }

    /// <summary>
    ///     Represents a visible entry in the main application menu.
    /// </summary>
    private readonly struct MainMenuOption
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MainMenuOption" /> struct.
        /// </summary>
        /// <param name="label">Visible label shown in the main menu.</param>
        /// <param name="kind">Behavior associated with the option.</param>
        internal MainMenuOption(string label, MainMenuOptionKind kind)
        {
            Label = label;
            Kind = kind;
        }

        /// <summary>
        ///     Gets the visible label shown in the main menu.
        /// </summary>
        internal string Label { get; }

        /// <summary>
        ///     Gets the behavior associated with the option.
        /// </summary>
        internal MainMenuOptionKind Kind { get; }
    }

    /// <summary>
    ///     Defines the behaviors available from the main menu.
    /// </summary>
    private enum MainMenuOptionKind
    {
        /// <summary>
        ///     Starts the AGS workflow by synchronizing skills and invoking the ags-start skill.
        /// </summary>
        Start,

        /// <summary>
        ///     Opens application settings.
        /// </summary>
        Settings,

        /// <summary>
        ///     Exits the application.
        /// </summary>
        Exit
    }
}
