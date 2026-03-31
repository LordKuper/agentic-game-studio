using AGS.prompt;

namespace AGS.Tests;

/// <summary>
///     Covers parsing of agent definition markdown files into <see cref="AgentDefinition" />
///     instances.
/// </summary>
public sealed class AgentDefinitionParserTests
{
    private const string MinimalAgentMarkdown = """
        # Test Agent

        | Field | Value |
        | --- | --- |
        | `name` | `test-agent` |
        | `description` | A test agent for unit testing. |
        | `must_not` | - Do prohibited thing one.<br>- Do prohibited thing two. |
        | `models` | - claude-sonnet<br>- chatgpt |
        | `max_iterations` | 10 |
        """;

    private const string AgentWithPracticalGuidanceMarkdown = """
        # Test Agent

        | Field | Value |
        | --- | --- |
        | `name` | `test-agent` |
        | `description` | A test agent for unit testing. |
        | `must_not` | - Do prohibited thing one. |
        | `models` | - claude-sonnet |
        | `max_iterations` | 5 |

        ## Practical Guidance

        - Follow test guidance.
        - Keep it simple.
        """;

    /// <summary>
    ///     Verifies that the agent name is parsed from the table.
    /// </summary>
    [Fact]
    public void ParseExtractsName()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal("test-agent", definition.Name);
    }

    /// <summary>
    ///     Verifies that the description is parsed from the table.
    /// </summary>
    [Fact]
    public void ParseExtractsDescription()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal("A test agent for unit testing.", definition.Description);
    }

    /// <summary>
    ///     Verifies that must_not list items are parsed and split on &lt;br&gt;.
    /// </summary>
    [Fact]
    public void ParseExtractsMustNotList()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal(["Do prohibited thing one.", "Do prohibited thing two."],
            definition.MustNot);
    }

    /// <summary>
    ///     Verifies that model names are parsed and split on &lt;br&gt;.
    /// </summary>
    [Fact]
    public void ParseExtractsModelsList()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal(["claude-sonnet", "chatgpt"], definition.Models);
    }

    /// <summary>
    ///     Verifies that max_iterations is parsed as an integer.
    /// </summary>
    [Fact]
    public void ParseExtractsMaxIterations()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal(10, definition.MaxIterations);
    }

    /// <summary>
    ///     Verifies that the Practical Guidance section body is captured when present.
    /// </summary>
    [Fact]
    public void ParseExtractsPracticalGuidanceWhenPresent()
    {
        var definition = AgentDefinitionParser.Parse(AgentWithPracticalGuidanceMarkdown);

        Assert.Contains("Follow test guidance.", definition.PracticalGuidance);
        Assert.Contains("Keep it simple.", definition.PracticalGuidance);
    }

    /// <summary>
    ///     Verifies that PracticalGuidance is empty when the section is absent.
    /// </summary>
    [Fact]
    public void ParseReturnEmptyPracticalGuidanceWhenSectionAbsent()
    {
        var definition = AgentDefinitionParser.Parse(MinimalAgentMarkdown);

        Assert.Equal(string.Empty, definition.PracticalGuidance);
    }

    /// <summary>
    ///     Verifies that a single must_not item without &lt;br&gt; is parsed correctly.
    /// </summary>
    [Fact]
    public void ParseExtractsSingleMustNotItem()
    {
        var definition = AgentDefinitionParser.Parse(AgentWithPracticalGuidanceMarkdown);

        Assert.Equal(["Do prohibited thing one."], definition.MustNot);
    }

    /// <summary>
    ///     Verifies that a real agent file (game-designer.md) is parsed without errors.
    /// </summary>
    [Fact]
    public void ParseHandlesRealGameDesignerAgentFile()
    {
        var agentPath = Path.Combine(AppContext.BaseDirectory, "agents", "game-designer.md");
        var markdown = File.ReadAllText(agentPath);

        var definition = AgentDefinitionParser.Parse(markdown);

        Assert.Equal("game-designer", definition.Name);
        Assert.NotEmpty(definition.Description);
        Assert.NotEmpty(definition.MustNot);
        Assert.NotEmpty(definition.Models);
        Assert.True(definition.MaxIterations > 0);
        Assert.NotEmpty(definition.PracticalGuidance);
    }

    /// <summary>
    ///     Verifies that empty markdown is rejected with an argument exception.
    /// </summary>
    [Fact]
    public void ParseThrowsOnEmptyMarkdown()
    {
        Assert.Throws<ArgumentException>(() => AgentDefinitionParser.Parse(string.Empty));
    }

    /// <summary>
    ///     Verifies that whitespace-only markdown is rejected with an argument exception.
    /// </summary>
    [Fact]
    public void ParseThrowsOnWhitespaceMarkdown()
    {
        Assert.Throws<ArgumentException>(() => AgentDefinitionParser.Parse("   "));
    }
}
