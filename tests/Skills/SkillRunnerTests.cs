using AGS.ai;
using AGS.orchestration;
using AGS.skills;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="SkillRunner" /> prompt assembly and invocation delegation.
/// </summary>
public sealed class SkillRunnerTests
{
    /// <summary>
    ///     Verifies that the skill name appears in the task prompt sent to the orchestrator.
    /// </summary>
    [Fact]
    public void InvokeSkillIncludesSkillNameInTaskPrompt()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1)));

        Assert.Contains("ags-start", stub.LastTaskPrompt);
    }

    /// <summary>
    ///     Verifies that optional context is appended to the task prompt.
    /// </summary>
    [Fact]
    public void InvokeSkillAppendsContextWhenProvided()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1), "stage: pre-production"));

        Assert.Contains("ags-start", stub.LastTaskPrompt);
        Assert.Contains("stage: pre-production", stub.LastTaskPrompt);
    }

    /// <summary>
    ///     Verifies that no extra content follows the skill name when no context is provided.
    /// </summary>
    [Fact]
    public void InvokeSkillOmitsContextWhenNotProvided()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1)));

        Assert.DoesNotContain("null", stub.LastTaskPrompt, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Verifies that the system prompt sent to the orchestrator is empty (skills are
    ///     discovered natively by the provider, not injected into the system prompt).
    /// </summary>
    [Fact]
    public void InvokeSkillSendsEmptySystemPrompt()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1)));

        Assert.Equal(string.Empty, stub.LastSystemPrompt);
    }

    /// <summary>
    ///     Verifies that the working directory from the request is forwarded to the orchestrator.
    /// </summary>
    [Fact]
    public void InvokeSkillForwardsWorkingDirectory()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\my-project",
            TimeSpan.FromMinutes(1)));

        Assert.Equal("C:\\my-project", stub.LastWorkingDirectory);
    }

    /// <summary>
    ///     Verifies that a zero timeout is replaced by the internal default timeout.
    /// </summary>
    [Fact]
    public void InvokeSkillUsesDefaultTimeoutWhenTimeoutIsZero()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project", TimeSpan.Zero));

        Assert.True(stub.LastTimeout > TimeSpan.Zero);
    }

    /// <summary>
    ///     Verifies that a positive timeout is forwarded as-is to the orchestrator.
    /// </summary>
    [Fact]
    public void InvokeSkillForwardsExplicitTimeout()
    {
        var stub = new StubOrchestrator();
        var runner = new SkillRunner(stub);
        var timeout = TimeSpan.FromMinutes(7);

        runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project", timeout));

        Assert.Equal(timeout, stub.LastTimeout);
    }

    /// <summary>
    ///     Verifies that the result carries the skill name and reflects the orchestrator outcome.
    /// </summary>
    [Fact]
    public void InvokeSkillReturnsResultWithSkillNameAndSuccess()
    {
        var stub = new StubOrchestrator(success: true);
        var runner = new SkillRunner(stub);

        var result = runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1)));

        Assert.Equal("ags-start", result.SkillName);
        Assert.True(result.Success);
    }

    /// <summary>
    ///     Verifies that a failed orchestrator response surfaces as a failed result.
    /// </summary>
    [Fact]
    public void InvokeSkillReturnsFailedResultWhenOrchestratorFails()
    {
        var stub = new StubOrchestrator(success: false);
        var runner = new SkillRunner(stub);

        var result = runner.InvokeSkill(new SkillInvocationRequest("ags-start", "C:\\project",
            TimeSpan.FromMinutes(1)));

        Assert.False(result.Success);
    }

    /// <summary>
    ///     Verifies that a null request throws.
    /// </summary>
    [Fact]
    public void InvokeSkillThrowsOnNullRequest()
    {
        var runner = new SkillRunner(new StubOrchestrator());
        Assert.Throws<ArgumentNullException>(() => runner.InvokeSkill(null));
    }

    /// <summary>
    ///     Verifies that a null orchestrator is rejected at construction time.
    /// </summary>
    [Fact]
    public void ConstructorThrowsOnNullOrchestrator()
    {
        Assert.Throws<ArgumentNullException>(() => new SkillRunner(null));
    }

    /// <summary>
    ///     Minimal <see cref="IAgentOrchestrator" /> stub that records the last invocation
    ///     parameters and returns a configurable result.
    /// </summary>
    private sealed class StubOrchestrator : IAgentOrchestrator
    {
        private readonly bool success;

        internal StubOrchestrator(bool success = true) => this.success = success;

        internal string LastSystemPrompt { get; private set; } = string.Empty;
        internal string LastTaskPrompt { get; private set; } = string.Empty;
        internal string LastWorkingDirectory { get; private set; } = string.Empty;
        internal TimeSpan LastTimeout { get; private set; }

        public AgentInvocationResult InvokeAgent(AgentInvocationRequest request) =>
            throw new NotSupportedException("SkillRunner does not use InvokeAgent.");

        public AgentInvocationResult InvokeDefault(string systemPrompt, string taskPrompt,
            string workingDirectory, TimeSpan timeout)
        {
            LastSystemPrompt = systemPrompt;
            LastTaskPrompt = taskPrompt;
            LastWorkingDirectory = workingDirectory;
            LastTimeout = timeout;

            var providerResult = success
                ? AIProviderResult.Succeeded("ok", 0, [])
                : AIProviderResult.Failed("error", 1);
            return new AgentInvocationResult("stub", providerResult, []);
        }
    }
}
