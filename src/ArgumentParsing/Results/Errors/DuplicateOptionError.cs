namespace ArgumentParsing.Results.Errors;

public sealed class DuplicateOptionError(string messageFormat, string optionName) : ParseError(messageFormat)
{
    public string OptionName { get; } = optionName;

    public DuplicateOptionError(string optionName)
        : this(DefaultErrorMessageFormats.DuplicateOptionError, optionName)

    {
    }

    public override bool Equals(ParseError? other)
        => other is DuplicateOptionError duplicateOptionError &&
            MessageFormat == duplicateOptionError.MessageFormat &&
            OptionName == duplicateOptionError.OptionName;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName);

    public override string GetMessage()
        => string.Format(MessageFormat, OptionName);
}
