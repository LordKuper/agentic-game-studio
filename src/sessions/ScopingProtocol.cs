using System.Text;
using AGS.orchestration;
using AGS.prompt;

namespace AGS.sessions;

/// <summary>
///     Orchestrates the scoping phase of an AGS session: top-level agents and the scope
///     writer are resolved at runtime from <c>agent-coordination.md</c> via the default AI,
///     each agent runs a Q&amp;A loop with the CEO, then the resolved scope writer writes
///     <c>session-scope.md</c>, and the CEO approves or requests revisions.
/// </summary>
internal sealed class ScopingProtocol
{
    /// <summary>Filename of the scope document written inside the session directory.</summary>
    internal const string ScopeFileName = "session-scope.md";

    /// <summary>
    ///     Output marker that an agent includes to signal it has no further scoping questions.
    /// </summary>
    internal const string CompletionMarker = "[SCOPING COMPLETE]";

    /// <summary>Maximum Q&amp;A rounds per agent before the loop terminates automatically.</summary>
    internal const int MaxQaRoundsPerAgent = 5;

    private readonly SessionManager sessionManager;
    private readonly IAgentOrchestrator orchestrator;
    private readonly string projectRootPath;
    private readonly Func<string> coordinationDocumentProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScopingProtocol" /> class.
    /// </summary>
    /// <param name="sessionManager">Session manager for state persistence.</param>
    /// <param name="orchestrator">Agent orchestrator for AI invocations.</param>
    /// <param name="resourceLoader">Resource loader used to read <c>agent-coordination.md</c>.</param>
    /// <param name="projectRootPath">Absolute path to the game project root.</param>
    internal ScopingProtocol(SessionManager sessionManager, IAgentOrchestrator orchestrator,
        ResourceLoader resourceLoader, string projectRootPath)
        : this(sessionManager, orchestrator, projectRootPath,
            () => (resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader)))
                .ReadResource("rules", "agent-coordination"))
    {
        if (resourceLoader == null) throw new ArgumentNullException(nameof(resourceLoader));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScopingProtocol" /> class with an
    ///     injectable coordination document provider (used in tests).
    /// </summary>
    /// <param name="sessionManager">Session manager for state persistence.</param>
    /// <param name="orchestrator">Agent orchestrator for AI invocations.</param>
    /// <param name="projectRootPath">Absolute path to the game project root.</param>
    /// <param name="coordinationDocumentProvider">
    ///     Delegate that returns the content of <c>agent-coordination.md</c>. Should throw
    ///     <see cref="FileNotFoundException" /> when the file does not exist.
    /// </param>
    internal ScopingProtocol(SessionManager sessionManager, IAgentOrchestrator orchestrator,
        string projectRootPath, Func<string> coordinationDocumentProvider)
    {
        this.sessionManager = sessionManager ??
                              throw new ArgumentNullException(nameof(sessionManager));
        this.orchestrator = orchestrator ??
                            throw new ArgumentNullException(nameof(orchestrator));
        if (string.IsNullOrWhiteSpace(projectRootPath))
            throw new ArgumentException("Project root path must not be null or empty.",
                nameof(projectRootPath));
        this.projectRootPath = projectRootPath;
        this.coordinationDocumentProvider = coordinationDocumentProvider ??
                                             throw new ArgumentNullException(
                                                 nameof(coordinationDocumentProvider));
    }

    // ── Public Entry Point ────────────────────────────────────────────────────

    /// <summary>
    ///     Runs the full scoping protocol for the specified session:
    ///     <list type="number">
    ///         <item>
    ///             Top-level scoping agents and the scope writer are resolved at runtime from
    ///             <c>agent-coordination.md</c> via the default AI (the file is created first
    ///             if absent).
    ///         </item>
    ///         <item>Each resolved scoping agent runs a Q&amp;A loop with the CEO.</item>
    ///         <item>The resolved scope writer writes <c>session-scope.md</c> from the Q&amp;A.</item>
    ///         <item>The CEO reviews and approves (or requests revisions) in a loop.</item>
    ///     </list>
    ///     On approval the session is transitioned to <see cref="SessionStatus.ScopeApproved" />.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sessionId" /> is empty.</exception>
    internal void Run(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID must not be null or empty.",
                nameof(sessionId));

        var state = sessionManager.ReadSessionState(sessionId);

        // Load coordination document once; create it if absent
        var coordinationContent = LoadCoordinationContent();

        var scopingAgents = ResolveScopingAgents(coordinationContent);
        var scopeWriterAgent = ResolveScopeWriterAgent(coordinationContent);

        var qaHistory = new List<QaEntry>();

        foreach (var agentName in scopingAgents)
            RunAgentQaLoop(state.Title, agentName, qaHistory);

        // Persist scoping agent list to session state
        var refreshed = sessionManager.ReadSessionState(sessionId);
        sessionManager.UpdateSessionState(new SessionState
        {
            SessionId = refreshed.SessionId,
            Title = refreshed.Title,
            Status = refreshed.Status,
            Created = refreshed.Created,
            LastUpdated = refreshed.LastUpdated,
            ScopingAgents = scopingAgents,
            PlanningAgents = refreshed.PlanningAgents,
            CurrentTask = refreshed.CurrentTask,
            RelevantFiles = refreshed.RelevantFiles,
            Decisions = refreshed.Decisions,
            NextStep = refreshed.NextStep
        });

        GenerateScopeDocument(sessionId, state.Title, qaHistory, scopeWriterAgent);
        RunApprovalFlow(sessionId, state.Title, qaHistory, scopeWriterAgent);
    }

    // ── Coordination Document ─────────────────────────────────────────────────

    /// <summary>
    ///     Loads <c>agent-coordination.md</c> content. If the file is absent, asks the default
    ///     AI to create it first.
    /// </summary>
    private string LoadCoordinationContent()
    {
        try
        {
            return coordinationDocumentProvider();
        }
        catch (FileNotFoundException)
        {
            return CreateAgentCoordinationFile();
        }
    }

    /// <summary>
    ///     Asks the default AI to create <c>agent-coordination.md</c> in the project overlay,
    ///     then returns the content of the newly created file (or the AI output as a fallback).
    /// </summary>
    private string CreateAgentCoordinationFile()
    {
        var targetRelativePath = Path.Combine(AgsSettings.AgsDirectoryName, "rules",
            "agent-coordination.md");

        var result = orchestrator.InvokeDefault(
            "You are a game development studio AI assistant responsible for maintaining " +
            "project coordination documents.",
            $"Create a project agent coordination document at `{targetRelativePath}`. " +
            "The document must define a hierarchy of game development agents, their levels " +
            "(C-Level, Lead, Specialist), and their areas of responsibility. " +
            "Write the file and return nothing else.",
            projectRootPath,
            TimeSpan.FromMinutes(5));

        try
        {
            return coordinationDocumentProvider();
        }
        catch (FileNotFoundException)
        {
            return result.ProviderResult.Output ?? string.Empty;
        }
    }

    // ── Agent Resolution ──────────────────────────────────────────────────────

    /// <summary>
    ///     Asks the default AI which agents should participate in the scoping phase, based on
    ///     the coordination document.
    /// </summary>
    private IReadOnlyList<string> ResolveScopingAgents(string coordinationContent)
    {
        var result = orchestrator.InvokeDefault(
            "You are an assistant that reads agent coordination documents and determines " +
            "which agents should participate in project scoping sessions.",
            "Based on the agent coordination document below, list the names of all agents " +
            "that should participate in the scoping phase of a new game development session. " +
            "Respond with a plain list of agent file-name stems (e.g. producer, game-designer), " +
            "one per line, with no other text, explanations, or punctuation.\n\n" +
            coordinationContent,
            projectRootPath,
            TimeSpan.FromMinutes(2));

        if (!result.ProviderResult.Success ||
            string.IsNullOrWhiteSpace(result.ProviderResult.Output))
            throw new InvalidOperationException(
                "Default AI failed to resolve scoping agents from agent-coordination.md.");

        return ParseAgentList(result.ProviderResult.Output);
    }

    /// <summary>
    ///     Asks the default AI which single agent is responsible for writing the scope document,
    ///     based on the coordination document.
    /// </summary>
    private string ResolveScopeWriterAgent(string coordinationContent)
    {
        var result = orchestrator.InvokeDefault(
            "You are an assistant that reads agent coordination documents and determines " +
            "agent responsibilities.",
            "Based on the agent coordination document below, name the single agent responsible " +
            "for writing the project scope document. Respond with only the agent file-name stem " +
            "(e.g. producer), with no other text.\n\n" +
            coordinationContent,
            projectRootPath,
            TimeSpan.FromMinutes(2));

        if (!result.ProviderResult.Success ||
            string.IsNullOrWhiteSpace(result.ProviderResult.Output))
            throw new InvalidOperationException(
                "Default AI failed to resolve scope writer agent from agent-coordination.md.");

        return ParseAgentList(result.ProviderResult.Output).FirstOrDefault()
               ?? throw new InvalidOperationException(
                   "Default AI returned an empty agent name for the scope writer.");
    }

    /// <summary>
    ///     Parses a raw agent list response from the AI into a list of kebab-case agent names.
    ///     Accepts one name per line or comma-separated values; strips bullets and whitespace.
    /// </summary>
    internal static IReadOnlyList<string> ParseAgentList(string output)
    {
        var agents = new List<string>();
        foreach (var rawToken in output.Split(['\n', '\r', ','],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var token = rawToken.Trim().TrimStart('-', '*', '•').Trim();
            if (string.IsNullOrWhiteSpace(token)) continue;
            var normalized = token.ToLowerInvariant().Replace(' ', '-');
            if (!string.IsNullOrWhiteSpace(normalized))
                agents.Add(normalized);
        }
        return agents.AsReadOnly();
    }

    // ── Q&A Loop ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Runs the Q&amp;A loop for a single agent: the agent is invoked up to
    ///     <see cref="MaxQaRoundsPerAgent" /> times; each output that does not contain
    ///     <see cref="CompletionMarker" /> is shown to the CEO for a response.
    /// </summary>
    private void RunAgentQaLoop(string sessionTitle, string agentName, List<QaEntry> qaHistory)
    {
        for (var round = 0; round < MaxQaRoundsPerAgent; round++)
        {
            var taskPrompt = round == 0
                ? $"You are participating in the scoping phase for session \"{sessionTitle}\". " +
                  $"Ask the CEO the key questions you need to define your contribution to this " +
                  $"project. When you have enough information, end your response with " +
                  $"{CompletionMarker}."
                : $"Continue scoping for session \"{sessionTitle}\". Ask your next question, " +
                  $"or end with {CompletionMarker} when you are satisfied.";

            var request = new AgentInvocationRequest(
                agentName,
                ruleNames: [],
                context: new PromptContext(ceoInstructions: BuildQaContext(sessionTitle, qaHistory)),
                taskPrompt: taskPrompt,
                workingDirectory: projectRootPath,
                timeout: TimeSpan.FromMinutes(5));

            var result = orchestrator.InvokeAgent(request);
            if (!result.ProviderResult.Success) return;

            var output = result.ProviderResult.Output ?? string.Empty;
            var isComplete = output.Contains(CompletionMarker, StringComparison.OrdinalIgnoreCase);
            var question = output
                .Replace(CompletionMarker, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!string.IsNullOrWhiteSpace(question))
            {
                Console.WriteLine();
                Console.WriteLine($"[{agentName}]: {question}");
            }

            if (isComplete) return;

            var answer = AgsPrompt.Input("Your response");
            qaHistory.Add(new QaEntry(agentName, question, answer));
        }
    }

    // ── Scope Document Generation ─────────────────────────────────────────────

    /// <summary>
    ///     Invokes the resolved scope writer agent to write <c>session-scope.md</c> from the
    ///     accumulated Q&amp;A.
    /// </summary>
    private void GenerateScopeDocument(string sessionId, string sessionTitle,
        IReadOnlyList<QaEntry> qaHistory, string scopeWriterAgent)
    {
        var scopeRelativePath = Path.GetRelativePath(projectRootPath,
            SessionManager.GetScopeFilePath(projectRootPath, sessionId));

        var request = new AgentInvocationRequest(
            scopeWriterAgent,
            ruleNames: [],
            context: new PromptContext(ceoInstructions: BuildQaContext(sessionTitle, qaHistory)),
            taskPrompt:
            $"Based on the Q&A discussion, write a comprehensive project scope document to " +
            $"`{scopeRelativePath}`. The document must cover: session goals, features in scope, " +
            $"out-of-scope items, constraints, success criteria, and key decisions agreed during " +
            $"scoping.",
            workingDirectory: projectRootPath,
            timeout: TimeSpan.FromMinutes(10));

        orchestrator.InvokeAgent(request);
    }

    // ── CEO Approval ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Presents the scope document to the CEO in a loop. On approval the session is
    ///     transitioned to <see cref="SessionStatus.ScopeApproved" />. On rejection the
    ///     resolved scope writer agent is asked to revise the document.
    /// </summary>
    private void RunApprovalFlow(string sessionId, string sessionTitle,
        IReadOnlyList<QaEntry> qaHistory, string scopeWriterAgent)
    {
        while (true)
        {
            var scopeFilePath = SessionManager.GetScopeFilePath(projectRootPath, sessionId);
            if (File.Exists(scopeFilePath))
            {
                Console.WriteLine();
                Console.WriteLine(File.ReadAllText(scopeFilePath));
            }

            if (AgsPrompt.Confirm("Approve this scope document?", true))
            {
                sessionManager.TransitionStatus(sessionId, SessionStatus.ScopeApproved);
                return;
            }

            var changes = AgsPrompt.Input("What changes do you want to the scope?");
            var scopeRelativePath = Path.GetRelativePath(projectRootPath, scopeFilePath);

            var request = new AgentInvocationRequest(
                scopeWriterAgent,
                ruleNames: [],
                context: new PromptContext(ceoInstructions: BuildQaContext(sessionTitle, qaHistory)),
                taskPrompt:
                $"The CEO has requested the following changes to the scope document " +
                $"`{scopeRelativePath}`: {changes}. Update the file accordingly.",
                workingDirectory: projectRootPath,
                timeout: TimeSpan.FromMinutes(10));

            orchestrator.InvokeAgent(request);
        }
    }

    // ── Context Builder ───────────────────────────────────────────────────────

    /// <summary>
    ///     Builds the CEO instructions block injected into each agent invocation, containing the
    ///     session title and the full accumulated Q&amp;A history.
    /// </summary>
    private static string BuildQaContext(string sessionTitle, IReadOnlyList<QaEntry> qaHistory)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Session: {sessionTitle}");

        if (qaHistory.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Scoping Q&A");
            foreach (var entry in qaHistory)
            {
                sb.AppendLine($"[{entry.AgentName}]: {entry.Question}");
                sb.AppendLine($"CEO: {entry.Answer}");
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    // ── Inner Types ───────────────────────────────────────────────────────────

    private sealed record QaEntry(string AgentName, string Question, string Answer);
}
