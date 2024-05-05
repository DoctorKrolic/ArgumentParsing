using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class DeclareSpecialCommandAliasCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.SpecialCommandHandlerMustHaveAliases.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root?.FindNode(context.Span) is not TypeDeclarationSyntax commandHandlerDeclaration)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagType = diagnostic.Properties["Type"];
        if (diagType == SpecialCommandHandlerAnalyzer.SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.NoAttribute)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add [SpecialCommandAliases] attribute",
                    ct => AddAliasesAttribute(document, root, commandHandlerDeclaration, ct),
                    $"{nameof(DeclareSpecialCommandAliasCodeFixProvider)}_{nameof(AddAliasesAttribute)}"),
                diagnostic);
        }
        else
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel?.GetDeclaredSymbol(commandHandlerDeclaration) is not { } commandHandler ||
                semanticModel.Compilation.SpecialCommandAliasesAttributeType() is not { } specialCommandAliasesAttributeType)
            {
                return;
            }

            var aliasesAttributeReference = commandHandler
                .GetAttributes()
                .First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, specialCommandAliasesAttributeType))
                .ApplicationSyntaxReference;

            if (aliasesAttributeReference is null)
            {
                return;
            }

            var attributeSyntax = (AttributeSyntax)(await aliasesAttributeReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false));
            var attributeDocument = document.Project.Solution.GetDocument(attributeSyntax.SyntaxTree)!;

            switch (diagType)
            {
                case SpecialCommandHandlerAnalyzer.SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.NullValues:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Replace 'null' with valid alias",
                            ct => ChangeToValidAliasFromNullValue(attributeDocument, commandHandler.Name, attributeSyntax, ct),
                            $"{nameof(DeclareSpecialCommandAliasCodeFixProvider)}_{nameof(ChangeToValidAliasFromNullValue)}"),
                        diagnostic);
                    break;
                case SpecialCommandHandlerAnalyzer.SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.EmptyValues:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Add command alias",
                            ct => AddAliasToAttributeArguments(attributeDocument, commandHandler.Name, attributeSyntax, ct),
                            $"{nameof(DeclareSpecialCommandAliasCodeFixProvider)}_{nameof(AddAliasToAttributeArguments)}"),
                        diagnostic);
                    break;
            }
        }
    }

    private static async Task<Document> AddAliasesAttribute(Document document, SyntaxNode root, TypeDeclarationSyntax commandHandlerDeclaration, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel?.Compilation.SpecialCommandAliasesAttributeType() is not { } specialCommandAliasesAttributeType)
        {
            Debug.Fail("Shouldn't really happen");
            return document;
        }

        var generator = SyntaxGenerator.GetGenerator(document);

        var attributeArgument =
            generator.AttributeArgument(
                generator.LiteralExpression(
                    GetBestCommandAliasPrediction(commandHandlerDeclaration.Identifier.ValueText)));
        var attribute = generator.Attribute(generator.TypeExpression(specialCommandAliasesAttributeType), [attributeArgument]);

        var fixedDeclaration = generator.AddAttributes(commandHandlerDeclaration, attribute);
        var fixedRoot = root.ReplaceNode(commandHandlerDeclaration, fixedDeclaration);
        return document.WithSyntaxRoot(fixedRoot);
    }

    private static async Task<Solution> ChangeToValidAliasFromNullValue(Document document, string commandHandlerName, AttributeSyntax attribute, CancellationToken cancellationToken)
    {
        var generator = SyntaxGenerator.GetGenerator(document);
        var attributeArgument =
            generator.AttributeArgument(
                generator.LiteralExpression(
                    GetBestCommandAliasPrediction(commandHandlerName)));

        var argumentList = attribute.ArgumentList!;
        var arguments = argumentList.Arguments;
        var originalArgument = arguments.First();

        var fixedAttribute = attribute
            .WithArgumentList(
                argumentList.WithArguments(
                    arguments.Replace(originalArgument, (AttributeArgumentSyntax)attributeArgument)));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var fixedRoot = root!.ReplaceNode(attribute, fixedAttribute);
        return document.WithSyntaxRoot(fixedRoot).Project.Solution;
    }

    private static async Task<Solution> AddAliasToAttributeArguments(Document document, string commandHandlerName, AttributeSyntax attribute, CancellationToken cancellationToken)
    {
        var generator = SyntaxGenerator.GetGenerator(document);
        var attributeArgument =
            generator.AttributeArgument(
                generator.LiteralExpression(
                    GetBestCommandAliasPrediction(commandHandlerName)));

        var argumentList = attribute.ArgumentList;
        AttributeSyntax fixedAttribute;
        if (argumentList is null or { Arguments.Count: 0 })
        {
            fixedAttribute = ((AttributeListSyntax)generator.AddAttributeArguments(attribute, [attributeArgument])).Attributes.First();
        }
        else
        {
            var arguments = argumentList.Arguments;
            var originalArgument = arguments.First();

            fixedAttribute = attribute
                .WithArgumentList(
                    argumentList.WithArguments(
                        arguments.Replace(originalArgument, (AttributeArgumentSyntax)attributeArgument)));
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var fixedRoot = root!.ReplaceNode(attribute, fixedAttribute);
        return document.WithSyntaxRoot(fixedRoot).Project.Solution;
    }

    private static string GetBestCommandAliasPrediction(string typeName)
    {
        const string specialCommandHandlerSuffix = "SpecialCommandHandler";
        const string commandHandlerSuffix = "CommandHandler";

        if (typeName.EndsWith(specialCommandHandlerSuffix))
        {
            typeName = typeName.Substring(0, typeName.Length - specialCommandHandlerSuffix.Length);
        }

        if (typeName.EndsWith(commandHandlerSuffix))
        {
            typeName = typeName.Substring(0, typeName.Length - commandHandlerSuffix.Length);
        }

        return $"--{typeName.ToKebabCase()}";
    }
}
