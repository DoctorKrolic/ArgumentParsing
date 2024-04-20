using ArgumentParsing.Generated;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;
using ArgumentParsing.SpecialCommands.Help;

namespace ArgumentParsing.Tests.Functional;

[Collection("SpecialCommandTests")]
public sealed partial class CustomHelpTextGeneratorTests
{
    #region OptionsAndParser
    [OptionsType, HelpTextGenerator(typeof(Options), nameof(GenerateHelpText))]
    internal sealed class Options
    {
        public static string GenerateHelpText(ParseErrorCollection? errors = null) => $"Pretend this is a custom help text. By the way, there are {errors?.Count ?? 0} parse errors";
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Fact]
    public void ParseHelpCommand()
    {
        var result = ParseArguments(["--help"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var helpCommandHandler = Assert.IsType<HelpCommandHandler_ArgumentParsing_Tests_Functional_CustomHelpTextGeneratorTests_Options>(result.SpecialCommandHandler);

        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var exitCode = helpCommandHandler.HandleCommand();

        Assert.Equal(0, exitCode);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedOutput = "Pretend this is a custom help text. By the way, there are 0 parse errors";
        var actualOutput = stringWriter.ToString();
        Assert.Equal(expectedOutput.ReplaceLineEndings() + Environment.NewLine, actualOutput.ReplaceLineEndings());
    }
}
