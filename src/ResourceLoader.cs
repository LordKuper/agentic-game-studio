namespace AGS;

/// <summary>
///     Resolves AGS resources from the project-local <c>.ags</c> overlay and the standard install
///     directory.
/// </summary>
internal sealed class ResourceLoader
{
    private const string MarkdownExtension = ".md";
    private const string SkillDefinitionFileName = "SKILL.md";
    private readonly string projectRootPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceLoader" /> class.
    /// </summary>
    /// <param name="projectRootPath">
    ///     Absolute or relative path to the game project root that may contain a local
    ///     <c>.ags</c> overlay.
    /// </param>
    internal ResourceLoader(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
            throw new ArgumentException("Project root path must be provided.",
                nameof(projectRootPath));
        this.projectRootPath = Path.GetFullPath(projectRootPath);
    }

    /// <summary>
    ///     Gets the normalized absolute path to the current project root.
    /// </summary>
    internal string ProjectRootPath => projectRootPath;

    /// <summary>
    ///     Resolves the absolute path to a resource by checking the project-local overlay first and
    ///     then the standard install directory.
    /// </summary>
    /// <param name="resourceType">
    ///     Logical resource type such as <c>agents</c>, <c>rules</c>, <c>skills</c>, or
    ///     <c>templates</c>. Singular aliases are also accepted.
    /// </param>
    /// <param name="resourceName">Logical resource name without the type root path.</param>
    /// <returns>The absolute path to the resolved resource.</returns>
    internal string ResolveResourcePath(string resourceType, string resourceName)
    {
        var normalizedResourceType = NormalizeResourceType(resourceType);
        var relativeResourcePath = BuildRelativeResourcePath(normalizedResourceType, resourceName);
        var projectResourceRootPath = GetProjectResourceRootPath(normalizedResourceType);
        var projectResourcePath =
            CombineDescendantPath(projectResourceRootPath, relativeResourcePath);
        if (ResourceExists(normalizedResourceType, projectResourcePath)) return projectResourcePath;
        var standardResourceRootPath =
            InstallDirectory.GetStandardResourcePath(normalizedResourceType);
        var standardResourcePath =
            CombineDescendantPath(standardResourceRootPath, relativeResourcePath);
        if (ResourceExists(normalizedResourceType, standardResourcePath))
            return standardResourcePath;
        throw new FileNotFoundException(
            $"Resource '{resourceName}' of type '{normalizedResourceType}' was not found.");
    }

    /// <summary>
    ///     Lists all available resource names for a resource type by merging the standard install
    ///     resources with the project-local overlay.
    /// </summary>
    /// <param name="resourceType">
    ///     Logical resource type such as <c>agents</c>, <c>rules</c>, <c>skills</c>, or
    ///     <c>templates</c>. Singular aliases are also accepted.
    /// </param>
    /// <returns>Sorted logical resource names with project overrides taking precedence.</returns>
    internal IReadOnlyList<string> ListResources(string resourceType)
    {
        var normalizedResourceType = NormalizeResourceType(resourceType);
        var resourceNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddResourceNames(resourceNames,
            InstallDirectory.GetStandardResourcePath(normalizedResourceType),
            normalizedResourceType);
        AddResourceNames(resourceNames, GetProjectResourceRootPath(normalizedResourceType),
            normalizedResourceType);
        var mergedResourceNames = resourceNames.Keys.ToList();
        mergedResourceNames.Sort(StringComparer.OrdinalIgnoreCase);
        return mergedResourceNames;
    }

    /// <summary>
    ///     Reads the content of a resolved resource.
    /// </summary>
    /// <param name="resourceType">
    ///     Logical resource type such as <c>agents</c>, <c>rules</c>, <c>skills</c>, or
    ///     <c>templates</c>. Singular aliases are also accepted.
    /// </param>
    /// <param name="resourceName">Logical resource name without the type root path.</param>
    /// <returns>UTF-8 text content for the resolved resource.</returns>
    internal string ReadResource(string resourceType, string resourceName)
    {
        var normalizedResourceType = NormalizeResourceType(resourceType);
        var resolvedResourcePath = ResolveResourcePath(normalizedResourceType, resourceName);
        if (IsDirectoryBackedResourceType(normalizedResourceType))
        {
            var definitionPath =
                CombineDescendantPath(resolvedResourcePath, SkillDefinitionFileName);
            if (!File.Exists(definitionPath))
                throw new FileNotFoundException(
                    $"Skill '{resourceName}' does not contain a {SkillDefinitionFileName} file.");
            return File.ReadAllText(definitionPath);
        }
        return File.ReadAllText(resolvedResourcePath);
    }

