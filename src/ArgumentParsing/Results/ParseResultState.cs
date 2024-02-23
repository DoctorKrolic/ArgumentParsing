namespace ArgumentParsing.Results;

/// <summary>
/// Specifies a state of a <see cref="ParseResult{TOptions}"/>
/// </summary>
public enum ParseResultState : byte
{
    /// <summary>
    /// Explicit default state. If <see cref="ParseResult{TOptions}"/> has this state
    /// it means that it is a <see langword="default"/> <see cref="ParseResult{TOptions}"/> value
    /// </summary>
    None = default,

    /// <summary>
    /// State of a <see cref="ParseResult{TOptions}"/> with options object is successfully parsed from arguments
    /// </summary>
    ParsedOptions,

    /// <summary>
    /// State of a <see cref="ParseResult{TOptions}"/> when at least one error occurred during options parsing
    /// </summary>
    ParsedWithErrors,

    /// <summary>
    /// State of a <see cref="ParseResult{TOptions}"/> when special command like <c>--help</c> is parsed
    /// </summary>
    ParsedSpecialCommand
}
