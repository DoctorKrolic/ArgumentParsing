using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SequenceTypedOptionSuppressor : DiagnosticSuppressor
{
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(SuppressionDescriptors.SequenceTypedOptionInOptionsType);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var diagnosticNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

            if (diagnosticNode is not PropertyDeclarationSyntax propertyDeclarationNode)
            {
                continue;
            }

            var semanticModel = context.GetSemanticModel(propertyDeclarationNode.SyntaxTree);
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclarationNode, context.CancellationToken);

            if (propertySymbol is null)
            {
                continue;
            }

            var compilation = semanticModel.Compilation;

            if (!propertySymbol.ContainingType.GetAttributes().Any(a => a.AttributeClass?.Equals(compilation.OptionsTypeAttributeType(), SymbolEqualityComparer.Default) == true))
            {
                continue;
            }

            if (!propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Equals(compilation.OptionAttributeType(), SymbolEqualityComparer.Default) == true))
            {
                continue;
            }

            if (propertySymbol.Type.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T or SpecialType.System_Collections_Generic_IReadOnlyCollection_T or SpecialType.System_Collections_Generic_IReadOnlyList_T)
            {
                context.ReportSuppression(Suppression.Create(SuppressionDescriptors.SequenceTypedOptionInOptionsType, diagnostic));
            }
        }
    }
}
