namespace ArgumentParsing.Results.Errors;

public sealed class UnrecognizedArgumentError(string messageFormat, string argument) : ParseError(messageFormat)
{
    public string Argument { get; } = argument;

    public UnrecognizedArgumentError(string argument)
        : this(DefaultErrorMessageFormats.UnrecognizedArgumentError, argument)
    {
    }

    public override bool Equals(ParseError? other)
        => base.Equals(other) &&
            other is UnrecognizedArgumentError unrecognizedArgumentError &&
            Argument == unrecognizedArgumentError.Argument;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Argument);

    public override string GetMessage()
        => string.Format(MessageFormat, Argument);
}
