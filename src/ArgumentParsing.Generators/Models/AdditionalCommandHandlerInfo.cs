using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record AdditionalCommandHandlerInfo(string Type, ImmutableEquatableArray<string> Aliases, string? HelpDescription);
