using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record OptionsInfo(
    string QualifiedTypeName,
    bool HasAtLeastInternalAccessibility,
    ImmutableEquatableArray<OptionInfo> OptionInfos,
    ImmutableEquatableArray<ParameterInfo> ParameterInfos,
    RemainingParametersInfo? RemainingParametersInfo,
    HelpTextGeneratorInfo? HelpTextGeneratorInfo);
