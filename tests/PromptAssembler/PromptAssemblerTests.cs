using AGS.prompt;

namespace AGS.Tests;

/// <summary>
///     Covers system prompt assembly from agent definitions, rules, and session context.
/// </summary>
public sealed class PromptAssemblerTests : IDisposable
{
    private readonly TemporaryDirectoryScope installDirectory = new();
    private readonly TemporaryDirectoryScope projectRoot = new();

    private const string TestAgentMarkdown = """
        # Test Agent

        | Field | Value |
        | --- | --- |
        | `name` | `test-agent` |
        | `description` | A test agent for unit testing purposes. |
        | `must_not` | - Do prohibited thing one.<br>- Do prohibited thing two. |
        | `models` | - claude-sonnet<br>- chatgpt |
        | `max_iterations` | 10 |

        ## Practical Guidance

        - Apply test guidance at all times.
        """;

    private const string TestRuleContent = "Agents must follow the test rule at all times.";

    /// <summary>
    ///     Initializes a new instance of the <see cref="PromptAssemblerTests" /> class.
    /// </summary>
    public PromptAssemblerTests()
    {
        InstallDirectory.SetBaseDirectoryProvider(() => installDirectory.Path);
    }

    /// <summary>
    ///     Restores the default install directory provider and removes temporary directories.
    /// </summary>
    public void Dispose()
    {
        InstallDirectory.ResetBaseDirectoryProvider();
        projectRoot.Dispose();
        installDirectory.Dispose();
    }

