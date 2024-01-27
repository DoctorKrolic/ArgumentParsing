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

    [Theory]
    [InlineData("string[]")]
    [InlineData("global::System.Span<string>")]
    [InlineData("global::System.ReadOnlySpan<string>")]
    [InlineData("global::System.Collections.Generic.IEnumerable<string>")]
    [InlineData("global::System.Collections.Generic.ICollection<string>")]
    [InlineData("global::System.Collections.Generic.IList<string>")]
    [InlineData("global::System.Collections.Generic.List<string>")]
    [InlineData("global::System.Collections.Generic.HashSet<string>")]
    public async Task SuggestChangingParameterNameToArgs(string parameterType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> ParseArguments({{parameterType}} {|ARGP0004:s|});
            }
            """;

        var generated = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                public static partial global::ArgumentParsing.Results.ParseResult<global::EmptyOptions> ParseArguments({{parameterType}} s)
                {

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in s)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::EmptyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::EmptyOptions options = new global::EmptyOptions
                    {
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::EmptyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", generated));
    }

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
    public async Task OptionsType_InvalidShortName()
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
    public async Task OptionsType_InvalidShortName_FromAttribute_EmptyChar_NoCustomDiagnostics()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Option({|CS1011:|}'')]
                public string A { get; set; }
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
                [Option('&')]
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
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option('o')]
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
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }
            }

            partial class MyOptions
            {
                [Option('o')]
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
                [Option('o')]
                public string {|ARGP0014:OptionA|} { get; set; }

                [Option('o')]
                public string {|ARGP0014:OptionB|} { get; set; }

                [Option('o')]
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

    [Theory]
    [InlineData("byte")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("ushort")]
    [InlineData("int")]
    [InlineData("uint")]
    [InlineData("long")]
    [InlineData("ulong")]
    [InlineData("global::System.Numerics.BigInteger")]
    public async Task EnsureEquivalentCodeGenForIntegerTypes(string integerType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Option]
                public {{integerType}} Option { get; set; }
            }
            """;

        var generated = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{integerType}} Option_val = default({{integerType}});

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                case "option":
                                    if ((seenOptions & 0b1) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option"));
                                    }
                                    state = 1;
                                    seenOptions |= 0b1;
                                    break;
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            case 1:
                                if (!{{integerType}}.TryParse(val, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out Option_val))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        Option = Option_val,
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated));
    }

    [Theory]
    [InlineData("float")]
    [InlineData("double")]
    [InlineData("decimal")]
    public async Task EnsureEquivalentCodeGenForFloatTypes(string floatType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Option]
                public {{floatType}} Option { get; set; }
            }
            """;

        var generated = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{floatType}} Option_val = default({{floatType}});

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                case "option":
                                    if ((seenOptions & 0b1) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option"));
                                    }
                                    state = 1;
                                    seenOptions |= 0b1;
                                    break;
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            case 1:
                                if (!{{floatType}}.TryParse(val, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, global::System.Globalization.CultureInfo.InvariantCulture, out Option_val))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        Option = Option_val,
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated));
    }

    [Fact]
    public async Task OptionsType_RequiredBoolOption()
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
                public required bool {|ARGP0019:OptionA|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
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
    [InlineData("global::System.Numerics.BigInteger")]
    public async Task OptionsType_RequiredNullableIntegerOption(string integerType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Option]
                public required {{integerType}}? {|ARGP0020:OptionA|} { get; set; }
            }
            """;

        var generated = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{integerType}}? OptionA_val = default({{integerType}}?);

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                case "option-a":
                                    if ((seenOptions & 0b1) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-a"));
                                    }
                                    state = 1;
                                    seenOptions |= 0b1;
                                    break;
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            case 1:
                                {{integerType}} OptionA_underlying = default({{integerType}});
                                if (!{{integerType}}.TryParse(val, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out OptionA_underlying))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                OptionA_val = OptionA_underlying;
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if ((seenOptions & 0b1) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-a"));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        OptionA = OptionA_val,
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated));
    }

    [Theory]
    [InlineData("float")]
    [InlineData("double")]
    [InlineData("decimal")]
    public async Task OptionsType_RequiredNullableFloatOption(string floatType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Option]
                public required {{floatType}}? {|ARGP0020:OptionA|} { get; set; }
            }
            """;

        var generated = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{floatType}}? OptionA_val = default({{floatType}}?);

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                case "option-a":
                                    if ((seenOptions & 0b1) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-a"));
                                    }
                                    state = 1;
                                    seenOptions |= 0b1;
                                    break;
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            case 1:
                                {{floatType}} OptionA_underlying = default({{floatType}});
                                if (!{{floatType}}.TryParse(val, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, global::System.Globalization.CultureInfo.InvariantCulture, out OptionA_underlying))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                OptionA_val = OptionA_underlying;
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if ((seenOptions & 0b1) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-a"));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        OptionA = OptionA_val,
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated));
    }

    [Fact]
    public async Task OptionsType_RequiredNullableEnumAndCharOptions()
    {
        var source = $$"""
            using System;

            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Option]
                public required DayOfWeek? {|ARGP0020:OptionA|} { get; set; }

                [Option]
                public required char? {|ARGP0020:OptionB|} { get; set; }
            }
            """;

        var generated = """
            // <auto-generated/>
            #nullable disable
            #pragma warning disable
            
            partial class C
            {
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    global::System.DayOfWeek? OptionA_val = default(global::System.DayOfWeek?);
                    char? OptionB_val = default(char?);

                    int state = 0;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (hasLetters && arg.StartsWith("--"))
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                            int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                            latestOptionName = slice[longArgSplit[0]];
                            switch (latestOptionName)
                            {
                                case "option-a":
                                    if ((seenOptions & 0b01) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-a"));
                                    }
                                    state = 1;
                                    seenOptions |= 0b01;
                                    break;
                                case "option-b":
                                    if ((seenOptions & 0b10) > 0)
                                    {
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-b"));
                                    }
                                    state = 2;
                                    seenOptions |= 0b10;
                                    break;
                                default:
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));
                                    if (written == 1)
                                    {
                                        state = -1;
                                    }
                                    goto continueMainLoop;
                            }

                            if (written == 2)
                            {
                                val = slice[longArgSplit[1]];
                                goto decodeValue;
                            }

                            goto continueMainLoop;
                        }

                        if (startsOption)
                        {
                            global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);

                            for (int i = 0; i < slice.Length; i++)
                            {
                                if (state > 0)
                                {
                                    val = slice.Slice(i);
                                    goto decodeValue;
                                }

                                char shortOptionName = slice[i];
                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);
                                switch (shortOptionName)
                                {
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));
                                        state = -1;
                                        goto continueMainLoop;
                                }
                            }

                            goto continueMainLoop;
                        }

                        val = global::System.MemoryExtensions.AsSpan(arg);

                    decodeValue:
                        switch (state)
                        {
                            case -1:
                                break;
                            case 1:
                                global::System.DayOfWeek OptionA_underlying = default(global::System.DayOfWeek);
                                if (!global::System.Enum.TryParse<global::System.DayOfWeek>(val, out OptionA_underlying))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                OptionA_val = OptionA_underlying;
                                break;
                            case 2:
                                if (val.Length == 1)
                                {
                                    OptionB_val = val[0];
                                }
                                else
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                break;
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        previousArgument = arg;
                    }

                    if (state > 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                    }

                    if ((seenOptions & 0b01) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-a"));
                    }

                    if ((seenOptions & 0b10) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-b"));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        OptionA = OptionA_val,
                        OptionB = OptionB_val,
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(options);
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated));
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
