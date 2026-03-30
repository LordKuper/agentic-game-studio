namespace AGS.Tests;

/// <summary>
///     Covers resource overlay resolution and merged resource listings.
/// </summary>
public sealed class ResourceLoaderTests : IDisposable
{
    private readonly TemporaryDirectoryScope installDirectory = new();
    private readonly TemporaryDirectoryScope projectRoot = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceLoaderTests" /> class.
    /// </summary>
    public ResourceLoaderTests()
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
    ///     Verifies that project-local markdown resources override standard install resources.
    /// </summary>
    [Fact]
    public void ResolveResourcePathPrefersProjectMarkdownOverride()
    {
        WriteStandardMarkdownResource("agents", "game-designer", "standard");
        var projectResourcePath =
            WriteProjectMarkdownResource("agents", "game-designer", "project");
        var resourceLoader = CreateResourceLoader();

        var resolvedResourcePath = resourceLoader.ResolveResourcePath("agents",
            "game-designer");

        Assert.Equal(projectResourcePath, resolvedResourcePath);
    }

    /// <summary>
    ///     Verifies that standard install resources are used when no project override exists.
    /// </summary>
    [Fact]
    public void ResolveResourcePathFallsBackToStandardMarkdownResource()
    {
        var standardResourcePath =
            WriteStandardMarkdownResource("rules", "session-workflow", "standard");
        var resourceLoader = CreateResourceLoader();

        var resolvedResourcePath =
            resourceLoader.ResolveResourcePath("rules", "session-workflow");

        Assert.Equal(standardResourcePath, resolvedResourcePath);
    }

    /// <summary>
    ///     Verifies that skill resources resolve to the project-local skill directory when present.
    /// </summary>
    [Fact]
    public void ResolveResourcePathPrefersProjectSkillDirectory()
    {
        WriteStandardSkill("ags-start", "standard");
        var projectSkillDirectory = WriteProjectSkill("ags-start", "project");
        var resourceLoader = CreateResourceLoader();

        var resolvedSkillPath = resourceLoader.ResolveResourcePath("skills", "ags-start");

        Assert.Equal(projectSkillDirectory, resolvedSkillPath);
    }

    /// <summary>
    ///     Verifies that merged file-backed listings contain standard-only resources, project-only
    ///     resources, and one logical name for overridden resources.
    /// </summary>
    [Fact]
    public void ListResourcesMergesFileBackedResources()
    {
        WriteStandardMarkdownResource("agents", "alpha", "standard alpha");
        WriteStandardMarkdownResource("agents", "shared", "standard shared");
        WriteProjectMarkdownResource("agents", "shared", "project shared");
        WriteProjectMarkdownResource("agents", "project-only", "project only");
        var resourceLoader = CreateResourceLoader();

        var resourceNames = resourceLoader.ListResources("agents");

        Assert.Equal(["alpha", "project-only", "shared"], resourceNames);
    }

    /// <summary>
    ///     Verifies that merged skill listings contain standard-only directories, project-only
    ///     directories, and one logical name for overridden skills.
    /// </summary>
    [Fact]
    public void ListResourcesMergesSkillDirectories()
    {
        WriteStandardSkill("alpha-skill", "alpha");
        WriteStandardSkill("shared-skill", "shared");
        WriteProjectSkill("shared-skill", "override");
        WriteProjectSkill("project-skill", "project");
        var resourceLoader = CreateResourceLoader();

        var resourceNames = resourceLoader.ListResources("skills");

        Assert.Equal(["alpha-skill", "project-skill", "shared-skill"], resourceNames);
    }

    /// <summary>
    ///     Verifies that reading a markdown resource returns the project-local override content.
    /// </summary>
    [Fact]
    public void ReadResourceReturnsProjectMarkdownOverrideContent()
    {
        WriteStandardMarkdownResource("templates", "agent-template", "standard");
        WriteProjectMarkdownResource("templates", "agent-template", "project");
        var resourceLoader = CreateResourceLoader();

        var content = resourceLoader.ReadResource("templates", "agent-template");

        Assert.Equal("project", content);
    }

    /// <summary>
    ///     Verifies that reading a skill resource returns the content of the resolved
    ///     <c>SKILL.md</c> file.
    /// </summary>
    [Fact]
    public void ReadResourceReturnsSkillDefinitionContent()
    {
        WriteStandardSkill("ags-start", "skill content");
        var resourceLoader = CreateResourceLoader();

        var content = resourceLoader.ReadResource("skill", "ags-start");

        Assert.Equal("skill content", content);
    }

    /// <summary>
    ///     Verifies that unsupported resource types are rejected.
    /// </summary>
    [Fact]
    public void ResolveResourcePathThrowsForUnsupportedResourceType()
    {
        var resourceLoader = CreateResourceLoader();

        var exception = Assert.Throws<ArgumentException>(() =>
            resourceLoader.ResolveResourcePath("unknown", "value"));

        Assert.Contains("Unsupported resource type", exception.Message);
    }

