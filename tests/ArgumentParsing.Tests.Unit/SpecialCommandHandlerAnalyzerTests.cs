using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;

namespace ArgumentParsing.Tests.Unit;

public sealed class SpecialCommandHandlerAnalyzerTests : AnalyzerTestBase<SpecialCommandHandlerAnalyzer>
{
    [Fact]
    public async Task NoDiagnosticsOnUnrelatedClass()
    {
        var source = """
            class NotCommandHandler
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoDiagnosticsOnUnrelatedStruct()
    {
        var source = """
            struct NotCommandHandler
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoDiagnosticsOnCommandHandlerClass()
    {
        var source = """
            [SpecialCommandAliases("--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task WarnOnCommandHandlerStruct()
    {
        var source = """
            [SpecialCommandAliases("--info")]
            struct {|ARGP0038:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
