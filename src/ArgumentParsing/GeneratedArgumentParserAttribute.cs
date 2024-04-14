namespace ArgumentParsing;

/// <summary>
/// Indicates an argument parser method, which implementation is gonna be supplied by the generator
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedArgumentParserAttribute : Attribute
{
    /// <summary>
    /// List of special command handlers' types of this parser.
    /// If <see langword="null"/> is supplied, parser generates and uses
    /// default implementations of <c>--help</c> and <c>--version</c> commands
    /// </summary>
    public Type[]? SpecialCommandHandlers { get; set; }
}
