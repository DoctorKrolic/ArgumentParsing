using System.Collections.Immutable;
using System.Text;
using ArgumentParsing.Generators.Utils;
using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Models;

internal sealed partial record HierarchyInfo(string Namespace, ImmutableEquatableArray<TypeInfo> Hierarchy)
{
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol)
    {
        var hierarchy = ImmutableArray.CreateBuilder<TypeInfo>();

        for (var parent = typeSymbol;
             parent is not null;
             parent = parent.ContainingType)
        {
            hierarchy.Add(new TypeInfo(
                parent.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                parent.TypeKind,
                parent.IsRecord));
        }

        return new(
            typeSymbol.ContainingNamespace.ToDisplayString(new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)),
            hierarchy.ToImmutable());
    }

    public string GetFullyQualifiedTypeName()
    {
        var fullyQualifiedTypeName = new StringBuilder();

        fullyQualifiedTypeName.Append("global::");

        if (Namespace.Length > 0)
        {
            fullyQualifiedTypeName.Append(Namespace);
            fullyQualifiedTypeName.Append('.');
        }

        fullyQualifiedTypeName.Append(Hierarchy[^1].QualifiedName);

        for (var i = Hierarchy.Length - 2; i >= 0; i--)
        {
            fullyQualifiedTypeName.Append('.');
            fullyQualifiedTypeName.Append(Hierarchy[i].QualifiedName);
        }

        return fullyQualifiedTypeName.ToString();
    }
}
