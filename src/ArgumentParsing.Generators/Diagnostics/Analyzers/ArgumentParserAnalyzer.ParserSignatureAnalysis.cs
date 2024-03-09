using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

public partial class ArgumentParserAnalyzer
{
    private static void AnalyzeParserSignature(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (!method.GetAttributes()
            .Any(a => a.AttributeClass?.Equals(knownTypes.GeneratedArgumentParserAttributeType, SymbolEqualityComparer.Default) == true))
        {
            return;
        }

        if (method.Parameters is not [var singleParameter])
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidParserParameterCount, method.Locations.First()));
        }
        else
        {
            var singleParameterSyntax = (BaseParameterSyntax)singleParameter.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken);
            var isInvalidParameter = false;

            if (singleParameter.IsParams ||
                singleParameter.RefKind != RefKind.None ||
                singleParameter.ScopedKind != ScopedKind.None ||
                method.IsExtensionMethod)
            {
                isInvalidParameter = true;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidArgsParameter, singleParameterSyntax.GetLocation()));
            }

            var singleParameterType = singleParameter.Type;

            if (!singleParameterType.IsEnumerableCollectionOfStrings())
            {
                isInvalidParameter = true;

                if (singleParameterType.TypeKind != TypeKind.Error)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidArgsParameterType,
                            singleParameterSyntax.Type?.GetLocation() ?? singleParameter.Locations.First()));
                }
            }

            if (!isInvalidParameter && singleParameter.Name != "args")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.PreferArgsParameterName, singleParameter.Locations.First()));
            }
        }

        var returnType = method.ReturnType;

        if (returnType.TypeKind != TypeKind.Error)
        {
            var returnTypeSyntax = ((MethodDeclarationSyntax)method.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken)).ReturnType;

            if (returnType is not INamedTypeSymbol { TypeArguments: [var optionsType] } namedReturnType ||
                !namedReturnType.ConstructedFrom.Equals(knownTypes.ParseResultOfTType, SymbolEqualityComparer.Default))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ReturnTypeMustBeParseResult, returnTypeSyntax.GetLocation()));
            }
            else if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType || !namedOptionsType.Constructors.Any(c => c.Parameters.Length == 0))
            {
                if (optionsType.TypeKind != TypeKind.Error)
                {
                    var errorNode = returnTypeSyntax is GenericNameSyntax { TypeArgumentList.Arguments: [var genericArgument] } ? genericArgument : returnTypeSyntax;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidOptionsType, errorNode.GetLocation()));
                }
            }
            else
            {
                AnalyzeOptionsType(context, namedOptionsType, knownTypes);
            }
        }
    }
}
