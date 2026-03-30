namespace AGS.Tests;

/// <summary>
///     Covers Sharprompt wrapper validation and option mapping.
/// </summary>
public sealed class AgsPromptTests
{
    /// <summary>
    ///     Verifies that confirmation prompts delegate to the configured handler.
    /// </summary>
    [Fact]
    public void ConfirmReturnsConfiguredHandlerValue()
    {
        using var prompts = new PromptStubScope(confirmations: [true]);
        var answer = AgsPrompt.Confirm("Continue?", false);
        Assert.True(answer);
        Assert.Equal(["Continue?"], prompts.ConfirmMessages);
    }

    /// <summary>
    ///     Verifies that selection prompts return the zero-based index of the selected option.
    /// </summary>
    [Fact]
    public void SelectReturnsIndexOfConfiguredOption()
    {
        using var prompts = new PromptStubScope(selectionIndexes: [1]);
        var selectedIndex = AgsPrompt.Select("Choose", ["Alpha", "Beta"]);
        Assert.Equal(1, selectedIndex);
        Assert.Equal(["Choose"], prompts.SelectMessages);
    }

    /// <summary>
    ///     Verifies that prompt argument validation rejects invalid messages and option sets.
    /// </summary>
    [Fact]
    public void PromptMethodsValidateArguments()
    {
        IReadOnlyList<string> missingOptions = null;
        Assert.Throws<ArgumentException>(() => AgsPrompt.Confirm(string.Empty, false));
        Assert.Throws<ArgumentNullException>(() => AgsPrompt.Select("Question", missingOptions));
        Assert.Throws<ArgumentException>(() => AgsPrompt.Select(string.Empty, ["Only option"]));
        Assert.Throws<ArgumentException>(() =>
            AgsPrompt.Select("Question", Array.Empty<string>()));
    }

    /// <summary>
    ///     Verifies that unknown selected values are rejected.
    /// </summary>
    [Fact]
    public void SelectThrowsWhenHandlerReturnsUnknownOption()
    {
        var originalHandler =
            PrivateAccess.GetStaticField<Func<string, IReadOnlyList<string>, string>>(
                typeof(AgsPrompt), "selectHandler");
        try
        {
            PrivateAccess.SetStaticField(typeof(AgsPrompt), "selectHandler",
                (Func<string, IReadOnlyList<string>, string>)((message, options) => "Unknown"));
            Assert.Throws<InvalidOperationException>(() =>
                AgsPrompt.Select("Question", ["Alpha", "Beta"]));
        }
        finally
        {
            PrivateAccess.SetStaticField(typeof(AgsPrompt), "selectHandler", originalHandler);
        }
    }
}
