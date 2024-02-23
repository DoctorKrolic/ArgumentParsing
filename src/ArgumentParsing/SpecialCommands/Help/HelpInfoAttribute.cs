namespace ArgumentParsing.SpecialCommands.Help;

/// <summary>
/// Used to specify help-related information, that will be used in default help text generation
/// </summary>
/// <param name="description">Help description of a member</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class HelpInfoAttribute(string description) : Attribute
{
    /// <summary>
    /// Help description of a member
    /// </summary>
    public string Description { get; } = description;
}
