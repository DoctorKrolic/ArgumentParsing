using System.ComponentModel.DataAnnotations;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleRequiredOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('r')]
        public required string RequiredOption { get; init; }

        [Option('a')]
        [Required] // Attribute is intentional, do not replace with `required` property
        public string AnotherRequiredOption { get; init; }

        [Option('n')]
        public string? NotRequiredOption { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlySpan<string> args);
    #endregion

    [Theory]
    [InlineData("-r r -a a -n n", "n")]
    [InlineData("--another-required-option a -n n --required-option r", "n")]
    [InlineData("--not-required-option not -rr --another-required-option=a", "not")]
    [InlineData("--another-required-option a --required-option=r", null)]
    public void CorrectlyParseWhenAllRequiredArgumentsArePresent(string argsString, string? notRequiredOptionValue)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal("r", options.RequiredOption);
        Assert.Equal("a", options.AnotherRequiredOption);
        Assert.Equal(notRequiredOptionValue, options.NotRequiredOption);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-a a -n n")]
    [InlineData("--another-required-option a -n n")]
    [InlineData("--not-required-option not --another-required-option=a")]
    [InlineData("--another-required-option a")]
    public void MissingFirstRequiredOption(string argsString)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(error);

        Assert.Equal('r', missingRequiredOptionError.ShortOptionName);
        Assert.Equal("required-option", missingRequiredOptionError.LongOptionName);
    }

    [Theory]
    [InlineData("-r r -n n")]
    [InlineData("-n n --required-option r")]
    [InlineData("--not-required-option not -rr")]
    [InlineData("--required-option=r")]
    public void MissingSecondRequiredOption(string argsString)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(error);

        Assert.Equal('a', missingRequiredOptionError.ShortOptionName);
        Assert.Equal("another-required-option", missingRequiredOptionError.LongOptionName);
    }

    [Theory]
    [InlineData("-n n")]
    [InlineData("--not-required-option n")]
    public void MissingBothRequiredOptions(string argsString)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        Assert.Collection(errors,
            e =>
            {
                var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(e);
                Assert.Equal('r', missingRequiredOptionError.ShortOptionName);
                Assert.Equal("required-option", missingRequiredOptionError.LongOptionName);
            },
            e =>
            {
                var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(e);
                Assert.Equal('a', missingRequiredOptionError.ShortOptionName);
                Assert.Equal("another-required-option", missingRequiredOptionError.LongOptionName);
            });
    }
}
