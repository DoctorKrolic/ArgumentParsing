namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates missing required option error
/// </summary>
/// <param name="messageFormat">Error message format with 1 or 2 argument placeholders</param>
/// <param name="shortOptionName">Short option name</param>
/// <param name="longOptionName">Long option name</param>
public sealed class MissingRequiredOptionError(string messageFormat, char? shortOptionName, string? longOptionName) : ParseError(messageFormat)
{
    /// <summary>
    /// Short option name. Can be <see langword="null"/> if option doesn't have a short name
    /// </summary>
    public char? ShortOptionName { get; } = shortOptionName;

    /// <summary>
    /// Long option name. Can be <see langword="null"/> if option doesn't have a long name
    /// </summary>
    public string? LongOptionName { get; } = longOptionName;

    /// <summary>
    /// Initializes error object with default message format when option doesn't have a long name
    /// </summary>
    /// <param name="shortOptionName">Short option name</param>
    public MissingRequiredOptionError(char shortOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_OneOptionName, shortOptionName, null)
    {
    }

    /// <summary>
    /// Initializes error object with default message format when option doesn't have a short name
    /// </summary>
    /// <param name="longOptionName">Long option name</param>
    public MissingRequiredOptionError(string longOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_OneOptionName, null, longOptionName)
    {
    }

    /// <summary>
    /// Initializes error object with default message format when option has both short and long name
    /// </summary>
    /// <param name="shortOptionName">Short option name</param>
    /// <param name="longOptionName">Long option name</param>
    public MissingRequiredOptionError(char shortOptionName, string longOptionName)
        : this(DefaultErrorMessageFormats.MissingRequiredOptionError_BothOptionNames, shortOptionName, longOptionName)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is MissingRequiredOptionError missingRequiredOptionError &&
            MessageFormat == missingRequiredOptionError.MessageFormat &&
            ShortOptionName == missingRequiredOptionError.ShortOptionName &&
            LongOptionName == missingRequiredOptionError.LongOptionName;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, ShortOptionName, LongOptionName);

    /// <inheritdoc/>
    public override string GetMessage() => (ShortOptionName, LongOptionName) switch
    {
        (not null, null) => string.Format(MessageFormat, ShortOptionName.Value),
        (null, not null) => string.Format(MessageFormat, LongOptionName),
        (not null, not null) => string.Format(MessageFormat, ShortOptionName.Value, LongOptionName),
        _ => throw new InvalidOperationException("Unreachable"),
    };
}
