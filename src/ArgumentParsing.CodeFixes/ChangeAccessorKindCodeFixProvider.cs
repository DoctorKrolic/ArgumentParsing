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
public sealed class ChangeAccessorKindCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.PreferInitPropertyAccessor.Id,
        DiagnosticDescriptors.CannotHaveInitAccessorWithADefaultValue.Id);

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

        var diagnostic = context.Diagnostics[0];
        var newAccessorKeywordKind = diagnostic.Descriptor.Id == DiagnosticDescriptors.PreferInitPropertyAccessor.Id ? SyntaxKind.InitKeyword : SyntaxKind.SetKeyword;

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Change to '{SyntaxFacts.GetText(newAccessorKeywordKind)}' accessor",
                _ => ChangeAccessorKind(document, root, accessor, newAccessorKeywordKind),
                nameof(ChangeAccessorKindCodeFixProvider)),
            context.Diagnostics[0]);
    }

    private static Task<Document> ChangeAccessorKind(Document document, SyntaxNode root, AccessorDeclarationSyntax accessor, SyntaxKind newAccessorKeywordKind)
    {
        var fixedAccessor = SyntaxFactory
            .AccessorDeclaration(
                SyntaxFacts.GetAccessorDeclarationKind(newAccessorKeywordKind),
                accessor.AttributeLists,
                accessor.Modifiers,
                SyntaxFactory.Token(accessor.Keyword.LeadingTrivia, newAccessorKeywordKind, accessor.Keyword.TrailingTrivia),
                accessor.Body,
                accessor.ExpressionBody,
                accessor.SemicolonToken);

        var fixedRoot = root.ReplaceNode(accessor, fixedAccessor);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
