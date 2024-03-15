using System.Collections.Immutable;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SampleSumAppTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('v')]
        public bool Verbose { get; init; }

        [Parameter(0)]
        public required int FirstRequiredParameter { get; init; }

        [Parameter(1)]
        public required int SecondRequiredParameter { get; init; }

        [RemainingParameters]
        public ImmutableArray<int> RemainingParameters { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("2 3", false, 2, 3, new int[] { })]
    [InlineData("2 3 5", false, 2, 3, new int[] { 5 })]
    [InlineData("-v 2 3 5", true, 2, 3, new int[] { 5 })]
    [InlineData("2 --verbose 3", true, 2, 3, new int[] { })]
    [InlineData("2 3 -v 7", true, 2, 3, new int[] { 7 })]
    public void ParseCorrectArguments(string argsString, bool verbose, int param1, int param2, int[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(verbose, options.Verbose);
        Assert.Equal(param1, options.FirstRequiredParameter);
        Assert.Equal(param2, options.SecondRequiredParameter);

        var remainingParametersAsserts = new Action<int>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParameters, remainingParametersAsserts);

        Assert.Null(result.Errors);
    }
}
