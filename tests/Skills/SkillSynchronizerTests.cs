using AGS.ai;
using AGS.skills;

namespace AGS.Tests;

/// <summary>
///     Covers <see cref="SkillSynchronizer" /> cleanup and copy behaviour.
/// </summary>
public sealed class SkillSynchronizerTests
{
    /// <summary>
    ///     Verifies that ags-prefixed skills are removed from a disabled provider's directory
    ///     even when that provider is not in the enabled list.
    /// </summary>
    [Fact]
    public void SynchronizeRemovesAgsSkillsFromDisabledProviders()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        // Create a skill in the disabled provider's directory
        var disabledSkillDir = Path.Combine(projectRoot, ".agents", "skills", "ags-old");
        Directory.CreateDirectory(disabledSkillDir);
        File.WriteAllText(Path.Combine(disabledSkillDir, "SKILL.md"), "old content");

        var disabledProvider = new StubProvider(".agents/skills");
        var synchronizer = BuildSynchronizer(projectRoot, allProviders: [disabledProvider]);

        synchronizer.Synchronize(enabledProviders: []);

        Assert.False(Directory.Exists(disabledSkillDir));
    }

    /// <summary>
    ///     Verifies that non-ags skills in provider directories are never touched.
    /// </summary>
    [Fact]
    public void SynchronizeLeavesNonAgsSkillsUntouched()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        var provider = new StubProvider(".claude/skills");
        var userSkillDir = Path.Combine(projectRoot, ".claude", "skills", "my-custom-skill");
        Directory.CreateDirectory(userSkillDir);
        File.WriteAllText(Path.Combine(userSkillDir, "SKILL.md"), "user skill");

        var synchronizer = BuildSynchronizer(projectRoot, allProviders: [provider]);
        synchronizer.Synchronize(enabledProviders: []);

        Assert.True(Directory.Exists(userSkillDir));
        Assert.Equal("user skill", File.ReadAllText(Path.Combine(userSkillDir, "SKILL.md")));
    }

    /// <summary>
    ///     Verifies that ags skills are copied only into enabled provider directories.
    /// </summary>
    [Fact]
    public void SynchronizeCopiesSkillsOnlyToEnabledProviders()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        // Create a source skill in the install directory
        var skillSource = Path.Combine(projectRoot, "skills", "ags-start");
        Directory.CreateDirectory(skillSource);
        File.WriteAllText(Path.Combine(skillSource, "SKILL.md"), "skill content");

        var enabledProvider = new StubProvider(".claude/skills");
        var disabledProvider = new StubProvider(".agents/skills");
        var resourceLoader = new ResourceLoader(projectRoot);

        // Override InstallDirectory for test: place skills in projectRoot/skills
        // ResourceLoader uses InstallDirectory.GetStandardResourcePath which points to the
        // real install location. To avoid that, we use the project overlay path instead.
        var overlaySkillDir = Path.Combine(projectRoot, AgsSettings.AgsDirectoryName, "skills",
            "ags-start");
        Directory.CreateDirectory(overlaySkillDir);
        File.WriteAllText(Path.Combine(overlaySkillDir, "SKILL.md"), "skill content");

        var synchronizer = new SkillSynchronizer(resourceLoader, [enabledProvider, disabledProvider]);
        synchronizer.Synchronize(enabledProviders: [enabledProvider]);

        var enabledTarget = Path.Combine(projectRoot, ".claude", "skills", "ags-start");
        var disabledTarget = Path.Combine(projectRoot, ".agents", "skills", "ags-start");
        Assert.True(Directory.Exists(enabledTarget), "Skill should be in enabled provider dir.");
        Assert.False(Directory.Exists(disabledTarget), "Skill should not be in disabled provider dir.");
    }

    /// <summary>
    ///     Verifies that the project overlay version of a skill wins over the install version.
    /// </summary>
    [Fact]
    public void SynchronizeUsesOverlayVersionOverInstallVersion()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        // Overlay skill (takes precedence)
        var overlaySkillDir = Path.Combine(projectRoot, AgsSettings.AgsDirectoryName, "skills",
            "ags-start");
        Directory.CreateDirectory(overlaySkillDir);
        File.WriteAllText(Path.Combine(overlaySkillDir, "SKILL.md"), "overlay content");

        var provider = new StubProvider(".claude/skills");
        var resourceLoader = new ResourceLoader(projectRoot);
        var synchronizer = new SkillSynchronizer(resourceLoader, [provider]);
        synchronizer.Synchronize(enabledProviders: [provider]);

        var targetFile = Path.Combine(projectRoot, ".claude", "skills", "ags-start", "SKILL.md");
        Assert.Equal("overlay content", File.ReadAllText(targetFile));
    }

    /// <summary>
    ///     Verifies that calling Synchronize twice leaves the provider directory in the same
    ///     correct state (idempotent).
    /// </summary>
    [Fact]
    public void SynchronizeIsIdempotent()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        var overlaySkillDir = Path.Combine(projectRoot, AgsSettings.AgsDirectoryName, "skills",
            "ags-start");
        Directory.CreateDirectory(overlaySkillDir);
        File.WriteAllText(Path.Combine(overlaySkillDir, "SKILL.md"), "skill content");

        var provider = new StubProvider(".claude/skills");
        var resourceLoader = new ResourceLoader(projectRoot);
        var synchronizer = new SkillSynchronizer(resourceLoader, [provider]);

        synchronizer.Synchronize(enabledProviders: [provider]);
        synchronizer.Synchronize(enabledProviders: [provider]);

        var targetFile = Path.Combine(projectRoot, ".claude", "skills", "ags-start", "SKILL.md");
        Assert.Equal("skill content", File.ReadAllText(targetFile));
    }

    /// <summary>
    ///     Verifies that provider directories are created when they do not exist yet.
    /// </summary>
    [Fact]
    public void SynchronizeCreatesProviderDirectoryWhenAbsent()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        var overlaySkillDir = Path.Combine(projectRoot, AgsSettings.AgsDirectoryName, "skills",
            "ags-start");
        Directory.CreateDirectory(overlaySkillDir);
        File.WriteAllText(Path.Combine(overlaySkillDir, "SKILL.md"), "content");

        var provider = new StubProvider(".claude/skills");
        var resourceLoader = new ResourceLoader(projectRoot);
        var synchronizer = new SkillSynchronizer(resourceLoader, [provider]);

        // Provider skill directory does not exist yet
        Assert.False(Directory.Exists(Path.Combine(projectRoot, ".claude", "skills")));

        synchronizer.Synchronize(enabledProviders: [provider]);

        Assert.True(Directory.Exists(Path.Combine(projectRoot, ".claude", "skills", "ags-start")));
    }

    /// <summary>
    ///     Verifies that subdirectories inside a skill (e.g. references/) are copied recursively.
    /// </summary>
    [Fact]
    public void SynchronizeCopiesSkillSubdirectoriesRecursively()
    {
        using var scope = new TemporaryDirectoryScope();
        var projectRoot = scope.Path;

        var overlaySkillDir = Path.Combine(projectRoot, AgsSettings.AgsDirectoryName, "skills",
            "ags-start");
        var referencesDir = Path.Combine(overlaySkillDir, "references");
        Directory.CreateDirectory(referencesDir);
        File.WriteAllText(Path.Combine(overlaySkillDir, "SKILL.md"), "main");
        File.WriteAllText(Path.Combine(referencesDir, "REFERENCE.md"), "ref content");

        var provider = new StubProvider(".claude/skills");
        var resourceLoader = new ResourceLoader(projectRoot);
        var synchronizer = new SkillSynchronizer(resourceLoader, [provider]);
        synchronizer.Synchronize(enabledProviders: [provider]);

        var copiedRef = Path.Combine(projectRoot, ".claude", "skills", "ags-start", "references",
            "REFERENCE.md");
        Assert.True(File.Exists(copiedRef));
        Assert.Equal("ref content", File.ReadAllText(copiedRef));
    }

    /// <summary>
    ///     Verifies that a null enabled providers list throws.
    /// </summary>
    [Fact]
    public void SynchronizeThrowsOnNullEnabledProviders()
    {
        using var scope = new TemporaryDirectoryScope();
        var resourceLoader = new ResourceLoader(scope.Path);
        var synchronizer = new SkillSynchronizer(resourceLoader, []);
        Assert.Throws<ArgumentNullException>(() => synchronizer.Synchronize(null));
    }

    /// <summary>
    ///     Verifies that a null resource loader is rejected at construction time.
    /// </summary>
    [Fact]
    public void ConstructorThrowsOnNullResourceLoader()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SkillSynchronizer(null, []));
    }

    /// <summary>
    ///     Verifies that a null provider list is rejected at construction time.
    /// </summary>
    [Fact]
    public void ConstructorThrowsOnNullAllProviders()
    {
        using var scope = new TemporaryDirectoryScope();
        var resourceLoader = new ResourceLoader(scope.Path);
        Assert.Throws<ArgumentNullException>(() =>
            new SkillSynchronizer(resourceLoader, null));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SkillSynchronizer BuildSynchronizer(string projectRoot,
        IReadOnlyList<IAIProvider> allProviders)
    {
        var resourceLoader = new ResourceLoader(projectRoot);
        return new SkillSynchronizer(resourceLoader, allProviders);
    }

    /// <summary>
    ///     Minimal <see cref="IAIProvider" /> stub whose skill directory is a fixed relative
    ///     path under the project root.
    /// </summary>
    private sealed class StubProvider : IAIProvider
    {
        private readonly string relativeSkillPath;

        internal StubProvider(string relativeSkillPath)
        {
            this.relativeSkillPath = relativeSkillPath;
        }

        public string ProviderId => relativeSkillPath;
        public bool IsAvailable => true;

        public bool TryGetVersion(out string version)
        {
            version = "1.0.0";
            return true;
        }

        public AIProviderResult Invoke(AIProviderRequest request) =>
            AIProviderResult.Succeeded(string.Empty, 0, []);

        public string GetSkillDirectory(string projectRootPath) =>
            Path.Combine(projectRootPath, relativeSkillPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
