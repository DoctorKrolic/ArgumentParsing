using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record ArgumentParserHelpInfo(
    OptionsInfo OptionsInfo,
    ImmutableEquatableArray<BuiltInCommandInfo> BuiltInCommandInfos,
    ImmutableEquatableArray<AdditionalCommandHandlerInfo> AdditionalCommandHandlersInfos,
    AssemblyVersionInfo AssemblyVersionInfo);
