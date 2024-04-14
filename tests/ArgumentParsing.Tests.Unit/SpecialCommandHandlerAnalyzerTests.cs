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

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoAttribute(string handlerName)
    {
        var source = $$"""
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [SpecialCommandAliases("--info")]
            class {{handlerName}} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NullValuesInAttribute(string handlerName)
    {
        var source = $$"""
            [SpecialCommandAliases(null)]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [SpecialCommandAliases("--info")]
            class {{handlerName}} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NullValuesInAttribute_AttributeOnAnotherPartialDeclaration(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [SpecialCommandAliases(null)]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute1(string handlerName)
    {
        var source = $$"""
            [SpecialCommandAliases]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute1_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            [Obsolete, SpecialCommandAliases]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [Obsolete, SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute1_AttributeOnAnotherPartialDeclaration(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [SpecialCommandAliases]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute1_AttributeOnAnotherPartialDeclaration_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [Obsolete, SpecialCommandAliases]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [Obsolete, SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute2(string handlerName)
    {
        var source = $$"""
            [SpecialCommandAliases()]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute2_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            [Obsolete, SpecialCommandAliases()]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [Obsolete, SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute2_AttributeOnAnotherPartialDeclaration(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [SpecialCommandAliases()]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute2_AttributeOnAnotherPartialDeclaration_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [Obsolete, SpecialCommandAliases()]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [Obsolete, SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute3(string handlerName)
    {
        var source = $$"""
            [SpecialCommandAliases([])]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute3_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            [Obsolete, SpecialCommandAliases([])]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource = $$"""
            [Obsolete, SpecialCommandAliases("--info")]
            class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute3_AttributeOnAnotherPartialDeclaration(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [SpecialCommandAliases([])]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("Info")]
    [InlineData("InfoCommandHandler")]
    [InlineData("InfoSpecialCommandHandler")]
    public async Task NoAliases_NoValuesInAttribute3_AttributeOnAnotherPartialDeclaration_MultipleAttributesInTheList(string handlerName)
    {
        var source = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [Obsolete, SpecialCommandAliases([])]
            partial class {{handlerName}}
            {
            }
            """;

        var fixedSource = $$"""
            partial class {|ARGP0039:{{handlerName}}|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            
            [Obsolete, SpecialCommandAliases("--info")]
            partial class {{handlerName}}
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<DeclareSpecialCommandAliasCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("--my$name")]
    [InlineData("2me")]
    [InlineData("my command")]
    public async Task InvalidAlias(string invalidName)
    {
        var source = $$"""
            [SpecialCommandAliases({|ARGP0040:"{{invalidName}}"|})]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("--my$name")]
    [InlineData("2me")]
    [InlineData("my command")]
    public async Task InvalidAlias_TwoAliases(string invalidName)
    {
        var source = $$"""
            [SpecialCommandAliases("--info", {|ARGP0040:"{{invalidName}}"|})]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidAlias_TwoAliases_Null()
    {
        var source = """
            [SpecialCommandAliases({|ARGP0040:null|}, "--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NameStartsWithLetter()
    {
        var source = """
            [SpecialCommandAliases({|ARGP0041:"info"|})]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource1 = """
            [SpecialCommandAliases("--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        var fixedSource2 = """
            [SpecialCommandAliases("-info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<PrefixAliasWithDashesCodeFixProvider>(source, fixedSource1, codeActionIndex: 0);
        await VerifyAnalyzerWithCodeFixAsync<PrefixAliasWithDashesCodeFixProvider>(source, fixedSource2, codeActionIndex: 1);
    }

    [Fact]
    public async Task NoParameterlessConstructor()
    {
        var source = """
            [SpecialCommandAliases("--info")]
            class {|ARGP0042:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;

                public InfoCommandHandler(int x)
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task InaccessibleParameterlessConstructor(string accessibility)
    {
        var source = $$"""
            [SpecialCommandAliases("--info")]
            class {|ARGP0042:InfoCommandHandler|} : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;

                {{accessibility}} InfoCommandHandler()
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
