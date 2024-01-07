using System.Collections.Immutable;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Models;
using ArgumentParsing.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static (ArgumentParserInfo? ArgumentParserInfo, ImmutableEquatableArray<DiagnosticInfo> Diagnostics) Extract(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var argumentParserMethodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var argumentParserMethodSymbol = (IMethodSymbol)context.TargetSymbol;

        var diagnosticsBuilder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        SimpleParameterInfo? parameterInfo = null;
        INamedTypeSymbol? validOptionsType = null;

        var hasErrors = false;

        if (argumentParserMethodSymbol.Parameters is not [var singleParameter])
        {
            hasErrors = true;
            diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidParserParameterCount, argumentParserMethodSyntax.Identifier));
        }
        else
        {
            var singleParameterSyntax = argumentParserMethodSyntax.ParameterList.Parameters[0];

            if (singleParameter.IsParams ||
                singleParameter.RefKind != RefKind.None ||
                singleParameter.ScopedKind != ScopedKind.None ||
                argumentParserMethodSymbol.IsExtensionMethod)
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidArgsParameter, singleParameterSyntax));
            }

            var singleParameterType = singleParameter.Type;

            if (!IsEnumerableCollectionOfStrings(singleParameterType))
            {
                hasErrors = true;

                if (singleParameterType.TypeKind != TypeKind.Error)
                {
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidArgsParameterType, singleParameterSyntax.Type!));
                }
            }

            if (!hasErrors && singleParameter.Name != "args")
            {
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.PreferArgsParameterName, singleParameterSyntax.Identifier));
            }

            parameterInfo = new(singleParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), singleParameter.Name);

            static bool IsEnumerableCollectionOfStrings(ITypeSymbol type)
            {
                return type is IArrayTypeSymbol { Rank: 1, ElementType.SpecialType: SpecialType.System_String } ||
                       type.GetMembers("GetEnumerator").Any(e => e is IMethodSymbol { Parameters.Length: 0 } getEnumeratorMethod && IsValidStringEnumerator(getEnumeratorMethod.ReturnType));
            }

            static bool IsValidStringEnumerator(ITypeSymbol type)
            {
                return type.GetMembers("MoveNext").Any(m => m is IMethodSymbol { Parameters.Length: 0, ReturnType.SpecialType: SpecialType.System_Boolean }) &&
                       type.GetMembers("Current").Any(c => c is IPropertySymbol { Type.SpecialType: SpecialType.System_String, GetMethod: not null });
            }
        }

        var returnTypeSyntax = argumentParserMethodSyntax.ReturnType;
        var returnType = argumentParserMethodSymbol.ReturnType;

        if (returnType is not INamedTypeSymbol { Name: "ParseResult", TypeArguments: [var optionsType], ContainingNamespace: { Name: "Results", ContainingNamespace: { Name: "ArgumentParsing", ContainingNamespace.IsGlobalNamespace: true } } })
        {
            hasErrors = true;

            if (returnType.TypeKind != TypeKind.Error)
            {
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.ReturnTypeMustBeParseResult, returnTypeSyntax));
            }
        }
        else
        {
            if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType || !namedOptionsType.Constructors.Any(c => c.Parameters.Length == 0))
            {
                hasErrors = true;

                if (optionsType.TypeKind != TypeKind.Error)
                {
                    var errorNode = returnTypeSyntax is GenericNameSyntax { TypeArgumentList.Arguments: [var genericArgument] } genericName ? genericArgument : returnTypeSyntax;
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidOptionsType, errorNode));
                }
            }
            else
            {
                validOptionsType = namedOptionsType;
            }
        }

        if (validOptionsType is null)
        {
            var diagnosticsArr = diagnosticsBuilder.ToImmutable();
            return (null, ImmutableEquatableArray.AsEquatableArray(diagnosticsArr));
        }

        // TODO: analyze and extract information from options type

        if (hasErrors)
        {
            var diagnosticsArr = diagnosticsBuilder.ToImmutable();
            return (null, ImmutableEquatableArray.AsEquatableArray(diagnosticsArr));
        }

        Debug.Assert(parameterInfo is not null);

        var methodInfo = new ArgumentParserMethodInfo(
            argumentParserMethodSyntax.Modifiers.ToString(),
            argumentParserMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            argumentParserMethodSymbol.Name,
            parameterInfo!);

        var argumentParserInfo = new ArgumentParserInfo(
            HierarchyInfo.From(argumentParserMethodSymbol.ContainingType),
            methodInfo,
            new(validOptionsType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))));

        return (argumentParserInfo, ImmutableEquatableArray<DiagnosticInfo>.Empty);
    }
}
