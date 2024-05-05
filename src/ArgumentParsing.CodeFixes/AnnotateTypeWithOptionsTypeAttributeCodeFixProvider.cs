using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArgumentParsing.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AnnotateTypeWithOptionsTypeAttributeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.OptionsTypeMustBeAnnotatedWithAttribute.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is not TypeSyntax optionsTypeSyntax)
        {
            return;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var optionsType = semanticModel.GetTypeInfo(optionsTypeSyntax).Type;
        var compilation = semanticModel.Compilation;

        // We can get ParseResult<ActualOptionsType> in case the whole type is aliased, so unwrap options type from ParseResult here
        if (SymbolEqualityComparer.Default.Equals(optionsType?.OriginalDefinition, compilation.ParseResultOfTType()))
        {
            optionsType = ((INamedTypeSymbol)optionsType!).TypeArguments[0];
        }

        if (optionsType?.Locations.First() is not { IsInSource: true } optionsTypeSourceLocation)
        {
            return;
        }

        var optionsTypeDeclarationDocument = document.Project.Solution.GetDocument(optionsTypeSourceLocation.SourceTree);
        if (optionsTypeDeclarationDocument is null)
        {
            return;
        }

        var optionsTypeDeclarationRoot = await optionsTypeDeclarationDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (optionsTypeDeclarationRoot?.FindNode(optionsTypeSourceLocation.SourceSpan) is TypeDeclarationSyntax optionsTypeDeclaration)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Annotate type with [OptionsType] attribute",
                    _ => AnnotateOptionsTypeWithOptionsTypeAttribute(optionsTypeDeclarationDocument, optionsTypeDeclarationRoot, optionsTypeDeclaration, compilation),
                    nameof(AnnotateTypeWithOptionsTypeAttributeCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static Task<Solution> AnnotateOptionsTypeWithOptionsTypeAttribute(Document optionsTypeDeclarationDocument, SyntaxNode optionsTypeDeclarationRoot, TypeDeclarationSyntax optionsTypeDeclaration, Compilation compilation)
    {
        var optionsTypeAttributeType = compilation.OptionsTypeAttributeType();
        if (optionsTypeAttributeType is null)
        {
            Debug.Fail("Shouldn't really happen");
            return Task.FromResult(optionsTypeDeclarationDocument.Project.Solution);
        }

        var generator = SyntaxGenerator.GetGenerator(optionsTypeDeclarationDocument);
        var optionsTypeAttributeName = generator.TypeExpression(optionsTypeAttributeType);
        var optionsTypeAttribute = generator.Attribute(optionsTypeAttributeName);
        var fixedTypeDeclaration = generator.AddAttributes(optionsTypeDeclaration, optionsTypeAttribute);

        var fixedRoot = optionsTypeDeclarationRoot.ReplaceNode(optionsTypeDeclaration, fixedTypeDeclaration);
        var fixedDocument = optionsTypeDeclarationDocument.WithSyntaxRoot(fixedRoot);
        return Task.FromResult(fixedDocument.Project.Solution);
    }
}
