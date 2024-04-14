using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record SpecialCommandHandlerInfo(string Type, ImmutableEquatableArray<string> Aliases);
