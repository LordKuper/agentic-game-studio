namespace AGS.Tests;

/// <summary>
///     Covers install directory resolution and standard resource path construction.
/// </summary>
public sealed class InstallDirectoryTests : IDisposable
{
    private readonly TemporaryDirectoryScope tempDirectory = new();

    /// <summary>
    ///     Restores the default base directory provider after each test.
    /// </summary>
    public void Dispose()
    {
        InstallDirectory.ResetBaseDirectoryProvider();
        tempDirectory.Dispose();
    }

    /// <summary>
    ///     Verifies that <see cref="InstallDirectory.GetInstallPath" /> returns the value from the
    ///     configured base directory provider.
    /// </summary>
    [Fact]
    public void GetInstallPathReturnsBaseDirectory()
    {
        InstallDirectory.SetBaseDirectoryProvider(() => tempDirectory.Path);

        var installPath = InstallDirectory.GetInstallPath();

        Assert.Equal(tempDirectory.Path, installPath);
    }

    /// <summary>
    ///     Verifies that <see cref="InstallDirectory.GetStandardResourcePath" /> combines the
    ///     install path with the resource directory name.
    /// </summary>
    [Theory]
    [InlineData("agents")]
    [InlineData("rules")]
    [InlineData("skills")]
    [InlineData("templates")]
    public void GetStandardResourcePathCombinesInstallPathWithResourceDirectory(
        string resourceDirectoryName)
    {
        InstallDirectory.SetBaseDirectoryProvider(() => tempDirectory.Path);

        var resourcePath = InstallDirectory.GetStandardResourcePath(resourceDirectoryName);

        Assert.Equal(Path.Combine(tempDirectory.Path, resourceDirectoryName), resourcePath);
    }

    /// <summary>
    ///     Verifies that <see cref="InstallDirectory.GetStandardResourceFilePath" /> combines
    ///     install path, resource directory, and file name.
    /// </summary>
    [Fact]
    public void GetStandardResourceFilePathCombinesAllSegments()
    {
        InstallDirectory.SetBaseDirectoryProvider(() => tempDirectory.Path);

        var filePath = InstallDirectory.GetStandardResourceFilePath("agents", "game-designer.md");

        Assert.Equal(Path.Combine(tempDirectory.Path, "agents", "game-designer.md"), filePath);
    }

    /// <summary>
    ///     Verifies that <see cref="InstallDirectory.StandardResourceDirectoryExists" /> returns
    ///     <see langword="true" /> when the directory exists.
    /// </summary>
    [Fact]
    public void StandardResourceDirectoryExistsReturnsTrueWhenDirectoryExists()
    {
        InstallDirectory.SetBaseDirectoryProvider(() => tempDirectory.Path);
        Directory.CreateDirectory(Path.Combine(tempDirectory.Path, "agents"));

        Assert.True(InstallDirectory.StandardResourceDirectoryExists("agents"));
    }

    /// <summary>
    ///     Verifies that <see cref="InstallDirectory.StandardResourceDirectoryExists" /> returns
    ///     <see langword="false" /> when the directory does not exist.
    /// </summary>
    [Fact]
    public void StandardResourceDirectoryExistsReturnsFalseWhenDirectoryMissing()
    {
        InstallDirectory.SetBaseDirectoryProvider(() => tempDirectory.Path);

        Assert.False(InstallDirectory.StandardResourceDirectoryExists("agents"));
    }

    /// <summary>
    ///     Verifies that the default base directory provider returns
    ///     <see cref="AppContext.BaseDirectory" />.
    /// </summary>
    [Fact]
    public void DefaultBaseDirectoryProviderReturnsAppContextBaseDirectory()
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        Assert.Equal(AppContext.BaseDirectory, InstallDirectory.GetInstallPath());
    }

    /// <summary>
    ///     Verifies that standard resource directories are present in the build output directory.
    /// </summary>
    [Theory]
    [InlineData("agents")]
    [InlineData("rules")]
    [InlineData("skills")]
    [InlineData("templates")]
    public void StandardResourceDirectoriesExistInBuildOutput(string resourceDirectoryName)
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        var resourcePath = InstallDirectory.GetStandardResourcePath(resourceDirectoryName);

        Assert.True(Directory.Exists(resourcePath),
            $"Standard resource directory '{resourceDirectoryName}' should exist at {resourcePath}");
    }

    /// <summary>
    ///     Verifies that the agents directory in the build output contains at least one .md file.
    /// </summary>
    [Fact]
    public void AgentsDirectoryInBuildOutputContainsFiles()
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        var agentsPath = InstallDirectory.GetStandardResourcePath("agents");
        var agentFiles = Directory.GetFiles(agentsPath, "*.md");

        Assert.NotEmpty(agentFiles);
    }

    /// <summary>
    ///     Verifies that the rules directory in the build output contains at least one .md file.
    /// </summary>
    [Fact]
    public void RulesDirectoryInBuildOutputContainsFiles()
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        var rulesPath = InstallDirectory.GetStandardResourcePath("rules");
        var ruleFiles = Directory.GetFiles(rulesPath, "*.md");

        Assert.NotEmpty(ruleFiles);
    }

    /// <summary>
    ///     Verifies that the templates directory in the build output contains at least one .md file.
    /// </summary>
    [Fact]
    public void TemplatesDirectoryInBuildOutputContainsFiles()
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        var templatesPath = InstallDirectory.GetStandardResourcePath("templates");
        var templateFiles = Directory.GetFiles(templatesPath, "*.md");

        Assert.NotEmpty(templateFiles);
    }

    /// <summary>
    ///     Verifies that the skills directory in the build output contains at least one subdirectory.
    /// </summary>
    [Fact]
    public void SkillsDirectoryInBuildOutputContainsSubdirectories()
    {
        InstallDirectory.ResetBaseDirectoryProvider();

        var skillsPath = InstallDirectory.GetStandardResourcePath("skills");
        var skillDirectories = Directory.GetDirectories(skillsPath);

        Assert.NotEmpty(skillDirectories);
    }
}
