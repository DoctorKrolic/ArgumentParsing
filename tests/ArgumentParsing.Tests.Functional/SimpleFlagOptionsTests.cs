using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleFlagOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('a')]
        public bool FlagA { get; init; }

        [Option('b')]
        public bool FlagB { get; init; }

        [Option('c')]
        public bool FlagC { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IReadOnlyList<string> args);
    #endregion

    [Theory]
    [InlineData("", false, false, false)]
    [InlineData("-a -b -c", true, true, true)]
    [InlineData("-ab -c", true, true, true)]
    [InlineData("-a -bc", true, true, true)]
    [InlineData("-ca -b", true, true, true)]
    [InlineData("-abc", true, true, true)]
    [InlineData("-b", false, true, false)]
    [InlineData("--flag-a --flag-b --flag-c", true, true, true)]
    [InlineData("--flag-a --flag-c", true, false, true)]
    [InlineData("--flag-c", false, false, true)]
    public void ParseCorrectArguments(string argsString, bool a, bool b, bool c)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(a, options.FlagA);
        Assert.Equal(b, options.FlagB);
        Assert.Equal(c, options.FlagC);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-a something -b -c", "a")]
    [InlineData("-ab true -c", "b")]
    [InlineData("-a -bcval", "c")]
    [InlineData("--flag-a false --flag-b --flag-c", "flag-a")]
    [InlineData("--flag-a --flag-c=123", "flag-c")]
    public void FlagOptionValueError(string argsString, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var flagOptionValueError = Assert.IsType<FlagOptionValueError>(error);

        Assert.Equal(optionName, flagOptionValueError.OptionName);
    }

    [Theory]
    [InlineData("-abb", "b")]
    [InlineData("-abca", "a")]
    [InlineData("-ab -bc", "b")]
    [InlineData("-caa -b", "a")]
    [InlineData("-abcc", "c")]
    public void DuplicateOptionError(string argsString, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        // It is easier to treat arguments like `-abb` as 3 separate options and report duplicate option error
        // rather than generate a bunch of additional code to parse it as 2 options and a value (which implies reporting flag option value error)
        // Since this configuration is invalid in any case, we pick up what is easier to achieve in code
        var error = Assert.Single(errors);
        var duplicateOptionError = Assert.IsType<DuplicateOptionError>(error);

        Assert.Equal(optionName, duplicateOptionError.OptionName);
    }
}
