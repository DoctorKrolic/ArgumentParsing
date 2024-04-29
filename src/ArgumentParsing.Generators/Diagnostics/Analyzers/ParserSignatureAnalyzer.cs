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
            DiagnosticDescriptors.ParserArgumentIsASet,
            DiagnosticDescriptors.InvalidSpecialCommandHandlerTypeSpecifier,
            DiagnosticDescriptors.OptionsTypeHasHelpTextGeneratorButNoHelpCommandHandlerInParser);

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
                ISetOfTType = comp.GetTypeByMetadataName("System.Collections.Generic.ISet`1"),
                ParseResultOfTType = comp.ParseResultOfTType(),
                OptionsTypeAttributeType = comp.OptionsTypeAttributeType(),
                ISpecialCommandHandlerType = comp.ISpecialCommandHandlerType(),
                SpecialCommandAliasesAttributeType = comp.SpecialCommandAliasesAttributeType(),
                HelpTextGeneratorAttributeType = comp.HelpTextGeneratorAttributeType(),
            };

            context.RegisterSymbolAction(context => AnalyzeParserSignature(context, knownTypes), SymbolKind.Method);
        });
    }

    private static void AnalyzeParserSignature(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Equals(knownTypes.GeneratedArgumentParserAttributeType, SymbolEqualityComparer.Default) == true) is not { } generatedArgParserAttrData)
        {
            return;
        }

        var hasHelpCommand = true;
        var iSpecialCommandHandlerType = knownTypes.ISpecialCommandHandlerType;

        if (iSpecialCommandHandlerType is not null &&
            generatedArgParserAttrData.NamedArguments
            .FirstOrDefault(static n => n.Key == "SpecialCommandHandlers").Value is { IsNull: false, Values: var specialCommandHandlers })
        {
            var attributeSyntax = (AttributeSyntax?)generatedArgParserAttrData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);
            var specialCommandHandlersCollectionSyntax = attributeSyntax?.ArgumentList?.Arguments.First(static a => a.NameEquals?.Name.Identifier.ValueText == "SpecialCommandHandlers").Expression;
            var namedParametersList = (specialCommandHandlersCollectionSyntax is CollectionExpressionSyntax collectionExpression
                ? collectionExpression.Elements.Select(static ce => ((ExpressionElementSyntax)ce).Expression)
                : (specialCommandHandlersCollectionSyntax is ArrayCreationExpressionSyntax arrayCreation
                    ? arrayCreation.Initializer?.Expressions
                    : null)).ToArray();

            hasHelpCommand = false;

            for (var i = 0; i < specialCommandHandlers.Length; i++)
            {
                var commandHandler = specialCommandHandlers[i];

                if (commandHandler is not { Value: INamedTypeSymbol namedHandlerType } ||
                    (namedHandlerType.TypeKind != TypeKind.Error && !namedHandlerType.AllInterfaces.Contains(iSpecialCommandHandlerType)))
                {
                    var associatedSyntaxNode = namedParametersList?[i];
                    if (associatedSyntaxNode is TypeOfExpressionSyntax typeofExpression)
                    {
                        associatedSyntaxNode = typeofExpression.Type;
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidSpecialCommandHandlerTypeSpecifier,
                            associatedSyntaxNode?.GetLocation() ?? attributeSyntax?.GetLocation() ?? method.Locations.First(),
                            commandHandler.IsNull ? (associatedSyntaxNode?.ToString() ?? "null") : commandHandler.Value));
                }
                else if (namedHandlerType.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.SpecialCommandAliasesAttributeType)) is { } aliasesAttr &&
                         aliasesAttr.ConstructorArguments is [{ Kind: TypedConstantKind.Array, Values: var aliases }])
                {
                    hasHelpCommand |= aliases.Any(static a => (string?)a.Value == "--help");
                }
            }
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
                if (singleParameterType.OriginalDefinition.Equals(knownTypes.ISetOfTType, SymbolEqualityComparer.Default) ||
                    singleParameterType.OriginalDefinition.AllInterfaces.Any(i => i.OriginalDefinition.Equals(knownTypes.ISetOfTType, SymbolEqualityComparer.Default)))
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
        else if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType ||
                 !namedOptionsType.Constructors.Any(static c => c.DeclaredAccessibility >= Accessibility.Internal && c.Parameters.IsEmpty))
        {
            if (optionsType.TypeKind != TypeKind.Error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidOptionsType, genericArgumentErrorSyntax.GetLocation()));
            }
        }
        else
        {
            var attributes = namedOptionsType.GetAttributes();

            if (!attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.OptionsTypeAttributeType)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.OptionsTypeMustBeAnnotatedWithAttribute, genericArgumentErrorSyntax.GetLocation()));
            }
            else if (!hasHelpCommand && attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.HelpTextGeneratorAttributeType)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.OptionsTypeHasHelpTextGeneratorButNoHelpCommandHandlerInParser,
                        method.Locations.First(),
                        namedOptionsType));
            }
        }
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? GeneratedArgumentParserAttributeType { get; init; }

        public required INamedTypeSymbol? ISetOfTType { get; init; }

        public required INamedTypeSymbol? ParseResultOfTType { get; init; }

        public required INamedTypeSymbol? OptionsTypeAttributeType { get; init; }

        public required INamedTypeSymbol? ISpecialCommandHandlerType { get; init; }

        public required INamedTypeSymbol? SpecialCommandAliasesAttributeType { get; init; }

        public required INamedTypeSymbol? HelpTextGeneratorAttributeType { get; init; }
    }
}
