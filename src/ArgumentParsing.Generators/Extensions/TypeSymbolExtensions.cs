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
}
