namespace ArgumentParsing.Results.Errors;

public sealed class BadParameterValueFormatError(string messageFormat, string value, int parameterIndex) : ParseError(messageFormat)
{
    public string Value { get; } = value;

    public int ParameterIndex { get; } = parameterIndex;

    public BadParameterValueFormatError(string value, int parameterIndex)
        : this(DefaultErrorMessageFormats.BadParameterValueFormatError, value, parameterIndex)
    { 
    }

    public override bool Equals(ParseError? other)
        => other is BadParameterValueFormatError badParameterValueFormatError &&
            MessageFormat == badParameterValueFormatError.MessageFormat &&
            Value == badParameterValueFormatError.Value &&
            ParameterIndex == badParameterValueFormatError.ParameterIndex;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, ParameterIndex);

    public override string GetMessage()
        => string.Format(MessageFormat, Value, ParameterIndex.ToString());
}
