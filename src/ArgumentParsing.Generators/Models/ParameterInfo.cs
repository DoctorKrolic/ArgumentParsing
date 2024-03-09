namespace ArgumentParsing.Generators.Models;

internal sealed record ParameterInfo(
    string Name,
    string PropertyName,
    string Type,
    ParseStrategy ParseStrategy,
    bool IsRequired,
    string? NullableUnderlyingType);
