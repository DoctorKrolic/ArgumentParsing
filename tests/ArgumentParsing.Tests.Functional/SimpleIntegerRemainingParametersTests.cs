using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleIntegerRemainingParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Parameter(0)]
        public int Param1 { get; init; }

        [Parameter(1)]
        public int Param2 { get; init; }

        [RemainingParameters]
        public ImmutableArray<int> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", 0, 0, new int[] { })]
    [InlineData("1", 1, 0, new int[] { })]
    [InlineData("27 145", 27, 145, new int[] { })]
    [InlineData("9 9 147", 9, 9, new int[] { 147 })]
    [InlineData("6514 123 9163 55", 6514, 123, new int[] { 9163, 55 })]
    public void ParseCorrectArguments(string argsString, int param1, int param2, int[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);

        var remainingParametersAsserts = new Action<int>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("1 2 a", "a", 2)]
    [InlineData("5 3 2147483648", "2147483648", 2)]
    [InlineData("20 40 3.7", "3.7", 2)]
    [InlineData("45 981 333 0s", "0s", 3)]
    [InlineData("762 1459 554 21 5.645", "5.645", 4)]
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
