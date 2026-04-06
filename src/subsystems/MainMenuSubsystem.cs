namespace AGS.subsystems;

/// <summary>
///     Displays the main application menu after initialization completes.
/// </summary>
internal static class MainMenuSubsystem
{
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
    internal static void Run()
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
                // TODO: invoke ags-start skill via skill runner
                continue;
            }
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
        ///     Starts the AGS workflow by invoking the ags-start skill.
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
