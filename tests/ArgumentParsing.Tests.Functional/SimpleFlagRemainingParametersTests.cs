using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleFlagRemainingParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Parameter(0)]
        public bool Param1 { get; init; }

        [Parameter(1)]
        public bool Param2 { get; init; }

        [RemainingParameters]
        public ImmutableArray<bool> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", false, false, new bool[] { })]
    [InlineData("true", true, false, new bool[] { })]
    [InlineData("false true", false, true, new bool[] { })]
    [InlineData("true true true", true, true, new bool[] { true })]
    [InlineData("false false false false", false, false, new bool[] { false, false })]
    public void ParseCorrectArguments(string argsString, bool param1, bool param2, bool[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);

        var remainingParametersAsserts = new Action<bool>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (b) => Assert.Equal(remainingParams[copy], b);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("true false a", "a", 2)]
    [InlineData("false false no", "no", 2)]
    [InlineData("false true yes", "yes", 2)]
    [InlineData("true true true 0", "0", 3)]
    [InlineData("false true false true 1", "1", 4)]
    public void BadRemainingParameterValueFormatError(string argsString, string badValue, int parameterIndex)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments([.. args]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(error);

        Assert.Equal(badValue, badRemainingParameterValueFormatError.Value);
        Assert.Equal(parameterIndex, badRemainingParameterValueFormatError.ParameterIndex);
    }
}
