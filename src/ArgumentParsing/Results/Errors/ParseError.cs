using System.Diagnostics;

namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Error, which occurred during argument parsing
/// </summary>
/// <param name="messageFormat">Template, suitable as a message format for <c>string.Format</c> call</param>
[DebuggerDisplay("{GetMessage(),nq}")]
public abstract class ParseError(string messageFormat) : IEquatable<ParseError>
{
    /// <summary>
    /// Template, suitable as a message format for <c>string.Format</c> call
    /// </summary>
    protected string MessageFormat { get; } = messageFormat;

    /// <summary>
    /// Computes final error message with substituted message arguments
    /// </summary>
    /// <returns>Final error message</returns>
    public abstract string GetMessage();

    /// <inheritdoc/>
    public abstract bool Equals(ParseError? other);

    /// <inheritdoc/>
    public sealed override bool Equals(object? obj)
        => obj is ParseError error && Equals(error);

    /// <inheritdoc/>
    public abstract override int GetHashCode();
}
