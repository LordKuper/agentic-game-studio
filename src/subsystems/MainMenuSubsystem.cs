namespace AGS.subsystems;

/// <summary>
///     Displays the main application menu after initialization completes.
/// </summary>
internal static class MainMenuSubsystem
{
    private static Action clearConsoleHandler = Console.Clear;

    private static Func<bool>
        isOutputRedirectedProvider = () => Console.IsOutputRedirected;

    private static Func<IReadOnlyList<string>> unfinishedSessionNamesProvider =
        GetUnfinishedSessionNames;

    /// <summary>
    ///     Builds the visible main menu options for the current application state.
    /// </summary>
    /// <returns>Ordered menu options to display.</returns>
    private static MainMenuOption[] BuildOptions()
    {
        var options = new List<MainMenuOption>();
        foreach (var sessionName in unfinishedSessionNamesProvider())
        {
            options.Add(new MainMenuOption($"Continue session {sessionName}",
                MainMenuOptionKind.ContinueSession, sessionName));
        }
        options.Add(new MainMenuOption("Start a new session", MainMenuOptionKind.StartNewSession,
            string.Empty));
        options.Add(new MainMenuOption("Settings", MainMenuOptionKind.Settings, string.Empty));
        options.Add(new MainMenuOption("Exit", MainMenuOptionKind.Exit, string.Empty));
        return [.. options];
    }

    /// <summary>
    ///     Clears the console before the main menu is rendered so repeated selections do not stack
    ///     multiple menu screens on top of each other.
    /// </summary>
    private static void ClearConsoleForMainMenu()
    {
        if (isOutputRedirectedProvider()) return;
        try
        {
            clearConsoleHandler();
        }
        catch (IOException) { }
        catch (ArgumentOutOfRangeException) { }
        catch (InvalidOperationException) { }
        catch (PlatformNotSupportedException) { }
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
    ///     Gets unfinished session names that should appear in the main menu.
    /// </summary>
    /// <returns>
    ///     Ordered unfinished session names. The current implementation returns no sessions until
    ///     session persistence is added.
    /// </returns>
    private static IReadOnlyList<string> GetUnfinishedSessionNames()
    {
        return [];
    }

    /// <summary>
    ///     Shows the main menu until the user chooses to exit the application.
    /// </summary>
    internal static void Run()
    {
        while (true)
        {
            ClearConsoleForMainMenu();
            var options = BuildOptions();
            var selectedIndex =
                ConsoleMenu.PromptForSelection("Main menu", GetOptionLabels(options));
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
            if (selectedOption.Kind == MainMenuOptionKind.ContinueSession ||
                selectedOption.Kind == MainMenuOptionKind.StartNewSession)
                continue;
        }
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
        /// <param name="sessionName">
        ///     Session name associated with the option, or an empty string when not applicable.
        /// </param>
        internal MainMenuOption(string label, MainMenuOptionKind kind, string sessionName)
        {
            Label = label;
            Kind = kind;
            SessionName = sessionName;
        }

        /// <summary>
        ///     Gets the visible label shown in the main menu.
        /// </summary>
        internal string Label { get; }

        /// <summary>
        ///     Gets the behavior associated with the option.
        /// </summary>
        internal MainMenuOptionKind Kind { get; }

        /// <summary>
        ///     Gets the session name associated with the option, or an empty string when not
        ///     applicable.
        /// </summary>
        internal string SessionName { get; }
    }

    /// <summary>
    ///     Defines the behaviors available from the main menu.
    /// </summary>
    private enum MainMenuOptionKind
    {
        /// <summary>
        ///     Continues an unfinished session.
        /// </summary>
        ContinueSession,

        /// <summary>
        ///     Starts a new session.
        /// </summary>
        StartNewSession,

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
