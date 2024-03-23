using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParserRelatedPropertyMarkedWithRequiredAttributeDiagnosticSuppressor : DiagnosticSuppressor
{
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(SuppressionDescriptors.ParserRelatedPropertyMarkedWithRequiredAttributeInOptionsType);

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

            if (propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Equals(compilation.OptionAttributeType(), SymbolEqualityComparer.Default) == true ||
                                                        a.AttributeClass?.Equals(compilation.ParameterAttributeType(), SymbolEqualityComparer.Default) == true ||
                                                        a.AttributeClass?.Equals(compilation.RemainingParametersAttributeType(), SymbolEqualityComparer.Default) == true))
            {
                context.ReportSuppression(Suppression.Create(SuppressionDescriptors.ParserRelatedPropertyMarkedWithRequiredAttributeInOptionsType, diagnostic));
            }
        }
    }
}
