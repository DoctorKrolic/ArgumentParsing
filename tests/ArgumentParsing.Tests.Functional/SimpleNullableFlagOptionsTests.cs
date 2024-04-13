using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleNullableFlagOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('a', "flag-a")]
        public bool? NullableFlagA { get; init; }

        [Option('b', "flag-b")]
        public bool? NullableFlagB { get; init; }

        [Option('c', "flag-c")]
        public bool? NullableFlagC { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IReadOnlyList<string> args);
    #endregion

    [Theory]
    [InlineData("", null, null, null)]
    [InlineData("-a -b -c", true, true, true)]
    [InlineData("-ab -c", true, true, true)]
    [InlineData("-a -bc", true, true, true)]
    [InlineData("-ca -b", true, true, true)]
    [InlineData("-abc", true, true, true)]
    [InlineData("-b", null, true, null)]
    [InlineData("-a true -b false -c", true, false, true)]
    [InlineData("-cfalse", null, null, false)]
    [InlineData("--flag-a --flag-b --flag-c", true, true, true)]
    [InlineData("--flag-a --flag-c", true, null, true)]
    [InlineData("--flag-c", null, null, true)]
    [InlineData("--flag-a false --flag-b=true --flag-c false", false, true, false)]
    [InlineData("--flag-a --flag-c=false", true, null, false)]
    [InlineData("--flag-b false", null, false, null)]
    public void ParseCorrectArguments(string argsString, bool? a, bool? b, bool? c)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(a, options.NullableFlagA);
        Assert.Equal(b, options.NullableFlagB);
        Assert.Equal(c, options.NullableFlagC);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("-a notTrue -b -c", "notTrue", "a")]
    [InlineData("-aIsFalse -b", "IsFalse", "a")]
    [InlineData("--flag-b=something", "something", "flag-b")]
    [InlineData("-a --flag-c value", "value", "flag-c")]
    public void BadOptionValueFormatError(string argsString, string badValue, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(error);

        Assert.Equal(badValue, badOptionValueFormatError.Value);
        Assert.Equal(optionName, badOptionValueFormatError.OptionName);
    }
}
