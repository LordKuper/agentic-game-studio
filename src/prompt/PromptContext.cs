namespace AGS.prompt;

/// <summary>
///     Carries the session and task context used when assembling a prompt.
/// </summary>
internal sealed class PromptContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PromptContext" /> class.
    /// </summary>
    /// <param name="taskBrief">Current task brief text from the session task directory.</param>
    /// <param name="sessionScope">Approved session scope document text.</param>
    /// <param name="ceoInstructions">Any specific instructions provided by the CEO for this invocation.</param>
    internal PromptContext(string taskBrief = null, string sessionScope = null,
        string ceoInstructions = null)
    {
        TaskBrief = taskBrief ?? string.Empty;
        SessionScope = sessionScope ?? string.Empty;
        CeoInstructions = ceoInstructions ?? string.Empty;
    }

    /// <summary>
    ///     Gets the current task brief text from the session task directory.
    /// </summary>
    internal string TaskBrief { get; }

    /// <summary>
    ///     Gets the approved session scope document text.
    /// </summary>
    internal string SessionScope { get; }

    /// <summary>
    ///     Gets any specific instructions provided by the CEO for this invocation.
    /// </summary>
    internal string CeoInstructions { get; }
}
