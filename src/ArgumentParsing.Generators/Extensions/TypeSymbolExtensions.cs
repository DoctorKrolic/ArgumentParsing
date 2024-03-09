using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Extensions;

internal static class TypeSymbolExtensions
{
    public static bool IsEnumerableCollectionOfStrings(this ITypeSymbol type)
    {
        return HasMember(type, "GetEnumerator", static e => e is IMethodSymbol { Parameters.Length: 0 } getEnumeratorMethod && IsValidStringEnumerator(getEnumeratorMethod.ReturnType));

        static bool IsValidStringEnumerator(ITypeSymbol type)
        {
            return HasMember(type, "MoveNext", static m => m is IMethodSymbol { Parameters.Length: 0, ReturnType.SpecialType: SpecialType.System_Boolean }) &&
                   HasMember(type, "Current", static c => c is IPropertySymbol { Type.SpecialType: SpecialType.System_String, GetMethod: not null });
        }
    }

    public static bool HasMember(this ITypeSymbol type, string memberName, Func<ISymbol, bool> predicate)
    {
        if (type.GetMembers(memberName).Any(predicate))
        {
            return true;
        }

        if (type.BaseType is { } baseType && HasMember(baseType, memberName, predicate))
        {
            return true;
        }

        foreach (var @interface in type.AllInterfaces)
        {
            if (HasMember(@interface, memberName, predicate))
            {
                return true;
            }
        }

        return false;
    }

    public static ParseStrategy? GetPotentialPrimaryParseStrategy(this ITypeSymbol type) => type switch
    {
        { TypeKind: TypeKind.Enum } => ParseStrategy.Enum,
        { SpecialType: SpecialType.System_String } => ParseStrategy.String,
        {
            SpecialType: SpecialType.System_Byte or
                         SpecialType.System_SByte or
                         SpecialType.System_Int16 or
                         SpecialType.System_UInt16 or
                         SpecialType.System_Int32 or
                         SpecialType.System_UInt32 or
                         SpecialType.System_Int64 or
                         SpecialType.System_UInt64
        } or { Name: "BigInteger", ContainingNamespace: { Name: "Numerics", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } => ParseStrategy.Integer,
        { SpecialType: SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal } => ParseStrategy.Float,
        { SpecialType: SpecialType.System_Boolean } => ParseStrategy.Flag,
        { SpecialType: SpecialType.System_Char } => ParseStrategy.Char,
        _ => null,
    };
}
