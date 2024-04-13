using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedNullableOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('b')]
        public byte? NullableByteOption { get; init; }

        [Option('f')]
        public float? NullableFloatOption { get; init; }

        [Option('d')]
        public DayOfWeek? NullableEnumOption { get; init; }

        [Option('c')]
        public char? NullableCharOption { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", null, null, null, null)]
    [InlineData("-b 7", (byte)7, null, null, null)]
    [InlineData("-b 74 -f 2.3", (byte)74, 2.3f, null, null)]
    [InlineData("-b 81 -f 3.8 -d Monday", (byte)81, 3.8f, DayOfWeek.Monday, null)]
    [InlineData("-b 144 -f 10.2 -d Sunday -c q", (byte)144, 10.2f, DayOfWeek.Sunday, 'q')]
    public void ParseCorrectArguments(string argsString, byte? b, float? f, DayOfWeek? d, char? c)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(b, options.NullableByteOption);
        Assert.Equal(f, options.NullableFloatOption);
        Assert.Equal(d, options.NullableEnumOption);
        Assert.Equal(c, options.NullableCharOption);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }
}
