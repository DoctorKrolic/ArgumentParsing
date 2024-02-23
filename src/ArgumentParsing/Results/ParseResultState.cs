namespace ArgumentParsing.Results;

public enum ParseResultState : byte
{
    None = default,
    ParsedOptions,
    ParsedWithErrors,
    ParsedSpecialCommand
}
