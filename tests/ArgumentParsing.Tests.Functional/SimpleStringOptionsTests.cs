using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

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

    [Fact]
    public void ParseSingleHyphenAsOptionValue()
    {
        var result = ParseArguments(["-a", "-", "--option-b", "-", "-c-"]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal("-", options.OptionA);
        Assert.Equal("-", options.OptionB);
        Assert.Equal("-", options.OptionC);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-a a -b b -c c -d e", "d", "-d")]
    [InlineData("-a a -b b -de -c c", "d", "-de")]
    [InlineData("-dabc -a a -b b -c c", "d", "-dabc")]
    [InlineData("-a a --option-d e -b b -c c", "option-d", "--option-d")]
    [InlineData("-a a --option-d=e -b b -c c", "option-d", "--option-d=e")]
    public void UnknownOptionError(string argsString, string unknownOptionName, string unknownOptionArgument)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unknownOptionError = Assert.IsType<UnknownOptionError>(error);

        Assert.Equal(unknownOptionName, unknownOptionError.OptionName);
        Assert.Equal(unknownOptionArgument, unknownOptionError.ContainingArgument);
    }

    [Theory]
    [InlineData("-c c d", "d")]
    [InlineData("-b b de", "de")]
    [InlineData("d=e -a a", "d=e")]
    [InlineData("--option-a a d -b b", "d")]
    [InlineData("--option-b=b d", "d")]
    public void UnrecognizedArgumentError(string argsString, string unrecognizedArgument)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(error);

        Assert.Equal(unrecognizedArgument, unrecognizedArgumentError.Argument);
    }

    [Theory]
    [InlineData("-d e")]
    [InlineData("--option-d f")]
    public void NoUnrecognizedArgumentErrorAfterUnknownOption(string argsString)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        Assert.IsType<UnknownOptionError>(error);
    }

    [Theory]
    [InlineData("-a -b b -c c", "-a")]
    [InlineData("-a a -b -c c", "-b")]
    [InlineData("-a a -b b -c", "-c")]
    [InlineData("--option-a --option-b b --option-c c", "--option-a")]
    [InlineData("--option-a a --option-b --option-c c", "--option-b")]
    [InlineData("--option-a a --option-b b --option-c", "--option-c")]
    public void OptionValueIsNotProvidedError(string argsString, string precedingArgument)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var optionValueIsNotProvidedError = Assert.IsType<OptionValueIsNotProvidedError>(error);

        Assert.Equal(precedingArgument, optionValueIsNotProvidedError.PrecedingArgument);
    }

    [Theory]
    [InlineData("-a a -b b -c c -a a", "a")]
    [InlineData("--option-b b -a a -b b -c c", "b")]
    [InlineData("-a a -b b -c c --option-c c", "option-c")]
    [InlineData("-a a -c c -b b --option-c c", "option-c")]
    public void DuplicateOptionError(string argsString, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var duplicateOptionError = Assert.IsType<DuplicateOptionError>(error);

        Assert.Equal(optionName, duplicateOptionError.OptionName);
    }

    [Fact]
    public void TwoDuplicateOptionErrors()
    {
        var result = ParseArguments(["-a", "a", "--option-a", "b", "-a", "c"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        Assert.Collection(errors,
            e =>
            {
                var dupError = Assert.IsType<DuplicateOptionError>(e);
                Assert.Equal("option-a", dupError.OptionName);
            },
            e =>
            {
                var dupError = Assert.IsType<DuplicateOptionError>(e);
                Assert.Equal("a", dupError.OptionName);
            });
    }

    [Fact]
    public void OptionValueIsNotProvidedAndDuplicateOptionErrors()
    {
        var result = ParseArguments(["--option-b", "-a", "a", "-b", "b"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        Assert.Collection(errors,
            e =>
            {
                var optValNotProvidedError = Assert.IsType<OptionValueIsNotProvidedError>(e);
                Assert.Equal("--option-b", optValNotProvidedError.PrecedingArgument);
            },
            e =>
            {
                var dupError = Assert.IsType<DuplicateOptionError>(e);
                Assert.Equal("b", dupError.OptionName);
            });
    }
}
