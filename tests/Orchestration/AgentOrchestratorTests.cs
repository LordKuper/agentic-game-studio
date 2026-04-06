using AGS.ai;
using AGS.orchestration;
using AGS.prompt;

namespace AGS.Tests;

/// <summary>
///     Covers provider selection and rate-limit failover behaviour in
///     <see cref="AgentOrchestrator" />.
/// </summary>
public sealed class AgentOrchestratorTests : IDisposable
{
    /// <summary>
    ///     Resets process-wide settings between tests.
    /// </summary>
    public void Dispose()
    {
        AgsTestState.ResetCurrentSettings();
    }

    /// <summary>
    ///     Verifies that a rate-limited provider is marked on cooldown and the task is restarted
    ///     with the next eligible provider from the agent's ordered model list.
    /// </summary>
    [Fact]
    public void InvokeAgentRetriesWithNextPreferredProviderWhenCurrentProviderIsRateLimited()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        using var projectRoot = new TemporaryDirectoryScope();
        WriteAgentDefinition(projectRoot.Path, "test-agent", ["claude-sonnet", "chatgpt"]);

        var cooldownExpiry = DateTimeOffset.UtcNow.AddMinutes(12);
        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.RateLimited("rate limit", 429, cooldownExpiry, "partial output"));
        var codex = new StubProvider(CodexAdapter.Id, true,
            AIProviderResult.Succeeded("completed", 0, ["file.txt"]));
        var registry = CreateRegistry(claude, codex);
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry);

        var result = orchestrator.InvokeAgent(CreateRequest("test-agent", projectRoot.Path));

        Assert.Equal(1, claude.InvocationCount);
        Assert.Equal(1, codex.InvocationCount);
        Assert.Equal(CodexAdapter.Id, result.ProviderId);
        Assert.True(result.ProviderResult.Success);
        Assert.Equal("completed", result.ProviderResult.Output);
        Assert.Equal([ClaudeCodeAdapter.Id, CodexAdapter.Id], result.AttemptedProviderIds);
        Assert.True(registry.IsInCooldown(ClaudeCodeAdapter.Id));
        Assert.Equal(cooldownExpiry.ToUniversalTime(),
            registry.GetCooldownExpiry(ClaudeCodeAdapter.Id).Value, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Verifies that the configured default cooldown is used when a rate-limited provider does
    ///     not return an explicit reset time.
    /// </summary>
    [Fact]
    public void InvokeAgentUsesConfiguredDefaultCooldownWhenProviderDoesNotReturnResetTime()
    {
        var now = new DateTimeOffset(2030, 3, 31, 9, 0, 0, TimeSpan.Zero);
        AgsSettings.SetCurrent(new AgsSettings(true, true, 45, null));
        using var projectRoot = new TemporaryDirectoryScope();
        WriteAgentDefinition(projectRoot.Path, "test-agent", ["claude-sonnet", "chatgpt"]);

        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.RateLimited("rate limit", 429, null, "partial output"));
        var codex = new StubProvider(CodexAdapter.Id, true,
            AIProviderResult.Succeeded("completed", 0, []));
        var registry = CreateRegistry(claude, codex);
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry,
            new FixedTimeProvider(now));

        var result = orchestrator.InvokeAgent(CreateRequest("test-agent", projectRoot.Path));
        var cooldownExpiry = registry.GetCooldownExpiry(ClaudeCodeAdapter.Id);

        Assert.True(result.ProviderResult.Success);
        Assert.NotNull(cooldownExpiry);
        Assert.Equal(now.AddMinutes(45), cooldownExpiry.Value);
    }

    /// <summary>
    ///     Verifies that the terminal rate-limited result is returned unchanged when no failover
    ///     provider is eligible.
    /// </summary>
    [Fact]
    public void InvokeAgentReturnsLastRateLimitedResultWhenNoFailoverProviderIsEligible()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));
        using var projectRoot = new TemporaryDirectoryScope();
        WriteAgentDefinition(projectRoot.Path, "test-agent", ["claude-sonnet", "chatgpt"]);

        var claudeCooldownExpiry = DateTimeOffset.UtcNow.AddMinutes(8);
        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.RateLimited("rate limit", 429, claudeCooldownExpiry, "partial output"));
        var codex = new StubProvider(CodexAdapter.Id, true,
            AIProviderResult.Succeeded("unused", 0, []));
        var registry = CreateRegistry(claude, codex);
        registry.MarkRateLimited(CodexAdapter.Id, DateTimeOffset.UtcNow.AddMinutes(20));
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry);

        var result = orchestrator.InvokeAgent(CreateRequest("test-agent", projectRoot.Path));

        Assert.Equal(1, claude.InvocationCount);
        Assert.Equal(0, codex.InvocationCount);
        Assert.Equal(ClaudeCodeAdapter.Id, result.ProviderId);
        Assert.False(result.ProviderResult.Success);
        Assert.True(result.ProviderResult.IsRateLimited);
        Assert.Equal([ClaudeCodeAdapter.Id], result.AttemptedProviderIds);
    }

    // ── InvokeDefault ─────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that InvokeDefault uses the DefaultModels list from settings to select a
    ///     provider and returns a successful result.
    /// </summary>
    [Fact]
    public void InvokeDefaultUsesDefaultModelsFromSettingsToSelectProvider()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true)
            .WithDefaultModels(["claude-sonnet"]));
        using var projectRoot = new TemporaryDirectoryScope();

        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.Succeeded("default ai response", 0, []));
        var registry = CreateRegistry(claude);
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry);

        var result = orchestrator.InvokeDefault("system prompt", "task prompt",
            projectRoot.Path, TimeSpan.FromSeconds(30));

        Assert.True(result.ProviderResult.Success);
        Assert.Equal("default ai response", result.ProviderResult.Output);
        Assert.Equal(ClaudeCodeAdapter.Id, result.ProviderId);
        Assert.Equal(1, claude.InvocationCount);
    }

    /// <summary>
    ///     Verifies that InvokeDefault returns a failure result when DefaultModels is empty.
    /// </summary>
    [Fact]
    public void InvokeDefaultReturnsFailureWhenDefaultModelsIsEmpty()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true));  // DefaultModels = []
        using var projectRoot = new TemporaryDirectoryScope();

        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.Succeeded("unused", 0, []));
        var registry = CreateRegistry(claude);
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry);

        var result = orchestrator.InvokeDefault("system", "task",
            projectRoot.Path, TimeSpan.FromSeconds(30));

        Assert.False(result.ProviderResult.Success);
        Assert.Equal(0, claude.InvocationCount);
    }

    /// <summary>
    ///     Verifies that InvokeDefault applies rate-limit failover using DefaultModels, just
    ///     like InvokeAgent does for agent-defined model lists.
    /// </summary>
    [Fact]
    public void InvokeDefaultAppliesRateLimitFailoverUsingDefaultModels()
    {
        AgsSettings.SetCurrent(new AgsSettings(true, true)
            .WithDefaultModels(["claude-sonnet", "chatgpt"]));
        using var projectRoot = new TemporaryDirectoryScope();

        var cooldownExpiry = DateTimeOffset.UtcNow.AddMinutes(10);
        var claude = new StubProvider(ClaudeCodeAdapter.Id, true,
            AIProviderResult.RateLimited("rate limit", 429, cooldownExpiry, "partial"));
        var codex = new StubProvider(CodexAdapter.Id, true,
            AIProviderResult.Succeeded("fallback response", 0, []));
        var registry = CreateRegistry(claude, codex);
        var orchestrator = CreateOrchestrator(projectRoot.Path, registry);

        var result = orchestrator.InvokeDefault("system", "task",
            projectRoot.Path, TimeSpan.FromSeconds(30));

        Assert.True(result.ProviderResult.Success);
        Assert.Equal("fallback response", result.ProviderResult.Output);
        Assert.Equal(CodexAdapter.Id, result.ProviderId);
        Assert.True(registry.IsInCooldown(ClaudeCodeAdapter.Id));
    }

    /// <summary>
    ///     Creates an orchestrator for the specified temporary project root.
    /// </summary>
    /// <param name="projectRootPath">Temporary project root used by the resource loader.</param>
    /// <param name="registry">Provider registry used by the orchestrator.</param>
    /// <param name="timeProvider">Clock used for cooldown calculations.</param>
    /// <returns>A configured <see cref="AgentOrchestrator" /> instance.</returns>
    private static AgentOrchestrator CreateOrchestrator(string projectRootPath,
        AIProviderRegistry registry, TimeProvider timeProvider = null)
    {
        var resourceLoader = new ResourceLoader(projectRootPath);
        var promptAssembler = new PromptAssembler(resourceLoader);
        return new AgentOrchestrator(resourceLoader, promptAssembler, registry, timeProvider);
    }

    /// <summary>
    ///     Creates a standard invocation request used by the orchestration tests.
    /// </summary>
    /// <param name="agentName">Logical name of the agent definition to invoke.</param>
    /// <param name="projectRootPath">Working directory used for the provider request.</param>
    /// <returns>A populated <see cref="AgentInvocationRequest" />.</returns>
    private static AgentInvocationRequest CreateRequest(string agentName, string projectRootPath)
    {
        return new AgentInvocationRequest(agentName, Array.Empty<string>(),
            new PromptContext("Implement the next task.", "Approved scope.", "Keep changes small."),
            "Implement the task.", projectRootPath, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    ///     Creates and registers a provider registry for the supplied providers.
    /// </summary>
    /// <param name="providers">Providers to register in order.</param>
    /// <returns>A populated <see cref="AIProviderRegistry" />.</returns>
    private static AIProviderRegistry CreateRegistry(params IAIProvider[] providers)
    {
        var registry = new AIProviderRegistry();
        foreach (var provider in providers)
            registry.Register(provider);
        return registry;
    }

    /// <summary>
    ///     Writes a minimal valid agent definition into the temporary project's overlay.
    /// </summary>
    /// <param name="projectRootPath">Temporary project root.</param>
    /// <param name="agentName">Logical name of the agent file to create.</param>
    /// <param name="models">Ordered model names written into the definition.</param>
    private static void WriteAgentDefinition(string projectRootPath, string agentName,
        IReadOnlyList<string> models)
    {
        var agentDirectoryPath = Path.Combine(projectRootPath, AgsSettings.AgsDirectoryName,
            InstallDirectory.AgentsDirectoryName);
        Directory.CreateDirectory(agentDirectoryPath);
        var agentPath = Path.Combine(agentDirectoryPath, agentName + ".md");
        var modelList = string.Join("<br>", models.Select(model => "- " + model));
        var content = $$"""
                        # Test Agent

                        | Field | Value |
                        | --- | --- |
                        | `name` | `{{agentName}}` |
                        | `description` | `Test-only agent definition.` |
                        | `must_not` | `- Skip validation.` |
                        | `models` | `{{modelList}}` |
                        | `max_iterations` | `5` |

                        ## Practical Guidance

                        Keep the request deterministic.
                        """;
        File.WriteAllText(agentPath, content);
    }

    /// <summary>
    ///     Fixed clock used to make cooldown calculations deterministic in tests.
    /// </summary>
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset now;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FixedTimeProvider" /> class.
        /// </summary>
        /// <param name="now">Timestamp returned by <see cref="GetUtcNow" />.</param>
        internal FixedTimeProvider(DateTimeOffset now)
        {
            this.now = now;
        }

        /// <summary>
        ///     Gets the fixed UTC time for the test.
        /// </summary>
        /// <returns>The configured timestamp.</returns>
        public override DateTimeOffset GetUtcNow()
        {
            return now;
        }
    }

    /// <summary>
    ///     Queue-driven <see cref="IAIProvider" /> test double.
    /// </summary>
    private sealed class StubProvider : IAIProvider
    {
        private readonly Queue<AIProviderResult> results;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StubProvider" /> class.
        /// </summary>
        /// <param name="providerId">Provider ID returned by the stub.</param>
        /// <param name="isAvailable">Whether the provider is considered installed.</param>
        /// <param name="results">Results returned for successive invocations.</param>
        internal StubProvider(string providerId, bool isAvailable, params AIProviderResult[] results)
        {
            ProviderId = providerId;
            IsAvailable = isAvailable;
            this.results = new Queue<AIProviderResult>(results);
            Requests = [];
        }

        /// <summary>
        ///     Gets the provider ID returned by the stub.
        /// </summary>
        public string ProviderId { get; }

        /// <summary>
        ///     Gets a value indicating whether the provider is considered installed.
        /// </summary>
        public bool IsAvailable { get; }

        /// <inheritdoc />
        public bool TryGetVersion(out string version) { version = string.Empty; return IsAvailable; }

        /// <summary>
        ///     Gets the number of invocations received by the stub.
        /// </summary>
        internal int InvocationCount => Requests.Count;

        /// <summary>
        ///     Gets the requests received by the stub.
        /// </summary>
        internal List<AIProviderRequest> Requests { get; }

        /// <summary>
        ///     Returns the next queued result and records the request.
        /// </summary>
        /// <param name="request">Provider request received from the orchestrator.</param>
        /// <returns>The next queued provider result.</returns>
        public AIProviderResult Invoke(AIProviderRequest request)
        {
            Requests.Add(request);
            Assert.NotEmpty(results);
            return results.Dequeue();
        }
    }
}
