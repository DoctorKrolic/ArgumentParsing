using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;
using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class CustomSpecialCommandHandlerTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
    }

    [SpecialCommandAliases("--info", "-i")]
    private sealed class InfoSpecialCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoSpecialCommandHandler)])]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("-i")]
    [InlineData("--info")]
    public void ParseCustomCommandHandler(string commandAlias)
    {
        var result = ParseArguments([commandAlias]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);
        Assert.IsType<InfoSpecialCommandHandler>(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public void NoHelpOrVersionCommands(string commandAlias)
    {
        var result = ParseArguments([commandAlias]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unknownOptionError = Assert.IsType<UnknownOptionError>(error);

        Assert.Equal(commandAlias, unknownOptionError.ContainingArgument);

        Assert.Null(result.SpecialCommandHandler);
    }
}
