using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record ArgumentParserInfo(
    HierarchyInfo ContainingTypeHierarchy,
    ArgumentParserMethodInfo MethodInfo,
    OptionsInfo OptionsInfo,
    string? ErrorMessageFormatProvider,
    ImmutableEquatableArray<BuiltInCommandInfo> BuiltInCommandInfos,
    ImmutableEquatableArray<AdditionalCommandHandlerInfo> AdditionalCommandHandlersInfos);
