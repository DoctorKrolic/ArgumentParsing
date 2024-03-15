using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedNullableParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Parameter(0)]
        public short? Param1 { get; init; }

        [Parameter(1)]
        public double? Param2 { get; init; }

        [Parameter(2)]
        public DayOfWeek? Param3 { get; init; }

        [Parameter(3)]
        public char? Param4 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", null, null, null, null)]
    [InlineData("3", (short)3, null, null, null)]
    [InlineData("4 0.7", (short)4, 0.7, null, null)]
    [InlineData("32755 12.5 Saturday", (short)32755, 12.5, DayOfWeek.Saturday, null)]
    [InlineData("1984 101 Sunday B", (short)1984, 101d, DayOfWeek.Sunday, 'B')]
    public void ParseCorrectArguments(string argsString, short? param1, double? param2, DayOfWeek? param3, char? param4)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);
        Assert.Equal(param3, options.Param3);
        Assert.Equal(param4, options.Param4);

        Assert.Null(result.Errors);
    }
}
