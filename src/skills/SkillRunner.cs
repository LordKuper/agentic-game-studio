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
    private const string OutputSchemaResourceName = "ai-output-schema.json";

    private readonly IAgentOrchestrator orchestrator;
    private readonly ResourceLoader resourceLoader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SkillRunner" /> class.
    /// </summary>
    /// <param name="orchestrator">Orchestrator used to invoke the default AI provider.</param>
    /// <param name="resourceLoader">Resource loader used to resolve the output schema template.</param>
    internal SkillRunner(IAgentOrchestrator orchestrator, ResourceLoader resourceLoader)
    {
        this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        this.resourceLoader =
            resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
    }

    /// <inheritdoc />
    public SkillInvocationResult InvokeSkill(SkillInvocationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var taskPrompt = BuildTaskPrompt(request);
        var timeout = request.Timeout > TimeSpan.Zero ? request.Timeout : DefaultTimeout;
        var outputSchemaPath = ResolveOutputSchemaPath();

        var result = orchestrator.InvokeDefault(
            string.Empty,
            taskPrompt,
            request.WorkingDirectory,
            timeout,
            outputSchemaPath);

        return new SkillInvocationResult(request.SkillName, result);
    }

    /// <summary>
    ///     Resolves the output schema path via the resource loader.
    ///     Returns an empty string when the schema file is not found.
    /// </summary>
    private string ResolveOutputSchemaPath()
    {
        try
        {
            return resourceLoader.ResolveResourcePath("templates", OutputSchemaResourceName);
        }
        catch (FileNotFoundException)
        {
            return string.Empty;
        }
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
