using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleRequiredParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public required string Param1 { get; init; }

        [Parameter(1)]
        [Required] // Attribute is intentional, do not replace with `required` property
        public string Param2 { get; init; } = null!;

        [Parameter(2)]
        public string? Param3 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlyCollection<string> args);
    #endregion

    [Theory]
    [InlineData("a b", "a", "b", null)]
    [InlineData("a b c", "a", "b", "c")]
    [InlineData("aa bb cc", "aa", "bb", "cc")]
    public void ParseCorrectArguments(string argsString, string? param1, string? param2, string? param3)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args.ToList().AsReadOnly());

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);
        Assert.Equal(param3, options.Param3);

        Assert.Null(result.Errors);
    }

    [Fact]
    public void MissingFirstRequiredParameter()
    {
        var result = ParseArguments(((List<string>)["a"]).AsReadOnly());

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredParameterError = Assert.IsType<MissingRequiredParameterError>(error);

        Assert.Equal("param2", missingRequiredParameterError.ParameterName);
        Assert.Equal(1, missingRequiredParameterError.ParameterIndex);
    }

    [Fact]
    public void MissingBothRequiredParameters()
    {
        var result = ParseArguments(ReadOnlyCollection<string>.Empty);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        Assert.Collection(errors,
            e =>
            {
                var missingRequiredParameterError = Assert.IsType<MissingRequiredParameterError>(e);
                Assert.Equal("param1", missingRequiredParameterError.ParameterName);
                Assert.Equal(0, missingRequiredParameterError.ParameterIndex);
            },
            e =>
            {
                var missingRequiredParameterError = Assert.IsType<MissingRequiredParameterError>(e);
                Assert.Equal("param2", missingRequiredParameterError.ParameterName);
                Assert.Equal(1, missingRequiredParameterError.ParameterIndex);
            });
    }
}
