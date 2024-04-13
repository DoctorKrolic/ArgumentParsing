namespace ArgumentParsing.Generators.Models;

internal enum ParseStrategy : byte
{
    None = default,
    String,
    Integer,
    Float,
    Flag,
    Enum,
    Char,
    DateTime
}
