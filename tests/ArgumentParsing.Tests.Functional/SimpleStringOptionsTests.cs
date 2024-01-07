using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class SimpleStringOptionsTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        // TODO: Add actual option properties here
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(string[] args);
    #endregion

    // TODO: write actual tests here. For now this thing just needs to compile
}
