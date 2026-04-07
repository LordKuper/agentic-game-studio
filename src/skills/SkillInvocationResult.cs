using AGS.orchestration;

namespace AGS.skills;

/// <summary>
///     Represents the outcome of a skill invocation.
/// </summary>
internal sealed class SkillInvocationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SkillInvocationResult" /> class.
    /// </summary>
    /// <param name="skillName">Logical name of the skill that was invoked.</param>
    /// <param name="invocationResult">Underlying agent invocation result from the orchestrator.</param>
    internal SkillInvocationResult(string skillName, AgentInvocationResult invocationResult)
    {
        SkillName = skillName ?? string.Empty;
        InvocationResult = invocationResult ??
                           throw new ArgumentNullException(nameof(invocationResult));
    }

    internal string SkillName { get; }
    internal AgentInvocationResult InvocationResult { get; }

    /// <summary>
    ///     Gets a value indicating whether the skill completed successfully.
    /// </summary>
    internal bool Success => InvocationResult.ProviderResult.Success;
}
