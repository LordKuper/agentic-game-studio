using AGS.orchestration;

namespace AGS.skills;

/// <summary>
///     Invokes AGS skills by instructing the AI provider to use the named skill. Skills are
///     discovered natively by the provider from its own skill directory (populated by
///     <see cref="SkillSynchronizer" /> before invocation).
/// </summary>
internal sealed class SkillRunner : ISkillRunner
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);

    private readonly IAgentOrchestrator orchestrator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SkillRunner" /> class.
    /// </summary>
    /// <param name="orchestrator">Orchestrator used to invoke the default AI provider.</param>
    internal SkillRunner(IAgentOrchestrator orchestrator)
    {
        this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public SkillInvocationResult InvokeSkill(SkillInvocationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var taskPrompt = BuildTaskPrompt(request);
        var timeout = request.Timeout > TimeSpan.Zero ? request.Timeout : DefaultTimeout;

        var result = orchestrator.InvokeDefault(
            string.Empty,
            taskPrompt,
            request.WorkingDirectory,
            timeout);

        return new SkillInvocationResult(request.SkillName, result);
    }

    /// <summary>
    ///     Builds the task prompt instructing the AI to execute the named skill.
    /// </summary>
    private static string BuildTaskPrompt(SkillInvocationRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Context)
            ? $"Use skill {request.SkillName}."
            : $"Use skill {request.SkillName}. {request.Context}";
    }
}
