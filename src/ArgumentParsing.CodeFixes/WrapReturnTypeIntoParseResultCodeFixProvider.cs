using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class WrapReturnTypeIntoParseResultCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.ReturnTypeMustBeParseResult.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is TypeSyntax returnTypeSyntax)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Wrap return type into ParseResult<T>",
                    ct => WrapReturnTypeIntoParseResult(document, root, returnTypeSyntax, ct),
                    nameof(WrapReturnTypeIntoParseResultCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static async Task<Document> WrapReturnTypeIntoParseResult(Document document, SyntaxNode root, TypeSyntax returnTypeSyntax, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel?.Compilation.ParseResultOfTType() is not { } parseResultOfTType)
        {
            Debug.Fail("Shouldn't really happen");
            return document;
        }

        var returnType = semanticModel.GetTypeInfo(returnTypeSyntax, cancellationToken).Type!;
        var parseResultOfReturnType = parseResultOfTType.Construct(returnType);

        var generator = SyntaxGenerator.GetGenerator(document);
        var wrappedReturnTypeSyntax = generator.TypeExpression(parseResultOfReturnType, addImport: true);

        var changedRoot = root.ReplaceNode(returnTypeSyntax, wrappedReturnTypeSyntax);
        return document.WithSyntaxRoot(changedRoot);
    }
}
