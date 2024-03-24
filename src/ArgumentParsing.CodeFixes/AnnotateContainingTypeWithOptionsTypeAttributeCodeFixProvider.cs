using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AnnotateContainingTypeWithOptionsTypeAttributeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.ParserRelatedPropertyInNonOptionsType.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is PropertyDeclarationSyntax { Parent: TypeDeclarationSyntax typeDeclaration })
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Annotate containing type with [OptionsType] attribute",
                    ct => MakeContainingTypeOptionsType(document, root, typeDeclaration, ct),
                    nameof(AnnotateContainingTypeWithOptionsTypeAttributeCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static async Task<Document> MakeContainingTypeOptionsType(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            Debug.Fail("Shouldn't really happen");
            return document;
        }

        var optionsTypeAttributeType = semanticModel.Compilation.OptionsTypeAttributeType();
        if (optionsTypeAttributeType is null)
        {
            Debug.Fail("Shouldn't really happen");
            return document;
        }

        var generator = SyntaxGenerator.GetGenerator(document);
        var optionsTypeAttributeName = generator.TypeExpression(optionsTypeAttributeType);
        var optionsTypeAttribute = generator.Attribute(optionsTypeAttributeName);
        var fixedTypeDeclaration = generator.AddAttributes(typeDeclaration, optionsTypeAttribute);

        var fixedRoot = root.ReplaceNode(typeDeclaration, fixedTypeDeclaration);
        return document.WithSyntaxRoot(fixedRoot);
    }
}
