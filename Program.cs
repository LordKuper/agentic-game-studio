using AGS;
using AGS.subsystems;

foreach (var argument in args)
{
    if (string.Equals(argument, "-version", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "-v", StringComparison.OrdinalIgnoreCase))
    {
        var assembly = typeof(AgsSettings).Assembly;
        var assemblyVersion = assembly.GetName().Version;
        var applicationVersion = assemblyVersion == null ? "0.0.0.0" : assemblyVersion.ToString();
        Console.WriteLine(applicationVersion);
        return;
    }
}
var currentDirectory = Directory.GetCurrentDirectory();
var agsDirectoryPath = Path.Combine(currentDirectory, AgsSettings.AgsDirectoryName);
if (Directory.Exists(agsDirectoryPath))
{
    var configPath = Path.Combine(agsDirectoryPath, AgsSettings.ConfigFileName);
    var canReadSettings = AgsSettings.TryReadFromConfig(configPath, out var settings);
    if (!canReadSettings)
    {
        canReadSettings = AgsSettings.TryMigrateLegacyConfig(agsDirectoryPath, out settings);
        if (canReadSettings)
            Console.WriteLine("Legacy configuration migrated to .ags/config.json.");
    }
    if (!canReadSettings || settings.AreAllModelsDisabled)
    {
        Console.WriteLine(
            "Existing settings are missing/invalid or both integrations are disabled. Starting setup...");
        SetupSubsystem.Run(currentDirectory, agsDirectoryPath);
        return;
    }
    Console.WriteLine(
        ".ags configuration already found and loaded. Initialization is not required.");
    return;
}
SetupSubsystem.Run(currentDirectory, agsDirectoryPath);