namespace AGS.subsystems;

/// <summary>
///     Displays and edits persisted application settings.
/// </summary>
internal static class SettingsSubsystem
{
    private const string FallbackNavigationHint = "Type use-codex, use-claude, or 0 to return.";

    private const string InteractiveNavigationHint =
        "Use Up/Down to choose, Left/Right to set no/yes, Enter to toggle, and 0 to return.";

    private const int OptionCount = 3;
    private const int ReturnOptionIndex = 2;
    private const string Title = "Settings";
    private const int UseClaudeOptionIndex = 1;
    private const int UseCodexOptionIndex = 0;
    private static Func<bool> isInputRedirectedProvider = () => Console.IsInputRedirected;

    private static Func<bool>
        isOutputRedirectedProvider = () => Console.IsOutputRedirected;

    private static Func<ConsoleKey> readKeyProvider = () => Console.ReadKey(true).Key;

    /// <summary>
    ///     Clears the console before the settings screen is redrawn.
    /// </summary>
    private static void ClearConsole()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException) { }
        catch (ArgumentOutOfRangeException) { }
        catch (InvalidOperationException) { }
        catch (PlatformNotSupportedException) { }
    }

    /// <summary>
    ///     Formats a Boolean setting value for display in the settings screen.
    /// </summary>
    /// <param name="value">Boolean value to format.</param>
    /// <returns><c>yes</c> when <paramref name="value" /> is true; otherwise, <c>no</c>.</returns>
    private static string FormatBooleanValue(bool value)
    {
        return value ? "yes" : "no";
    }

    /// <summary>
    ///     Persists the provided settings and updates the process-wide current settings instance on
    ///     success.
    /// </summary>
    /// <param name="updatedSettings">Settings instance to persist.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string PersistSettings(AgsSettings updatedSettings)
    {
        if (!updatedSettings.TryWriteToProjectConfig(Directory.GetCurrentDirectory(),
                out var errorMessage))
            return errorMessage;
        AgsSettings.SetCurrent(updatedSettings);
        return string.Empty;
    }

    /// <summary>
    ///     Renders the current interactive settings screen.
    /// </summary>
    /// <param name="settings">Current persisted application settings.</param>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <param name="statusMessage">
    ///     Status message shown above the settings rows, or an empty string when no message should
    ///     be displayed.
    /// </param>
    private static void RenderInteractive(AgsSettings settings, int selectedIndex,
        string statusMessage)
    {
        ClearConsole();
        Console.WriteLine(Title);
        Console.WriteLine(InteractiveNavigationHint);
        if (string.IsNullOrEmpty(statusMessage))
            Console.WriteLine();
        else
            Console.WriteLine(statusMessage);
        WriteOptionLine(selectedIndex == UseCodexOptionIndex,
            $"use-codex: {FormatBooleanValue(settings.UseCodex)}");
        WriteOptionLine(selectedIndex == UseClaudeOptionIndex,
            $"use-claude: {FormatBooleanValue(settings.UseClaude)}");
        WriteOptionLine(selectedIndex == ReturnOptionIndex, "0. Return to main menu");
    }

    /// <summary>
    ///     Shows the settings screen and persists any configuration changes made by the user.
    /// </summary>
    internal static void Run()
    {
        if (!AgsSettings.HasCurrentSettings) return;
        if (isInputRedirectedProvider() || isOutputRedirectedProvider())
        {
            RunFallback();
            return;
        }
        RunInteractive();
    }

    /// <summary>
    ///     Shows a fallback settings screen for redirected input or output streams.
    /// </summary>
    private static void RunFallback()
    {
        while (true)
        {
            var settings = AgsSettings.Current;
            Console.WriteLine(Title);
            Console.WriteLine(FallbackNavigationHint);
            Console.WriteLine($"use-codex: {FormatBooleanValue(settings.UseCodex)}");
            Console.WriteLine($"use-claude: {FormatBooleanValue(settings.UseClaude)}");
            Console.WriteLine("0. Return to main menu");
            Console.Write("Enter command: ");
            var command = Console.ReadLine();
            if (string.Equals(command, "0", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(command, "use-codex", StringComparison.OrdinalIgnoreCase))
            {
                var errorMessage = ToggleSetting(UseCodexOptionIndex);
                if (!string.IsNullOrEmpty(errorMessage)) Console.WriteLine(errorMessage);
                continue;
            }
            if (string.Equals(command, "use-claude", StringComparison.OrdinalIgnoreCase))
            {
                var errorMessage = ToggleSetting(UseClaudeOptionIndex);
                if (!string.IsNullOrEmpty(errorMessage)) Console.WriteLine(errorMessage);
                continue;
            }
            Console.WriteLine("Please enter use-codex, use-claude, or 0.");
        }
    }

    /// <summary>
    ///     Shows the interactive settings screen for consoles that support key input.
    /// </summary>
    private static void RunInteractive()
    {
        var selectedIndex = 0;
        var statusMessage = string.Empty;
        while (true)
        {
            RenderInteractive(AgsSettings.Current, selectedIndex, statusMessage);
            var pressedKey = readKeyProvider();
            if (pressedKey == ConsoleKey.D0 || pressedKey == ConsoleKey.NumPad0) return;
            if (pressedKey == ConsoleKey.UpArrow)
            {
                selectedIndex = selectedIndex == 0 ? OptionCount - 1 : selectedIndex - 1;
                continue;
            }
            if (pressedKey == ConsoleKey.DownArrow)
            {
                selectedIndex = selectedIndex == OptionCount - 1 ? 0 : selectedIndex + 1;
                continue;
            }
            if (pressedKey == ConsoleKey.Enter)
            {
                if (selectedIndex == ReturnOptionIndex) return;
                statusMessage = ToggleSetting(selectedIndex);
                continue;
            }
            if (pressedKey == ConsoleKey.LeftArrow)
            {
                statusMessage = SetSettingValue(selectedIndex, false);
                continue;
            }
            if (pressedKey == ConsoleKey.RightArrow)
                statusMessage = SetSettingValue(selectedIndex, true);
        }
    }

    /// <summary>
    ///     Sets the selected Boolean setting to the requested value and persists the updated
    ///     configuration.
    /// </summary>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <param name="value">Boolean value to persist for the selected settings row.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string SetSettingValue(int selectedIndex, bool value)
    {
        var currentSettings = AgsSettings.Current;
        if (selectedIndex == UseCodexOptionIndex)
        {
            if (currentSettings.UseCodex == value) return string.Empty;
            return PersistSettings(currentSettings.WithUseCodex(value));
        }
        if (selectedIndex == UseClaudeOptionIndex)
        {
            if (currentSettings.UseClaude == value) return string.Empty;
            return PersistSettings(currentSettings.WithUseClaude(value));
        }
        return string.Empty;
    }

    /// <summary>
    ///     Toggles the selected Boolean setting and persists the updated configuration.
    /// </summary>
    /// <param name="selectedIndex">Zero-based index of the selected settings row.</param>
    /// <returns>
    ///     Error message when persistence fails; otherwise, an empty string.
    /// </returns>
    private static string ToggleSetting(int selectedIndex)
    {
        var currentSettings = AgsSettings.Current;
        if (selectedIndex == UseCodexOptionIndex)
            return PersistSettings(currentSettings.WithUseCodex(!currentSettings.UseCodex));
        if (selectedIndex == UseClaudeOptionIndex)
            return PersistSettings(currentSettings.WithUseClaude(!currentSettings.UseClaude));
        return string.Empty;
    }

    /// <summary>
    ///     Writes a selectable settings screen row with the appropriate selection marker.
    /// </summary>
    /// <param name="isSelected">
    ///     <see langword="true" /> when the row is currently selected; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <param name="text">Visible text for the settings row.</param>
    private static void WriteOptionLine(bool isSelected, string text)
    {
        var prefix = isSelected ? "> " : "  ";
        Console.WriteLine(prefix + text);
    }
}