    /// <summary>
    ///     Normalizes supported resource type aliases to the canonical directory name.
    /// </summary>
    /// <param name="resourceType">Caller-supplied resource type.</param>
    /// <returns>Canonical directory name for the resource type.</returns>
    private static string NormalizeResourceType(string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("Resource type must be provided.", nameof(resourceType));
        return resourceType.Trim().ToLowerInvariant() switch
        {
            "agent" => InstallDirectory.AgentsDirectoryName,
            "agents" => InstallDirectory.AgentsDirectoryName,
            "rule" => InstallDirectory.RulesDirectoryName,
            "rules" => InstallDirectory.RulesDirectoryName,
            "skill" => InstallDirectory.SkillsDirectoryName,
            "skills" => InstallDirectory.SkillsDirectoryName,
            "template" => InstallDirectory.TemplatesDirectoryName,
            "templates" => InstallDirectory.TemplatesDirectoryName,
            _ => throw new ArgumentException(
                $"Unsupported resource type '{resourceType}'.", nameof(resourceType))
        };
    }

    /// <summary>
    ///     Gets the resource root under the project-local <c>.ags</c> directory for a resource
    ///     type.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <returns>Absolute path to the resource root under <c>.ags</c>.</returns>
    private string GetProjectResourceRootPath(string resourceType)
    {
        return Path.Combine(projectRootPath, AgsSettings.AgsDirectoryName, resourceType);
    }

    /// <summary>
    ///     Builds the relative path used to resolve a resource within its type root.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <param name="resourceName">Logical resource name supplied by the caller.</param>
    /// <returns>Normalized relative path to the resource.</returns>
    private static string BuildRelativeResourcePath(string resourceType, string resourceName)
    {
        var normalizedResourceName = NormalizeRelativeResourceName(resourceName);
        if (IsDirectoryBackedResourceType(resourceType)) return normalizedResourceName;
        if (Path.HasExtension(normalizedResourceName))
            return normalizedResourceName;
        return normalizedResourceName + MarkdownExtension;
    }

