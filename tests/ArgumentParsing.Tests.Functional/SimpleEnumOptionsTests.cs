using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleEnumOptionsTests
{
    #region OptionsAndParser
    private enum MyEnum
    {
        EnumValue1, EnumValue2
    }

    private sealed class Options
    {
        [Option('e')]
        public MyEnum EnumOption { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("-e EnumValue1", (int)MyEnum.EnumValue1)]
    [InlineData("-eEnumValue2", (int)MyEnum.EnumValue2)]
    [InlineData("--enum-option EnumValue2", (int)MyEnum.EnumValue2)]
    [InlineData("--enum-option=EnumValue1", (int)MyEnum.EnumValue1)]
    public void ParseCorrectArguments(string argsString, int expectedValue)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal((MyEnum)expectedValue, options.EnumOption);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-e MyEnum", "MyEnum", "e")]
    [InlineData("-eVal", "Val", "e")]
    [InlineData("--enum-option enumvalue1", "enumvalue1", "enum-option")]
    [InlineData("--enum-option=enumvalue2", "enumvalue2", "enum-option")]
    public void BadOptionValueFormatError(string argsString, string badValue, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(error);

        Assert.Equal(badValue, badOptionValueFormatError.Value);
        Assert.Equal(optionName, badOptionValueFormatError.OptionName);
    }
}
