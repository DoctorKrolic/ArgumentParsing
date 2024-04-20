using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Extensions;

internal static class SymbolExtensions
{
    public static bool HasMinimalAccessibility(this ISymbol symbol, Accessibility minimalAccessibility)
    {
        if (symbol.DeclaredAccessibility < minimalAccessibility)
        {
            return false;
        }

        var containingType = symbol.ContainingType;

        while (containingType is not null)
        {
            if (containingType.DeclaredAccessibility < minimalAccessibility)
            {
                return false;
            }

            containingType = containingType.ContainingType;
        }

        return true;
    }
}
