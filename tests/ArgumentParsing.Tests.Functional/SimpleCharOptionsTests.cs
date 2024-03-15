using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleCharOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('c')]
        public char CharOption { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(Span<string> args);
    #endregion

    [Theory]
    [InlineData("", default(char))]
    [InlineData("-c d", 'd')]
    [InlineData("-cf", 'f')]
    [InlineData("--char-option g", 'g')]
    [InlineData("--char-option=h", 'h')]
    public void ParseCorrectArguments(string argsString, char c)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(c, options.CharOption);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-c str", "str", "c")]
    [InlineData("--char-option something", "something", "char-option")]
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
