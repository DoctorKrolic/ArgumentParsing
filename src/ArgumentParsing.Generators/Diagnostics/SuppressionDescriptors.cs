using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Diagnostics;

internal static class SuppressionDescriptors
{
    public static readonly SuppressionDescriptor ParserRelatedPropertyMarkedWithRequiredAttributeInOptionsType = new(
        id: "ARGPSPR01",
        suppressedDiagnosticId: "CS8618",
        justification: "Parser-related property marked with [Required] attribute is always assigned by the argument parser");

    public static readonly SuppressionDescriptor SequenceTypedOptionInOptionsType = new(
        id: "ARGPSPR02",
        suppressedDiagnosticId: "CS8618",
        justification: "Sequence-typed option is always assigned by the argument parser");
}
