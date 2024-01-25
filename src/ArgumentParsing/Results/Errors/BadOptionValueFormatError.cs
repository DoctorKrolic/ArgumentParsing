namespace ArgumentParsing.Results.Errors;

public sealed class BadOptionValueFormatError(string messageFormat, string value, string optionName) : ParseError(messageFormat)
{
    public string Value { get; } = value;

    public string OptionName { get; } = optionName;

    public BadOptionValueFormatError(string value, string optionName)
        : this(DefaultErrorMessageFormats.BadOptionValueFormatError, value, optionName)
    {
    }

    public override bool Equals(ParseError? other)
        => base.Equals(other) &&
            other is BadOptionValueFormatError badOptionValueFormatError &&
            Value == badOptionValueFormatError.Value &&
            OptionName == badOptionValueFormatError.OptionName;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, OptionName);

    public override string GetMessage()
        => string.Format(MessageFormat, Value, OptionName);
}
