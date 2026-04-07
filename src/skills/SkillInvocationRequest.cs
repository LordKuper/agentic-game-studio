namespace AGS.skills;

/// <summary>
///     Represents a request to invoke a skill by name.
/// </summary>
internal sealed class SkillInvocationRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SkillInvocationRequest" /> class.
    /// </summary>
    /// <param name="skillName">
    ///     Logical name of the skill to invoke (e.g. <c>ags-start</c>).
    /// </param>
    /// <param name="workingDirectory">Working directory for the AI subprocess.</param>
    /// <param name="timeout">
    ///     Maximum time to wait for the AI provider. Pass <see cref="TimeSpan.Zero" /> to use
    ///     the <see cref="SkillRunner" /> default timeout.
    /// </param>
    /// <param name="context">
    ///     Optional additional context appended to the task prompt. Pass <see langword="null" />
    ///     when no extra context is needed.
    /// </param>
    internal SkillInvocationRequest(string skillName, string workingDirectory, TimeSpan timeout,
        string context = null)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            throw new ArgumentException("Skill name must be provided.", nameof(skillName));
        if (string.IsNullOrWhiteSpace(workingDirectory))
            throw new ArgumentException("Working directory must be provided.",
                nameof(workingDirectory));
        SkillName = skillName;
        WorkingDirectory = workingDirectory;
        Timeout = timeout;
        Context = context ?? string.Empty;
    }

    internal string SkillName { get; }
    internal string WorkingDirectory { get; }
    internal TimeSpan Timeout { get; }

    /// <summary>
    ///     Gets additional context appended to the task prompt. Empty when no context was provided.
    /// </summary>
    internal string Context { get; }
}