    /// <summary>
    ///     Verifies that the assembled system prompt contains the agent name section header.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesAgentNameHeader()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.Contains("# Agent: test-agent", prompt);
    }

    /// <summary>
    ///     Verifies that the assembled system prompt contains the agent description.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesAgentDescription()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.Contains("A test agent for unit testing purposes.", prompt);
    }

    /// <summary>
    ///     Verifies that the assembled system prompt contains all must_not constraints.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesMustNotConstraints()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.Contains("Do prohibited thing one.", prompt);
        Assert.Contains("Do prohibited thing two.", prompt);
    }

    /// <summary>
    ///     Verifies that the assembled system prompt contains the practical guidance.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesPracticalGuidance()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.Contains("Apply test guidance at all times.", prompt);
    }

    /// <summary>
    ///     Verifies that the assembled system prompt contains rule content when rules are provided.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesRuleContent()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        WriteInstallRule("test-rule", TestRuleContent);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", ["test-rule"],
            new PromptContext());

        Assert.Contains(TestRuleContent, prompt);
    }

    /// <summary>
    ///     Verifies that the rules section header is included when rules are provided.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesRulesSectionHeader()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        WriteInstallRule("test-rule", TestRuleContent);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", ["test-rule"],
            new PromptContext());

        Assert.Contains("# Rules", prompt);
    }

    /// <summary>
    ///     Verifies that the rules section is absent when no rules are provided.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptOmitsRulesSectionWhenNoRulesProvided()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.DoesNotContain("# Rules", prompt);
    }

    /// <summary>
    ///     Verifies that the context section is absent when all context fields are empty.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptOmitsContextSectionWhenContextIsEmpty()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], new PromptContext());

        Assert.DoesNotContain("# Context", prompt);
    }

    /// <summary>
    ///     Verifies that the session scope is included in the context section.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesSessionScopeWhenProvided()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();
        var context = new PromptContext(sessionScope: "Implement the combat system.");

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], context);

        Assert.Contains("Implement the combat system.", prompt);
        Assert.Contains("## Session Scope", prompt);
    }

    /// <summary>
    ///     Verifies that the current task brief is included in the context section.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesTaskBriefWhenProvided()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();
        var context = new PromptContext(taskBrief: "Design the damage formula.");

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], context);

        Assert.Contains("Design the damage formula.", prompt);
        Assert.Contains("## Current Task", prompt);
    }

    /// <summary>
    ///     Verifies that CEO instructions are included in the context section.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesCeoInstructionsWhenProvided()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();
        var context = new PromptContext(ceoInstructions: "Keep it simple, no critical hits.");

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], context);

        Assert.Contains("Keep it simple, no critical hits.", prompt);
        Assert.Contains("## CEO Instructions", prompt);
    }

    /// <summary>
    ///     Verifies that the context section header is present when any context field is populated.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesContextSectionHeaderWhenContextIsProvided()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();
        var context = new PromptContext(taskBrief: "Some task.");

        var prompt = assembler.AssembleSystemPrompt("test-agent", [], context);

        Assert.Contains("# Context", prompt);
    }

    /// <summary>
    ///     Verifies that multiple rules are all included in the assembled prompt.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptIncludesMultipleRules()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        WriteInstallRule("rule-a", "Rule A content.");
        WriteInstallRule("rule-b", "Rule B content.");
        var assembler = CreateAssembler();

        var prompt = assembler.AssembleSystemPrompt("test-agent", ["rule-a", "rule-b"],
            new PromptContext());

        Assert.Contains("Rule A content.", prompt);
        Assert.Contains("Rule B content.", prompt);
    }

    /// <summary>
    ///     Verifies that BuildRequest returns an AIProviderRequest with the assembled system prompt.
    /// </summary>
    [Fact]
    public void BuildRequestReturnsRequestWithAssembledSystemPrompt()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        var request = assembler.BuildRequest("test-agent", [], new PromptContext(),
            taskPrompt: "Do the task.", workingDirectory: projectRoot.Path,
            timeout: TimeSpan.FromMinutes(5));

        Assert.Contains("# Agent: test-agent", request.SystemPrompt);
        Assert.Equal("Do the task.", request.TaskPrompt);
        Assert.Equal(projectRoot.Path, request.WorkingDirectory);
        Assert.Equal(TimeSpan.FromMinutes(5), request.Timeout);
    }

    /// <summary>
    ///     Verifies that an unknown agent name throws a file-not-found exception.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptThrowsForMissingAgent()
    {
        var assembler = CreateAssembler();

        Assert.Throws<FileNotFoundException>(() =>
            assembler.AssembleSystemPrompt("nonexistent-agent", [], new PromptContext()));
    }

    /// <summary>
    ///     Verifies that an unknown rule name throws a file-not-found exception.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptThrowsForMissingRule()
    {
        WriteInstallAgent("test-agent", TestAgentMarkdown);
        var assembler = CreateAssembler();

        Assert.Throws<FileNotFoundException>(() =>
            assembler.AssembleSystemPrompt("test-agent", ["nonexistent-rule"],
                new PromptContext()));
    }

    /// <summary>
    ///     Verifies that an empty agent name is rejected with an argument exception.
    /// </summary>
    [Fact]
    public void AssembleSystemPromptThrowsForEmptyAgentName()
    {
        var assembler = CreateAssembler();

        Assert.Throws<ArgumentException>(() =>
            assembler.AssembleSystemPrompt(string.Empty, [], new PromptContext()));
    }

    /// <summary>
    ///     Creates a <see cref="PromptAssembler" /> backed by the temporary install directory.
    /// </summary>
    /// <returns>Configured <see cref="PromptAssembler" /> instance.</returns>
    private PromptAssembler CreateAssembler()
    {
        return new PromptAssembler(new ResourceLoader(projectRoot.Path));
    }

    /// <summary>
    ///     Writes an agent definition file to the standard install directory.
    /// </summary>
    /// <param name="agentName">Logical agent name without the markdown extension.</param>
    /// <param name="content">Markdown content written to the file.</param>
    private void WriteInstallAgent(string agentName, string content)
    {
        var directoryPath = Path.Combine(installDirectory.Path, "agents");
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(Path.Combine(directoryPath, agentName + ".md"), content);
    }

    /// <summary>
    ///     Writes a rule file to the standard install directory.
    /// </summary>
    /// <param name="ruleName">Logical rule name without the markdown extension.</param>
    /// <param name="content">Markdown content written to the file.</param>
    private void WriteInstallRule(string ruleName, string content)
    {
        var directoryPath = Path.Combine(installDirectory.Path, "rules");
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(Path.Combine(directoryPath, ruleName + ".md"), content);
    }
}
