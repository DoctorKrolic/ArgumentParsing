namespace ArgumentParsing.Results.Errors;

public sealed class MissingRequiredOptionError(string messageFormat, char? shortOptionName, string? longOptionName) : ParseError(messageFormat)
{
    public char? ShortOptionName { get; } = shortOptionName;

    public string? LongOptionName { get; } = longOptionName;

    public MissingRequiredOptionError(char shortOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_OneOptionName, shortOptionName, null)
    {
    }

    public MissingRequiredOptionError(string longOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_OneOptionName, null, longOptionName)
    {
    }

    public MissingRequiredOptionError(char shortOptionName, string longOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_BothOptionNames, shortOptionName, longOptionName)
    {
    }

    public override bool Equals(ParseError? other)
        => other is MissingRequiredOptionError missingRequiredOptionError &&
            MessageFormat == missingRequiredOptionError.MessageFormat &&
            ShortOptionName == missingRequiredOptionError.ShortOptionName &&
            LongOptionName == missingRequiredOptionError.LongOptionName;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, ShortOptionName, LongOptionName);

    public override string GetMessage() => (ShortOptionName, LongOptionName) switch
    {
        (not null, null) => string.Format(MessageFormat, ShortOptionName.Value),
        (null, not null) => string.Format(MessageFormat, LongOptionName),
        (not null, not null) => string.Format(MessageFormat, ShortOptionName.Value, LongOptionName),
        _ => throw new InvalidOperationException("Unreachable"),
    };
}
