using AGS.ai;

namespace AGS.skills;

/// <summary>
///     Synchronizes AGS skills into each AI provider's native skill directory so that the
///     provider can discover and invoke them autonomously during a session.
/// </summary>
/// <remarks>
///     On each <see cref="Synchronize" /> call:
///     <list type="number">
///         <item>
///             All skill directories whose names start with <c>ags-</c> are removed from
///             <em>every</em> supported provider's skill directory, regardless of whether that
///             provider is currently enabled. This ensures stale copies from previously enabled
///             providers are always cleaned up.
///         </item>
///         <item>
///             Current AGS skills resolved via <see cref="ResourceLoader" /> (project overlay
///             takes precedence over the standard install) are copied into the skill directory of
///             each <em>enabled</em> provider.
///         </item>
///     </list>
///     Skills whose names do not start with <c>ags-</c> are never touched.
/// </remarks>
internal sealed class SkillSynchronizer
{
    private const string AgsSkillPrefix = "ags-";

    private readonly ResourceLoader resourceLoader;
    private readonly IReadOnlyList<IAIProvider> allProviders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SkillSynchronizer" /> class.
    /// </summary>
    /// <param name="resourceLoader">
    ///     Resource loader used to discover and resolve skills. Its
    ///     <see cref="ResourceLoader.ProjectRootPath" /> is used as the base for provider skill
    ///     directories.
    /// </param>
    /// <param name="allProviders">
    ///     All supported providers (not only enabled ones). AGS skills are removed from every
    ///     provider in this list before copying to enabled providers.
    /// </param>
    internal SkillSynchronizer(ResourceLoader resourceLoader,
        IReadOnlyList<IAIProvider> allProviders)
    {
        this.resourceLoader =
            resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
        this.allProviders =
            allProviders ?? throw new ArgumentNullException(nameof(allProviders));
    }

    /// <summary>
    ///     Removes all <c>ags-</c> prefixed skills from every supported provider and copies the
    ///     current resolved skill set into each enabled provider's native directory.
    /// </summary>
    /// <param name="enabledProviders">
    ///     Providers that are currently enabled. AGS skills are copied only into the directories
    ///     belonging to these providers.
    /// </param>
    internal void Synchronize(IReadOnlyList<IAIProvider> enabledProviders)
    {
        if (enabledProviders == null) throw new ArgumentNullException(nameof(enabledProviders));

        foreach (var provider in allProviders)
            RemoveAgsSkills(provider.GetSkillDirectory(resourceLoader.ProjectRootPath));

        var agsSkills = ResolveAgsSkillPaths();
        foreach (var provider in enabledProviders)
        {
            var targetRoot = provider.GetSkillDirectory(resourceLoader.ProjectRootPath);
            foreach (var (skillName, sourcePath) in agsSkills)
                CopySkillDirectory(sourcePath, Path.Combine(targetRoot, skillName));
        }
    }

    /// <summary>
    ///     Deletes all <c>ags-</c> prefixed subdirectories from a provider's skill directory.
    ///     Does nothing when the directory does not exist.
    /// </summary>
    private static void RemoveAgsSkills(string providerSkillDirectory)
    {
        if (!Directory.Exists(providerSkillDirectory)) return;
        foreach (var directory in Directory.GetDirectories(providerSkillDirectory, "*",
                     SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(directory);
            if (name != null && name.StartsWith(AgsSkillPrefix, StringComparison.OrdinalIgnoreCase))
                Directory.Delete(directory, true);
        }
    }

    /// <summary>
    ///     Returns the resolved source paths for all <c>ags-</c> prefixed skills, applying the
    ///     overlay resolution order (project overlay overrides standard install).
    /// </summary>
    private IReadOnlyList<(string Name, string SourcePath)> ResolveAgsSkillPaths()
    {
        var result = new List<(string, string)>();
        foreach (var skillName in resourceLoader.ListResources("skills"))
        {
            if (!skillName.StartsWith(AgsSkillPrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var sourcePath = resourceLoader.ResolveResourcePath("skills", skillName);
            result.Add((skillName, sourcePath));
        }
        return result;
    }

    /// <summary>
    ///     Recursively copies a skill directory into the target path, creating directories as
    ///     needed and overwriting existing files.
    /// </summary>
    private static void CopySkillDirectory(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);
        foreach (var file in Directory.GetFiles(sourcePath))
            File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)), overwrite: true);
        foreach (var subdir in Directory.GetDirectories(sourcePath))
            CopySkillDirectory(subdir, Path.Combine(targetPath, Path.GetFileName(subdir)));
    }
}
