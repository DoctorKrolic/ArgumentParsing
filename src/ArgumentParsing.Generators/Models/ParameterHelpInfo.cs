namespace ArgumentParsing.Generators.Models;

internal sealed record ParameterHelpInfo(
    string Name,
    bool IsRequired,
    string? HelpDescription);
