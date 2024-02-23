namespace ArgumentParsing;

/// <summary>
/// Indicates an argument parser method, which implementation is gonna be supplied by the generator
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedArgumentParserAttribute : Attribute
{
}
