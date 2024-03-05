namespace ArgumentParsing.Generators.Models;

internal sealed record OptionHelpInfo(
    char? ShortName,
    string? LongName,
    bool IsRequired,
    string? HelpDescription);
