using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing;

/// <summary>
/// Indicates an argument parser method, which implementation is gonna be supplied by the generator
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedArgumentParserAttribute : Attribute
{
    /// <summary>
    /// Built-in special command handlers of this parser
    /// </summary>
    public BuiltInCommandHandlers BuiltInCommandHandlers { get; set; } = BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version;

    /// <summary>
    /// Additional special command handlers' types of this parser
    /// </summary>
    public Type[] AdditionalCommandHandlers { get; set; } = [];
}
