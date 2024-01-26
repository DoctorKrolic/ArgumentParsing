namespace ArgumentParsing.Results.Errors;

public sealed class FlagOptionValueError(string messageFormat, string optionName) : ParseError(messageFormat)
{
    public string OptionName { get; } = optionName;

    public FlagOptionValueError(string optionName)
        : this(DefaultErrorMessageFormats.FlagOptionValueError, optionName)
    {
        OptionName = optionName;
    }

    public override bool Equals(ParseError? other)
        => other is FlagOptionValueError flagOptionValueError &&
            MessageFormat == flagOptionValueError.MessageFormat &&
            OptionName == flagOptionValueError.OptionName;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName);

    public override string GetMessage()
        => string.Format(MessageFormat, OptionName);
}
