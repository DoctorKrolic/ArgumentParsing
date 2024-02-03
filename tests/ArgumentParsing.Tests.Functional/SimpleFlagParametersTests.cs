using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleFlagParametersTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Parameter(0)]
        public bool Param1 { get; init; }

        [Parameter(1)]
        public bool Param2 { get; init; }

        [Parameter(2)]
        public bool Param3 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IList<string> args);
    #endregion

    [Theory]
    [InlineData("", false, false, false)]
    [InlineData("true", true, false, false)]
    [InlineData("false true", false, true, false)]
    [InlineData("true true true", true, true, true)]
    public void ParseCorrectArguments(string argsString, bool param1, bool param2, bool param3)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(param1, options.Param1);
        Assert.Equal(param2, options.Param2);
        Assert.Equal(param3, options.Param3);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("a true false", "a", "param1", 0)]
    [InlineData("false no false", "no", "param2", 1)]
    [InlineData("true true yes", "yes", "param3", 2)]
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
