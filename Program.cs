const string AgsDirectoryName = ".ags";
const string ConfigFileName = "config";

var currentDirectory = Directory.GetCurrentDirectory();
var agsDirectoryPath = Path.Combine(currentDirectory, AgsDirectoryName);

if (Directory.Exists(agsDirectoryPath))
{
    var configPath = Path.Combine(agsDirectoryPath, ConfigFileName);
    var canReadSettings = AgsSettings.TryReadFromConfig(configPath, out var settings);

    if (!canReadSettings || settings.AreAllDisabled)
    {
        Console.WriteLine("Existing settings are missing/invalid or both integrations are disabled. Starting setup...");
        SetupSubsystem.Run(currentDirectory, agsDirectoryPath);
        return;
    }

    Console.WriteLine(".ags configuration already found and loaded. Initialization is not required.");
    return;
}

SetupSubsystem.Run(currentDirectory, agsDirectoryPath);
