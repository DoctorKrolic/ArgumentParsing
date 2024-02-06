using System.Collections.Immutable;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleStringRemainingParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public string? Param1 { get; init; }

        [Parameter(1)]
        public string? Param2 { get; init; }

        [RemainingParameters]
        public ImmutableArray<string> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", null, null, new string[] { })]
    [InlineData("a", "a", null, new string[] { })]
    [InlineData("a b", "a", "b", new string[] { })]
    [InlineData("a b c", "a", "b", new string[] { "c" })]
    [InlineData("a b c d", "a", "b", new string[] { "c", "d" })]
    [InlineData("aa bb cc dd", "aa", "bb", new string[] { "cc", "dd" })]
    public void ParseCorrectArguments(string argsString, string? param1, string? param2, string[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);

        var remainingParametersAsserts = new Action<string>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
    }
}