    /// <summary>
    ///     Verifies that missing resources produce a file-not-found error.
    /// </summary>
    [Fact]
    public void ResolveResourcePathThrowsForMissingResource()
    {
        var resourceLoader = CreateResourceLoader();

        Assert.Throws<FileNotFoundException>(() =>
            resourceLoader.ResolveResourcePath("agents", "missing-resource"));
    }

    /// <summary>
    ///     Verifies that resource names cannot escape their resource root.
    /// </summary>
    [Fact]
    public void ResolveResourcePathRejectsPathTraversal()
    {
        var resourceLoader = CreateResourceLoader();

        Assert.Throws<ArgumentException>(() =>
            resourceLoader.ResolveResourcePath("agents", "..\\outside"));
    }

    /// <summary>
    ///     Creates a resource loader for the current temporary project root.
    /// </summary>
    /// <returns>Configured resource loader instance.</returns>
    private ResourceLoader CreateResourceLoader()
    {
        return new ResourceLoader(projectRoot.Path);
    }

    /// <summary>
    ///     Writes a standard markdown-backed resource under the temporary install directory.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <param name="resourceName">Logical resource name without the markdown extension.</param>
    /// <param name="content">Content written to the resource file.</param>
    /// <returns>Absolute path to the created file.</returns>
    private string WriteStandardMarkdownResource(string resourceType, string resourceName,
        string content)
    {
        return WriteMarkdownResource(installDirectory.Path, resourceType, resourceName, content);
    }

    /// <summary>
    ///     Writes a project-local markdown-backed resource under the temporary <c>.ags</c>
    ///     overlay.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <param name="resourceName">Logical resource name without the markdown extension.</param>
    /// <param name="content">Content written to the resource file.</param>
    /// <returns>Absolute path to the created file.</returns>
    private string WriteProjectMarkdownResource(string resourceType, string resourceName,
        string content)
    {
        var overlayRootPath = Path.Combine(projectRoot.Path, AgsSettings.AgsDirectoryName);
        return WriteMarkdownResource(overlayRootPath, resourceType, resourceName, content);
    }

    /// <summary>
    ///     Writes a markdown-backed resource under the provided base directory.
    /// </summary>
    /// <param name="baseDirectoryPath">Base directory that contains the resource type folder.</param>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <param name="resourceName">Logical resource name without the markdown extension.</param>
    /// <param name="content">Content written to the resource file.</param>
    /// <returns>Absolute path to the created file.</returns>
    private static string WriteMarkdownResource(string baseDirectoryPath, string resourceType,
        string resourceName, string content)
    {
        var directoryPath = Path.Combine(baseDirectoryPath, resourceType);
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, resourceName + ".md");
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    ///     Writes a standard skill directory under the temporary install directory.
    /// </summary>
    /// <param name="skillName">Logical skill name.</param>
    /// <param name="skillDefinition">Content written to <c>SKILL.md</c>.</param>
    /// <returns>Absolute path to the created skill directory.</returns>
    private string WriteStandardSkill(string skillName, string skillDefinition)
    {
        return WriteSkill(installDirectory.Path, skillName, skillDefinition);
    }

    /// <summary>
    ///     Writes a project-local skill directory under the temporary <c>.ags</c> overlay.
    /// </summary>
    /// <param name="skillName">Logical skill name.</param>
    /// <param name="skillDefinition">Content written to <c>SKILL.md</c>.</param>
    /// <returns>Absolute path to the created skill directory.</returns>
    private string WriteProjectSkill(string skillName, string skillDefinition)
    {
        var overlayRootPath = Path.Combine(projectRoot.Path, AgsSettings.AgsDirectoryName);
        return WriteSkill(overlayRootPath, skillName, skillDefinition);
    }

    /// <summary>
    ///     Writes a skill directory with a <c>SKILL.md</c> file under the provided base directory.
    /// </summary>
    /// <param name="baseDirectoryPath">Base directory that contains the skills folder.</param>
    /// <param name="skillName">Logical skill name.</param>
    /// <param name="skillDefinition">Content written to <c>SKILL.md</c>.</param>
    /// <returns>Absolute path to the created skill directory.</returns>
    private static string WriteSkill(string baseDirectoryPath, string skillName,
        string skillDefinition)
    {
        var skillDirectoryPath = Path.Combine(baseDirectoryPath, "skills", skillName);
        Directory.CreateDirectory(skillDirectoryPath);
        File.WriteAllText(Path.Combine(skillDirectoryPath, "SKILL.md"), skillDefinition);
        return skillDirectoryPath;
    }
}
