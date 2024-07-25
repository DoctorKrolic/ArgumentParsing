namespace ArgumentParsing.Generators.Models;

// Keep in sync with the same type in ArgumentParsing project
[Flags]
internal enum BuiltInCommandHandlers : byte
{
    None = 0,
    Help = 1,
    Version = 2,
}
