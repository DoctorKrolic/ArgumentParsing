using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Models;

internal sealed record TypeInfo(string QualifiedName, TypeKind Kind, bool IsRecord)
{
    public string GetTypeKeyword()
    {
        return Kind switch
        {
            TypeKind.Struct when IsRecord => "record struct",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            TypeKind.Class when IsRecord => "record",
            _ => "class"
        };
    }
}
