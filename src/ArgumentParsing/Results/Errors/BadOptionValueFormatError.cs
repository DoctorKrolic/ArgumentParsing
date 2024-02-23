namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates bad option value error, e.g. supplied value is "a" while option is of a numeric type
/// </summary>
/// <param name="messageFormat">Error message format with 2 argument placeholders</param>
/// <param name="value">Provided value</param>
/// <param name="optionName">Option name (either short or long)</param>
public sealed class BadOptionValueFormatError(string messageFormat, string value, string optionName) : ParseError(messageFormat)
{
    /// <summary>
    /// Provided value
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Option name (either short or long)
    /// </summary>
    public string OptionName { get; } = optionName;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="value">Provided option value</param>
    /// <param name="optionName">Option name (either short or long)</param>
    public BadOptionValueFormatError(string value, string optionName)
        : this(DefaultErrorMessageFormats.BadOptionValueFormatError, value, optionName)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is BadOptionValueFormatError badOptionValueFormatError &&
            MessageFormat == badOptionValueFormatError.MessageFormat &&
            Value == badOptionValueFormatError.Value &&
            OptionName == badOptionValueFormatError.OptionName;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Value, OptionName);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, Value, OptionName);
}
