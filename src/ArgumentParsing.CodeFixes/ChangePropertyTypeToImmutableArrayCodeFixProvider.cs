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
public sealed class ChangePropertyTypeToImmutableArrayCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.PreferImmutableArrayAsSequenceType.Id);

    public override FixAllProvider? GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span) is TypeSyntax propertyTypeSyntax)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Change property type to ImmutableArray<T>",
                    ct => ChangePropertyTypeToImmutableArray(document, root, propertyTypeSyntax, ct),
                    nameof(ChangePropertyTypeToImmutableArrayCodeFixProvider)),
                context.Diagnostics[0]);
        }
    }

    private static async Task<Document> ChangePropertyTypeToImmutableArray(Document document, SyntaxNode root, TypeSyntax propertyTypeSyntax, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            Debug.Fail("Shouldn't really happen");
            return document;
        }

        var immutableArrayOfTType = semanticModel.Compilation.ImmutableArrayOfTType();
        Debug.Assert(immutableArrayOfTType is not null);

        var propertyType = semanticModel.GetTypeInfo(propertyTypeSyntax).Type!;
        Debug.Assert(propertyType.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T
                                                                 or SpecialType.System_Collections_Generic_IReadOnlyCollection_T
                                                                 or SpecialType.System_Collections_Generic_IReadOnlyList_T);

        var sequenceUnderlyingType = ((INamedTypeSymbol)propertyType).TypeArguments[0];
        var immutableArrayOfSequenceType = immutableArrayOfTType!.Construct(sequenceUnderlyingType);

        var generator = SyntaxGenerator.GetGenerator(document);
        var finalTypeSyntax = generator.TypeExpression(immutableArrayOfSequenceType, addImport: true);

        var changedRoot = root.ReplaceNode(propertyTypeSyntax, finalTypeSyntax);
        return document.WithSyntaxRoot(changedRoot);
    }
}
