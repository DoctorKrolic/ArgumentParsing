namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates bad ordinal parameter value error, e.g. supplied value is "a" while parameter is of a numeric type
/// </summary>
/// <param name="messageFormat">Error message format with 3 argument placeholders</param>
/// <param name="value">Provided value</param>
/// <param name="parameterName">Parameter name</param>
/// <param name="parameterIndex">Parameter index</param>
public sealed class BadParameterValueFormatError(string messageFormat, string value, string parameterName, int parameterIndex) : ParseError(messageFormat)
{
    /// <summary>
    /// Provided value
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Parameter name
    /// </summary>
    public string ParameterName { get; } = parameterName;

    /// <summary>
    /// Parameter index
    /// </summary>
    public int ParameterIndex { get; } = parameterIndex;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="value">Provided value</param>
    /// <param name="parameterName">Parameter name</param>
    /// <param name="parameterIndex">Parameter index</param>
    public BadParameterValueFormatError(string value, string parameterName, int parameterIndex)
        : this(DefaultErrorMessageFormats.BadParameterValueFormatError, value, parameterName, parameterIndex)
    { 
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is BadParameterValueFormatError badParameterValueFormatError &&
            MessageFormat == badParameterValueFormatError.MessageFormat &&
            Value == badParameterValueFormatError.Value &&
            ParameterName == badParameterValueFormatError.ParameterName &&
            ParameterIndex == badParameterValueFormatError.ParameterIndex;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, ParameterName, ParameterIndex);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, Value, ParameterName, ParameterIndex.ToString());
}
