using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemainingParametersSuppressor : DiagnosticSuppressor
{
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(SuppressionDescriptors.RemainingParametersInOptionsType);

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

            if (!propertySymbol.ContainingType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, compilation.OptionsTypeAttributeType())))
            {
                continue;
            }

            if (!propertySymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, compilation.RemainingParametersAttributeType())))
            {
                continue;
            }

            if (propertySymbol.Type.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T or SpecialType.System_Collections_Generic_IReadOnlyCollection_T or SpecialType.System_Collections_Generic_IReadOnlyList_T)
            {
                context.ReportSuppression(Suppression.Create(SuppressionDescriptors.RemainingParametersInOptionsType, diagnostic));
            }
        }
    }
}
