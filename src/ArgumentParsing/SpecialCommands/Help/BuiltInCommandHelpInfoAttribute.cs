namespace ArgumentParsing.SpecialCommands.Help;

/// <summary>
/// Used to specify help-related information for a built-in command,
/// that will be used in default help text generation. This attribute is applied
/// to an argument parser method
/// </summary>
/// <param name="handler">Handler, for which help info is specified</param>
/// <param name="description">Help description of a command</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class BuiltInCommandHelpInfoAttribute(BuiltInCommandHandlers handler, string description) : Attribute
{
    /// <summary>
    /// Handler, for which help info is specified
    /// </summary>
    public BuiltInCommandHandlers Handler { get; } = handler;

    /// <summary>
    /// Help description of a command
    /// </summary>
    public string Description { get; } = description;
}
