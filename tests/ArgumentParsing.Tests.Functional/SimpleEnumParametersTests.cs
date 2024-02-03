using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleEnumParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public DayOfWeek Param1 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(List<string> args);
    #endregion

    [Theory]
    [InlineData("", default(DayOfWeek))]
    [InlineData("Monday", DayOfWeek.Monday)]
    [InlineData("Tuesday", DayOfWeek.Tuesday)]
    public void ParseCorrectArguments(string argsString, DayOfWeek param1)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments([.. args]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("SomeDay", "SomeDay", "param1", 0)]
    [InlineData("monday", "monday", "param1", 0)]
    public void BadParameterValueFormatError(string argsString, string badValue, string parameterName, int parameterIndex)
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
        Assert.Equal(parameterName, badParameterValueFormatError.ParameterName);
        Assert.Equal(parameterIndex, badParameterValueFormatError.ParameterIndex);
    }
}
