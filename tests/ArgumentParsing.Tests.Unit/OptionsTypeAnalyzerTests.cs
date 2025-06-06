using ArgumentParsing.CodeFixes;
using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|CS9032:A|} { get; init; }
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
            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|CS9032:A|} { get; {{setterAccessibility}} init; }
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
            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int {|ARGP0008:A|} { get; init; }
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
            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int {|ARGP0008:A|} { get; {{setterAccessibility}} init; }
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
            [OptionsType]
            class MyOptions
            {
                [Option]
                {{accessibility}} string {|ARGP0009:A|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoSetterOfOptionProperty_CanAddInitSetter()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string {|ARGP0010:A|} { get; }
            }
            """;

        var fixedSource = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AddSetAccessorCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task NoSetterOfOptionProperty_LanguageVersionLessThan9()
    {
        var source = """
            using ArgumentParsing;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string {|ARGP0010:A|} { get; }
            }
            """;

        var fixedSource = """
            using ArgumentParsing;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; set; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AddSetAccessorCodeFixProvider>(source, fixedSource, LanguageVersion.CSharp8);
    }

    [Fact]
    public async Task NoSetterOfOptionProperty_NoIsExternalInit()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string {|ARGP0010:A|} { get; }
            }
            """;

        var fixedSource = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; set; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AddSetAccessorCodeFixProvider>(source, fixedSource, LanguageVersion.Latest, ReferenceAssemblies.NetFramework.Net48.Default);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task TooLowAccessibilityOfASetterOfOptionProperty(string accessibility)
    {
        var source = $$"""
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; {{accessibility}} {|ARGP0010:set|}; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AddSetAccessorCodeFixProvider>(source, source);
    }

    [Fact]
    public async Task SuggestInitAccessorForParserProperty()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; {|ARGP0011:set|}; }
            }
            """;

        var fixedSource = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ChangeAccessorKindCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task DoNotSuggestInitAccessorForParserPropertyForLanguageVersionLessThan9()
    {
        var source = """
            using ArgumentParsing;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.CSharp8);
    }

    [Fact]
    public async Task DoNotSuggestInitAccessorForParserPropertyWhenNoIsExternalInit()
    {
        var source = """
            using ArgumentParsing;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.Latest, ReferenceAssemblies.NetFramework.Net48.Default);
    }

    [Fact]
    public async Task DoNotSuggestInitAccessorForParserPropertyWithDefaultValueWhenRuntimeDoesNotSupportUnsafeAccessor()
    {
        var source = """
            using ArgumentParsing;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; set; } = "Default";
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                internal static class IsExternalInit
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.Latest, ReferenceAssemblies.NetStandard.NetStandard20);
    }

    [Fact]
    public async Task InvalidShortName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('%')]
                public string {|ARGP0012:A|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortName_EmptyChar()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option({|CS1011:|}'')]
                public string A { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidLongName_FromPropertyName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('a')]
                public string {|ARGP0013:_A|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidLongName_FromAttribute()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option("my-long-$name$")]
                public string {|ARGP0013:A|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortAndLongName_FromPropertyName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('&')]
                public string {|ARGP0012:{|ARGP0013:_A|}|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidShortAndLongName_FromAttribute()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('^', "my-long-$name$")]
                public string {|ARGP0012:{|ARGP0013:A|}|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; init; }

                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            [OptionsType]
            partial class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; init; }
            }

            partial class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateShortName_ThreeDuplicates()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; init; }

                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; init; }

                [Option('o')]
                public string {|ARGP0014:OptionC|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; init; }

                [Option("option")]
                public string {|ARGP0015:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            [OptionsType]
            partial class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; init; }
            }

            partial class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateLongName_ThreeDuplicates()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option("option")]
                public string {|ARGP0015:A|} { get; init; }

                [Option("option")]
                public string {|ARGP0015:B|} { get; init; }

                [Option("option")]
                public string {|ARGP0015:C|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("char")]
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
    [InlineData("bool")]
    [InlineData("DayOfWeek")]
    [InlineData("DateTime")]
    [InlineData("TimeSpan")]
    [InlineData("DateOnly")]
    [InlineData("TimeOnly")]
    public async Task ValidOptionType(string validType)
    {
        var source = $$"""
            using System;
            using System.Numerics;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public {{validType}} Option { get; init; }
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
            [OptionsType]
            class MyOptions
            {
                [Option]
                public {|ARGP0016:{{invalidType}}|} OptionA { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidOptionType_ErrorType()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public {|CS0246:ErrorType|} OptionA { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SuggestUsingRequiredProperty_SeparateAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option]
                {|ARGP0017:[Required]|}
                public string Option { get; init; }
            }
            """;

        var fixedSource = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                public required string Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ChangeRequiredAttributeToRequiredPropertyCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task SuggestUsingRequiredProperty_SameAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option, {|ARGP0017:Required|}]
                public string Option { get; init; }
            }
            """;

        var fixedSource = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                public required string Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ChangeRequiredAttributeToRequiredPropertyCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task DoNotSuggestUsingRequiredPropertyForLanguageVersionLessThan11_SeparateAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                [Required]
                public string Option { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.CSharp10);
    }

    [Fact]
    public async Task DoNotSuggestUsingRequiredPropertyForLanguageVersionLessThan11_SameAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option, Required]
                public string Option { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.CSharp10);
    }

    [Fact]
    public async Task DoNotSuggestUsingRequiredPropertyWhenNoRequiredMemberAttribute_SeparateAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                [Required]
                public string Option { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.Latest, ReferenceAssemblies.NetFramework.Net48.Default);
    }

    [Fact]
    public async Task DoNotSuggestUsingRequiredPropertyWhenNoRequiredMemberAttribute_SameAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option, Required]
                public string Option { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source, LanguageVersion.Latest, ReferenceAssemblies.NetFramework.Net48.Default);
    }

    [Fact]
    public async Task UnnecessaryRequiredAttribute_SeparateAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option]
                {|ARGP0018:[Required]|}
                public required string Option { get; init; }
            }
            """;

        var fixedSource = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                public required string Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<RemoveUnnecessaryRequiredAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task UnnecessaryRequiredAttribute_SameAttributeList()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Option, {|ARGP0018:Required|}]
                public required string Option { get; init; }
            }
            """;

        var fixedSource = """
            using System.ComponentModel.DataAnnotations;
            
            [OptionsType]
            class MyOptions
            {
                [Option]
                public required string Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<RemoveUnnecessaryRequiredAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task RequiredBoolOption()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                public required bool {|ARGP0019:OptionA|} { get; init; }
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
            using System.Numerics;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required {{optionBaseType}}? {|ARGP0020:OptionA|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("IEnumerable", "string")]
    [InlineData("IReadOnlyCollection", "int")]
    [InlineData("IReadOnlyList", "char")]
    public async Task SuggestImmutableArrayForSequenceOptionType(string sequenceBaseType, string sequenceUnderlyingType)
    {
        var source = $$"""
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required {|ARGP0021:{{sequenceBaseType}}<{{sequenceUnderlyingType}}>|} Option { get; init; }
            }
            """;

        var fixedSource = $$"""
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required ImmutableArray<{{sequenceUnderlyingType}}> Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ChangePropertyTypeToImmutableArrayCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task NegativeParameterIndex()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(-1)]
                public string {|ARGP0022:Parameter|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; init; }

                [Parameter(0)]
                public string {|ARGP0023:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex_NotDefault()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(1)]
                public string {|ARGP0023:B|} { get; init; }

                [Parameter(1)]
                public string {|ARGP0023:C|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameter_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            [OptionsType]
            partial class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; init; }
            }

            partial class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateParameterIndex_ThreeDuplicates()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; init; }

                [Parameter(0)]
                public string {|ARGP0023:B|} { get; init; }

                [Parameter(0)]
                public string {|ARGP0023:C|} { get; init; }
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
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public {|ARGP0024:{{invalidType}}|} Parameter { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidParameterType_ErrorType()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public {|CS0246:ErrorType|} Parameter { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task MissingParameterWithIndex1()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(2)]
                public string B { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(1)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex2()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(1)]
                public string B { get; init; }

                [Parameter(3)]
                public string C { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(2)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex1And2()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(3)]
                public string B { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 2)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndex1And3()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(2)]
                public string B { get; init; }

                [Parameter(4)]
                public string C { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(1),
            DiagnosticResult
                .CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom1To3()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(4)]
                public string B { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom2To3()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(1)]
                public string B { get; init; }

                [Parameter(4)]
                public string C { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(2, 3)
        ]);
    }

    [Fact]
    public async Task MissingParameterWithIndexFrom1To2AndFrom4To5()
    {
        var source = """
            [OptionsType]
            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; init; }

                [Parameter(3)]
                public string B { get; init; }

                [Parameter(6)]
                public string C { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 2),
            DiagnosticResult
                .CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(4, 5)
        ]);
    }

    [Fact]
    public async Task InvalidRequiredParameter_RequiredProperty()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; init; }

                [Parameter(1)]
                public required string {|ARGP0027:Parameter2|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidRequiredParameter_RequiredAttribute()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; init; }

                [Parameter(1)]
                {|ARGP0017:[Required]|}
                public string {|ARGP0027:Parameter2|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task TwoInvalidRequiredParameters()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; init; }

                [Parameter(1)]
                public string Parameter2 { get; init; }

                [Parameter(2)]
                public required string {|ARGP0027:Parameter3|} { get; init; }

                [Parameter(3)]
                public required string {|ARGP0027:Parameter4|} { get; init; }
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
            [OptionsType]
            class MyOptions
            {
                [Parameter(0, Name = "{{invalidName}}")]
                public string {|ARGP0028:Param|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters()
    {
        var source = """
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:A|} { get; init; }

                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters_DuplicatesInDifferentPartialDeclarations()
    {
        var source = """
            using System.Collections.Immutable;

            [OptionsType]
            partial class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:A|} { get; init; }
            }

            partial class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:B|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DuplicateRemainingParameters_ThreeDuplicates()
    {
        var source = """
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:A|} { get; init; }

                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:B|} { get; init; }

                [RemainingParameters]
                public ImmutableArray<string> {|ARGP0029:C|} { get; init; }
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
            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|ARGP0030:{{invalidType}}|} RemainingParams { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidRemainingParametersType_ErrorType()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|CS0246:ErrorType|} OptionA { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("IEnumerable", "string")]
    [InlineData("IReadOnlyCollection", "int")]
    [InlineData("IReadOnlyList", "char")]
    public async Task SuggestImmutableArrayForRemainingParametersType(string sequenceBaseType, string sequenceUnderlyingType)
    {
        var source = $$"""
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|ARGP0021:{{sequenceBaseType}}<{{sequenceUnderlyingType}}>|} Option { get; init; }
            }
            """;

        var fixedSource = $$"""
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<{{sequenceUnderlyingType}}> Option { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<ChangePropertyTypeToImmutableArrayCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task RequiredRemainingParameters_RequiredProperty()
    {
        var source = """
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public {|ARGP0031:required|} ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        var fixedSource = """
            using System.Collections.Immutable;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<RemoveUnnecessaryRequiredKeywordCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task RequiredRemainingParameters_RequiredAttribute_SeparateAttributeList()
    {
        var source = """
            using System.Collections.Immutable;
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                {|ARGP0031:[Required]|}
                public ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        var fixedSource = """
            using System.Collections.Immutable;
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<RemoveUnnecessaryRequiredAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task RequiredRemainingParameters_RequiredAttribute_SameAttributeList()
    {
        var source = """
            using System.Collections.Immutable;
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters, {|ARGP0031:Required|}]
                public ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        var fixedSource = """
            using System.Collections.Immutable;
            using System.ComponentModel.DataAnnotations;

            [OptionsType]
            class MyOptions
            {
                [RemainingParameters]
                public ImmutableArray<string> RemainingParameters { get; init; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<RemoveUnnecessaryRequiredAttributeCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    public async Task TooLowOptionsTypeAccessibility(string accessibility)
    {
        var source = $$"""
            class C
            {
                [OptionsType]
                {{accessibility}} class {|ARGP0032:Options|}
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
    public async Task TooLowOptionsTypeAccessibility_NestedType(string containingTypeAccessibility)
    {
        var source = $$"""
            class Outer
            {
                {{containingTypeAccessibility}} class C
                {
                    [OptionsType]
                    public class {|ARGP0032:Options|}
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }


    [Fact]
    public async Task NoShortAndLongName()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option(null)]
                public string {|ARGP0033:Option|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task MultipleParserRelatedAttributes()
    {
        var source = """
            [OptionsType]
            class MyOptions
            {
                [Option]
                [Parameter(0)]
                public string {|ARGP0036:Prop1|} { get; init; }

                [Parameter(1)]
                [RemainingParameters]
                public string {|ARGP0036:Prop2|} { get; init; }

                [Option]
                [Parameter(2)]
                [RemainingParameters]
                public string {|ARGP0036:Prop3|} { get; init; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_Null()
    {
        var source = """
            [OptionsType, HelpTextGenerator({|#0:null|}, "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments("null")
        ]);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_Default()
    {
        var source = """
            [OptionsType, HelpTextGenerator({|#0:default|}, "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments("default")
        ]);
    }

    [Theory]
    [InlineData("int[]")]
    [InlineData("MyOptions[]")]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_NotNamedType(string notNamedType)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(typeof({|#0:{{notNamedType}}|}), "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments(notNamedType)
        ]);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("object")]
    [InlineData("string")]
    [InlineData("System.DateTime")]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_SpecialType(string specialType)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(typeof({|#0:{{specialType}}|}), "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments(specialType)
        ]);
    }

    [Theory]
    [InlineData("System.Collections.Generic.List<>")]
    [InlineData("System.Collections.Generic.Dictionary<,>")]
    [InlineData("System.Collections.Immutable.ImmutableArray<>")]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_OpenGeneric(string openGeneric)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(typeof({|#0:{{openGeneric}}|}), "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments(openGeneric)
        ]);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidTypeSpecifier_ErrorType()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof({|CS0246:ErrorType|}), "A")]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidMethodName_Null()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|#0:null|})]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0046")
                .WithLocation(0)
                .WithArguments("null")
        ]);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidMethodName_Default()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|#0:default|})]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0046")
                .WithLocation(0)
                .WithArguments("default")
        ]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("$name")]
    [InlineData("my name")]
    [InlineData("2name")]
    [InlineData("name&")]
    [InlineData("my-name")]
    public async Task HelpTextGenerator_InvalidMethodName(string invalidName)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|#0:"{{invalidName}}"|})]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0046")
                .WithLocation(0)
                .WithArguments(invalidName)
        ]);
    }

    [Fact]
    public async Task HelpTextGenerator_InvalidTypeSpecifierAndMethodName()
    {
        var source = """
            [OptionsType, HelpTextGenerator({|#0:null|}, {|#1:default|})]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0045")
                .WithLocation(0)
                .WithArguments("null"),
            DiagnosticResult
                .CompilerError("ARGP0046")
                .WithLocation(1)
                .WithArguments("default")
        ]);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_NoMethod()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_NotMethod()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"MyHelpText"|})]
            class MyOptions
            {
                public static string MyHelpText { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_NotStatic()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                public string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_NoParameters()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                public static string GenerateHelpText() => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_TooManyParameters()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection e1 = null, ParseErrorCollection e2 = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_NoDefaultValue()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_ReturnTypeNotString()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                public static void GenerateHelpText(ParseErrorCollection errors = null) { }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_NotAccessible(string accessibility)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
            class MyOptions
            {
                {{accessibility}} static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task HelpTextGenerator_UnableToFindMethod_InvalidSignature_NotAccessible_NestedType(string containingTypeAccessibility)
    {
        var source = $$"""
            class Outer
            {
                [OptionsType, HelpTextGenerator(typeof(MyOptions), {|ARGP0047:"GenerateHelpText"|})]
                {{containingTypeAccessibility}} class {|ARGP0032:MyOptions|}
                {
                    public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_Valid_InOptionsType()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task HelpTextGenerator_Valid_InOtherType()
    {
        var source = """
            [OptionsType, HelpTextGenerator(typeof(MyOptionsHelp), "GetHelpText")]
            class MyOptions
            {
            }

            class MyOptionsHelp
            {
                public static string GetHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ImplementISpanParsable()
    {
        var source = """
            using System.Collections.Immutable;
            using System.Diagnostics.CodeAnalysis;

            [OptionsType]
            class MyOptions
            {
                [Option]
                public MyParsable Option { get; init; }

                [Option]
                public ImmutableArray<MyParsable> SequenceOption { get; init; }

                [Parameter(0)]
                public MyParsable Parameter { get; init; }

                [RemainingParameters]
                public ImmutableArray<MyParsable> RemainingParameters { get; init; }
            }

            class {|#0:MyParsable|} : IParsable<MyParsable>
            {
                public static MyParsable Parse(string s, IFormatProvider? provider) => default;

                public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MyParsable result)
                {
                    result = default;
                    return false;
                }
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            new DiagnosticResult("ARGP0054", DiagnosticSeverity.Info)
                .WithLocation(0)
                .WithArguments("Option", "MyOptions", "MyParsable"),
            new DiagnosticResult("ARGP0054", DiagnosticSeverity.Info)
                .WithLocation(0)
                .WithArguments("SequenceOption", "MyOptions", "MyParsable"),
            new DiagnosticResult("ARGP0054", DiagnosticSeverity.Info)
                .WithLocation(0)
                .WithArguments("Parameter", "MyOptions", "MyParsable"),
            new DiagnosticResult("ARGP0054", DiagnosticSeverity.Info)
                .WithLocation(0)
                .WithArguments("RemainingParameters", "MyOptions", "MyParsable")
        ], referenceAssemblies: ReferenceAssemblies.Net.Net80);
    }

    [Theory]
    [InlineData("Option", "int", "3")]
    [InlineData("Parameter(0)", "string", "\"Test\"")]
    [InlineData("RemainingParameters", "System.Collections.Generic.IReadOnlyList<int>", "[1, 2, 3]")]
    public async Task DefaultValues_InitOnlyMemberWhenRuntimeDoesNotSupportUnsafeAccessor(string attributeContent, string propertyType, string initializerSyntax)
    {
        await VerifyAnalyzerWithCodeFixAsync<ChangeAccessorKindCodeFixProvider>($$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [{{attributeContent}}]
                public {{propertyType}} ParserProp { get; {|ARGP0055:init|}; } = {{initializerSyntax}};
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                internal static class IsExternalInit
                {
                }
            }
            """, $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                [{{attributeContent}}]
                public {{propertyType}} ParserProp { get; set; } = {{initializerSyntax}};
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                internal static class IsExternalInit
                {
                }
            }
            """, referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
    }
}
