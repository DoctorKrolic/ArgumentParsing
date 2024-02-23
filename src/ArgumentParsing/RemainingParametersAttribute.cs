namespace ArgumentParsing;

/// <summary>
/// Indicates remaining parameters property,
/// which captures all arguments, supplied after ordinal parameters.
/// Property, marked with this attribute, must be of a valid sequence type
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RemainingParametersAttribute : Attribute
{
}