    /// <summary>
    ///     Normalizes a caller-supplied logical resource name to a relative path.
    /// </summary>
    /// <param name="resourceName">Logical resource name supplied by the caller.</param>
    /// <returns>Normalized relative resource path.</returns>
    private static string NormalizeRelativeResourceName(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            throw new ArgumentException("Resource name must be provided.", nameof(resourceName));
        var normalizedResourceName = resourceName.Trim()
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);
        if (normalizedResourceName.Length == 0)
            throw new ArgumentException("Resource name must be provided.", nameof(resourceName));
        if (Path.IsPathRooted(normalizedResourceName))
            throw new ArgumentException("Resource name must be relative.", nameof(resourceName));
        return normalizedResourceName;
    }

    /// <summary>
    ///     Determines whether the resource type is stored as a directory rather than a file.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <returns>
    ///     <see langword="true" /> when the resource type is directory-backed; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool IsDirectoryBackedResourceType(string resourceType)
    {
        return string.Equals(resourceType, InstallDirectory.SkillsDirectoryName,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines whether a concrete resource path exists for the resource type.
    /// </summary>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    /// <param name="resourcePath">Absolute path to inspect.</param>
    /// <returns>
    ///     <see langword="true" /> when the expected file system entry exists; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    private static bool ResourceExists(string resourceType, string resourcePath)
    {
        if (IsDirectoryBackedResourceType(resourceType))
            return Directory.Exists(resourcePath);
        return File.Exists(resourcePath);
    }

    /// <summary>
    ///     Adds discovered resource names from a resource root to the merged listing map.
    /// </summary>
    /// <param name="resourceNames">Merged logical resource names keyed case-insensitively.</param>
    /// <param name="resourceRootPath">Absolute resource root path to scan.</param>
    /// <param name="resourceType">Canonical resource type directory name.</param>
    private static void AddResourceNames(IDictionary<string, string> resourceNames,
        string resourceRootPath, string resourceType)
    {
        if (!Directory.Exists(resourceRootPath)) return;
        if (IsDirectoryBackedResourceType(resourceType))
        {
            AddDirectoryResourceNames(resourceNames, resourceRootPath);
            return;
        }
        AddFileResourceNames(resourceNames, resourceRootPath);
    }

    /// <summary>
    ///     Adds file-backed resources to the merged listing map.
    /// </summary>
    /// <param name="resourceNames">Merged logical resource names keyed case-insensitively.</param>
    /// <param name="resourceRootPath">Absolute resource root path to scan.</param>
    private static void AddFileResourceNames(IDictionary<string, string> resourceNames,
        string resourceRootPath)
    {
        foreach (var filePath in Directory.GetFiles(resourceRootPath, "*" + MarkdownExtension,
                     SearchOption.AllDirectories))
        {
            var logicalResourceName = GetLogicalFileResourceName(resourceRootPath, filePath);
            resourceNames[logicalResourceName] = logicalResourceName;
        }
    }

    /// <summary>
    ///     Adds directory-backed resources to the merged listing map.
    /// </summary>
    /// <param name="resourceNames">Merged logical resource names keyed case-insensitively.</param>
    /// <param name="resourceRootPath">Absolute resource root path to scan.</param>
    private static void AddDirectoryResourceNames(IDictionary<string, string> resourceNames,
        string resourceRootPath)
    {
        foreach (var directoryPath in Directory.GetDirectories(resourceRootPath, "*",
                     SearchOption.TopDirectoryOnly))
        {
            var resourceName = Path.GetFileName(directoryPath);
            if (string.IsNullOrWhiteSpace(resourceName)) continue;
            resourceNames[resourceName] = resourceName;
        }
    }

    /// <summary>
    ///     Converts a file-backed resource path to its logical name without the markdown
    ///     extension.
    /// </summary>
    /// <param name="resourceRootPath">Absolute resource root path.</param>
    /// <param name="filePath">Absolute resource file path.</param>
    /// <returns>Logical resource name relative to the resource root.</returns>
    private static string GetLogicalFileResourceName(string resourceRootPath, string filePath)
    {
        var relativePath = Path.GetRelativePath(resourceRootPath, filePath);
        var extensionLength = MarkdownExtension.Length;
        if (relativePath.EndsWith(MarkdownExtension, StringComparison.OrdinalIgnoreCase))
            relativePath = relativePath[..^extensionLength];
        return relativePath.Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    /// <summary>
    ///     Combines a resource root with a relative path and verifies that the resulting path stays
    ///     within the root.
    /// </summary>
    /// <param name="rootPath">Absolute resource root path.</param>
    /// <param name="relativePath">Relative path under the resource root.</param>
    /// <returns>The normalized absolute combined path.</returns>
    private static string CombineDescendantPath(string rootPath, string relativePath)
    {
        var fullRootPath = Path.GetFullPath(rootPath);
        var combinedPath = Path.GetFullPath(Path.Combine(fullRootPath, relativePath));
        if (!IsDescendantPath(fullRootPath, combinedPath))
            throw new ArgumentException("Resource name must stay within the resource root.",
                nameof(relativePath));
        return combinedPath;
    }

    /// <summary>
    ///     Determines whether a candidate path is the same as or nested under the specified root.
    /// </summary>
    /// <param name="rootPath">Absolute root path.</param>
    /// <param name="candidatePath">Absolute path to validate.</param>
    /// <returns>
    ///     <see langword="true" /> when <paramref name="candidatePath" /> stays inside
    ///     <paramref name="rootPath" />; otherwise, <see langword="false" />.
    /// </returns>
    private static bool IsDescendantPath(string rootPath, string candidatePath)
    {
        if (string.Equals(rootPath, candidatePath, StringComparison.OrdinalIgnoreCase)) return true;
        var normalizedRootPath = EnsureTrailingDirectorySeparator(rootPath);
        return candidatePath.StartsWith(normalizedRootPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Ensures that a directory path ends with a separator so descendant checks cannot match
    ///     partial path prefixes.
    /// </summary>
    /// <param name="path">Absolute directory path.</param>
    /// <returns>The directory path with a trailing separator.</returns>
    private static string EnsureTrailingDirectorySeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar) ||
            path.EndsWith(Path.AltDirectorySeparatorChar))
            return path;
        return path + Path.DirectorySeparatorChar;
    }
}
