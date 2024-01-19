using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Results;

public readonly struct ParseResult<TOptions>
{
    public TOptions? Options { get; }

    public ParseErrorCollection? Errors { get; }

    public ParseResultState State { get; }

    public ParseResult(TOptions options)
    {
        Options = options;
        State = ParseResultState.ParsedOptions;
    }

    public ParseResult(ParseErrorCollection errors)
    {
        Errors = errors;
        State = ParseResultState.ParsedWithErrors;
    }
}
