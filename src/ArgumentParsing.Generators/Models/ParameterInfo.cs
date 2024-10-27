namespace ArgumentParsing.Generators.Models;

internal sealed record ParameterInfo(
    string Name,
    string PropertyName,
    string BaseType,
    ParseStrategy ParseStrategy,
    bool IsRequired,
    bool IsNullable,
    string? HelpDescription);
