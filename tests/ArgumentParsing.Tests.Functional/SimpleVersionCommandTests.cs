using ArgumentParsing.Generated;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleVersionCommandTests
{
    #region OptionsAndParser
    private sealed class Options
    {
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    [Theory]
    [InlineData("--version")]
    [InlineData("--version this does not matter")]
    public void ParseCorrectArguments(string argsString)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedSpecialCommand, result.State);

        Assert.Null(result.Options);
        Assert.Null(result.Errors);

        var versionCommandHandler = Assert.IsType<VersionCommandHandler>(result.SpecialCommandHandler);

        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var exitCode = versionCommandHandler.HandleCommand();

        Assert.Equal(0, exitCode);

        var assemblyName = typeof(Options).Assembly.GetName();
        var expectedOutput = $"{assemblyName.Name} {assemblyName.Version!.ToString(3)}{Environment.NewLine}";
        var actualOutput = stringWriter.ToString();
        Assert.Equal(expectedOutput, actualOutput.ReplaceLineEndings());
    }
}
