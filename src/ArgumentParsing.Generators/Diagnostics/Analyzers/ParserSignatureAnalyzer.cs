using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
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
            DiagnosticDescriptors.OptionsTypeHasHelpTextGeneratorButNoHelpCommandHandlerInParser,
            DiagnosticDescriptors.DuplicateSpecialCommand,
            DiagnosticDescriptors.BuiltInCommandHelpInfoNeedsSpecificHandler,
            DiagnosticDescriptors.UnnecessaryBuiltInCommandHelpInfo,
            DiagnosticDescriptors.DuplicateBuiltInCommandHelpInfo);

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
                BuiltInCommandHelpInfoAttributeType = comp.BuiltInCommandHelpInfoAttributeType(),
            };

            context.RegisterSymbolAction(context => AnalyzeParserSignature(context, knownTypes), SymbolKind.Method);
        });
    }

    private static void AnalyzeParserSignature(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var method = (IMethodSymbol)context.Symbol;
        var attributes = method.GetAttributes();

        if (attributes.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.GeneratedArgumentParserAttributeType)) is not { } generatedArgParserAttrData)
        {
            return;
        }

        var hasHelpCommand = false;
        var registeredCommands = new HashSet<string>();
        var namedArgs = generatedArgParserAttrData.NamedArguments;

        var builtInHandlers = BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version;

        if (namedArgs.FirstOrDefault(static n => n.Key == "BuiltInCommandHandlers").Value is { Value: byte builtInHandlersByte })
        {
            builtInHandlers = (BuiltInCommandHandlers)builtInHandlersByte;
        }

        if (builtInHandlers.HasFlag(BuiltInCommandHandlers.Help))
        {
            hasHelpCommand = true;
            registeredCommands.Add("--help");
        }

        if (builtInHandlers.HasFlag(BuiltInCommandHandlers.Version))
        {
            registeredCommands.Add("--version");
        }

        var iSpecialCommandHandlerType = knownTypes.ISpecialCommandHandlerType;

        if (iSpecialCommandHandlerType is not null &&
            namedArgs.FirstOrDefault(static n => n.Key == "AdditionalCommandHandlers").Value is { IsNull: false, Values: var additionalCommandHandlers })
        {
            var attributeSyntax = (AttributeSyntax?)generatedArgParserAttrData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);
            var additionalCommandHandlersCollectionSyntax = attributeSyntax?.ArgumentList?.Arguments.First(static a => a.NameEquals?.Name.Identifier.ValueText == "AdditionalCommandHandlers").Expression;
            var namedParametersList = (additionalCommandHandlersCollectionSyntax is CollectionExpressionSyntax collectionExpression
                ? collectionExpression.Elements.Select(static ce => ((ExpressionElementSyntax)ce).Expression)
                : (additionalCommandHandlersCollectionSyntax is ArrayCreationExpressionSyntax arrayCreation
                    ? arrayCreation.Initializer?.Expressions
                    : null)).ToArray();

            for (var i = 0; i < additionalCommandHandlers.Length; i++)
            {
                var commandHandler = additionalCommandHandlers[i];

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
                    foreach (var alias in aliases)
                    {
                        var aliasValue = (string?)alias.Value;

                        if (aliasValue is null)
                        {
                            continue;
                        }

                        if (aliasValue == "--help")
                        {
                            hasHelpCommand = true;
                        }

                        if (!registeredCommands.Add(aliasValue))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.DuplicateSpecialCommand,
                                    method.Locations.First(),
                                    aliasValue));
                        }
                    }
                }
            }
        }

        var builtInHelpInfoFirstAttributeNodes = new Dictionary<BuiltInCommandHandlers, AttributeSyntax>();
        var firstBuiltInHelpInfoFirstAttributeReported = new HashSet<BuiltInCommandHandlers>();

        foreach (var attribute in attributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, knownTypes.BuiltInCommandHelpInfoAttributeType) ||
                attribute.ConstructorArguments is not [{ Value: byte firstCtorArgValByte }, ..])
            {
                continue;
            }

            var firstConstructorVal = (BuiltInCommandHandlers)firstCtorArgValByte;

            if (firstConstructorVal is not (BuiltInCommandHandlers.Help or BuiltInCommandHandlers.Version))
            {
                var constructorArgsSyntax = ((AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken)).ArgumentList!.Arguments;
                SyntaxNode diagnosticNode = constructorArgsSyntax.FirstOrDefault(c => c.NameColon is { Name.Identifier.ValueText: "handler" }) is { } handlerNamedArg
                    ? handlerNamedArg.Expression
                    : constructorArgsSyntax[0];

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.BuiltInCommandHelpInfoNeedsSpecificHandler,
                        diagnosticNode.GetLocation()));
            }
            else
            {
                var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken);

                if (!builtInHandlers.HasFlag(firstConstructorVal))
                {
                    SyntaxNode diagnosticNode = attributeSyntax.Parent is AttributeListSyntax { Attributes.Count: 1 } attrList
                        ? attrList
                        : attributeSyntax;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.UnnecessaryBuiltInCommandHelpInfo,
                            diagnosticNode.GetLocation(),
                            $"BuiltInCommandHandlers.{firstConstructorVal}"));
                }

                if (builtInHelpInfoFirstAttributeNodes.TryGetValue(firstConstructorVal, out var firstAttributeSyntax))
                {
                    if (firstBuiltInHelpInfoFirstAttributeReported.Add(firstConstructorVal))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateBuiltInCommandHelpInfo,
                                firstAttributeSyntax.Name.GetLocation(),
                                $"BuiltInCommandHandlers.{firstConstructorVal}"));
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateBuiltInCommandHelpInfo,
                            attributeSyntax.Name.GetLocation(),
                            $"BuiltInCommandHandlers.{firstConstructorVal}"));
                }
                else
                {
                    builtInHelpInfoFirstAttributeNodes.Add(firstConstructorVal, attributeSyntax);
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
            if (returnType.TypeKind != TypeKind.Error || returnType.Name != "ParseResult")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ReturnTypeMustBeParseResult,
                        returnTypeSyntax.GetLocation(),
                        effectiveSeverity: returnType.TypeKind == TypeKind.Error ? DiagnosticSeverity.Hidden : DiagnosticDescriptors.ReturnTypeMustBeParseResult.DefaultSeverity,
                        additionalLocations: null,
                        properties: null));
            }
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
            var optionsTypeAttributes = namedOptionsType.GetAttributes();

            if (!optionsTypeAttributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.OptionsTypeAttributeType)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.OptionsTypeMustBeAnnotatedWithAttribute, genericArgumentErrorSyntax.GetLocation()));
            }
            else if (!hasHelpCommand && optionsTypeAttributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.HelpTextGeneratorAttributeType)))
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

        public required INamedTypeSymbol? BuiltInCommandHelpInfoAttributeType { get; init; }
    }
}
