namespace ArgumentParsing.Generators.Extensions;

internal static class StringExtensions
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
}
