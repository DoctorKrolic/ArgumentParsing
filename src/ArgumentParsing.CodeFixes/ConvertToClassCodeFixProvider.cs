using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ConvertToClassCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.SpecialCommandHandlerShouldBeClass.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is StructDeclarationSyntax structDeclaration)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Convert to class",
                    ct => ConvertToClass(document, structDeclaration, ct),
                    nameof(ConvertToClassCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static async Task<Solution> ConvertToClass(Document document, StructDeclarationSyntax structDeclaration, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel?.GetDeclaredSymbol(structDeclaration, cancellationToken) is not { } structType)
        {
            Debug.Fail("Shouldn't really happen");
            return solution;
        }

        var currentDocumentsRoots = new Dictionary<DocumentId, SyntaxNode>();

        foreach (var location in structType.Locations)
        {
            Debug.Assert(location.IsInSource);
            var locationTree = location.SourceTree!;
            var locationDocumentId = solution.GetDocumentId(locationTree)!;
            var seenDocumentId = currentDocumentsRoots.ContainsKey(locationDocumentId);

            var locationRoot = seenDocumentId
                ? currentDocumentsRoots[locationDocumentId]
                : await locationTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var declaration = (StructDeclarationSyntax)locationRoot.FindNode(location.SourceSpan);
            var structKeyword = declaration.Keyword;

            var fixedDeclaration = SyntaxFactory.ClassDeclaration(
                declaration.AttributeLists,
                declaration.Modifiers,
                SyntaxFactory.Token(structKeyword.LeadingTrivia, SyntaxKind.ClassKeyword, structKeyword.TrailingTrivia),
                declaration.Identifier,
                declaration.TypeParameterList,
                declaration.ParameterList,
                declaration.BaseList,
                declaration.ConstraintClauses,
                declaration.OpenBraceToken,
                declaration.Members,
                declaration.CloseBraceToken,
                declaration.SemicolonToken);

            var fixedRoot = locationRoot.ReplaceNode(declaration, fixedDeclaration);

            if (seenDocumentId)
            {
                currentDocumentsRoots[locationDocumentId] = fixedRoot;
            }
            else
            {
                currentDocumentsRoots.Add(locationDocumentId, fixedRoot);
            }
        }

        foreach (var pair in currentDocumentsRoots)
        {
            solution = solution.WithDocumentSyntaxRoot(pair.Key, pair.Value);
        }

        return solution;
    }
}
