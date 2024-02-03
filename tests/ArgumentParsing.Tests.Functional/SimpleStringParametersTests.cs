using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleStringParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public string? Param1 { get; init; }

        [Parameter(1)]
        public string? Param2 { get; init; }

        [Parameter(2)]
        public string? Param3 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", null, null, null)]
    [InlineData("a", "a", null, null)]
    [InlineData("a b", "a", "b", null)]
    [InlineData("a b c", "a", "b", "c")]
    [InlineData("aa bb cc", "aa", "bb", "cc")]
    public void ParseCorrectArguments(string argsString, string? param1, string? param2, string? param3)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);
        Assert.Equal(param3, options.Param3);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("a b c d", "d")]
    [InlineData("a b c abc", "abc")]
    public void UnrecognizedArgumentError(string argsString, string argument)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments([.. args]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(error);

        Assert.Equal(argument, unrecognizedArgumentError.Argument);
    }

    [Fact]
    public void TwoUnrecognizedArgumentErrors()
    {
        var result = ParseArguments(["a", "b", "c", "d", "e"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        Assert.Collection(errors,
            e =>
            {
                var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(e);
                Assert.Equal("d", unrecognizedArgumentError.Argument);
            },
            e =>
            {
                var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(e);
                Assert.Equal("e", unrecognizedArgumentError.Argument);
            });
    }
}
