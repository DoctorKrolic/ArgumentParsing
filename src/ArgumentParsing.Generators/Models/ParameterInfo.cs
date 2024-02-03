namespace ArgumentParsing.Generators.Models;

internal sealed record ParameterInfo(
    string PropertyName,
    string Type,
    ParseStrategy ParseStrategy,
    bool IsRequired);
