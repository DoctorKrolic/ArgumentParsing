using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record OptionsInfo(
    string QualifiedTypeName,
    ImmutableEquatableArray<OptionInfo> OptionInfos,
    ImmutableEquatableArray<ParameterInfo> ParameterInfos,
    RemainingParametersInfo? RemainingParametersInfo,
    AssemblyVersionInfo AssemblyVersionInfo);
