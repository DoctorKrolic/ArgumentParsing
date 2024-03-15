using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedFlagsOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('f')]
        public bool Flag { get; init; }

        [Option('n')]
        public bool? NullableFlag { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IReadOnlyList<string> args);
    #endregion

    [Theory]
    [InlineData("", false, null)]
    [InlineData("-f -n", true, true)]
    [InlineData("-fn", true, true)]
    [InlineData("-nf", true, true)]
    [InlineData("-f", true, null)]
    [InlineData("-n", false, true)]
    [InlineData("-f -n false", true, false)]
    [InlineData("--flag --nullable-flag", true, true)]
    [InlineData("--nullable-flag", false, true)]
    [InlineData("--flag", true, null)]
    [InlineData("--flag --nullable-flag false", true, false)]
    [InlineData("--nullable-flag=true --flag", true, true)]
    public void ParseCorrectArguments(string argsString, bool f, bool? n)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(f, options.Flag);
        Assert.Equal(n, options.NullableFlag);

        Assert.Null(result.Errors);
    }
}
