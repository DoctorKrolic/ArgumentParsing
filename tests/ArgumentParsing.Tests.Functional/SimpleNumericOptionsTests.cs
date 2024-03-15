using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleNumericOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('i')]
        public int IntOption { get; init; }

        [Option('l')]
        public long LongOption { get; init; }

        [Option('d')]
        public double DoubleOption { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IEnumerable<string> args);
    #endregion

    [Theory]
    [InlineData("-i 1 -l 2 -d 3", 1, 2, 3)]
    [InlineData("-i20 -l32768 -d3.14", 20, 32768, 3.14)]
    [InlineData("--int-option -100 --long-option -20000000000 --double-option -5.26", -100, -20000000000, -5.26)]
    [InlineData("--int-option=0 --long-option=7523669854366 --double-option=0.00", 0, 7523669854366, 0.00)]
    [InlineData("--int-option=10 -l 7895461230444 --double-option=1.23456789", 10, 7895461230444, 1.23456789)]
    [InlineData("-l 83 --int-option=666 -d7", 666, 83, 7)]
    [InlineData("-d7855656.6 --long-option 123456789 --int-option=123456789", 123456789, 123456789, 7855656.6)]
    public void ParseCorrectValues(string argsString, int intOptionValue, long longOptionValue, double doubleOptionValue)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(intOptionValue, options.IntOption);
        Assert.Equal(longOptionValue, options.LongOption);
        Assert.Equal(doubleOptionValue, options.DoubleOption);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-i 1. -l 2 -d 3", "1.", "i")]
    [InlineData("-iab -l32768 -d3.14", "ab", "i")]
    [InlineData("--int-option -100 --long-option --20000000000 --double-option -5.26", "--20000000000", "long-option")]
    [InlineData("--int-option=0 --long-option=7523669854366.7 --double-option=0.00", "7523669854366.7", "long-option")]
    [InlineData("--int-option=10 -l 7895461230444 --double-option=1/23456789", "1/23456789", "double-option")]
    [InlineData("-l 83 --int-option=666 -djj3e", "jj3e", "d")]
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
