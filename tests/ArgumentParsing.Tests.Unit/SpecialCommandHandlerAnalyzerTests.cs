using ArgumentParsing.CodeFixes;
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

        var fixedSource = """
            [SpecialCommandAliases("--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ConvertToClassCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task WarnOnCommandHandlerStruct_MultiplePartialDeclarations()
    {
        var source = """
            [SpecialCommandAliases("--info")]
            partial struct {|ARGP0038:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            partial struct InfoCommandHandler
            {
            }
            """;

        var fixedSource = """
            [SpecialCommandAliases("--info")]
            partial class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            partial class InfoCommandHandler
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ConvertToClassCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task NoAliases_NoAttribute()
    {
        var source = """
            class {|ARGP0039:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoAliases_NullValuesInAttribute()
    {
        var source = """
            [SpecialCommandAliases(null)]
            class {|ARGP0039:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoAliases_NoValuesInAttribute1()
    {
        var source = """
            [SpecialCommandAliases]
            class {|ARGP0039:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoAliases_NoValuesInAttribute2()
    {
        var source = """
            [SpecialCommandAliases()]
            class {|ARGP0039:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoAliases_NoValuesInAttribute3()
    {
        var source = """
            [SpecialCommandAliases([])]
            class {|ARGP0039:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
