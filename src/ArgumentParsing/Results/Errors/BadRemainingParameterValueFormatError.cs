namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates bad value error of one of remaining parameters, e.g. supplied value is "a" while remaining parameters are of a numeric type
/// </summary>
/// <param name="messageFormat">Error message format with 2 argument placeholders</param>
/// <param name="value">Provided value</param>
/// <param name="parameterIndex">Parameter index</param>
public sealed class BadRemainingParameterValueFormatError(string messageFormat, string value, int parameterIndex) : ParseError(messageFormat)
{
    /// <summary>
    /// Provided value
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Global parameter index, meaning that the first remaining parameter will have index equal to count of ordinal parameters
    /// </summary>
    public int ParameterIndex { get; } = parameterIndex;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="value">Provided value</param>
    /// <param name="parameterIndex">Parameter index</param>
    public BadRemainingParameterValueFormatError(string value, int parameterIndex)
        : this(DefaultErrorMessageFormats.BadRemainingParameterValueFormatError, value, parameterIndex)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is BadRemainingParameterValueFormatError badRemainingParameterValueFormatError &&
            MessageFormat == badRemainingParameterValueFormatError.MessageFormat &&
            Value == badRemainingParameterValueFormatError.Value &&
            ParameterIndex == badRemainingParameterValueFormatError.ParameterIndex;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, ParameterIndex);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, Value, ParameterIndex.ToString());
}
