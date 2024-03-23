using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Diagnostics;

internal static class SuppressionDescriptors
{
    public static readonly SuppressionDescriptor ParserRelatedPropertyMarkedWithRequiredAttributeInOptionsType = new(
        id: "ARGPSPR01",
        suppressedDiagnosticId: "CS8618",
        justification: "Parser-related property marked with [Required] attribute in options type is always assigned by the argument parser");
}
