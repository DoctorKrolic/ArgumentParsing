using ArgumentParsing.Generated;
using ArgumentParsing.Results;
using ArgumentParsing.SpecialCommands;
using ArgumentParsing.SpecialCommands.Help;

namespace ArgumentParsing.Tests.Functional;

[Collection("UsesSTDIO")]
public sealed partial class DefaultHelpScreenWithNonDefaultSpecialCommandHandlersTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
    }

    [SpecialCommandAliases("--my-command")]
    public sealed class MySpecialCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [SpecialCommandAliases("--my-other-command"), HelpInfo("Special command description")]
    public sealed class MyOtherSpecialCommandHandler : ISpecialCommandHandler
    {
        public int HandleCommand() => 0;
    }

    [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.Help, AdditionalCommandHandlers = [typeof(MySpecialCommandHandler), typeof(MyOtherSpecialCommandHandler)])]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Fact]
    public void ShowCorrectCommandsOnHelpScreen()
    {
        var result = ParseArguments(["--help"]);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var helpCommandHandler = Assert.IsType<HelpCommandHandler_ArgumentParsing_Tests_Functional_DefaultHelpScreenWithNonDefaultSpecialCommandHandlersTests_Options>(result.SpecialCommandHandler);

        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var exitCode = helpCommandHandler.HandleCommand();

        Assert.Equal(0, exitCode);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedOutput = $"""
            {assemblyName.Name} {assemblyName.Version!.ToString(3)}
            Copyright (C) {DateTime.UtcNow.Year}

            COMMANDS:

              --help{'\t'}Show help screen

              --my-command

              --my-other-command{'\t'}Special command description

            """;
        var actualOutput = stringWriter.ToString();
        Assert.Equal(expectedOutput.ReplaceLineEndings() + Environment.NewLine, actualOutput.ReplaceLineEndings());
    }
}
