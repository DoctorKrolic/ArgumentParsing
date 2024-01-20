using System.Diagnostics;

namespace ArgumentParsing.Results.Errors;

[DebuggerDisplay("{GetMessage(),nq}")]
public abstract class ParseError(string messageFormat) : IEquatable<ParseError>
{
    protected string MessageFormat { get; } = messageFormat;

    public abstract string GetMessage();

    public virtual bool Equals(ParseError? other) => MessageFormat == other?.MessageFormat;

    public sealed override bool Equals(object? obj)
        => obj is ParseError error && Equals(error);

    public abstract override int GetHashCode();
}