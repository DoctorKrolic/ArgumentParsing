namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Indicates unrecognized argument error, e.g. unbounded unexpected parameter
/// </summary>
/// <param name="messageFormat">Error message format with 1 argument placeholder</param>
/// <param name="argument">Unrecognized argument</param>
public sealed class UnrecognizedArgumentError(string messageFormat, string argument) : ParseError(messageFormat)
{
    /// <summary>
    /// Unrecognized argument
    /// </summary>
    public string Argument { get; } = argument;

    /// <summary>
    /// Initializes error object with default message format
    /// </summary>
    /// <param name="argument">Unrecognized argument</param>
    public UnrecognizedArgumentError(string argument)
        : this(DefaultErrorMessageFormats.UnrecognizedArgumentError, argument)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(ParseError? other)
        => other is UnrecognizedArgumentError unrecognizedArgumentError &&
            MessageFormat == unrecognizedArgumentError.MessageFormat &&
            Argument == unrecognizedArgumentError.Argument;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MessageFormat, Argument);

    /// <inheritdoc/>
    public override string GetMessage()
        => string.Format(MessageFormat, Argument);
}
