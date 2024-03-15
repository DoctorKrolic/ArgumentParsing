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
public sealed class UseArgsParameterNameCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.PreferArgsParameterName.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is ParameterSyntax parameter)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Use 'args' name for the parameter",
                    _ => ChangeNameToArgs(document, root, parameter),
                    nameof(UseArgsParameterNameCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static Task<Document> ChangeNameToArgs(Document document, SyntaxNode root, ParameterSyntax parameter)
    {
        var fixedParameter = parameter
            .WithIdentifier(
                SyntaxFactory.Identifier("args")
                    .WithTriviaFrom(parameter.Identifier));

        var fixedRoot = root.ReplaceNode(parameter, fixedParameter);
        return Task.FromResult(document.WithSyntaxRoot(fixedRoot));
    }
}
