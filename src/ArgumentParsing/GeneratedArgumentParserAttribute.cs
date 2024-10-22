using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing;

/// <summary>
/// Indicates an argument parser method, which implementation is gonna be supplied by the generator
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedArgumentParserAttribute : Attribute
{
    /// <summary>
    /// Type, which will be used as an error message format provider.
    /// If <see langword="null"/> is supplied, either explicitly or implicitly,
    /// default error message formats are used
    /// </summary>
    public Type? ErrorMessageFormatProvider { get; set; }

    /// <summary>
    /// Built-in special command handlers of this parser
    /// </summary>
    public BuiltInCommandHandlers BuiltInCommandHandlers { get; set; } = BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version;

    /// <summary>
    /// Additional special command handlers' types of this parser
    /// </summary>
    public Type[] AdditionalCommandHandlers { get; set; } = [];
}
