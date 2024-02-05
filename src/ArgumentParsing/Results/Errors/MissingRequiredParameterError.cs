namespace ArgumentParsing.Results.Errors;

public sealed class MissingRequiredParameterError(string messageFormat, string parameterName, int parameterIndex) : ParseError(messageFormat)
{
    public string ParameterName { get; } = parameterName;

    public int ParameterIndex { get; } = parameterIndex;

    public MissingRequiredParameterError(string parameterName, int parameterIndex)
        : this(DefaultErrorMessageFormats.MissingRequiredParameterError, parameterName, parameterIndex)
    {
    }

    public override bool Equals(ParseError? other)
        => other is MissingRequiredParameterError missingRequiredParameterError &&
            MessageFormat == missingRequiredParameterError.MessageFormat &&
            ParameterName == missingRequiredParameterError.ParameterName &&
            ParameterIndex == missingRequiredParameterError.ParameterIndex;

    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, ParameterName, ParameterIndex);

    public override string GetMessage()
        => string.Format(MessageFormat, ParameterName, ParameterIndex.ToString());
}
