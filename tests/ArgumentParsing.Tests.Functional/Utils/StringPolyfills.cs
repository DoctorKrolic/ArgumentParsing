namespace System;

internal static class StringPolyfills
{
#if NET472
    public static string ReplaceLineEndings(this string str)
        => str.Replace("\r", string.Empty).Replace("\n", Environment.NewLine);
#endif
}
