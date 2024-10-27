namespace ArgumentParsing.Generators.Models;

internal sealed record RemainingParametersInfo(
    string PropertyName,
    string BaseType,
    ParseStrategy ParseStrategy,
    SequenceType SequenceType,
    string? HelpDescription);
