using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleEnumRemainingParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Parameter(0)]
        public DayOfWeek Param1 { get; init; }

        [Parameter(1)]
        public DayOfWeek Param2 { get; init; }

        [RemainingParameters]
        public ImmutableArray<DayOfWeek> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("", default(DayOfWeek), default(DayOfWeek), new DayOfWeek[] { })]
    [InlineData("Thursday", DayOfWeek.Thursday, default(DayOfWeek), new DayOfWeek[] { })]
    [InlineData("Monday Tuesday", DayOfWeek.Monday, DayOfWeek.Tuesday, new DayOfWeek[] { })]
    [InlineData("Wednesday Monday Friday", DayOfWeek.Wednesday, DayOfWeek.Monday, new DayOfWeek[] { DayOfWeek.Friday })]
    [InlineData("Monday Tuesday Wednesday Thursday", DayOfWeek.Monday, DayOfWeek.Tuesday, new DayOfWeek[] { DayOfWeek.Wednesday, DayOfWeek.Thursday })]
    public void ParseCorrectArguments(string argsString, DayOfWeek param1, DayOfWeek param2, DayOfWeek[] remainingParams)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);

        var remainingParametersAsserts = new Action<DayOfWeek>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ch) => Assert.Equal(remainingParams[copy], ch);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("Monday Friday a", "a", 2)]
    [InlineData("Saturday Sunday MyDay", "MyDay", 2)]
    [InlineData("Friday Wednesday Sunday monday", "monday", 3)]
    [InlineData("Thursday Saturday Monday Tuesday ThisDay", "ThisDay", 4)]
    public void BadRemainingParameterValueFormatError(string argsString, string badValue, int parameterIndex)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

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
