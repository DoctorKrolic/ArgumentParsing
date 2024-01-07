using System.Collections.Immutable;
using ArgumentParsing.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ArgumentParsing.Generators.Models;

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    SyntaxTree? SyntaxTree,
    TextSpan TextSpan,
    ImmutableEquatableArray<string> Arguments)
{
    public Diagnostic ToDiagnostic()
    {
        return SyntaxTree is not null
            ? Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray())
            : Diagnostic.Create(Descriptor, null, Arguments.ToArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args)
    {
        var location = symbol.Locations.First();
        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxToken token, params object[] args)
    {
        var location = token.GetLocation();
        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode node, params object[] args)
    {
        var location = node.GetLocation();
        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }
}
