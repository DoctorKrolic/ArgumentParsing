using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleNumericParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public int Param1 { get; init; }

        [Parameter(1)]
        public long Param2 { get; init; }

        [Parameter(2)]
        public float Param3 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ImmutableArray<string> args);
    #endregion

    [Theory]
    [InlineData("", 0, 0, 0)]
    [InlineData("1", 1, 0, 0)]
    [InlineData("1 2", 1, 2, 0)]
    [InlineData("1 2 3", 1, 2, 3)]
    [InlineData("-10 2147483648 3.15", -10, 2147483648, 3.15f)]
    public void ParseCorrectArguments(string argsString, int param1, long param2, float param3)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments([.. args]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);
        Assert.Equal(param3, options.Param3);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("a 2 3", "a", 0)]
    [InlineData("2147483648 5 3.7", "2147483648", 0)]
    [InlineData("20 long 3.7", "long", 1)]
    [InlineData("20 8.37 3.7", "8.37", 1)]
    [InlineData("20 45 f", "f", 2)]
    [InlineData("20 45 3;7", "3;7", 2)]
    public void BadParameterValueFormatError(string argsString, string badValue, int parameterIndex)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments([.. args]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badParameterValueFormatError = Assert.IsType<BadParameterValueFormatError>(error);

        Assert.Equal(badValue, badParameterValueFormatError.Value);
        Assert.Equal(parameterIndex, badParameterValueFormatError.ParameterIndex);
    }
}
