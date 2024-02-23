namespace ArgumentParsing;

/// <summary>
/// Indicates an option property
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionAttribute : Attribute
{
    /// <summary>
    /// Short name of an option. Used in short name option syntax, e.g. <c>-a</c>.
    /// This value is just the name itself (so just <c>a</c> in a previous example)
    /// </summary>
    public char? ShortName { get; }

    /// <summary>
    /// Long name of an option. Used in long name option syntax, e.g. <c>--option-name</c>.
    /// This value is just the name itself (so just <c>option-name</c> in a previous example)
    /// </summary>
    /// <remarks>
    /// If an attribute constructor without <c>longName</c> parameter is used generator assigns option a default long name,
    /// which is option's property name in lower kebab case
    /// </remarks>
    public string? LongName { get; }

    /// <summary>
    /// Initializes an option property with no short name and a default long name
    /// </summary>
    public OptionAttribute()
    {
    }

    /// <summary>
    /// Initializes an option property with a specified short name and a default long name
    /// </summary>
    public OptionAttribute(char shortName)
    {
        ShortName = shortName;
    }

    /// <summary>
    /// Initializes an option property with no short name and a specified long name.
    /// Supplying <see langword="null"/> results in option without a long name
    /// </summary>
    public OptionAttribute(string? longName)
    {
        LongName = longName;
    }

    /// <summary>
    /// Initializes an option property with a specified short name and a specified long name.
    /// Supplying <see langword="null"/> for <paramref name="longName"/> results in option without a long name
    /// </summary>
    public OptionAttribute(char shortName, string? longName)
    {
        ShortName = shortName;
        LongName = longName;
    }
}
