namespace ArgumentParsing;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ParameterAttribute(int index) : Attribute
{
    public int Index { get; } = index;

    public string? Name { get; set; }
}
