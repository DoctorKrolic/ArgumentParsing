namespace ArgumentParsing.Results.Errors;

public sealed class UnknownOptionError(string messageFormat, string optionName, string containingArgument) : ParseError(messageFormat)
{
    public string OptionName { get; } = optionName;

    public string ContainingArgument { get; } = containingArgument;

    public UnknownOptionError(string optionName, string containingArgument)
        : this(DefaultErrorMessageFormats.UnknownOptionError, optionName, containingArgument)
    {
    }

    public override bool Equals(ParseError? other)
        => base.Equals(other) &&
            other is UnknownOptionError unknownOptionError &&
            OptionName == unknownOptionError.OptionName &&
            ContainingArgument == unknownOptionError.ContainingArgument;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName, ContainingArgument);

    public override string GetMessage()
        => string.Format(MessageFormat, OptionName, ContainingArgument);
}
