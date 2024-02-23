namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates missing required ordinal parameter error
/// </summary>
/// <param name="messageFormat">Error message format with 2 argument placeholders</param>
/// <param name="parameterName">Parameter name</param>
/// <param name="parameterIndex">Parameter index</param>
public sealed class MissingRequiredParameterError(string messageFormat, string parameterName, int parameterIndex) : ParseError(messageFormat)
{
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
    /// <param name="parameterName">Parameter name</param>
    /// <param name="parameterIndex">Parameter index</param>
    public MissingRequiredParameterError(string parameterName, int parameterIndex)
        : this(DefaultErrorMessageFormats.MissingRequiredParameterError, parameterName, parameterIndex)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is MissingRequiredParameterError missingRequiredParameterError &&
            MessageFormat == missingRequiredParameterError.MessageFormat &&
            ParameterName == missingRequiredParameterError.ParameterName &&
            ParameterIndex == missingRequiredParameterError.ParameterIndex;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, ParameterName, ParameterIndex);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, ParameterName, ParameterIndex.ToString());
}
