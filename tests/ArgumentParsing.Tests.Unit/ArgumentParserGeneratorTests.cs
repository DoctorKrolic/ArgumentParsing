using ArgumentParsing.Generators;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed partial class ArgumentParserGeneratorTests
{
    [Fact]
    public async Task NoGeneratedParserAttribute()
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}();
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(int a, int b);
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(params string[] s);
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(ref string[] s);
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(scoped ReadOnlySpan<string> s);
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
                public partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(scoped ref string[] s);
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
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(this string[] s);
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
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({{invalidType}} s);
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
    public async Task EnsureEquivalentCodeGenForValidParameterTypes(string parameterType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> ParseArguments({{parameterType}} s);
            }
            """;

        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", GetMainCodeGenForEmptyOptions(parameterType, "s")), ("HelpCommandHandler.EmptyOptions.g.cs", HelpCodeGenForEmptyOptions), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
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
                public static partial {{invalidType}} {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
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
    [InlineData("ClassWithoutParameterlessConstructor")]
    [InlineData("ClassWithInaccessibleParameterlessConstructor")]
    public async Task InvalidOptionsType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{{invalidType}}> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class ClassWithoutParameterlessConstructor
            {
                public ClassWithoutParameterlessConstructor(int a)
                {
                }
            }

            [OptionsType]
            class ClassWithInaccessibleParameterlessConstructor
            {
                private ClassWithInaccessibleParameterlessConstructor()
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
    public async Task OptionsTypeNotAnnotatedWithOptionsTypeAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
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
                public static partial int {|CS8795:ParseArguments|}(string args);
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

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{fieldAccessibility}} required int a;
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
    public async Task OptionsType_RequiredField_TooLowFieldAccessibility(string optionsTypeAccessibility, string fieldAccessibility)
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

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_UnannotatedRequiredProperty_NoSetter()
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
    public async Task OptionsType_UnannotatedRequiredProperty_TooLowPropertyAccessibility(string optionsTypeAccessibility, string propertyAccessibility)
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
    public async Task OptionsType_UnannotatedRequiredProperty_TooLowSetterAccessibility(string optionsTypeAccessibility, string setterAccessibility)
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

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                {{propertyAccessibility}} required int A { get; set; }
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

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int A { get; {{setterAccessibility}} set; }
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

            [OptionsType]
            {{optionsTypeAccessibility}} class MyOptions
            {
                public required int A { get; {{setterAccessibility}} {|CS0273:set|}; }
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

            [OptionsType]
            class MyOptions
            {
                [Option]
                {{accessibility}} string A { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; }
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

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string A { get; {{accessibility}} set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option('%')]
                public string A { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidShortName_EmptyChar()
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

            [OptionsType]
            class MyOptions
            {
                [Option('a')]
                public string _A { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option("my-long-$name$")]
                public string A { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option('&')]
                public string _A { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option('^', "my-long-$name$")]
                public string A { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string OptionA { get; set; }

                [Option('o')]
                public string OptionB { get; set; }
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

            [OptionsType]
            partial class MyOptions
            {
                [Option('o')]
                public string OptionA { get; set; }
            }

            partial class MyOptions
            {
                [Option('o')]
                public string OptionB { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option('o')]
                public string OptionA { get; set; }

                [Option('o')]
                public string OptionB { get; set; }

                [Option('o')]
                public string OptionC { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option("option")]
                public string A { get; set; }

                [Option("option")]
                public string B { get; set; }
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

            [OptionsType]
            partial class MyOptions
            {
                [Option("option")]
                public string A { get; set; }
            }

            partial class MyOptions
            {
                [Option("option")]
                public string B { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option("option")]
                public string A { get; set; }

                [Option("option")]
                public string B { get; set; }

                [Option("option")]
                public string C { get; set; }
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

            [OptionsType]
            class MyOptions
            {
                [Option]
                public {{invalidType}} OptionA { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidOptionType_ErrorType()
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

            [OptionsType]
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

            namespace ArgumentParsing.Generated
            {
                internal static partial class ParseResultExtensions
                {
                    /// <summary>
                    /// Executes common default actions for the given <see cref="global::ArgumentParsing.Results.ParseResult{TOptions}"/>
                    /// <list type="bullet">
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedOptions"/> state invokes provided <paramref name="action"/> with parsed options object</item>
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedWithErrors"/> state writes help screen text with parse errors to <see cref="global::System.Console.Error"/> and exits application with code 1</item>
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedSpecialCommand"/> state executes parsed handler and exits application with code, returned from the handler</item>
                    /// </list>
                    /// </summary>
                    /// <param name="result">Parse result</param>
                    /// <param name="action">Action, which will be invoked if options type is correctly parsed</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                    public static void ExecuteDefaults(this global::ArgumentParsing.Results.ParseResult<global::MyOptions> result, global::System.Action<global::MyOptions> action)
                    {
                        switch (result.State)
                        {
                            case global::ArgumentParsing.Results.ParseResultState.ParsedOptions:
                                action(result.Options);
                                break;
                            case global::ArgumentParsing.Results.ParseResultState.ParsedWithErrors:
                                string errorScreenText = global::ArgumentParsing.Generated.HelpCommandHandler_MyOptions.GenerateHelpText(result.Errors);
                                global::System.Console.Error.WriteLine(errorScreenText);
                                global::System.Environment.Exit(1);
                                break;
                            case global::ArgumentParsing.Results.ParseResultState.ParsedSpecialCommand:
                                int exitCode = result.SpecialCommandHandler.HandleCommand();
                                global::System.Environment.Exit(exitCode);
                                break;
                        }
                    }
                }
            }

            partial class C
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{integerType}} Option_val = default({{integerType}});

                    int state = -3;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        if (state == -3)
                        {
                            switch (arg)
                            {
                                case "--help":
                                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(new global::ArgumentParsing.Generated.HelpCommandHandler_MyOptions());
                                case "--version":
                                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(new global::ArgumentParsing.Generated.VersionCommandHandler());
                            }

                            state = 0;
                        }

                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (state != -2)
                        {
                            if (arg.StartsWith("--") && (hasLetters || arg.Length == 2 || arg.Contains('=')))
                            {
                                global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                                int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                                latestOptionName = slice[longArgSplit[0]];
                                switch (latestOptionName)
                                {
                                    case "":
                                        if (written == 1)
                                        {
                                            state = -2;
                                        }
                                        else
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                        }
                                        continue;
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

        var helpCommandHandler = """
            // <auto-generated/>
            #nullable disable
            #pragma warning disable

            namespace ArgumentParsing.Generated
            {
                /// <summary>
                /// Default implementation of <c>--help</c> command for <see cref="global::MyOptions"/> type
                /// </summary>
                [global::ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute("--help")]
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                internal sealed class HelpCommandHandler_MyOptions : global::ArgumentParsing.SpecialCommands.ISpecialCommandHandler
                {
                    /// <summary>
                    /// Generates help text for <see cref="global::MyOptions"/> type.
                    /// If <paramref name="errors"/> parameter is supplied, generated text will contain an error section
                    /// </summary>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine();
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                        }
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("OPTIONS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("COMMANDS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --help\tShow help screen");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --version\tShow version information");
                        return helpBuilder.ToString();
                    }

                    /// <inheritdoc/>
                    public int HandleCommand()
                    {
                        global::System.Console.Out.WriteLine(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated), ("HelpCommandHandler.MyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
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

            [OptionsType]
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

            namespace ArgumentParsing.Generated
            {
                internal static partial class ParseResultExtensions
                {
                    /// <summary>
                    /// Executes common default actions for the given <see cref="global::ArgumentParsing.Results.ParseResult{TOptions}"/>
                    /// <list type="bullet">
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedOptions"/> state invokes provided <paramref name="action"/> with parsed options object</item>
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedWithErrors"/> state writes help screen text with parse errors to <see cref="global::System.Console.Error"/> and exits application with code 1</item>
                    /// <item>If <paramref name="result"/> is in <see cref="global::ArgumentParsing.Results.ParseResultState.ParsedSpecialCommand"/> state executes parsed handler and exits application with code, returned from the handler</item>
                    /// </list>
                    /// </summary>
                    /// <param name="result">Parse result</param>
                    /// <param name="action">Action, which will be invoked if options type is correctly parsed</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                    public static void ExecuteDefaults(this global::ArgumentParsing.Results.ParseResult<global::MyOptions> result, global::System.Action<global::MyOptions> action)
                    {
                        switch (result.State)
                        {
                            case global::ArgumentParsing.Results.ParseResultState.ParsedOptions:
                                action(result.Options);
                                break;
                            case global::ArgumentParsing.Results.ParseResultState.ParsedWithErrors:
                                string errorScreenText = global::ArgumentParsing.Generated.HelpCommandHandler_MyOptions.GenerateHelpText(result.Errors);
                                global::System.Console.Error.WriteLine(errorScreenText);
                                global::System.Environment.Exit(1);
                                break;
                            case global::ArgumentParsing.Results.ParseResultState.ParsedSpecialCommand:
                                int exitCode = result.SpecialCommandHandler.HandleCommand();
                                global::System.Environment.Exit(exitCode);
                                break;
                        }
                    }
                }
            }

            partial class C
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                private static partial global::ArgumentParsing.Results.ParseResult<global::MyOptions> ParseArguments(string[] args)
                {
                    {{floatType}} Option_val = default({{floatType}});

                    int state = -3;
                    int seenOptions = 0;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);
                    string previousArgument = null;

                    foreach (string arg in args)
                    {
                        if (state == -3)
                        {
                            switch (arg)
                            {
                                case "--help":
                                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(new global::ArgumentParsing.Generated.HelpCommandHandler_MyOptions());
                                case "--version":
                                    return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(new global::ArgumentParsing.Generated.VersionCommandHandler());
                            }

                            state = 0;
                        }

                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

                        if (state > 0 && startsOption)
                        {
                            errors ??= new();
                            errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));
                            state = 0;
                        }

                        if (state != -2)
                        {
                            if (arg.StartsWith("--") && (hasLetters || arg.Length == 2 || arg.Contains('=')))
                            {
                                global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);
                                int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');

                                latestOptionName = slice[longArgSplit[0]];
                                switch (latestOptionName)
                                {
                                    case "":
                                        if (written == 1)
                                        {
                                            state = -2;
                                        }
                                        else
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                        }
                                        continue;
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

        var helpCommandHandler = $$"""
            // <auto-generated/>
            #nullable disable
            #pragma warning disable

            namespace ArgumentParsing.Generated
            {
                /// <summary>
                /// Default implementation of <c>--help</c> command for <see cref="global::MyOptions"/> type
                /// </summary>
                [global::ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute("--help")]
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                internal sealed class HelpCommandHandler_MyOptions : global::ArgumentParsing.SpecialCommands.ISpecialCommandHandler
                {
                    /// <summary>
                    /// Generates help text for <see cref="global::MyOptions"/> type.
                    /// If <paramref name="errors"/> parameter is supplied, generated text will contain an error section
                    /// </summary>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine();
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                        }
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("OPTIONS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("COMMANDS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --help\tShow help screen");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --version\tShow version information");
                        return helpBuilder.ToString();
                    }

                    /// <inheritdoc/>
                    public int HandleCommand()
                    {
                        global::System.Console.Out.WriteLine(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated), ("HelpCommandHandler.MyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
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

            [OptionsType]
            class MyOptions
            {
                [Option]
                public required bool OptionA { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task OptionsType_TooLowAccessibilityOfParameterProperty(string accessibility)
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
                {{accessibility}} string A { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_NoSetterOfParameterProperty()
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
                public string A { get; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task OptionsType_TooLowAccessibilityOfASetterOfParameterProperty(string accessibility)
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
                public string A { get; {{accessibility}} set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_NegativeParameterIndex()
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
                public string Parameter { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateParameterIndex()
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

                [Parameter(0)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateParameterIndex_NotDefault()
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
                public string B { get; set; }

                [Parameter(1)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateParameter_DuplicatesInDifferentPartialDeclarations()
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
                public string A { get; set; }
            }

            partial class MyOptions
            {
                [Parameter(0)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateParameterIndex_ThreeDuplicates()
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

                [Parameter(0)]
                public string B { get; set; }

                [Parameter(0)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    [InlineData("System.Collections.Generic.IEnumerable<string>")]
    public async Task OptionsType_InvalidParameterType(string invalidType)
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
                public {{invalidType}} Parameter { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidParameterType_ErrorType()
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

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndex1()
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

                [Parameter(2)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndex2()
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
                public string B { get; set; }

                [Parameter(3)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndex1And2()
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

                [Parameter(3)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndex1And3()
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

                [Parameter(2)]
                public string B { get; set; }

                [Parameter(4)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndexFrom1To3()
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

                [Parameter(4)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndexFrom2To3()
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
                public string B { get; set; }

                [Parameter(4)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MissingParameterWithIndexFrom1To2AndFrom4To5()
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

                [Parameter(3)]
                public string B { get; set; }

                [Parameter(6)]
                public string C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidRequiredParameter_RequiredProperty()
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
                public required string Parameter2 { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidRequiredParameter_RequiredAttribute()
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
                public string Parameter2 { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_TwoInvalidRequiredParameters()
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
                public required string Parameter3 { get; set; }

                [Parameter(3)]
                public required string Parameter4 { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("5param")]
    [InlineData("-param")]
    [InlineData("$param")]
    [InlineData("param name")]
    [InlineData("invalid&name")]
    public async Task OptionsType_InvalidParameterName(string invalidName)
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
                public string Param { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateRemainingParameters()
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
                public IEnumerable<string> A { get; set; }

                [RemainingParameters]
                public IEnumerable<string> B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateRemainingParameters_DuplicatesInDifferentPartialDeclarations()
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
                public IEnumerable<string> A { get; set; }
            }

            partial class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_DuplicateRemainingParameters_ThreeDuplicates()
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
                public IEnumerable<string> A { get; set; }

                [RemainingParameters]
                public IEnumerable<string> B { get; set; }

                [RemainingParameters]
                public IEnumerable<string> C { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("int?")]
    [InlineData("string")]
    [InlineData("object")]
    [InlineData("dynamic")]
    [InlineData("MyOptions")]
    [InlineData("System.Collections.Generic.IEnumerable<int?>")]
    public async Task OptionsType_InvalidRemainingParametersType(string invalidType)
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
                public {{invalidType}} RemainingParams { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidRemainingParametersType_ErrorType()
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

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_NoShortAndLongName()
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
                public string Option { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_MultipleParserRelatedAttributes()
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
                [Parameter(0)]
                public string Prop1 { get; set; }

                [Parameter(1)]
                [RemainingParameters]
                public string Prop2 { get; set; }

                [Option]
                [Parameter(2)]
                [RemainingParameters]
                public string Prop3 { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("default")]
    [InlineData("default(System.Type)")]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_Default(string defaultSyntax)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = {{defaultSyntax}})]
                public static partial ParseResult<EmptyOptions> ParseArguments(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", GetMainCodeGenForEmptyOptions("string[]", "args")), ("HelpCommandHandler.EmptyOptions.g.cs", HelpCodeGenForEmptyOptions), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    [Fact]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_InvalidValue()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = {|CS0029:5|})]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof({|CS0246:ErrorType|}))]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_Invalid_NotNamedType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof({|#0:C[]|}))]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_Invalid_SpecialType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof({|#0:int|}))]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
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
                    global using ArgumentParsing.Results.Errors;
                    global using ArgumentParsing.SpecialCommands;
                    global using ArgumentParsing.SpecialCommands.Help;
                    global using System;

                    [OptionsType]
                    class EmptyOptions { }
                    """
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
            test.TestState.GeneratedSources.Add((typeof(ArgumentParserGenerator), hint, content.Replace("<GENERATOR_ASSEMBLY_VERSION>", '"' + typeof(ArgumentParserGenerator).Assembly.GetName().Version.ToString() + '"')));
        }

        await test.RunAsync();
    }

    private const string VersionCommandHander = """
        // <auto-generated/>
        #nullable disable
        #pragma warning disable
        
        namespace ArgumentParsing.Generated
        {
            /// <summary>
            /// Default implementation of <c>--version</c> command for <c>TestProject</c> assembly
            /// </summary>
            [global::ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute("--version")]
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
            [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
            internal sealed class VersionCommandHandler : global::ArgumentParsing.SpecialCommands.ISpecialCommandHandler
            {
                /// <inheritdoc/>
                public int HandleCommand()
                {
                    global::System.Console.WriteLine("TestProject 0.0.0");
                    return 0;
                }
            }
        }
        """;
}
