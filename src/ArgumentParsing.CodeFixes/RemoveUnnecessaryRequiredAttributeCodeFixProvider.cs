using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class RemoveUnnecessaryRequiredAttributeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.UnnecessaryRequiredAttribute.Id);

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
                    "Remove unnecessary [Required] attribute",
                    _ => RemoveUnnecessaryRequiredAttribute(document, root!, diagnosticNode),
                    nameof(RemoveUnnecessaryRequiredAttributeCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static Task<Document> RemoveUnnecessaryRequiredAttribute(Document document, SyntaxNode root, SyntaxNode diagnosticNode)
    {
        switch (diagnosticNode)
        {
            case AttributeListSyntax attributeList:
                Debug.Assert(attributeList.Attributes.Count == 1);
                var rootWithoutAttributeList = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia)!;
                return Task.FromResult(document.WithSyntaxRoot(rootWithoutAttributeList));
            case AttributeSyntax { Parent: AttributeListSyntax containingAttributeList }:
                var fixedContainingList = containingAttributeList.RemoveNode(diagnosticNode, SyntaxRemoveOptions.KeepNoTrivia)!;
                var rootWithoutAttribute = root.ReplaceNode(containingAttributeList, fixedContainingList);
                return Task.FromResult(document.WithSyntaxRoot(rootWithoutAttribute));
            default:
                throw new InvalidOperationException("Unreachable");
        }
    }
}
