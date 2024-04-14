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
public sealed class PrefixAliasWithDashesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.AliasShouldStartWithDash.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root?.FindNode(context.Span, getInnermostNodeForTie: true) is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } aliasLiteral)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Prefix alias with '--'",
                    _ => PrefixAlias(document, root, aliasLiteral, "--"),
                    $"--{nameof(PrefixAliasWithDashesCodeFixProvider)}"),
                diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Prefix alias with '-'",
                    _ => PrefixAlias(document, root, aliasLiteral, "-"),
                    $"-{nameof(PrefixAliasWithDashesCodeFixProvider)}"),
                diagnostic);
        }
    }

    private static Task<Document> PrefixAlias(Document document, SyntaxNode root, LiteralExpressionSyntax aliasLiteral, string prefix)
    {
        var token = aliasLiteral.Token;
        var fixedAliasText = prefix + token.ValueText;
        var fixedLiteral = aliasLiteral
            .WithToken(
                SyntaxFactory.Token(token.LeadingTrivia, SyntaxKind.StringLiteralToken, "\"" + fixedAliasText + "\"", fixedAliasText, token.TrailingTrivia));
        var fixedRoot = root.ReplaceNode(aliasLiteral, fixedLiteral);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
