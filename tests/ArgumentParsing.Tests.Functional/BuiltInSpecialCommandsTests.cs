using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class BuiltInSpecialCommandsTests
{
    #region OptionsAndParsers
    [OptionsType]
    internal sealed class Options1
    {
    }

    [OptionsType]
    internal sealed class Options2
    {
    }

    [OptionsType]
    internal sealed class Options3
    {
    }

    [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.None)]
    private static partial ParseResult<Options1> ParseArgumentsWithNoBuiltIns(string[] args);

    [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.Help)]
    private static partial ParseResult<Options2> ParseArgumentsWithOnlyHelpBuiltIn(string[] args);

    [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.Version)]
    private static partial ParseResult<Options3> ParseArgumentsWithOnlyVersionBuiltIn(string[] args);
    #endregion

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public void ParseNoBuiltIns(string builtIn)
    {
        var result = ParseArgumentsWithNoBuiltIns([builtIn]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ParseNoVersionBuiltIn()
    {
        var result = ParseArgumentsWithOnlyHelpBuiltIn(["--version"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ParseNoHelpBuiltIn()
    {
        var result = ParseArgumentsWithOnlyVersionBuiltIn(["--help"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }
}
