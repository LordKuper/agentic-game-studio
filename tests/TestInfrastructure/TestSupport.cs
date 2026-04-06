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

    /// <summary>
    ///     Restores the default prompt handlers used by the application.
    /// </summary>
    internal static void ResetPromptHandlers()
    {
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "confirmHandler",
            (Func<string, bool, bool>)((message, defaultValue) =>
                Sharprompt.Prompt.Confirm(message, defaultValue: defaultValue)));
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "selectHandler",
            (Func<string, IReadOnlyList<string>, string>)((message, options) =>
                Sharprompt.Prompt.Select(message, [.. options])));
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "textHandler",
            (Func<string, string, string>)((message, defaultValue) =>
                Sharprompt.Prompt.Input<string>(message, defaultValue: defaultValue)));
    }
}

/// <summary>
///     Temporarily replaces prompt handlers with deterministic test responses.
/// </summary>
internal sealed class PromptStubScope : IDisposable
{
    private readonly Func<string, bool, bool> originalConfirmHandler;
    private readonly Func<string, IReadOnlyList<string>, string> originalSelectHandler;
    private readonly Func<string, string, string> originalTextHandler;
    private readonly Queue<bool> queuedConfirmations;
    private readonly Queue<string> queuedInputs;
    private readonly Queue<int> queuedSelections;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PromptStubScope" /> class.
    /// </summary>
    /// <param name="confirmations">Queued confirmation responses returned to the caller.</param>
    /// <param name="selectionIndexes">Queued zero-based selection indexes returned to the caller.</param>
    /// <param name="inputs">Queued text inputs returned to the caller.</param>
    internal PromptStubScope(IEnumerable<bool> confirmations = null,
        IEnumerable<int> selectionIndexes = null, IEnumerable<string> inputs = null)
    {
        originalConfirmHandler = PrivateAccess.GetStaticField<Func<string, bool, bool>>(
            typeof(AgsPrompt), "confirmHandler");
        originalSelectHandler =
            PrivateAccess.GetStaticField<Func<string, IReadOnlyList<string>, string>>(
                typeof(AgsPrompt), "selectHandler");
        originalTextHandler =
            PrivateAccess.GetStaticField<Func<string, string, string>>(
                typeof(AgsPrompt), "textHandler");
        queuedConfirmations = confirmations == null ? [] : new Queue<bool>(confirmations);
        queuedSelections = selectionIndexes == null ? [] : new Queue<int>(selectionIndexes);
        queuedInputs = inputs == null ? [] : new Queue<string>(inputs);
        ConfirmMessages = [];
        InputDefaultValues = [];
        InputMessages = [];
        SelectMessages = [];
        SelectOptions = [];
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "confirmHandler",
            (Func<string, bool, bool>)((message, defaultValue) =>
            {
                ConfirmMessages.Add(message);
                if (queuedConfirmations.Count == 0) return defaultValue;
                return queuedConfirmations.Dequeue();
            }));
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "selectHandler",
            (Func<string, IReadOnlyList<string>, string>)((message, options) =>
            {
                SelectMessages.Add(message);
                SelectOptions.Add(options.ToArray());
                Assert.NotEmpty(options);
                if (queuedSelections.Count == 0)
                    throw new InvalidOperationException("No queued selection is available.");
                var selectedIndex = queuedSelections.Dequeue();
                Assert.InRange(selectedIndex, 0, options.Count - 1);
                return options[selectedIndex];
            }));
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "textHandler",
            (Func<string, string, string>)((message, defaultValue) =>
            {
                InputMessages.Add(message);
                InputDefaultValues.Add(defaultValue);
                if (queuedInputs.Count == 0) return defaultValue;
                return queuedInputs.Dequeue();
            }));
    }

    /// <summary>
    ///     Gets the confirmation prompt messages that were shown during the scope lifetime.
    /// </summary>
    internal List<string> ConfirmMessages { get; }

    /// <summary>
    ///     Gets the default values shown for each text input prompt during the scope lifetime.
    /// </summary>
    internal List<string> InputDefaultValues { get; }

    /// <summary>
    ///     Gets the text input prompt messages that were shown during the scope lifetime.
    /// </summary>
    internal List<string> InputMessages { get; }

    /// <summary>
    ///     Gets the selection prompt messages that were shown during the scope lifetime.
    /// </summary>
    internal List<string> SelectMessages { get; }

    /// <summary>
    ///     Gets the option lists shown for each selection prompt during the scope lifetime.
    /// </summary>
    internal List<string[]> SelectOptions { get; }

    /// <summary>
    ///     Restores the original prompt handlers.
    /// </summary>
    public void Dispose()
    {
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "confirmHandler", originalConfirmHandler);
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "selectHandler", originalSelectHandler);
        PrivateAccess.SetStaticField(typeof(AgsPrompt), "textHandler", originalTextHandler);
    }
}

/// <summary>
///     Temporarily replaces the provider factory in <see cref="AGS.subsystems.ProviderCheckSubsystem" />
///     with a stub that returns a single always-available or always-unavailable provider.
/// </summary>
internal sealed class ProviderCheckStubScope : IDisposable
{
    private readonly Func<IReadOnlyList<AGS.ai.IAIProvider>> originalFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProviderCheckStubScope" /> class.
    /// </summary>
    /// <param name="available">
    ///     <see langword="true" /> to simulate an installed provider;
    ///     <see langword="false" /> to simulate no installed providers.
    /// </param>
    internal ProviderCheckStubScope(bool available)
    {
        originalFactory = PrivateAccess
            .GetStaticField<Func<IReadOnlyList<AGS.ai.IAIProvider>>>(
                typeof(AGS.subsystems.ProviderCheckSubsystem), "providersFactory");
        PrivateAccess.SetStaticField(
            typeof(AGS.subsystems.ProviderCheckSubsystem),
            "providersFactory",
            (Func<IReadOnlyList<AGS.ai.IAIProvider>>)(() =>
                new[] { (AGS.ai.IAIProvider)new StubProvider(available) }));
    }

    /// <summary>
    ///     Restores the original provider factory.
    /// </summary>
    public void Dispose()
    {
        PrivateAccess.SetStaticField(
            typeof(AGS.subsystems.ProviderCheckSubsystem), "providersFactory", originalFactory);
    }

    private sealed class StubProvider : AGS.ai.IAIProvider
    {
        private readonly bool available;
        internal StubProvider(bool available) => this.available = available;
        public string ProviderId => "stub";
        public bool IsAvailable => available;
        public bool TryGetVersion(out string version)
        {
            version = available ? "1.0.0-stub" : string.Empty;
            return available;
        }
        public AGS.ai.AIProviderResult Invoke(AGS.ai.AIProviderRequest request)
            => AGS.ai.AIProviderResult.Succeeded(string.Empty, 0, []);
    }
}
