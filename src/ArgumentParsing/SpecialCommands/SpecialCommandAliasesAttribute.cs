namespace ArgumentParsing.SpecialCommands;

/// <summary>
/// Indicates special command aliases
/// </summary>
/// <param name="aliases">Command aliases</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SpecialCommandAliasesAttribute(params string[] aliases) : Attribute
{
    /// <summary>
    /// Command aliases
    /// </summary>
    public string[] Aliases { get; } = aliases;
}
