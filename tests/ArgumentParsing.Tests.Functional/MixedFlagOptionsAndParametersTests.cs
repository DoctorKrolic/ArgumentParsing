using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedFlagOptionsAndParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('f', "flag")]
        public bool FlagOption { get; init; }

        [Parameter(0)]
        public bool Param { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", false, false)]
    [InlineData("-f", true, false)]
    [InlineData("-f false", true, false)]
    [InlineData("-f true", true, true)]
    [InlineData("--flag", true, false)]
    [InlineData("--flag false", true, false)]
    [InlineData("--flag true", true, true)]
    public void ParseCorrectArguments(string argsString, bool flagOption, bool param)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(flagOption, options.FlagOption);
        Assert.Equal(param, options.Param);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("true -f true", "f")]
    [InlineData("false -f false", "f")]
    [InlineData("false --flag false", "flag")]
    [InlineData("false --flag true", "flag")]
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
}
