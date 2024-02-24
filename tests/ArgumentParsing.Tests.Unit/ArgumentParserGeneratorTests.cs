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
    public async Task InvalidParameterType_ErrorType_NoCustomDiagnostics()
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
                    public static void ExecuteDefaults(this global::ArgumentParsing.Results.ParseResult<global::EmptyOptions> result, global::System.Action<global::EmptyOptions> action)
                    {
                        switch (result.State)
                        {
                            case global::ArgumentParsing.Results.ParseResultState.ParsedOptions:
                                action(result.Options);
                                break;
                            case global::ArgumentParsing.Results.ParseResultState.ParsedWithErrors:
                                string errorScreenText = global::ArgumentParsing.Generated.HelpCommandHandler_EmptyOptions.GenerateHelpText(result.Errors);
                                global::System.Console.Error.Write(errorScreenText);
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
                public static partial global::ArgumentParsing.Results.ParseResult<global::EmptyOptions> ParseArguments({{parameterType}} s)
                {

                    int state = -3;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);

                    foreach (string arg in s)
                    {
                        if (state == -3)
                        {
                            switch (arg)
                            {
                                case "--help":
                                    return new global::ArgumentParsing.Results.ParseResult<global::EmptyOptions>(new global::ArgumentParsing.Generated.HelpCommandHandler_EmptyOptions());
                                case "--version":
                                    return new global::ArgumentParsing.Results.ParseResult<global::EmptyOptions>(new global::ArgumentParsing.Generated.VersionCommandHandler());
                            }

                            state = 0;
                        }

                        global::System.ReadOnlySpan<char> val;

                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

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
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        ;
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

        var helpCommandHandler = """
            // <auto-generated/>
            #nullable disable
            #pragma warning disable

            namespace ArgumentParsing.Generated
            {
                /// <summary>
                /// Default implementation of <c>--help</c> command for <see cref="global::EmptyOptions"/> type
                /// </summary>
                [global::ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute("--help")]
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                internal sealed class HelpCommandHandler_EmptyOptions : global::ArgumentParsing.SpecialCommands.ISpecialCommandHandler
                {
                    /// <summary>
                    /// Generates help text for <see cref="global::EmptyOptions"/> type.
                    /// If <paramref name="errors"/> parameter is supplied, generated text will contain an error section
                    /// </summary>
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
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
                        global::System.Console.Out.Write(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", generated), ("HelpCommandHandler.EmptyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
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

    [Fact]
    public async Task OptionsType_InvalidOptionType_ErrorType_NoCustomDiagnostics()
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
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
                        global::System.Console.Out.Write(GenerateHelpText());
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
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
                        global::System.Console.Out.Write(GenerateHelpText());
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    {{integerType}}? OptionA_val = default({{integerType}}?);

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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
                        helpBuilder.AppendLine("OPTIONS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option-a");
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
                        global::System.Console.Out.Write(GenerateHelpText());
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    {{floatType}}? OptionA_val = default({{floatType}}?);

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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
                        helpBuilder.AppendLine("OPTIONS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option-a");
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
                        global::System.Console.Out.Write(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated), ("HelpCommandHandler.MyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    [Fact]
    public async Task OptionsType_RequiredNullableFlagEnumAndCharOptions()
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
                public required bool? {|ARGP0020:OptionA|} { get; set; }

                [Option]
                public required DayOfWeek? {|ARGP0020:OptionB|} { get; set; }

                [Option]
                public required char? {|ARGP0020:OptionC|} { get; set; }
            }
            """;

        var generated = """
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    bool? OptionA_val = default(bool?);
                    global::System.DayOfWeek? OptionB_val = default(global::System.DayOfWeek?);
                    char? OptionC_val = default(char?);

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
                                    case "option-a":
                                        if ((seenOptions & 0b001) > 0)
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-a"));
                                        }
                                        OptionA_val = true;
                                        state = -2147483648;
                                        seenOptions |= 0b001;
                                        break;
                                    case "option-b":
                                        if ((seenOptions & 0b010) > 0)
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-b"));
                                        }
                                        state = 2;
                                        seenOptions |= 0b010;
                                        break;
                                    case "option-c":
                                        if ((seenOptions & 0b100) > 0)
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError("option-c"));
                                        }
                                        state = 3;
                                        seenOptions |= 0b100;
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
                                            if (state <= -10)
                                            {
                                                val = slice.Slice(i);
                                                latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i - 1]);
                                                goto decodeValue;
                                            }
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
                            case -2147483648:
                                bool OptionA_underlying = default(bool);
                                if (!bool.TryParse(val, out OptionA_underlying))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                OptionA_val = OptionA_underlying;
                                break;
                            case 2:
                                global::System.DayOfWeek OptionB_underlying = default(global::System.DayOfWeek);
                                if (!global::System.Enum.TryParse<global::System.DayOfWeek>(val, out OptionB_underlying))
                                {
                                    errors ??= new();
                                    errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));
                                }
                                OptionB_val = OptionB_underlying;
                                break;
                            case 3:
                                if (val.Length == 1)
                                {
                                    OptionC_val = val[0];
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

                    if ((seenOptions & 0b001) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-a"));
                    }

                    if ((seenOptions & 0b010) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-b"));
                    }

                    if ((seenOptions & 0b100) == 0)
                    {
                        errors ??= new();
                        errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError("option-c"));
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        OptionA = OptionA_val,
                        OptionB = OptionB_val,
                        OptionC = OptionC_val,
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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
                        helpBuilder.AppendLine("OPTIONS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option-a");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option-b");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  --option-c");
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
                        global::System.Console.Out.Write(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated), ("HelpCommandHandler.MyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
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

            class MyOptions
            {
                [Parameter(0)]
                {{accessibility}} string {|ARGP0009:A|} { get; set; }
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

            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0010:A|} { get; }
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

            class MyOptions
            {
                [Parameter(0)]
                public string A { get; {{accessibility}} {|ARGP0010:set|}; }
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

            class MyOptions
            {
                [Parameter(-1)]
                public string {|ARGP0022:Parameter|} { get; set; }
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

            class MyOptions
            {
                [Parameter(0)]
                public string {|ARGP0023:A|} { get; set; }

                [Parameter(0)]
                public string {|ARGP0023:B|} { get; set; }
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

            class MyOptions
            {
                [Parameter(0)]
                public {|ARGP0024:{{invalidType}}|} Parameter { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidParameterType_ErrorType_NoCustomDiagnostics()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

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

            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(2)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(1)
        ]);
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

        await VerifyGeneratorAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0025")
                .WithLocation(0)
                .WithArguments(2)
        ]);
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

            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(3)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 2)
        ]);
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

        await VerifyGeneratorAsync(source,
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
    public async Task OptionsType_MissingParameterWithIndexFrom1To3()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class {|#0:MyOptions|}
            {
                [Parameter(0)]
                public string A { get; set; }

                [Parameter(4)]
                public string B { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(1, 3)
        ]);
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

        await VerifyGeneratorAsync(source,
        [
            DiagnosticResult.CompilerError("ARGP0026")
                .WithLocation(0)
                .WithArguments(2, 3)
        ]);
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

        await VerifyGeneratorAsync(source,
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
    public async Task OptionsType_InvalidRequiredParameter_RequiredProperty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; set; }

                [Parameter(1)]
                public required string {|ARGP0027:Parameter2|} { get; set; }
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

            class MyOptions
            {
                [Parameter(0)]
                public string Parameter1 { get; set; }

                [Parameter(1)]
                [Required]
                public string {|ARGP0027:Parameter2|} { get; set; }
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

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidNullableParameter()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Parameter(0)]
                public int? Parameter1 { get; set; }

                [Parameter(1)]
                public int {|ARGP0028:Parameter2|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_TwoInvalidNullableParameters()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Parameter(0)]
                public int? Parameter1 { get; set; }

                [Parameter(1)]
                public int {|ARGP0028:Parameter2|} { get; set; }

                [Parameter(2)]
                public int {|ARGP0028:Parameter3|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidNullableParameter_ReferenceType_NoDiagnostic()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> ParseArguments(string[] args);
            }

            class MyOptions
            {
                [Parameter(0)]
                public int? Parameter1 { get; set; }

                [Parameter(1)]
                public string Parameter2 { get; set; }
            }
            """;

        var generated = """
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
                                global::System.Console.Error.Write(errorScreenText);
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
                    int? Parameter1_val = default(int?);
                    string Parameter2_val = default(string);

                    int state = -3;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);
                    int parameterIndex = 0;

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
                            default:
                                switch (parameterIndex++)
                                {
                                    case 0:
                                        int Parameter1_underlying = default(int);
                                        if (!int.TryParse(val, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out Parameter1_underlying))
                                        {
                                            errors ??= new();
                                            errors.Add(new global::ArgumentParsing.Results.Errors.BadParameterValueFormatError(arg, "parameter1", parameterIndex - 1));
                                        }
                                        Parameter1_val = Parameter1_underlying;
                                        break;
                                    case 1:
                                        Parameter2_val = arg;
                                        break;
                                    default:
                                        errors ??= new();
                                        errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                        break;
                                }
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        ;
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::MyOptions>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::MyOptions options = new global::MyOptions
                    {
                        Parameter1 = Parameter1_val,
                        Parameter2 = Parameter2_val,
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
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("PARAMETERS:");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  parameter1 (at index 0)");
                        helpBuilder.AppendLine();
                        helpBuilder.AppendLine("  parameter2 (at index 1)");
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
                        global::System.Console.Out.Write(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("MyOptions.g.cs", generated), ("HelpCommandHandler.MyOptions.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    [Fact]
    public async Task OptionsType_InvalidNullableParameter_ReferenceTypeInBetween()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Parameter(0)]
                public int? Parameter1 { get; set; }

                [Parameter(1)]
                public string Parameter2 { get; set; }

                [Parameter(2)]
                public int {|ARGP0028:Parameter3|} { get; set; }
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

            class MyOptions
            {
                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:A|} { get; set; }

                [RemainingParameters]
                public IEnumerable<string> {|ARGP0029:B|} { get; set; }
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

            class MyOptions
            {
                [RemainingParameters]
                public {|ARGP0030:{{invalidType}}|} RemainingParams { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task OptionsType_InvalidRemainingParametersType_ErrorType_NoCustomDiagnostics()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [RemainingParameters]
                public {|CS0246:ErrorType|} OptionA { get; set; }
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
    public async Task OptionsType_InvalidParametersName(string invalidName)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
                [Parameter(0, Name = "{{invalidName}}")]
                public string {|ARGP0032:Param|} { get; set; }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    public async Task OptionsType_TooLowOptionsTypeAccessibility(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                private static partial ParseResult<Options> ParseArguments(string[] args);

                {{accessibility}} class {|ARGP0033:Options|}
                {
                }
            }
            """;

        var generated = """
            // <auto-generated/>
            #nullable disable
            #pragma warning disable

            partial class C
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                private static partial global::ArgumentParsing.Results.ParseResult<global::C.Options> ParseArguments(string[] args)
                {

                    int state = -3;
                    global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                    global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                    global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);

                    foreach (string arg in args)
                    {
                        if (state == -3)
                        {
                            switch (arg)
                            {
                                case "--help":
                                    return new global::ArgumentParsing.Results.ParseResult<global::C.Options>(new global::ArgumentParsing.Generated.HelpCommandHandler_C_Options());
                                case "--version":
                                    return new global::ArgumentParsing.Results.ParseResult<global::C.Options>(new global::ArgumentParsing.Generated.VersionCommandHandler());
                            }

                            state = 0;
                        }

                        global::System.ReadOnlySpan<char> val;
            
                        bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);
                        bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');

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
                            default:
                                errors ??= new();
                                errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));
                                break;
                        }

                        state = 0;

                    continueMainLoop:
                        ;
                    }

                    if (errors != null)
                    {
                        return new global::ArgumentParsing.Results.ParseResult<global::C.Options>(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));
                    }

                    global::C.Options options = new global::C.Options
                    {
                    };

                    return new global::ArgumentParsing.Results.ParseResult<global::C.Options>(options);
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
                /// Default implementation of <c>--help</c> command for <see cref="global::C.Options"/> type
                /// </summary>
                [global::ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute("--help")]
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ArgumentParsing.Generators.ArgumentParserGenerator", <GENERATOR_ASSEMBLY_VERSION>)]
                [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
                internal sealed class HelpCommandHandler_C_Options : global::ArgumentParsing.SpecialCommands.ISpecialCommandHandler
                {
                    /// <summary>
                    /// Generates help text for <see cref="global::C.Options"/> type.
                    /// If <paramref name="errors"/> parameter is supplied, generated text will contain an error section
                    /// </summary>
                    /// <remarks>
                    /// Help text, produced by this method, already contains a trailing new line.
                    /// In order to avoid double trailing new line problem, use <c>Write</c> method instead of <c>WriteLine</c> when writing this text to console or other text writers
                    /// </remarks>
                    /// <param name="errors">Parse errors to include into help text</param>
                    /// <returns>Generated help text with included trailing new line</returns>
                    public static string GenerateHelpText(global::ArgumentParsing.Results.Errors.ParseErrorCollection? errors = null)
                    {
                        global::System.Text.StringBuilder helpBuilder = new();
                        helpBuilder.AppendLine("TestProject 0.0.0");
                        helpBuilder.AppendLine("Copyright (C) " + global::System.DateTime.UtcNow.Year.ToString());
                        helpBuilder.AppendLine();
                        if ((object)errors != null)
                        {
                            helpBuilder.AppendLine("ERROR(S):");
                            foreach (global::ArgumentParsing.Results.Errors.ParseError error in errors)
                            {
                                helpBuilder.AppendLine("  " + error.GetMessage());
                            }
                            helpBuilder.AppendLine();
                        }
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
                        global::System.Console.Out.Write(GenerateHelpText());
                        return 0;
                    }
                }
            }
            """;

        await VerifyGeneratorAsync(source, ("C.Options.g.cs", generated), ("HelpCommandHandler.C.Options.g.cs", helpCommandHandler), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    private static async Task VerifyGeneratorAsync(string source, DiagnosticResult[] diagnostics)
        => await VerifyGeneratorAsync(source, diagnostics, []);

    private static async Task VerifyGeneratorAsync(string source, params (string Hint, string Content)[] generatedDocuments)
        => await VerifyGeneratorAsync(source, [], generatedDocuments);

    private static async Task VerifyGeneratorAsync(string source, DiagnosticResult[] diagnostics, (string Hint, string Content)[] generatedDocuments)
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

        test.TestState.ExpectedDiagnostics.AddRange(diagnostics);

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
