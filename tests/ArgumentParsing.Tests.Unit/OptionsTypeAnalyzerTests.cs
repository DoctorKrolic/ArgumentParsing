using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class OptionsTypeAnalyzerTests : AnalyzerTestBase<OptionsTypeAnalyzer>
{
    [Theory]
    [InlineData("public", "public")]
    [InlineData("internal", "public")]
    [InlineData("internal", "internal")]
    [InlineData("internal", "protected internal")]
    [InlineData("", "internal")]
    [InlineData("", "protected internal")]
    public async Task RequiredField(string optionsTypeAccessibility, string fieldAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{fieldAccessibility}} required int {|ARGP0007:a|};
            }
            """;

        await VerifyAnalyzerAsync(source);
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
    public async Task RequiredField_TooLowFieldAccessibility(string optionsTypeAccessibility, string fieldAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{fieldAccessibility}} required int {|CS9032:a|};
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task UnannotatedRequiredProperty_NoSetter()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                public required int {|CS9034:A|} { get; }
            }
            """;

        await VerifyAnalyzerAsync(source);
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
    public async Task UnannotatedRequiredProperty_TooLowPropertyAccessibility(string optionsTypeAccessibility, string propertyAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|CS9032:A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
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
    public async Task UnannotatedRequiredProperty_TooLowSetterAccessibility(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|CS9032:A|} { get; {{setterAccessibility}} set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("public", "public")]
    [InlineData("internal", "public")]
    [InlineData("internal", "internal")]
    [InlineData("internal", "protected internal")]
    [InlineData("", "internal")]
    [InlineData("", "protected internal")]
    public async Task UnannotatedRequiredProperty_ValidPropertyAccessibility(string optionsTypeAccessibility, string propertyAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|ARGP0008:A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("internal", "internal")]
    [InlineData("internal", "protected internal")]
    [InlineData("", "internal")]
    [InlineData("", "protected internal")]
    public async Task UnannotatedRequiredProperty_ValidSetterAccessibility(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|ARGP0008:A|} { get; {{setterAccessibility}} set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("public", "public")]
    [InlineData("internal", "public")]
    public async Task UnannotatedRequiredProperty_SetterIsMoreAccessibleThanProperty(string optionsTypeAccessibility, string setterAccessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|ARGP0008:A|} { get; {{setterAccessibility}} {|CS0273:set|}; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task TooLowAccessibilityOfOptionProperty(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                {{accessibility}} string {|ARGP0009:A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoSetterOfOptionProperty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string {|ARGP0010:A|} { get; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task TooLowAccessibilityOfASetterOfOptionProperty(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; {{accessibility}} {|ARGP0010:set|}; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('%')]
                public string {|ARGP0012:A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortName_EmptyChar()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option({|CS1011:|}'')]
                public string A { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidLongName_FromPropertyName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('a')]
                public string {|ARGP0013:_A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidLongName_FromAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option("my-long-$name$")]
                public string {|ARGP0013:A|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortAndLongName_FromPropertyName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('&')]
                public string {|ARGP0012:{|ARGP0013:_A|}|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortAndLongName_FromAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('^', "my-long-$name$")]
                public string {|ARGP0012:{|ARGP0013:A|}|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            partial class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }
            }

            partial class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName_ThreeDuplicates()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; set; }

                [Option('o')]
                public string {|ARGP0014:OptionC|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; set; }

                [Option("option")]
                public string {|ARGP0015:B|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
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

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName_ThreeDuplicates()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
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

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    public async Task InvalidOptionType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public {|ARGP0016:{{invalidType}}|} OptionA { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidOptionType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public {|CS0246:ErrorType|} OptionA { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task RequiredBoolOption()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required bool {|ARGP0019:OptionA|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("byte")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("ushort")]
    [InlineData("int")]
    [InlineData("uint")]
    [InlineData("long")]
    [InlineData("ulong")]
    [InlineData("BigInteger")]
    [InlineData("float")]
    [InlineData("double")]
    [InlineData("decimal")]
    [InlineData("bool")]
    [InlineData("DayOfWeek")]
    [InlineData("char")]
    public async Task RequiredNullableOption(string optionBaseType)
    {
        var source = $$"""
            using System;
            using System.Numerics;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required {{optionBaseType}}? {|ARGP0020:OptionA|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NegativeParameterIndex()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(-1)]
                public string {|ARGP0022:Parameter|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; set; }

                [Parameter(0)]
                public string {|ARGP0023:B|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex_NotDefault()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(1)]
                public string {|ARGP0023:B|} { get; set; }

                [Parameter(1)]
                public string {|ARGP0023:C|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameter_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            partial class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; set; }
            }

            partial class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:B|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex_ThreeDuplicates()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; set; }

                [Parameter(0)]
                public string {|ARGP0023:B|} { get; set; }

                [Parameter(0)]
                public string {|ARGP0023:C|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    [InlineData("System.Collections.Generic.IEnumerable<string>")]
    public async Task InvalidParameterType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public {|ARGP0024:{{invalidType}}|} Parameter { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidParameterType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public {|CS0246:ErrorType|} Parameter { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task MissingParameterWithIndex1()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(2)]
                public string B { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(1)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex2()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(1)]
                public string B { get; set; }

                [Parameter(3)]
                public string C { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(2)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex1And2()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(3)]
                public string B { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 2)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex1And3()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(2)]
                public string B { get; set; }

                [Parameter(4)]
                public string C { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(1),
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom1To3()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(4)]
                public string B { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom2To3()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(1)]
                public string B { get; set; }

                [Parameter(4)]
                public string C { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(2, 3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom1To2AndFrom4To5()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(3)]
                public string B { get; set; }

                [Parameter(6)]
                public string C { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 2),
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(4, 5)
        ]);
    }

    [Fact]
    public async Task InvalidRequiredParameter_RequiredProperty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; set; }

                [Parameter(1)]
                public required string {|ARGP0027:Parameter2|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidRequiredParameter_RequiredAttribute()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; set; }

                [Parameter(1)]
                [Required]
                public string {|ARGP0027:Parameter2|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task TwoInvalidRequiredParameters()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; set; }

                [Parameter(1)]
                public string Parameter2 { get; set; }

                [Parameter(2)]
                public required string {|ARGP0027:Parameter3|} { get; set; }

                [Parameter(3)]
                public required string {|ARGP0027:Parameter4|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("5param")]
    [InlineData("-param")]
    [InlineData("$param")]
    [InlineData("param name")]
    [InlineData("invalid&name")]
    public async Task InvalidParameterName(string invalidName)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Parameter(0, Name = "{{invalidName}}")]
                public string {|ARGP0028:Param|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters()
    {
        var source = """
            using System.Collections.Generic;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:A|} { get; set; }

                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:B|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            using System.Collections.Generic;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            partial class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:A|} { get; set; }
            }

            partial class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:B|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters_ThreeDuplicates()
    {
        var source = """
            using System.Collections.Generic;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:A|} { get; set; }

                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:B|} { get; set; }

                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:C|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("int?")]
    [InlineData("string")]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    [InlineData("System.Collections.Generic.IEnumerable<int?>")]
    public async Task InvalidRemainingParametersType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|ARGP0030:{{invalidType}}|} RemainingParams { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidRemainingParametersType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|CS0246:ErrorType|} OptionA { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    public async Task TooLowOptionsTypeAccessibility(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<Options> {|CS8795:ParseArguments|}(string[] args);

                [OptionsType]
                {{accessibility}} class {|ARGP0032:Options|}
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoShortAndLongName()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [Option(null)]
                public string {|ARGP0033:Option|} { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
