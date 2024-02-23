namespace ArgumentParsing;

/// <summary>
/// Indicates an ordinal parameter property
/// </summary>
/// <param name="index">Parameter index</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ParameterAttribute(int index) : Attribute
{
    /// <summary>
    /// Parameter index
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// Parameter name. Used in parse error messages and in default help text
    /// </summary>
    /// <remarks>
    /// If this property is not specified or set to <see langword="null"/> generator assigns parameter a default name,
    /// which is parameter's property name in lower kebab case
    /// </remarks>
    public string? Name { get; set; }
}
