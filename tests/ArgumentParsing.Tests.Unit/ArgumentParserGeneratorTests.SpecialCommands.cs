namespace ArgumentParsing.Tests.Unit;

public partial class ArgumentParserGeneratorTests
{
    [Fact]
    public async Task SpecialCommandHandlers_ExplicitNull()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = null)]
                public static partial ParseResult<EmptyOptions> ParseArguments(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", GetMainCodeGenForEmptyOptions("string[]", "args")), ("HelpCommandHandler.EmptyOptions.g.cs", HelpCodeGenForEmptyOptions), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    [Fact]
    public async Task SpecialCommandHandlers_Empty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [])]
                public static partial ParseResult<EmptyOptions> ParseArguments(string[] args);
            }
            """;

        // TODO: this should have non-default codegen
        await VerifyGeneratorAsync(source, ("EmptyOptions.g.cs", GetMainCodeGenForEmptyOptions("string[]", "args")), ("HelpCommandHandler.EmptyOptions.g.cs", HelpCodeGenForEmptyOptions), ("VersionCommandHandler.TestProject.g.cs", VersionCommandHander));
    }

    [Fact]
    public async Task SpecialCommandHandlers_NullElement()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [null])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("int")]
    [InlineData("string")]
    public async Task SpecialCommandHandlers_InvalidType(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof({{invalidType}})])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_CommandHandlerWithoutAliasesAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_CommandHandlerWithNullAliases()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases(null)]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("()")]
    [InlineData("([])")]
    [InlineData("(new string[0])")]
    [InlineData("(new string[] { })")]
    public async Task SpecialCommandHandlers_CommandHandlerWithEmptyAliases(string emptyAliasesSyntax)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases{{emptyAliasesSyntax}}]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("--my$name")]
    [InlineData("2me")]
    [InlineData("my command")]
    public async Task SpecialCommandHandlers_InvalidAlias(string invalidAlias)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases("{{invalidAlias}}")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_InvalidAlias_Null()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases("--info", null)]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_NoParameterlessConstructor()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases("--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;

                public InfoCommandHandler(int x)
                {
                }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public async Task SpecialCommandHandlers_InaccessibleParameterlessConstructor(string accessibility)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(SpecialCommandHandlers = [typeof(InfoCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases("--info")]
            class InfoCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;

                {{accessibility}} InfoCommandHandler()
                {
                }
            }
            """;

        await VerifyGeneratorAsync(source);
    }

    private static string GetMainCodeGenForEmptyOptions(string argsParameterType, string argsParameterName) => $$"""
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
            public static partial global::ArgumentParsing.Results.ParseResult<global::EmptyOptions> ParseArguments({{argsParameterType}} {{argsParameterName}})
            {

                int state = -3;
                global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;
                global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];
                global::System.ReadOnlySpan<char> latestOptionName = default(global::System.ReadOnlySpan<char>);

                foreach (string arg in {{argsParameterName}})
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

    public const string HelpCodeGenForEmptyOptions = """
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
                /// <param name="errors">Parse errors to include into help text</param>
                /// <returns>Generated help text</returns>
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
                    global::System.Console.Out.WriteLine(GenerateHelpText());
                    return 0;
                }
            }
        }
        """;
}
