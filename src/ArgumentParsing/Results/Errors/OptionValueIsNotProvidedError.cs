namespace ArgumentParsing.Results.Errors;

public sealed class OptionValueIsNotProvidedError(string messageFormat, string precedingArgument) : ParseError(messageFormat)
{
    public string PrecedingArgument { get; } = precedingArgument;

    public OptionValueIsNotProvidedError(string precedingArgument)
        : this(DefaultErrorMessageFormats.OptionValueIsNotProvidedError, precedingArgument)
    {
    }

    public override bool Equals(ParseError? other)
        => base.Equals(other) &&
            other is OptionValueIsNotProvidedError optionValueIsNotProvidedError &&
            PrecedingArgument == optionValueIsNotProvidedError.PrecedingArgument;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, PrecedingArgument);

    public override string GetMessage()
        => string.Format(MessageFormat, PrecedingArgument);
}
