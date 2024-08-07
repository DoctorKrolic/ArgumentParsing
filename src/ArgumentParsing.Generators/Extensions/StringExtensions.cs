using System.Diagnostics.CodeAnalysis;

namespace ArgumentParsing.Generators.Extensions;

public static class StringExtensions
{
    public static string ToKebabCase(this string s)
    {
        var buffer = new List<char>((int)(1.2 * s.Length))
        {
            char.ToLowerInvariant(s[0])
        };

        for (var i = 1; i < s.Length; i++)
        {
            var ch = s[i];

            if (char.IsUpper(ch))
            {
                buffer.Add('-');
                buffer.Add(char.ToLowerInvariant(ch));
            }
            else
            {
                buffer.Add(ch);
            }
        }

        return new string([.. buffer]);
    }

    public static bool IsValidName([NotNullWhen(true)] this string? s, bool allowDashPrefix = false)
    {
        if (s is null)
        {
            return false;
        }

        var s0 = s[0];
        if (!char.IsLetter(s0))
        {
            if (s0 != '-' || !allowDashPrefix)
            {
                return false;
            }
        }

        foreach (var ch in s)
        {
            if (ch != '-' && !char.IsLetterOrDigit(ch))
            {
                return false;
            }
        }

        return true;
    }
}
