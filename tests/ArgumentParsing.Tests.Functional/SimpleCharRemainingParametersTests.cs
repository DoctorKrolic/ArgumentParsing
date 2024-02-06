using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleCharRemainingParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public char Param1 { get; init; }

        [Parameter(1)]
        public char Param2 { get; init; }

        [RemainingParameters]
        public ImmutableArray<char> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", default(char), default(char), new char[] { })]
    [InlineData("a", 'a', default(char), new char[] { })]
    [InlineData("a b", 'a', 'b', new char[] { })]
    [InlineData("a b c", 'a', 'b', new char[] { 'c' })]
    [InlineData("a b c d", 'a', 'b', new char[] { 'c', 'd' })]
    public void ParseCorrectArguments(string argsString, char param1, char param2, char[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);

        var remainingParametersAsserts = new Action<char>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("a 5 ab", "ab", 2)]
    [InlineData("h 2 no", "no", 2)]
    [InlineData("r 0 yes", "yes", 2)]
    [InlineData("b 7 p zz", "zz", 3)]
    [InlineData("c q y 9 45", "45", 4)]
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
