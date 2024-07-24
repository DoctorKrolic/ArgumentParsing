using ArgumentParsing.Results;
using ArgumentParsing.SpecialCommands;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class ReplaceBuiltInCommandsTests
{
    #region OptionsAndParser
    [OptionsType]
    internal sealed class Options
    {
    }

    [SpecialCommandAliases("--help")]
    public sealed class MyHelpCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [SpecialCommandAliases("--version")]
    public sealed class MyVersionCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.None, AdditionalCommandHandlers = [typeof(MyHelpCommandHandler), typeof(MyVersionCommandHandler)])]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Fact]
    public void ParseCustomHelpCommand()
    {
        var result = ParseArguments(["--help"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var specialCommandHandler = result.SpecialCommandHandler;
        Assert.NotNull(specialCommandHandler);
        Assert.IsType<MyHelpCommandHandler>(specialCommandHandler);
    }

    [Fact]
    public void ParseCustomVersionCommand()
    {
        var result = ParseArguments(["--version"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var specialCommandHandler = result.SpecialCommandHandler;
        Assert.NotNull(specialCommandHandler);
        Assert.IsType<MyVersionCommandHandler>(specialCommandHandler);
    }
}
