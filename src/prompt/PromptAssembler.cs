using System.Text;
using AGS.ai;

namespace AGS.prompt;

/// <summary>
///     Assembles system prompts for AI provider invocations from agent definitions, rules, and
///     session context.
/// </summary>
/// <remarks>
///     Assembly order follows the plan specification:
///     <list type="number">
///         <item>Agent definition — establishes the agent's role, responsibilities, and constraints.</item>
///         <item>Rules — constrain the agent's behaviour across all tasks.</item>
///         <item>Task context — scopes the agent's work to the current session and task.</item>
///         <item>CEO instructions — override or focus the above for the current invocation.</item>
///     </list>
/// </remarks>
internal sealed class PromptAssembler
{
    private readonly ResourceLoader resourceLoader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PromptAssembler" /> class.
    /// </summary>
    /// <param name="resourceLoader">Resource loader used to resolve agent and rule files.</param>
    internal PromptAssembler(ResourceLoader resourceLoader)
    {
        this.resourceLoader = resourceLoader ??
                              throw new ArgumentNullException(nameof(resourceLoader));
    }

    /// <summary>
    ///     Assembles the system prompt from an agent definition, applicable rules, and context.
    /// </summary>
    /// <param name="agentName">Logical name of the agent (e.g. <c>game-designer</c>).</param>
    /// <param name="ruleNames">
    ///     Ordered list of rule names to include (e.g. <c>session-workflow</c>,
    ///     <c>agent-coordination</c>).
    /// </param>
    /// <param name="context">Session and task context for the current invocation.</param>
    /// <returns>Assembled system prompt text.</returns>
    internal string AssembleSystemPrompt(string agentName, IReadOnlyList<string> ruleNames,
        PromptContext context)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            throw new ArgumentException("Agent name must be provided.", nameof(agentName));
        ruleNames ??= Array.Empty<string>();
        context ??= new PromptContext();

        var agentMarkdown = resourceLoader.ReadResource("agents", agentName);
        var agentDefinition = AgentDefinitionParser.Parse(agentMarkdown);

        var sb = new StringBuilder();
        AppendAgentSection(sb, agentDefinition);

        if (ruleNames.Count > 0)
        {
            AppendSectionDivider(sb);
            AppendRulesSection(sb, ruleNames);
        }

        var hasContext = !string.IsNullOrWhiteSpace(context.SessionScope) ||
                         !string.IsNullOrWhiteSpace(context.TaskBrief) ||
                         !string.IsNullOrWhiteSpace(context.CeoInstructions);
        if (hasContext)
        {
            AppendSectionDivider(sb);
            AppendContextSection(sb, context);
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Builds a complete <see cref="AIProviderRequest" /> ready for submission to an AI provider.
    /// </summary>
    /// <param name="agentName">Logical name of the agent (e.g. <c>game-designer</c>).</param>
    /// <param name="ruleNames">Ordered list of rule names to include in the system prompt.</param>
    /// <param name="context">Session and task context for the current invocation.</param>
    /// <param name="taskPrompt">The user-facing task instruction passed as the primary prompt.</param>
    /// <param name="workingDirectory">Working directory for the AI subprocess.</param>
    /// <param name="timeout">Maximum time to wait for a provider response.</param>
    /// <param name="providerArguments">Optional additional provider-specific arguments.</param>
    /// <returns>A fully assembled <see cref="AIProviderRequest" />.</returns>
    internal AIProviderRequest BuildRequest(
        string agentName,
        IReadOnlyList<string> ruleNames,
        PromptContext context,
        string taskPrompt,
        string workingDirectory,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string> providerArguments = null)
    {
        var systemPrompt = AssembleSystemPrompt(agentName, ruleNames, context);
        return new AIProviderRequest(systemPrompt, taskPrompt, workingDirectory, timeout,
            providerArguments);
    }

    /// <summary>
    ///     Appends the agent identity section: name, description, constraints, and practical guidance.
    /// </summary>
    /// <param name="sb">String builder to append to.</param>
    /// <param name="agent">Parsed agent definition.</param>
    private static void AppendAgentSection(StringBuilder sb, AgentDefinition agent)
    {
        sb.AppendLine($"# Agent: {agent.Name}");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(agent.Description))
        {
            sb.AppendLine(agent.Description);
            sb.AppendLine();
        }

        if (agent.MustNot.Count > 0)
        {
            sb.AppendLine("## Must Not");
            sb.AppendLine();
            foreach (var prohibition in agent.MustNot) sb.AppendLine($"- {prohibition}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(agent.PracticalGuidance))
        {
            sb.AppendLine("## Practical Guidance");
            sb.AppendLine();
            sb.AppendLine(agent.PracticalGuidance);
            sb.AppendLine();
        }
    }

    /// <summary>
    ///     Appends the rules section, loading each rule file via the resource loader.
    /// </summary>
    /// <param name="sb">String builder to append to.</param>
    /// <param name="ruleNames">Ordered list of logical rule names to load and include.</param>
    private void AppendRulesSection(StringBuilder sb, IReadOnlyList<string> ruleNames)
    {
        sb.AppendLine("# Rules");
        sb.AppendLine();
        foreach (var ruleName in ruleNames)
        {
            var ruleContent = resourceLoader.ReadResource("rules", ruleName);
            sb.AppendLine($"## {ruleName}");
            sb.AppendLine();
            sb.AppendLine(ruleContent.Trim());
            sb.AppendLine();
        }
    }

    /// <summary>
    ///     Appends the context section containing session scope, current task, and CEO instructions.
    /// </summary>
    /// <param name="sb">String builder to append to.</param>
    /// <param name="context">Session and task context for the current invocation.</param>
    private static void AppendContextSection(StringBuilder sb, PromptContext context)
    {
        sb.AppendLine("# Context");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.SessionScope))
        {
            sb.AppendLine("## Session Scope");
            sb.AppendLine();
            sb.AppendLine(context.SessionScope.Trim());
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(context.TaskBrief))
        {
            sb.AppendLine("## Current Task");
            sb.AppendLine();
            sb.AppendLine(context.TaskBrief.Trim());
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(context.CeoInstructions))
        {
            sb.AppendLine("## CEO Instructions");
            sb.AppendLine();
            sb.AppendLine(context.CeoInstructions.Trim());
            sb.AppendLine();
        }
    }

    /// <summary>
    ///     Appends a horizontal rule divider between major sections.
    /// </summary>
    /// <param name="sb">String builder to append to.</param>
    private static void AppendSectionDivider(StringBuilder sb)
    {
        sb.AppendLine("---");
        sb.AppendLine();
    }
}
