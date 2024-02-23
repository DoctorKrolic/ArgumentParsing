namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates unknown option error
/// </summary>
/// <param name="messageFormat">Error message format with 2 argument placeholders</param>
/// <param name="optionName">Encountered unknown option name</param>
/// <param name="containingArgument">Argument, in which unknown option name is encountered</param>
public sealed class UnknownOptionError(string messageFormat, string optionName, string containingArgument) : ParseError(messageFormat)
{
    /// <summary>
    /// Encountered unknown option name
    /// </summary>
    public string OptionName { get; } = optionName;

    /// <summary>
    /// Argument, in which unknown option name is encountered
    /// </summary>
    public string ContainingArgument { get; } = containingArgument;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="optionName">Encountered unknown option name</param>
    /// <param name="containingArgument">Argument, in which unknown option name is encountered</param>
    public UnknownOptionError(string optionName, string containingArgument)
        : this(DefaultErrorMessageFormats.UnknownOptionError, optionName, containingArgument)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is UnknownOptionError unknownOptionError && 
            MessageFormat == unknownOptionError.MessageFormat &&
            OptionName == unknownOptionError.OptionName &&
            ContainingArgument == unknownOptionError.ContainingArgument;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, OptionName, ContainingArgument);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, OptionName, ContainingArgument);
}
