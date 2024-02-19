namespace ArgumentParsing.SpecialCommands.Help;

[AttributeUsage(AttributeTargets.Property)]
public sealed class HelpInfoAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
