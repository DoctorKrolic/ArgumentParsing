namespace ArgumentParsing;

/// <summary>
/// Indicates an options type, i.e. type, parsed arguments are mapped to
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class OptionsTypeAttribute : Attribute
{
}
