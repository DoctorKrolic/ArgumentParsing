namespace ArgumentParsing.Results.Errors;

public sealed class BadRemainingParameterValueFormatError(string messageFormat, string value, int parameterIndex) : ParseError(messageFormat)
{
    public string Value { get; } = value;

    public int ParameterIndex { get; } = parameterIndex;

    public BadRemainingParameterValueFormatError(string value, int parameterIndex)
        : this(DefaultErrorMessageFormats.BadRemainingParameterValueFormatError, value, parameterIndex)
    {
    }

    public override bool Equals(ParseError? other)
        => other is BadRemainingParameterValueFormatError badRemainingParameterValueFormatError &&
            MessageFormat == badRemainingParameterValueFormatError.MessageFormat &&
            Value == badRemainingParameterValueFormatError.Value &&
            ParameterIndex == badRemainingParameterValueFormatError.ParameterIndex;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, ParameterIndex);

    public override string GetMessage()
        => string.Format(MessageFormat, Value, ParameterIndex.ToString());
}
