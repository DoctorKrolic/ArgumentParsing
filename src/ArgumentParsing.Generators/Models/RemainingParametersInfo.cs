namespace ArgumentParsing.Generators.Models;

internal sealed record RemainingParametersInfo(
    string PropertyName,
    string Type,
    ParseStrategy ParseStrategy,
    SequenceType SequenceType,
    string? HelpDescription);
