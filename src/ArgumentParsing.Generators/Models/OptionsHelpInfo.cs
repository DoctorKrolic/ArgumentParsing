using ArgumentParsing.Generators.Utils;

namespace ArgumentParsing.Generators.Models;

internal sealed record OptionsHelpInfo(
    string QualifiedName,
    ImmutableEquatableArray<OptionHelpInfo> OptionHelpInfos,
    ImmutableEquatableArray<ParameterHelpInfo> ParameterHelpInfos,
    RemainingParametersHelpInfo? RemainingParametersHelpInfo,
    HelpTextGeneratorInfo? HelpTextGenerator);
