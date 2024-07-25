using ArgumentParsing.Results;
using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class AdditionalSpecialCommandsTests
{
    #region OptionsAndParser
    [OptionsType]
    internal sealed class Options
    {
    }

    [SpecialCommandAliases("--my-command")]
    public sealed class MySpecialCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof(MySpecialCommandHandler)])]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public void StillCanParseBuiltIns(string builtIn)
    {
        var result = ParseArguments([builtIn]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);
        Assert.NotNull(result.SpecialCommandHandler);
    }

    [Fact]
    public void ParseAdditionalCommand()
    {
        var result = ParseArguments(["--my-command"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var specialCommandHandler = result.SpecialCommandHandler;
        Assert.NotNull(specialCommandHandler);
        Assert.IsType<MySpecialCommandHandler>(specialCommandHandler);
    }
}
