using ArgumentParsing.Generators;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class ArgumentParserGeneratorTests
{
    [Fact]
    public async Task NoGeneratedParserAttribute_NoCustomDiagnostics()
    {
        var source = """
            partial class C
            {
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}();
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameterCount_NoParameters()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:{|ARGP0001:ParseArguments|}|}();
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameterCount_TooManyParameters()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:{|ARGP0001:ParseArguments|}|}(int a, int b);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameter_Params()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0002:params string[] s|});
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameter_Ref()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0002:ref string[] s|});
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameter_Scoped()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0002:scoped ReadOnlySpan<string> s|});
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameter_ScopedRef()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0002:scoped ref string[] s|});
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameter_Extension()
    {
        var source = """
            static partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0002:this string[] s|});
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("string")]
    [InlineData("string[][]")]
    [InlineData("string[,]")]
    [InlineData("System.Collections.IEnumerable")]
    [InlineData("System.Collections.Generic.IEnumerable<int>")]
    public async Task InvalidParameterType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0003:{{invalidType}}|} s);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidParameterType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|CS0246:ErrorType|} s);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    // TODO: Test ARGP0004 with different valid types when we get reasonable codegen

    [Theory]
    [InlineData("C")]
    [InlineData("int")]
    [InlineData("string")]
    [InlineData("MyOptions")]
    [InlineData("EmptyOptions")]
    public async Task InvalidReturnType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0005:{{invalidType}}|} {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                public string S { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidReturnType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|CS0246:ErrorType|} {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("string")]
    [InlineData("Action")]
    [InlineData("DayOfWeek")]
    [InlineData("ClassWithParameterConstructor")]
    public async Task InvalidOptionsType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{|ARGP0006:{{invalidType}}|}> {|CS8795:ParseArguments|}(string[] args);
            }

            class ClassWithParameterConstructor
            {
                public ClassWithParameterConstructor(int a)
                {
                }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidOptionsType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{|CS0246:ErrorType|}> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task InvalidReturnTypeAndInvalidParameterType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0005:int|} {|CS8795:ParseArguments|}({|ARGP0003:string|} args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("public", "public")]
    [InlineData("internal", "public")]
    [InlineData("internal", "internal")]
    [InlineData("internal", "protected internal")]
    [InlineData("", "internal")]
    [InlineData("", "protected internal")]
    public async Task OptionsType_RequiredField(string optionsTypeAccessibility, string fieldAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                {{fieldAccessibility}} required int {|ARGP0007:a|};
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("public", "private")]
    [InlineData("public", "protected")]
    [InlineData("public", "internal")]
    [InlineData("public", "private protected")]
    [InlineData("public", "protected internal")]
    [InlineData("internal", "private")]
    [InlineData("internal", "protected")]
    [InlineData("internal", "private protected")]
    [InlineData("", "private")]
    [InlineData("", "protected")]
    [InlineData("", "private protected")]
    public async Task OptionsType_RequiredField_TooLowFieldAccessibility_NoCustomDiagnostic(string optionsTypeAccessibility, string fieldAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                {{fieldAccessibility}} required int {|CS9032:a|};
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    private static async Task VerifyGeneratorAsync(string source, params (string Hint, string Content)[] generatedDocuments)
    {
        var test = new CSharpSourceGeneratorTest<ArgumentParserGenerator>()
        {
            TestState =
            {
                Sources =
                {
                    source,
                    """
                    global using ArgumentParsing;
                    global using ArgumentParsing.Results;
                    global using System;
                    """,
                    "class EmptyOptions { }"
                },
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GeneratedArgumentParserAttribute).Assembly.Location),
                }
            },
            LanguageVersion = LanguageVersion.Latest,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        foreach (var (hint, content) in generatedDocuments)
        {
            test.TestState.GeneratedSources.Add((typeof(ArgumentParserGenerator), hint, content));
        }

        await test.RunAsync();
    }
}
