using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParserSignatureAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.InvalidParserParameterCount,
            DiagnosticDescriptors.InvalidArgsParameter,
            DiagnosticDescriptors.InvalidArgsParameterType,
            DiagnosticDescriptors.PreferArgsParameterName,
            DiagnosticDescriptors.ReturnTypeMustBeParseResult,
            DiagnosticDescriptors.InvalidOptionsType,
            DiagnosticDescriptors.OptionsTypeMustBeAnnotatedWithAttribute,
            DiagnosticDescriptors.ParserArgumentIsASet);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                GeneratedArgumentParserAttributeType = comp.GetTypeByMetadataName(ArgumentParserGenerator.GeneratedArgumentParserAttributeName),
                ParseResultOfTType = comp.ParseResultOfTType(),
                OptionsTypeAttributeType = comp.OptionsTypeAttributeType(),
            };

            context.RegisterSymbolAction(context => AnalyzeParserSignature(context, knownTypes), SymbolKind.Method);
        });
    }

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
            var singleParameterLocation = singleParameter.Locations.First();
            var singleParameterTypeDiagnosticsLocation = singleParameterSyntax.Type?.GetLocation() ?? singleParameterLocation;

            if (!singleParameterType.IsEnumerableCollectionOfStrings())
            {
                isInvalidParameter = true;

                if (singleParameterType.TypeKind != TypeKind.Error)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidArgsParameterType,
                            singleParameterTypeDiagnosticsLocation));
                }
            }

            if (!isInvalidParameter)
            {
                // Not an equals check with a known type due to https://github.com/dotnet/roslyn/issues/72692
                if (singleParameterType is { MetadataName: "ISet`1", ContainingNamespace: { Name: "Generic", ContainingNamespace: { Name: "Collections", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } } ||
                    singleParameterType.OriginalDefinition.AllInterfaces.Any(i => i is { MetadataName: "ISet`1", ContainingNamespace: { Name: "Generic", ContainingNamespace: { Name: "Collections", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } }))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParserArgumentIsASet,
                            singleParameterTypeDiagnosticsLocation));
                }

                if (singleParameter.Name != "args")
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.PreferArgsParameterName, singleParameterLocation));
                }
            }
        }

        var returnType = method.ReturnType;

        var returnTypeSyntax = ((MethodDeclarationSyntax)method.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken)).ReturnType;
        var genericArgumentErrorSyntax = returnTypeSyntax is GenericNameSyntax { TypeArgumentList.Arguments: [var genericArgument] } ? genericArgument : returnTypeSyntax;

        if (returnType is not INamedTypeSymbol { TypeArguments: [var optionsType] } namedReturnType ||
            !namedReturnType.ConstructedFrom.Equals(knownTypes.ParseResultOfTType, SymbolEqualityComparer.Default))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.ReturnTypeMustBeParseResult,
                    returnTypeSyntax.GetLocation(),
                    effectiveSeverity: returnType.TypeKind == TypeKind.Error ? DiagnosticSeverity.Hidden : DiagnosticDescriptors.ReturnTypeMustBeParseResult.DefaultSeverity,
                    additionalLocations: null,
                    properties: null));
        }
        else if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType || !namedOptionsType.Constructors.Any(c => c.Parameters.Length == 0))
        {
            if (optionsType.TypeKind != TypeKind.Error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidOptionsType, genericArgumentErrorSyntax.GetLocation()));
            }
        }
        else if (!namedOptionsType.GetAttributes().Any(a => a.AttributeClass?.Equals(knownTypes.OptionsTypeAttributeType, SymbolEqualityComparer.Default) == true))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.OptionsTypeMustBeAnnotatedWithAttribute, genericArgumentErrorSyntax.GetLocation()));
        }
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? GeneratedArgumentParserAttributeType { get; init; }

        public required INamedTypeSymbol? ParseResultOfTType { get; init; }

        public required INamedTypeSymbol? OptionsTypeAttributeType { get; init; }
    }
}
