namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates that there are 2 or more duplicate options in parsed arguments.
/// An error is reported for every duplicate option
/// </summary>
/// <param name="messageFormat">Error message format with 1 argument placeholder</param>
/// <param name="optionName">Option name (either short or long)</param>
public sealed class DuplicateOptionError(string messageFormat, string optionName) : ParseError(messageFormat)
{
    /// <summary>
    /// Option name (either short or long) of a duplicate option.
    /// E.g. if there is option "-a" and then duplicate option "--option-a" this property will have "option-a" value
    /// </summary>
    public string OptionName { get; } = optionName;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="optionName">Option name (either short or long)</param>
    public DuplicateOptionError(string optionName)
        : this(DefaultErrorMessageFormats.DuplicateOptionError, optionName)

    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is DuplicateOptionError duplicateOptionError &&
            MessageFormat == duplicateOptionError.MessageFormat &&
            OptionName == duplicateOptionError.OptionName;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, OptionName);
}
