using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace AGS.Tests;

/// <summary>
///     Provides reflection helpers for invoking non-public members in the production assembly.
/// </summary>
internal static class PrivateAccess
{
    /// <summary>
    ///     Gets the value of a non-public static field.
    /// </summary>
    /// <typeparam name="T">Expected field value type.</typeparam>
    /// <param name="type">Type that declares the field.</param>
    /// <param name="fieldName">Name of the field to read.</param>
    /// <returns>The field value cast to <typeparamref name="T" />.</returns>
    internal static T GetStaticField<T>(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return Assert.IsType<T>(field.GetValue(null));
    }

    /// <summary>
    ///     Gets a non-public static method from the specified type.
    /// </summary>
    /// <param name="type">Type that declares the method.</param>
    /// <param name="methodName">Name of the method to locate.</param>
    /// <returns>The requested method information.</returns>
    internal static MethodInfo GetStaticMethod(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method;
    }

    /// <summary>
    ///     Invokes a non-public static method on the specified type.
    /// </summary>
    /// <param name="type">Type that declares the method.</param>
    /// <param name="methodName">Name of the method to invoke.</param>
    /// <param name="arguments">Arguments passed to the method.</param>
    /// <returns>Return value produced by the invoked method.</returns>
    internal static object InvokeStatic(Type type, string methodName, params object[] arguments)
    {
        var method = GetStaticMethod(type, methodName);
        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    /// <summary>
    ///     Sets the value of a non-public static field.
    /// </summary>
    /// <param name="type">Type that declares the field.</param>
    /// <param name="fieldName">Name of the field to update.</param>
    /// <param name="value">Value assigned to the field.</param>
    internal static void SetStaticField(Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        field.SetValue(null, value);
    }
}

/// <summary>
///     Creates and cleans up a temporary directory for a test.
/// </summary>
internal sealed class TemporaryDirectoryScope : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryDirectoryScope" /> class.
    /// </summary>
    internal TemporaryDirectoryScope()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            "ags-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    /// <summary>
    ///     Gets the absolute path to the temporary directory.
    /// </summary>
    internal string Path { get; }

    /// <summary>
    ///     Deletes the temporary directory and its contents.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

/// <summary>
///     Temporarily changes the current working directory for a test.
/// </summary>
internal sealed class CurrentDirectoryScope : IDisposable
{
    private readonly string originalCurrentDirectory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentDirectoryScope" /> class.
    /// </summary>
    /// <param name="currentDirectory">Current directory to use within the scope.</param>
    internal CurrentDirectoryScope(string currentDirectory)
    {
        originalCurrentDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(currentDirectory);
    }

    /// <summary>
    ///     Restores the original current working directory.
    /// </summary>
    public void Dispose()
    {
        Directory.SetCurrentDirectory(originalCurrentDirectory);
    }
}

/// <summary>
///     Redirects console input, output, and error streams for a test.
/// </summary>
internal sealed class ConsoleRedirectionScope : IDisposable
{
    private readonly TextWriter originalError;
    private readonly TextReader originalInput;
    private readonly TextWriter originalOutput;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConsoleRedirectionScope" /> class.
    /// </summary>
    /// <param name="input">Console input content to expose through <see cref="Console.In" />.</param>
    internal ConsoleRedirectionScope(string input)
    {
        originalInput = Console.In;
        originalOutput = Console.Out;
        originalError = Console.Error;
        OutputWriter = new StringWriter();
        ErrorWriter = new StringWriter();
        Console.SetIn(new StringReader(input));
        Console.SetOut(OutputWriter);
        Console.SetError(ErrorWriter);
    }

    /// <summary>
    ///     Gets the captured standard error text.
    /// </summary>
    internal string Error => ErrorWriter.ToString();

    /// <summary>
    ///     Gets the writer that captures standard error.
    /// </summary>
    internal StringWriter ErrorWriter { get; }

    /// <summary>
    ///     Gets the captured standard output text.
    /// </summary>
    internal string Output => OutputWriter.ToString();

