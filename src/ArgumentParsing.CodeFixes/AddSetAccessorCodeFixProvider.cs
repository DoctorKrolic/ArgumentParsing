using System.Collections.Immutable;
using System.Composition;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AddSetAccessorCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.PropertyMustHaveAccessibleSetter.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is not PropertyDeclarationSyntax propertySyntax)
        {
            return;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var propertySymbol = semanticModel.GetDeclaredSymbol(propertySyntax, cancellationToken);
        if (propertySymbol is not { SetMethod: null })
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];

        var canHaveInitAccessor = ((CSharpParseOptions)root.SyntaxTree.Options).LanguageVersion >= LanguageVersion.CSharp9 && semanticModel.Compilation.IsExternalInitType() is not null;
        var keyword = canHaveInitAccessor ? SyntaxKind.InitKeyword : SyntaxKind.SetKeyword;
        var keywordText = SyntaxFacts.GetText(keyword);

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Add '{keywordText}' accessor",
                _ => AddAccessor(document, root, propertySyntax, SyntaxFacts.GetAccessorDeclarationKind(keyword)),
                nameof(AddSetAccessorCodeFixProvider) + ";" + keywordText),
            diagnostic);
    }

    private static Task<Document> AddAccessor(Document document, SyntaxNode root, PropertyDeclarationSyntax propertySyntax, SyntaxKind accessorKind)
    {
        var initAccessor = SyntaxFactory.AccessorDeclaration(accessorKind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        var propertyWithInitAccessor = propertySyntax.AddAccessorListAccessors(initAccessor);

        var changedRoot = root.ReplaceNode(propertySyntax, propertyWithInitAccessor);
        return Task.FromResult(document.WithSyntaxRoot(changedRoot));
    }
}
