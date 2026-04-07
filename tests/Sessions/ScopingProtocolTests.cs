using AGS.ai;
using AGS.orchestration;
using AGS.prompt;
using AGS.sessions;

namespace AGS.Tests;

/// <summary>
///     Unit tests for <see cref="ScopingProtocol" /> covering agent resolution via the default
///     AI, the Q&amp;A loop, scope document generation, CEO approval flow, and session state
///     updates.
/// </summary>
public sealed class ScopingProtocolTests : IDisposable
{
    private readonly TemporaryDirectoryScope tempDir;
    private readonly SessionManager sessionManager;
    private readonly ResourceLoader resourceLoader;

    public ScopingProtocolTests()
    {
        tempDir = new TemporaryDirectoryScope();
        sessionManager = new SessionManager(tempDir.Path);
        resourceLoader = new ResourceLoader(tempDir.Path);
    }

    public void Dispose()
    {
        tempDir.Dispose();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void ConstructorThrowsWhenSessionManagerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScopingProtocol(null, new StubOrchestrator(), resourceLoader, tempDir.Path));
    }

    [Fact]
    public void ConstructorThrowsWhenOrchestratorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScopingProtocol(sessionManager, null, resourceLoader, tempDir.Path));
    }

    [Fact]
    public void ConstructorThrowsWhenResourceLoaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScopingProtocol(sessionManager, new StubOrchestrator(), (ResourceLoader)null,
                tempDir.Path));
    }

    [Fact]
    public void ConstructorThrowsWhenCoordinationDocumentProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScopingProtocol(sessionManager, new StubOrchestrator(), tempDir.Path,
                (Func<string>)null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorThrowsWhenProjectRootPathIsNullOrEmpty(string path)
    {
        Assert.Throws<ArgumentException>(() =>
            new ScopingProtocol(sessionManager, new StubOrchestrator(), resourceLoader, path));
    }

    // ── Run: argument validation ──────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RunThrowsWhenSessionIdIsNullOrEmpty(string sessionId)
    {
        var protocol = BuildProtocol(new StubOrchestrator());
        Assert.Throws<ArgumentException>(() => protocol.Run(sessionId));
    }

    // ── Agent Resolution ──────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that the default AI is invoked twice — once for scoping agents and once for
    ///     the scope writer — and that both are resolved from the coordination document.
    /// </summary>
    [Fact]
    public void RunResolvesAgentsFromAgentCoordinationDocument()
    {
        var sessionId = CreateSession("My Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [
                Succeeded("agent-a\nagent-b"),  // scoping agents
                Succeeded("scope-writer")        // scope writer
            ],
            agentResults: [
                Succeeded(CompletionMarker),   // agent-a Q&A
                Succeeded(CompletionMarker),   // agent-b Q&A
                Succeeded("scope written")     // scope generation
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(2, stub.DefaultRequests.Count);
        Assert.Equal(3, stub.AgentRequests.Count);
        Assert.Equal("agent-a", stub.AgentRequests[0].AgentName);
        Assert.Equal("agent-b", stub.AgentRequests[1].AgentName);
        Assert.Equal("scope-writer", stub.AgentRequests[2].AgentName);
        Assert.Equal(["agent-a", "agent-b"],
            sessionManager.ReadSessionState(sessionId).ScopingAgents);
    }

    /// <summary>
    ///     Verifies that when <c>agent-coordination.md</c> is absent the default AI is first
    ///     asked to create the file, resulting in three InvokeDefault calls total.
    /// </summary>
    [Fact]
    public void RunCreatesAgentCoordinationFileWhenMissing()
    {
        var sessionId = CreateSession("New Game");

        var stub = new StubOrchestrator(
            defaultResults: [
                Succeeded("# Agent Coordination\n..."),  // create file
                Succeeded("agent-a"),                     // scoping agents
                Succeeded("scope-writer")                 // scope writer
            ],
            agentResults: [
                Succeeded(CompletionMarker),  // agent-a Q&A
                Succeeded("scope written")    // scope generation
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);

        var protocol = new ScopingProtocol(sessionManager, stub, tempDir.Path,
            () => throw new FileNotFoundException("agent-coordination.md not found"));
        protocol.Run(sessionId);

        Assert.Equal(3, stub.DefaultRequests.Count);
    }

    /// <summary>
    ///     Verifies that Run throws when the default AI fails to return a valid scoping agent list.
    /// </summary>
    [Fact]
    public void RunThrowsWhenDefaultAiFailsToResolveAgents()
    {
        var sessionId = CreateSession("Broken Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Failed("provider unavailable")]
        );

        using var prompts = new PromptStubScope();
        Assert.Throws<InvalidOperationException>(() => BuildProtocol(stub).Run(sessionId));
    }

    /// <summary>
    ///     Verifies that Run throws when the default AI fails to resolve the scope writer.
    /// </summary>
    [Fact]
    public void RunThrowsWhenDefaultAiFailsToResolveScopeWriter()
    {
        var sessionId = CreateSession("Broken Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [
                Succeeded("agent-a"),         // scoping agents ok
                Failed("provider unavailable") // scope writer fails
            ]
        );

        using var prompts = new PromptStubScope();
        Assert.Throws<InvalidOperationException>(() => BuildProtocol(stub).Run(sessionId));
    }

    // ── ParseAgentList ────────────────────────────────────────────────────────

    [Fact]
    public void ParseAgentListHandlesOnePerLine()
    {
        var result = ScopingProtocol.ParseAgentList("producer\ngame-designer\nqa-lead");
        Assert.Equal(["producer", "game-designer", "qa-lead"], result);
    }

    [Fact]
    public void ParseAgentListHandlesCommaSeparated()
    {
        var result = ScopingProtocol.ParseAgentList("producer, game-designer, qa-lead");
        Assert.Equal(["producer", "game-designer", "qa-lead"], result);
    }

    [Fact]
    public void ParseAgentListStripsLeadingBulletsAndWhitespace()
    {
        var result = ScopingProtocol.ParseAgentList("- producer\n* game-designer\n• qa-lead");
        Assert.Equal(["producer", "game-designer", "qa-lead"], result);
    }

    [Fact]
    public void ParseAgentListNormalizesSpacesToHyphens()
    {
        var result = ScopingProtocol.ParseAgentList("creative director\ntechnical director");
        Assert.Equal(["creative-director", "technical-director"], result);
    }

    [Fact]
    public void ParseAgentListSkipsBlankLines()
    {
        var result = ScopingProtocol.ParseAgentList("producer\n\n\ngame-designer");
        Assert.Equal(["producer", "game-designer"], result);
    }

    // ── Q&A Loop ──────────────────────────────────────────────────────────────

    [Fact]
    public void RunSkipsQaInputWhenAgentSignalsCompletionOnFirstTurn()
    {
        var sessionId = CreateSession("My Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [
                Succeeded(CompletionMarker),  // agent-a: immediate completion
                Succeeded("scope written")    // scope generation
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Empty(prompts.InputMessages);
    }

    [Fact]
    public void RunAccumulatesQaHistoryAcrossRounds()
    {
        var sessionId = CreateSession("Space Shooter");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [
                Succeeded("What is the target platform?"),  // round 0: asks question
                Succeeded("Thanks. " + CompletionMarker),   // round 1: completion
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(
            confirmations: [true],
            inputs: ["PC and console"]
        );

        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(3, stub.AgentRequests.Count);
        var round1Context = stub.AgentRequests[1].Context.CeoInstructions;
        Assert.Contains("What is the target platform?", round1Context);
        Assert.Contains("PC and console", round1Context);
    }

    [Fact]
    public void RunStopsQaLoopAfterMaxRoundsWhenAgentNeverSignalsCompletion()
    {
        var sessionId = CreateSession("Puzzle Game");
        WriteAgentCoordinationFile();

        var questionResults = Enumerable
            .Range(1, ScopingProtocol.MaxQaRoundsPerAgent)
            .Select(i => Succeeded($"Question {i}?"))
            .ToArray();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [.. questionResults, Succeeded("scope written")]
        );

        using var prompts = new PromptStubScope(
            confirmations: [true],
            inputs: Enumerable.Repeat("answer", ScopingProtocol.MaxQaRoundsPerAgent).ToArray()
        );

        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(ScopingProtocol.MaxQaRoundsPerAgent + 1, stub.AgentRequests.Count);
    }

    [Fact]
    public void RunStopsQaLoopOnProviderFailure()
    {
        var sessionId = CreateSession("RPG");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [
                Failed("provider unavailable"),
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Empty(prompts.InputMessages);
    }

    [Fact]
    public void RunInvokesEachResolvedAgentInOrder()
    {
        var sessionId = CreateSession("Tower Defense");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a\nagent-b"), Succeeded("scope-writer")],
            agentResults: [
                Succeeded(CompletionMarker),
                Succeeded(CompletionMarker),
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Equal("agent-a", stub.AgentRequests[0].AgentName);
        Assert.Equal("agent-b", stub.AgentRequests[1].AgentName);
    }

    // ── Scope Document Generation ─────────────────────────────────────────────

    /// <summary>
    ///     Verifies that the resolved scope writer agent (not a hardcoded name) is invoked to
    ///     generate the scope document, and that the task prompt references the scope file path.
    /// </summary>
    [Fact]
    public void RunInvokesResolvedScopeWriterForDocumentGeneration()
    {
        var sessionId = CreateSession("Strategy Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("my-scope-writer")],
            agentResults: [
                Succeeded(CompletionMarker),
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        var generateRequest = stub.AgentRequests[1];
        Assert.Equal("my-scope-writer", generateRequest.AgentName);
        Assert.Contains(ScopingProtocol.ScopeFileName, generateRequest.TaskPrompt);
    }

    [Fact]
    public void RunPassesQaHistoryToScopeDocumentGeneration()
    {
        var sessionId = CreateSession("Platformer");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [
                Succeeded("What engine are you targeting?"),
                Succeeded(CompletionMarker),
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(
            confirmations: [true],
            inputs: ["Unity"]
        );

        BuildProtocol(stub).Run(sessionId);

        var generateContext = stub.AgentRequests[2].Context.CeoInstructions;
        Assert.Contains("What engine are you targeting?", generateContext);
        Assert.Contains("Unity", generateContext);
    }

    // ── CEO Approval ──────────────────────────────────────────────────────────

    [Fact]
    public void RunTransitionsToScopeApprovedWhenCeoApproves()
    {
        var sessionId = CreateSession("Survival Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [Succeeded(CompletionMarker), Succeeded("scope written")]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(SessionStatus.ScopeApproved,
            sessionManager.ReadSessionState(sessionId).Status);
    }

    /// <summary>
    ///     Verifies that the resolved scope writer (not a hardcoded name) is used for revisions.
    /// </summary>
    [Fact]
    public void RunRevisesUsingResolvedScopeWriterAndApprovesOnSecondPass()
    {
        var sessionId = CreateSession("Roguelike");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("my-scope-writer")],
            agentResults: [
                Succeeded(CompletionMarker),
                Succeeded("scope written"),
                Succeeded("scope revised")
            ]
        );

        using var prompts = new PromptStubScope(
            confirmations: [false, true],
            inputs: ["Add multiplayer to the scope"]
        );

        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(3, stub.AgentRequests.Count);
        Assert.Equal("my-scope-writer", stub.AgentRequests[2].AgentName);
        Assert.Contains("Add multiplayer to the scope", stub.AgentRequests[2].TaskPrompt);
        Assert.Equal(SessionStatus.ScopeApproved,
            sessionManager.ReadSessionState(sessionId).Status);
    }

    [Fact]
    public void RunDisplaysScopeDocumentDuringApproval()
    {
        var sessionId = CreateSession("Card Game");
        WriteAgentCoordinationFile();

        var scopeFilePath = SessionManager.GetScopeFilePath(tempDir.Path, sessionId);
        File.WriteAllText(scopeFilePath, "## Goals\nBuild a collectible card game.");

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a"), Succeeded("scope-writer")],
            agentResults: [Succeeded(CompletionMarker), Succeeded("scope written")]
        );

        using var consoleOut = new ConsoleRedirectionScope(string.Empty);
        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Contains("Build a collectible card game.", consoleOut.Output);
    }

    // ── Session State ─────────────────────────────────────────────────────────

    [Fact]
    public void RunPersistsResolvedScopingAgentsToSessionState()
    {
        var sessionId = CreateSession("Racing Game");
        WriteAgentCoordinationFile();

        var stub = new StubOrchestrator(
            defaultResults: [Succeeded("agent-a\nagent-b"), Succeeded("scope-writer")],
            agentResults: [
                Succeeded(CompletionMarker),
                Succeeded(CompletionMarker),
                Succeeded("scope written")
            ]
        );

        using var prompts = new PromptStubScope(confirmations: [true]);
        BuildProtocol(stub).Run(sessionId);

        Assert.Equal(["agent-a", "agent-b"],
            sessionManager.ReadSessionState(sessionId).ScopingAgents);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private const string CompletionMarker = ScopingProtocol.CompletionMarker;

    private string CreateSession(string title)
    {
        var slug = title.ToLowerInvariant().Replace(' ', '-');
        return sessionManager.CreateSession(title, slug).SessionId;
    }

    private ScopingProtocol BuildProtocol(StubOrchestrator stub) =>
        new(sessionManager, stub, resourceLoader, tempDir.Path);

    private ScopingProtocol BuildProtocolWithCoordination(StubOrchestrator stub,
        string coordinationContent) =>
        new(sessionManager, stub, tempDir.Path, () => coordinationContent);

    private void WriteAgentCoordinationFile(string content = "# Agent Coordination\n(stub)")
    {
        var rulesPath = Path.Combine(tempDir.Path, AgsSettings.AgsDirectoryName, "rules");
        Directory.CreateDirectory(rulesPath);
        File.WriteAllText(Path.Combine(rulesPath, "agent-coordination.md"), content);
    }

    private static AgentInvocationResult Succeeded(string output) =>
        new(string.Empty, AIProviderResult.Succeeded(output, 0, []), []);

    private static AgentInvocationResult Failed(string error) =>
        new(string.Empty, AIProviderResult.Failed(error, -1), []);

    // ── Test Double ───────────────────────────────────────────────────────────

    /// <summary>
    ///     Queue-driven <see cref="IAgentOrchestrator" /> test double with separate queues for
    ///     <see cref="InvokeAgent" /> and <see cref="InvokeDefault" />. Exhausted queues fall
    ///     back to an immediate <see cref="ScopingProtocol.CompletionMarker" /> success result.
    /// </summary>
    private sealed class StubOrchestrator : IAgentOrchestrator
    {
        private readonly Queue<AgentInvocationResult> agentResults;
        private readonly Queue<AgentInvocationResult> defaultResults;

        internal StubOrchestrator(
            IEnumerable<AgentInvocationResult> agentResults = null,
            IEnumerable<AgentInvocationResult> defaultResults = null)
        {
            this.agentResults = agentResults == null
                ? []
                : new Queue<AgentInvocationResult>(agentResults);
            this.defaultResults = defaultResults == null
                ? []
                : new Queue<AgentInvocationResult>(defaultResults);
            AgentRequests = [];
            DefaultRequests = [];
        }

        internal List<AgentInvocationRequest> AgentRequests { get; }
        internal List<(string SystemPrompt, string TaskPrompt, string WorkDir, TimeSpan Timeout)>
            DefaultRequests { get; }

        public AgentInvocationResult InvokeAgent(AgentInvocationRequest request)
        {
            AgentRequests.Add(request);
            if (agentResults.Count > 0) return agentResults.Dequeue();
            return new AgentInvocationResult(string.Empty,
                AIProviderResult.Succeeded(ScopingProtocol.CompletionMarker, 0, []), []);
        }

        public AgentInvocationResult InvokeDefault(string systemPrompt, string taskPrompt,
            string workingDirectory, TimeSpan timeout, string outputSchemaPath = null)
        {
            DefaultRequests.Add((systemPrompt, taskPrompt, workingDirectory, timeout));
            if (defaultResults.Count > 0) return defaultResults.Dequeue();
            return new AgentInvocationResult(string.Empty,
                AIProviderResult.Succeeded(string.Empty, 0, []), []);
        }
    }
}
