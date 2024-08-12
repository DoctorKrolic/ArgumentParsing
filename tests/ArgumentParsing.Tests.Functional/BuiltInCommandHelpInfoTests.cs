using ArgumentParsing.Generated;
using ArgumentParsing.Results;
using ArgumentParsing.SpecialCommands;
using ArgumentParsing.SpecialCommands.Help;

namespace ArgumentParsing.Tests.Functional;

[Collection("UsesSTDIO")]
public sealed partial class BuiltInCommandHelpInfoTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
    }

    [GeneratedArgumentParser]
    [BuiltInCommandHelpInfo(BuiltInCommandHandlers.Help, "Custom help command description")]
    [BuiltInCommandHelpInfo(BuiltInCommandHandlers.Version, "")]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Fact]
    public void ConsiderBuiltInHelpInfoForHelpScreen()
    {
        var result = ParseArguments(["--help"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var helpCommandHandler = Assert.IsType<HelpCommandHandler_ArgumentParsing_Tests_Functional_BuiltInCommandHelpInfoTests_Options>(result.SpecialCommandHandler);

        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var exitCode = helpCommandHandler.HandleCommand();

        Assert.Equal(0, exitCode);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedOutput = $"""
            {assemblyName.Name} {assemblyName.Version!.ToString(3)}
            Copyright (C) {DateTime.UtcNow.Year}

            COMMANDS:

              --help{'\t'}Custom help command description

              --version

            """;
        var actualOutput = stringWriter.ToString();
        Assert.Equal(expectedOutput.ReplaceLineEndings() + Environment.NewLine, actualOutput.ReplaceLineEndings());
    }
}
