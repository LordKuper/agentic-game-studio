namespace AGS.subsystems;

/// <summary>
///     Holds the outcome of the startup dependency check performed by
///     <see cref="DependencyCheckSubsystem" />.
/// </summary>
internal sealed class DependencyCheckResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DependencyCheckResult" /> class.
    /// </summary>
    /// <param name="jqAvailable">
    ///     Whether the <c>jq</c> command-line tool is installed and reachable.
    /// </param>
    internal DependencyCheckResult(bool jqAvailable)
    {
        JqAvailable = jqAvailable;
    }

    /// <summary>
    ///     Gets a value indicating whether <c>jq</c> is installed and reachable on the current
    ///     machine. When <see langword="false" />, structured output processing falls back to
    ///     regex-based extraction.
    /// </summary>
    internal bool JqAvailable { get; }
}
