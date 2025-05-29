namespace ArgumentParsing.Generators.Models;

internal sealed record RemainingParametersInfo(
    string PropertyName,
    string BaseType,
    string FullType,
    ParseStrategy ParseStrategy,
    SequenceType SequenceType,
    DefaultValueStrategy DefaultValueStrategy,
    string? HelpDescription);
