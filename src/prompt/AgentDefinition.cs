namespace AGS.prompt;

/// <summary>
///     Represents a parsed agent definition loaded from an agent markdown file.
/// </summary>
internal sealed class AgentDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentDefinition" /> class.
    /// </summary>
    /// <param name="name">Unique agent identifier in kebab-case.</param>
    /// <param name="description">One-paragraph description of the agent's purpose and responsibilities.</param>
    /// <param name="mustNot">Explicit list of behaviors this agent must never perform.</param>
    /// <param name="models">Priority-ordered list of suitable AI model names.</param>
    /// <param name="maxIterations">Maximum number of execution iterations allowed per invocation.</param>
    /// <param name="practicalGuidance">Optional role-specific tactical guidance for the agent.</param>
    internal AgentDefinition(
        string name,
        string description,
        IReadOnlyList<string> mustNot,
        IReadOnlyList<string> models,
        int maxIterations,
        string practicalGuidance)
    {
        Name = name ?? string.Empty;
        Description = description ?? string.Empty;
        MustNot = mustNot ?? Array.Empty<string>();
        Models = models ?? Array.Empty<string>();
        MaxIterations = maxIterations;
        PracticalGuidance = practicalGuidance ?? string.Empty;
    }

    /// <summary>
    ///     Gets the unique agent identifier in kebab-case (e.g. <c>game-designer</c>).
    /// </summary>
    internal string Name { get; }

    /// <summary>
    ///     Gets the one-paragraph description of the agent's purpose and responsibilities.
    /// </summary>
    internal string Description { get; }

    /// <summary>
    ///     Gets the explicit list of behaviors this agent must never perform.
    /// </summary>
    internal IReadOnlyList<string> MustNot { get; }

    /// <summary>
    ///     Gets the priority-ordered list of suitable AI model names.
    /// </summary>
    internal IReadOnlyList<string> Models { get; }

    /// <summary>
    ///     Gets the maximum number of execution iterations allowed per invocation.
    /// </summary>
    internal int MaxIterations { get; }

    /// <summary>
    ///     Gets optional role-specific tactical guidance for the agent.
    /// </summary>
    internal string PracticalGuidance { get; }
}
