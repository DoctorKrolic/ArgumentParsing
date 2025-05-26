namespace ArgumentParsing.Generators.Models;

internal sealed record OptionInfo(
    string PropertyName,
    string BaseType,
    char? ShortName,
    string? LongName,
    ParseStrategy ParseStrategy,
    bool IsRequired,
    bool IsNullable,
    SequenceType SequenceType,
    DefaultValueStrategy DefaultValueStrategy,
    string? HelpDescription);
