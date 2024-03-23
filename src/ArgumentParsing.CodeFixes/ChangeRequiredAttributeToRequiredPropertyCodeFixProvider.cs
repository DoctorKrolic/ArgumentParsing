using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ChangeRequiredAttributeToRequiredPropertyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.UseRequiredProperty.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var diagnosticNode = root?.FindNode(context.Span);

        if (diagnosticNode is AttributeSyntax or AttributeListSyntax)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Change to required property",
                    _ => ChangeToRequiredProperty(document, root!, diagnosticNode),
                    nameof(ChangeRequiredAttributeToRequiredPropertyCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static Task<Document> ChangeToRequiredProperty(Document document, SyntaxNode root, SyntaxNode diagnosticNode)
    {
        SyntaxNode propertyDeclaration;
        SyntaxNode propertyDeclarationWithoutAttribute;

        switch (diagnosticNode)
        {
            case AttributeListSyntax attributeList:
                Debug.Assert(attributeList.Attributes.Count == 1);
                propertyDeclaration = attributeList.Parent!;
                propertyDeclarationWithoutAttribute = propertyDeclaration.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia)!;
                break;
            case AttributeSyntax { Parent: AttributeListSyntax containingAttributeList }:
                var fixedContainingList = containingAttributeList.RemoveNode(diagnosticNode, SyntaxRemoveOptions.KeepNoTrivia)!;
                propertyDeclaration = containingAttributeList.Parent!;
                propertyDeclarationWithoutAttribute = propertyDeclaration.ReplaceNode(containingAttributeList, fixedContainingList);
                break;
            default:
                throw new InvalidOperationException("Unreachable");
        }

        var generator = SyntaxGenerator.GetGenerator(document);
        var fixedModifiers = generator.GetModifiers(propertyDeclarationWithoutAttribute).WithIsRequired(true);
        var fixedDeclaration = generator.WithModifiers(propertyDeclarationWithoutAttribute, fixedModifiers);

        var fixedRoot = root.ReplaceNode(propertyDeclaration, fixedDeclaration);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
