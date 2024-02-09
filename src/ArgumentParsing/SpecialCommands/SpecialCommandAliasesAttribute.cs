namespace ArgumentParsing.SpecialCommands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SpecialCommandAliasesAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; } = aliases;
}
