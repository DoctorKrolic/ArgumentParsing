namespace ArgumentParsing.Results.Errors;

public sealed class BadParameterValueFormatError(string messageFormat, string value, string parameterName, int parameterIndex) : ParseError(messageFormat)
{
    public string Value { get; } = value;

    public string ParameterName { get; } = parameterName;

    public int ParameterIndex { get; } = parameterIndex;

    public BadParameterValueFormatError(string value, string parameterName, int parameterIndex)
        : this(DefaultErrorMessageFormats.BadParameterValueFormatError, value, parameterName, parameterIndex)
    { 
    }

    public override bool Equals(ParseError? other)
        => other is BadParameterValueFormatError badParameterValueFormatError &&
            MessageFormat == badParameterValueFormatError.MessageFormat &&
            Value == badParameterValueFormatError.Value &&
            ParameterName == badParameterValueFormatError.ParameterName &&
            ParameterIndex == badParameterValueFormatError.ParameterIndex;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, ParameterName, ParameterIndex);

    public override string GetMessage()
        => string.Format(MessageFormat, Value, ParameterName, ParameterIndex.ToString());
}
