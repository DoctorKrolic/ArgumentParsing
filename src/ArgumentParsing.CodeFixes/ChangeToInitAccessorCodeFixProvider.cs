using System.Collections.Immutable;
using System.Composition;
using ArgumentParsing.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ChangeToInitAccessorCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.PreferInitPropertyAccessor.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is not AccessorDeclarationSyntax accessor)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Change to 'init' accessor",
                _ => ChangeToInitAccessor(document, root, accessor),
                nameof(ChangeToInitAccessorCodeFixProvider)),
            context.Diagnostics[0]);
    }

    private static Task<Document> ChangeToInitAccessor(Document document, SyntaxNode root, AccessorDeclarationSyntax accessor)
    {
        var fixedAccessor = SyntaxFactory
            .AccessorDeclaration(
                SyntaxKind.InitAccessorDeclaration,
                accessor.AttributeLists,
                accessor.Modifiers,
                SyntaxFactory.Token(accessor.Keyword.LeadingTrivia, SyntaxKind.InitKeyword, accessor.Keyword.TrailingTrivia),
                accessor.Body,
                accessor.ExpressionBody,
                accessor.SemicolonToken);

        var fixedRoot = root.ReplaceNode(accessor, fixedAccessor);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