    /// <summary>
    ///     Gets the writer that captures standard output.
    /// </summary>
    internal StringWriter OutputWriter { get; }

    /// <summary>
    ///     Restores the original console streams.
    /// </summary>
    public void Dispose()
    {
        Console.SetIn(originalInput);
        Console.SetOut(originalOutput);
        Console.SetError(originalError);
        OutputWriter.Dispose();
        ErrorWriter.Dispose();
    }
}

/// <summary>
///     Resets process-wide AGS state between tests.
/// </summary>
internal static class AgsTestState
{
    /// <summary>
    ///     Clears the process-wide current settings instance.
    /// </summary>
    internal static void ResetCurrentSettings()
    {
        PrivateAccess.SetStaticField(typeof(AgsSettings), "currentSettings",
            new AgsSettings(false, false));
        PrivateAccess.SetStaticField(typeof(AgsSettings), "hasCurrentSettings", false);
    }
}

/// <summary>
///     Temporarily replaces installer script files in the test output directory.
/// </summary>
internal sealed class InstallerScriptsScope : IDisposable
{
    private readonly List<InstallerScriptBackup> backups = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallerScriptsScope" /> class.
    /// </summary>
    /// <param name="definitions">Installer scripts to create for the scope lifetime.</param>
    internal InstallerScriptsScope(params InstallerScriptDefinition[] definitions)
    {
        var scriptsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "scripts");
        Directory.CreateDirectory(scriptsDirectoryPath);
        foreach (var definition in definitions)
        {
            var scriptPath = Path.Combine(scriptsDirectoryPath, definition.FileName);
            var hadOriginalContent = File.Exists(scriptPath);
            var originalContent = hadOriginalContent ? File.ReadAllText(scriptPath) : string.Empty;
            backups.Add(new InstallerScriptBackup(scriptPath, hadOriginalContent, originalContent));
            File.WriteAllText(scriptPath, definition.Content, Encoding.UTF8);
        }
    }

    /// <summary>
    ///     Restores the original installer scripts.
    /// </summary>
    public void Dispose()
    {
        foreach (var backup in backups)
        {
            if (backup.HadOriginalContent)
                File.WriteAllText(backup.ScriptPath, backup.OriginalContent, Encoding.UTF8);
            else if (File.Exists(backup.ScriptPath))
                File.Delete(backup.ScriptPath);
        }
    }
}

/// <summary>
///     Describes a temporary installer script file used by a test.
/// </summary>
internal readonly struct InstallerScriptDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallerScriptDefinition" /> struct.
    /// </summary>
    /// <param name="fileName">Installer file name created in the scripts directory.</param>
    /// <param name="content">PowerShell script content written to the file.</param>
    internal InstallerScriptDefinition(string fileName, string content)
    {
        FileName = fileName;
        Content = content;
    }

    /// <summary>
    ///     Gets the installer file name created in the scripts directory.
    /// </summary>
    internal string FileName { get; }

    /// <summary>
    ///     Gets the PowerShell script content written to the file.
    /// </summary>
    internal string Content { get; }
}

/// <summary>
///     Stores the original state of a script file that is temporarily replaced during a test.
/// </summary>
internal readonly struct InstallerScriptBackup
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallerScriptBackup" /> struct.
    /// </summary>
    /// <param name="scriptPath">Absolute path to the script file.</param>
    /// <param name="hadOriginalContent">
    ///     <see langword="true" /> when the file existed before the test; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <param name="originalContent">Original script content when the file existed.</param>
    internal InstallerScriptBackup(string scriptPath, bool hadOriginalContent,
        string originalContent)
    {
        ScriptPath = scriptPath;
        HadOriginalContent = hadOriginalContent;
        OriginalContent = originalContent;
    }

    /// <summary>
    ///     Gets the absolute path to the script file.
    /// </summary>
    internal string ScriptPath { get; }

    /// <summary>
    ///     Gets a value indicating whether the file existed before the test.
    /// </summary>
    internal bool HadOriginalContent { get; }

    /// <summary>
    ///     Gets the original script content when the file existed before the test.
    /// </summary>
    internal string OriginalContent { get; }
}