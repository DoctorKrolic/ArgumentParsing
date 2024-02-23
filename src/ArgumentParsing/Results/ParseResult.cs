using ArgumentParsing.Results.Errors;
using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing.Results;

/// <summary>
/// Represents a result of option parsing operation
/// </summary>
/// <typeparam name="TOptions">Type of options object</typeparam>
public readonly struct ParseResult<TOptions>
{
    /// <summary>
    /// Options object, constructed during argument parsing.
    /// Not <see langword="default"/> only if <see cref="State"/> is <see cref="ParseResultState.ParsedOptions"/>
    /// </summary>
    public TOptions? Options { get; }

    /// <summary>
    /// Collection of errors occurred during argument parsing.
    /// Not <see langword="null"/> only if <see cref="State"/> is <see cref="ParseResultState.ParsedWithErrors"/>
    /// </summary>
    public ParseErrorCollection? Errors { get; }

    /// <summary>
    /// Special command handler, corresponding to parsed command.
    /// Not <see langword="null"/> only if <see cref="State"/> is <see cref="ParseResultState.ParsedSpecialCommand"/>
    /// </summary>
    public ISpecialCommandHandler? SpecialCommandHandler { get; }

    /// <summary>
    /// State of this <see cref="ParseResult{TOptions}"/>
    /// </summary>
    public ParseResultState State { get; }

    /// <summary>
    /// Initializes a parse result when options object is successfully parsed from arguments
    /// </summary>
    /// <param name="options">Options object</param>
    public ParseResult(TOptions options)
    {
        Options = options;
        State = ParseResultState.ParsedOptions;
    }

    /// <summary>
    /// Initializes a parse result when at least one error occurred during argument parsing
    /// </summary>
    /// <param name="errors">Collection of occurred errors</param>
    public ParseResult(ParseErrorCollection errors)
    {
        Errors = errors;
        State = ParseResultState.ParsedWithErrors;
    }

    /// <summary>
    /// Initializes a parse result when special command like <c>--help</c> is parsed
    /// </summary>
    /// <param name="specialCommandHandler">Special command handler, corresponding to parsed command</param>
    public ParseResult(ISpecialCommandHandler specialCommandHandler)
    {
        SpecialCommandHandler = specialCommandHandler;
        State = ParseResultState.ParsedSpecialCommand;
    }
}
