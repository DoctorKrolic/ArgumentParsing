namespace ArgumentParsing;

[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionAttribute : Attribute
{
    public char? ShortName { get; }

    public string? LongName { get; }

    public OptionAttribute()
    {
    }

    public OptionAttribute(char shortName)
    {
        ShortName = shortName;
    }

    public OptionAttribute(string? longName)
    {
        LongName = longName;
    }

    public OptionAttribute(char shortName, string? longName)
    {
        ShortName = shortName;
        LongName = longName;
    }
}
