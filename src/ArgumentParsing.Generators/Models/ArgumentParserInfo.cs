using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record ArgumentParserInfo(
    HierarchyInfo ContainingTypeHierarchy,
    ArgumentParserMethodInfo MethodInfo,
    OptionsInfo OptionsInfo,
    BuiltInCommandHandlers BuiltInCommandHandlers,
    ImmutableEquatableArray<SpecialCommandHandlerInfo> AdditionalCommandHandlersInfos);
