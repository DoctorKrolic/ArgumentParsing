using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedStringOptionsAndParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('o')]
        public string? Option { get; init; }

        [Parameter(0)]
        public string? Param { get; init; }

        [RemainingParameters]
        public ImmutableArray<string> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", null, null, new string[] { })]
    [InlineData("-o a", "a", null, new string[] { })]
    [InlineData("--option a b", "a", "b", new string[] { })]
    [InlineData("param -o opt", "opt", "param", new string[] { })]
    [InlineData("p --option=value some remaining params", "value", "p", new string[] { "some", "remaining", "params" })]
    [InlineData("-oval param for you", "val", "param", new string[] { "for", "you" })]
    [InlineData("-- -oval", null, "-oval", new string[] { })]
    [InlineData("-- --option value", null, "--option", new string[] { "value" })]
    [InlineData("p -- -o val", null, "p", new string[] { "-o", "val" })]
    public void ParseCorrectArguments(string argsString, string? option, string? param, string[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(option, options.Option);
        Assert.Equal(param, options.Param);

        var remainingParametersAsserts = new Action<string>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("--=", "--=")]
    [InlineData("--=a", "--=a")]
    [InlineData("-o opt --=val", "--=val")]
    [InlineData("param --=val", "--=val")]
    [InlineData("param1 param2 --=", "--=")]
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
}
