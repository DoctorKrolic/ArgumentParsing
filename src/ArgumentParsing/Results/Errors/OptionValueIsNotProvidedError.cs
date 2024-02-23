namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates that option, which requires a value, hasn't been supplied with one.
/// This can happen e.g. when 2 option name immediately follow each other
/// </summary>
/// <example>
/// <c>-a -b</c> - if option "a" requires a value this error is reported
/// </example>
/// <param name="messageFormat">Error message format with 1 argument placeholder</param>
/// <param name="precedingArgument">Argument, after which no value is provided while it was expected there</param>
public sealed class OptionValueIsNotProvidedError(string messageFormat, string precedingArgument) : ParseError(messageFormat)
{
    /// <summary>
    /// Argument, after which no value is provided while it was expected there
    /// </summary>
    public string PrecedingArgument { get; } = precedingArgument;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="precedingArgument">Argument, after which no value is provided while it was expected there</param>
    public OptionValueIsNotProvidedError(string precedingArgument)
        : this(DefaultErrorMessageFormats.OptionValueIsNotProvidedError, precedingArgument)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is OptionValueIsNotProvidedError optionValueIsNotProvidedError &&
            MessageFormat == optionValueIsNotProvidedError.MessageFormat &&
            PrecedingArgument == optionValueIsNotProvidedError.PrecedingArgument;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, PrecedingArgument);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, PrecedingArgument);
}
