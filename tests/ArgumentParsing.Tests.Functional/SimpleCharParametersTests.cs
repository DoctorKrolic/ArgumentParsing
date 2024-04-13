using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleCharParametersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Parameter(0)]
        public char Param1 { get; init; }

        [Parameter(1)]
        public char Param2 { get; init; }

        [Parameter(2)]
        public char Param3 { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ImmutableList<string> args);
    #endregion

    [Theory]
    [InlineData("", default(char), default(char), default(char))]
    [InlineData("a", 'a', default(char), default(char))]
    [InlineData("a b", 'a', 'b', default(char))]
    [InlineData("a b c", 'a', 'b', 'c')]
    public void ParseCorrectArguments(string argsString, char param1, char param2, char param3)
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
        Assert.Null(result.SpecialCommandHandler);
    }

    [Theory]
    [InlineData("aa b c", "aa", "param1", 0)]
    [InlineData("a no c", "no", "param2", 1)]
    [InlineData("a b yes", "yes", "param3", 2)]
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
