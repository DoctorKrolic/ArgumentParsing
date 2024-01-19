using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleStringOptionsTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Option('a')]
        public string? OptionA { get; init; }

        [Option('b')]
        public string? OptionB { get; init; }

        [Option('c')]
        public string? OptionC { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("-a a -b b -c c")]
    [InlineData("-aa -bb -cc")]
    [InlineData("--option-a a --option-b b --option-c c")]
    [InlineData("--option-a=a --option-b=b --option-c=c")]
    [InlineData("--option-a=a -b b --option-c=c")]
    [InlineData("-b b --option-a=a -cc")]
    [InlineData("-cc --option-b b --option-a=a")]
    public void ParseCorrectArguments(string argsString)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal("a", options.OptionA);
        Assert.Equal("b", options.OptionB);
        Assert.Equal("c", options.OptionC);

        Assert.Null(result.Errors);
    }
}
