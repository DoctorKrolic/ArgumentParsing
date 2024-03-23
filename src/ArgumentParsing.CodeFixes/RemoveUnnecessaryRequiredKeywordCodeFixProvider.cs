using System.Collections.Immutable;
using System.Composition;
using ArgumentParsing.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class RemoveUnnecessaryRequiredKeywordCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.RequiredRemainingParameters.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var diagnosticNode = root?.FindNode(context.Span);

        if (root?.FindNode(context.Span) is PropertyDeclarationSyntax propertyDeclaration &&
            propertyDeclaration.Modifiers.Any(SyntaxKind.RequiredKeyword))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Remove unnecessary 'required' keyword",
                    _ => RemoveUnnecessaryRequiredKeyword(document, root, propertyDeclaration),
                    nameof(RemoveUnnecessaryRequiredKeywordCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static Task<Document> RemoveUnnecessaryRequiredKeyword(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration)
    {
        var generator = SyntaxGenerator.GetGenerator(document);
        var fixedModifiers = generator.GetModifiers(propertyDeclaration).WithIsRequired(false);
        var fixedPropertyDeclaration = generator.WithModifiers(propertyDeclaration, fixedModifiers);
        var fixedRoot = root.ReplaceNode(propertyDeclaration, fixedPropertyDeclaration);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
