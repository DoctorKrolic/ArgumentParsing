namespace ArgumentParsing.Results.Errors;

public abstract class ParseError(string messageFormat) : IEquatable<ParseError>
{
    protected string MessageFormat { get; } = messageFormat;

    public abstract string GetMessage();

    public abstract bool Equals(ParseError? other);

    public sealed override bool Equals(object? obj)
        => obj is ParseError error && Equals(error);

    public abstract override int GetHashCode();
}
