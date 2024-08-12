using System.Collections.Immutable;
using ArgumentParsing.Generated;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;
using ArgumentParsing.SpecialCommands.Help;

namespace ArgumentParsing.Tests.Functional;

[Collection("UsesSTDIO")]
public sealed partial class SimpleHelpCommandTest
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('o'), HelpInfo("Option help description")]
        public int Option { get; init; }

        [Parameter(0), HelpInfo("Parameter help description")]
        public string? Parameter { get; init; }

        [RemainingParameters, HelpInfo("Remaining parameters description")]
        public ImmutableArray<string> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("--help")]
    [InlineData("--help this does not matter")]
    public void ParseCorrectArguments(string argsString)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var helpCommandHandler = Assert.IsType<HelpCommandHandler_ArgumentParsing_Tests_Functional_SimpleHelpCommandTest_Options>(result.SpecialCommandHandler);

        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var exitCode = helpCommandHandler.HandleCommand();

        Assert.Equal(0, exitCode);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedOutput = $"""
            {assemblyName.Name} {assemblyName.Version!.ToString(3)}
            Copyright (C) {DateTime.UtcNow.Year}

            OPTIONS:

              -o, --option{'\t'}Option help description

            PARAMETERS:

              parameter (at index 0){'\t'}Parameter help description

              Remaining parameters{'\t'}Remaining parameters description

            COMMANDS:

              --help{'\t'}Show help screen

              --version{'\t'}Show version information

            """;
        var actualOutput = stringWriter.ToString();
        Assert.Equal(expectedOutput.ReplaceLineEndings() + Environment.NewLine, actualOutput.ReplaceLineEndings());
    }

    [Fact]
    public void GenerateErrorScreen()
    {
        var errors = new ParseError[]
        {
            new UnknownOptionError("a", "-a"),
            new DuplicateOptionError("option")
        };

        var errorCollection = ParseErrorCollection.AsErrorCollection([.. errors]);

        var helpScreen = HelpCommandHandler_ArgumentParsing_Tests_Functional_SimpleHelpCommandTest_Options.GenerateHelpText(errorCollection);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedHelpScreen = $"""
            {assemblyName.Name} {assemblyName.Version!.ToString(3)}
            Copyright (C) {DateTime.UtcNow.Year}

            ERROR(S):
              {errors[0].GetMessage()}
              {errors[1].GetMessage()}

            OPTIONS:

              -o, --option{'\t'}Option help description

            PARAMETERS:

              parameter (at index 0){'\t'}Parameter help description

              Remaining parameters{'\t'}Remaining parameters description

            COMMANDS:

              --help{'\t'}Show help screen

              --version{'\t'}Show version information

            """;
        Assert.Equal(expectedHelpScreen.ReplaceLineEndings(), helpScreen.ReplaceLineEndings());
    }
}
