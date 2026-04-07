namespace AGS.skills;

/// <summary>
///     Defines the contract for invoking AGS skills via an AI provider.
/// </summary>
internal interface ISkillRunner
{
    /// <summary>
    ///     Invokes a skill by name and returns the result.
    /// </summary>
    /// <param name="request">Skill invocation request.</param>
    /// <returns>The result of the invocation.</returns>
    SkillInvocationResult InvokeSkill(SkillInvocationRequest request);
}
