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

    [Fact]
    public async Task OptionsType_UnannotatedRequiredProperty_NoSetter_NoCustomDiagnostics()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                public required int {|CS9034:A|} { get; }
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
    public async Task OptionsType_UnannotatedRequiredProperty_TooLowPropertyAccessibility_NoCustomDiagnostics(string optionsTypeAccessibility, string propertyAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|CS9032:A|} { get; set; }
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
    public async Task OptionsType_UnannotatedRequiredProperty_TooLowSetterAccessibility_NoCustomDiagnostics(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|CS9032:A|} { get; {{setterAccessibility}} set; }
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
    public async Task OptionsType_UnannotatedRequiredProperty_ValidPropertyAccessibility(string optionsTypeAccessibility, string propertyAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|ARGP0008:A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("internal", "internal")]
    [InlineData("internal", "protected internal")]
    [InlineData("", "internal")]
    [InlineData("", "protected internal")]
    public async Task OptionsType_UnannotatedRequiredProperty_ValidSetterAccessibility(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|ARGP0008:A|} { get; {{setterAccessibility}} set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("public", "public")]
    [InlineData("internal", "public")]
    public async Task OptionsType_UnannotatedRequiredProperty_SetterIsMoreAccessibleThanProperty(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|ARGP0008:A|} { get; {{setterAccessibility}} {|CS0273:set|}; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task OptionsType_TooLowAccessibilityOfOptionProperty(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                {{accessibility}} string {|ARGP0009:A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_NoSetterOfOptionProperty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public string {|ARGP0010:A|} { get; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task OptionsType_TooLowAccessibilityOfASetterOfOptionProperty(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public string A { get; {{accessibility}} {|ARGP0010:set|}; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidShortName_FromPropertyName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option("a")]
                public string {|ARGP0012:_A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidShortName_FromAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option('%')]
                public string {|ARGP0012:A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidLongName_FromPropertyName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option('a')]
                public string {|ARGP0013:_A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidLongName_FromAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option("my-long-$name$")]
                public string {|ARGP0013:A|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidShortAndLongName_FromPropertyName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public string {|ARGP0012:{|ARGP0013:_A|}|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidShortAndLongName_FromAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option('^', "my-long-$name$")]
                public string {|ARGP0012:{|ARGP0013:A|}|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateShortName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option]
                public string {|ARGP0014:OptionB|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateShortName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            partial class MyOptions
            {
                [Option]
                public string {|ARGP0014:OptionA|} { get; set; }
            }

            partial class MyOptions
            {
                [Option]
                public string {|ARGP0014:OptionB|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateShortName_ThreeDuplicates()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option]
                public string {|ARGP0014:OptionB|} { get; set; }

                [Option]
                public string {|ARGP0014:OptionC|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateLongName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; set; }

                [Option("option")]
                public string {|ARGP0015:B|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateLongName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            partial class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; set; }
            }

            partial class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:B|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateLongName_ThreeDuplicates()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; set; }

                [Option("option")]
                public string {|ARGP0015:B|} { get; set; }

                [Option("option")]
                public string {|ARGP0015:C|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    public async Task OptionsType_InvalidOptionType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option]
                public {|ARGP0016:{{invalidType}}|} OptionA { get; set; }
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
