namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates that a value is provided for a flag option while flag options don't accept values
/// </summary>
/// <param name="messageFormat">Error message format with 1 argument placeholder</param>
/// <param name="optionName">Flag option name (either short or long)</param>
public sealed class FlagOptionValueError(string messageFormat, string optionName) : ParseError(messageFormat)
{
    /// <summary>
    /// Flag option name (either short or long), for which a value is supplied
    /// </summary>
    public string OptionName { get; } = optionName;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="optionName">Flag option name (either short or long)</param>
    public FlagOptionValueError(string optionName)
        : this(DefaultErrorMessageFormats.FlagOptionValueError, optionName)
    {
        OptionName = optionName;
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is FlagOptionValueError flagOptionValueError &&
            MessageFormat == flagOptionValueError.MessageFormat &&
            OptionName == flagOptionValueError.OptionName;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, OptionName);
}
