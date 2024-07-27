using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record ArgumentParserHelpInfo(
    OptionsInfo OptionsInfo,
    BuiltInCommandHandlers BuiltInCommandHandlers,
    ImmutableEquatableArray<SpecialCommandHandlerInfo> AdditionalCommandHandlersInfos,
    AssemblyVersionInfo AssemblyVersionInfo);
