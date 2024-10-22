using ArgumentParsing.CodeFixes;
using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class ParserSignatureAnalyzerTests : AnalyzerTestBase<ParserSignatureAnalyzer>
{
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("string[]")]
    [InlineData("IEnumerable<string>")]
    [InlineData("IReadOnlyCollection<string>")]
    [InlineData("IReadOnlyList<string>")]
    [InlineData("List<string>")]
    [InlineData("Span<string>")]
    [InlineData("ReadOnlySpan<string>")]
    [InlineData("ImmutableArray<string>")]
    [InlineData("ImmutableList<string>")]
    public async Task ValidParameterType_NotSet(string validType)
    {
        var source = $$"""
            using System.Collections.Generic;
            using System.Collections.Immutable;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({{validType}} args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("ISet<string>")]
    [InlineData("HashSet<string>")]
    [InlineData("ImmutableHashSet<string>")]
    [InlineData("FrozenSet<string>")]
    public async Task ValidParameterType_Set(string validType)
    {
        var source = $$"""
            using System.Collections.Frozen;
            using System.Collections.Generic;
            using System.Collections.Immutable;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}({|ARGP0037:{{validType}}|} args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SuggestArgsParameterNameForValidParameter()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] {|ARGP0004:s|});
            }
            """;

        var fixedSource = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<UseArgsParameterNameCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("MyOptions")]
    [InlineData("EmptyOptions")]
    public async Task InvalidReturnType_ResultsInValidOptionsTypeAfterFix(string returnType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0005:{{returnType}}|} {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
                public string S { get; set; }
            }
            """;

        var fixedSource = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{{returnType}}> {|CS8795:ParseArguments|}(string[] args);
            }
            
            [OptionsType]
            class MyOptions
            {
                public string S { get; set; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<WrapReturnTypeIntoParseResultCodeFixProvider>(source, fixedSource);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("string")]
    public async Task InvalidReturnType_ResultsInInvalidOptionsTypeAfterFix(string returnType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0005:{{returnType}}|} {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        var fixedSource = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{|ARGP0006:{{returnType}}|}> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<WrapReturnTypeIntoParseResultCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task InvalidReturnType_ErrorType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|#0:{|CS0246:ErrorType|}|} {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        var fixedSource = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{|CS0246:ErrorType|}> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<WrapReturnTypeIntoParseResultCodeFixProvider>(source, fixedSource,
        [
            new DiagnosticResult("ARGP0005", DiagnosticSeverity.Hidden)
                .WithLocation(0)
        ]);
    }

    [Theory]
    [InlineData("ParseResult")]
    [InlineData("ParseResult<string>")]
    [InlineData("ParseResult<{|CS0246:ErrorType|}>")]
    public async Task InvalidReturnType_ErrorType_LooksLikeParseResult(string typeNameMarkup)
    {
        var source = $$"""
            using ArgumentParsing;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|CS0246:{{typeNameMarkup}}|} {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source, addCommonUsings: false);
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
                public static partial ParseResult<{|ARGP0006:{{invalidType}}|}> {|CS8795:ParseArguments|}(string[] args);
            }

            class ClassWithoutParameterlessConstructor
            {
                public ClassWithoutParameterlessConstructor(int a)
                {
                }
            }

            class ClassWithInaccessibleParameterlessConstructor
            {
                private ClassWithInaccessibleParameterlessConstructor()
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("string")]
    [InlineData("System.Action")]
    [InlineData("System.DayOfWeek")]
    [InlineData("ClassWithoutParameterlessConstructor")]
    public async Task InvalidOptionsType_NotGenericName(string invalidType)
    {
        var source = $$"""
            using InvalidParserReturnType = ArgumentParsing.Results.ParseResult<{{invalidType}}>;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0006:InvalidParserReturnType|} {|CS8795:ParseArguments|}(string[] args);
            }

            class ClassWithoutParameterlessConstructor
            {
                public ClassWithoutParameterlessConstructor(int a)
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task OptionsTypeNotAnnotatedWithOptionsTypeAttribute()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<{|ARGP0034:MyOptions|}> {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
            }
            """;

        var fixedSource = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            
            [OptionsType]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AnnotateTypeWithOptionsTypeAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task OptionsTypeNotAnnotatedWithOptionsTypeAttribute_NotGenericName()
    {
        var source = """
            using UnannotatedOptionsType = ArgumentParsing.Results.ParseResult<MyOptions>;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial {|ARGP0034:UnannotatedOptionsType|} {|CS8795:ParseArguments|}(string[] args);
            }

            class MyOptions
            {
            }
            """;

        var fixedSource = """
            using UnannotatedOptionsType = ArgumentParsing.Results.ParseResult<MyOptions>;

            partial class C
            {
                [GeneratedArgumentParser]
                public static partial UnannotatedOptionsType {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AnnotateTypeWithOptionsTypeAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_ExplicitNull()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = null)]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_Empty()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_NullElement_CollectionExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [{|#0:null|}])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("null")
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_NullElement_ArrayCreationExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = new Type[] { {|#0:null|} })]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("null")
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_DefaultElement_CollectionExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [{|#0:default|}])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("default")
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_DefaultElement_ArrayCreationExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = new Type[] { {|#0:default|} })]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("default")
        ]);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("int")]
    [InlineData("string")]
    public async Task SpecialCommandHandlers_Additional_InvalidType_CollectionExpression(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof({|#0:{{invalidType}}|})])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments(invalidType)
        ]);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("int")]
    [InlineData("string")]
    public async Task SpecialCommandHandlers_Additional_InvalidType_ArrayCreationExpression(string invalidType)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = new Type[] { typeof({|#0:{{invalidType}}|}) })]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments(invalidType)
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_InvalidType_ErrorType_CollectionExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof({|CS0246:ErrorType|})])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_InvalidType_ErrorType_ArrayCreationExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = new Type[] { typeof({|CS0246:ErrorType|}) })]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_MultipleInvalidTypes_CollectionExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof({|#0:C|}), typeof({|#1:MyClass|})])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            public class MyClass
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("C"),
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(1)
                .WithArguments("MyClass")
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_MultipleInvalidTypes_ArrayCreationExpression()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = new Type[] { typeof({|#0:C|}), typeof({|#1:MyClass|}) })]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            public class MyClass
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(0)
                .WithArguments("C"),
            DiagnosticResult
                .CompilerError("ARGP0043")
                .WithLocation(1)
                .WithArguments("MyClass")
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_ValidType()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof(InfoSpecialCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [SpecialCommandAliases("--info")]
            class InfoSpecialCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_ValidType_EvenWithoutAliases()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof(InfoSpecialCommandHandler)])]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            class InfoSpecialCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("BuiltInCommandHandlers.None")]
    [InlineData("BuiltInCommandHandlers.Version")]
    [InlineData("BuiltInCommandHandlers.None | BuiltInCommandHandlers.Version")]
    public async Task SpecialCommandHandlers_Additional_HelpTextGeneratorButNoHelpCommand(string nonHelpBuiltInHandlers)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{nonHelpBuiltInHandlers}})]
                public static partial ParseResult<MyOptions> {|ARGP0048:{|CS8795:ParseArguments|}|}(string[] args);
            }

            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_HelpTextGeneratorAndHelpCommand1()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("BuiltInCommandHandlers.Help")]
    [InlineData("BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version")]
    [InlineData("BuiltInCommandHandlers.None | BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version")]
    public async Task SpecialCommandHandlers_Additional_HelpTextGeneratorAndHelpCommand2(string helpBuiltInHandlers)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{helpBuiltInHandlers}})]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_HelpTextGeneratorAndHelpCommand3()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.None, AdditionalCommandHandlers = [typeof(MySpecialCommandHandler)])]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }

            [SpecialCommandAliases("--help")]
            class MySpecialCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Additional_HelpTextGeneratorAndHelpCommand4()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.None, AdditionalCommandHandlers = [typeof(MySpecialCommandHandler)])]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType, HelpTextGenerator(typeof(MyOptions), "GenerateHelpText")]
            class MyOptions
            {
                public static string GenerateHelpText(ParseErrorCollection errors = null) => "";
            }

            [SpecialCommandAliases("-?", "--help")]
            class MySpecialCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task SpecialCommandHandlers_Duplicates_WithBuiltIns_ImplicitBuiltIns(string builtIn)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof(MyBuiltInReplacementCommandHandler)])]
                public static partial ParseResult<MyOptions> {|#0:{|CS8795:ParseArguments|}|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }

            [SpecialCommandAliases("{{builtIn}}")]
            class MyBuiltInReplacementCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0049")
                .WithLocation(0)
                .WithArguments(builtIn)
        ]);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task SpecialCommandHandlers_Duplicates_WithBuiltIns_ExplicitBuiltIns(string builtIn)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version, AdditionalCommandHandlers = [typeof(MyBuiltInReplacementCommandHandler)])]
                public static partial ParseResult<MyOptions> {|#0:{|CS8795:ParseArguments|}|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }

            [SpecialCommandAliases("{{builtIn}}")]
            class MyBuiltInReplacementCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0049")
                .WithLocation(0)
                .WithArguments(builtIn)
        ]);
    }

    [Fact]
    public async Task SpecialCommandHandlers_Duplicates_AmongAdditionalHandlers()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(AdditionalCommandHandlers = [typeof(MyHandler1), typeof(MyHandler2)])]
                public static partial ParseResult<MyOptions> {|#0:{|CS8795:ParseArguments|}|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }

            [SpecialCommandAliases("--info")]
            class MyHandler1 : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }

            [SpecialCommandAliases("--info")]
            class MyHandler2 : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0049")
                .WithLocation(0)
                .WithArguments("--info")
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutHelp), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_NoDuplicateWithoutBuiltIn_Help(string builtInsWithoutHelp)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutHelp}}, AdditionalCommandHandlers = [typeof(MyHelpCommandHandler)])]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }

            [SpecialCommandAliases("--help")]
            class MyHelpCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutVersion), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_NoDuplicateWithoutBuiltIn_Version(string builtInsWithoutVersion)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutVersion}}, AdditionalCommandHandlers = [typeof(MyVersionCommandHandler)])]
                public static partial ParseResult<MyOptions> {|CS8795:ParseArguments|}(string[] args);
            }

            [OptionsType]
            class MyOptions
            {
            }

            [SpecialCommandAliases("--version")]
            class MyVersionCommandHandler : ISpecialCommandHandler
            {
                public int HandleCommand() => 0;
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_NoConstructorArgs()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser]
                [{|CS7036:BuiltInCommandHelpInfo|}]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.InvalidHandlerForBuiltInCommandHelpInfo), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_InvalidHandler_FirstArg(string invalidHandler)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                [BuiltInCommandHelpInfo({|ARGP0050:{{invalidHandler}}|}, "")]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.InvalidHandlerForBuiltInCommandHelpInfo), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_InvalidHandler_NamedArgsSwapped(string invalidHandler)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                [BuiltInCommandHelpInfo(description: "", handler: {|ARGP0050:{{invalidHandler}}|})]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.SingleNonDefaultBuiltInCommandHandlers), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_ValidHandler(string validHandler)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                [BuiltInCommandHelpInfo({{validHandler}}, "")]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutHelp), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_UnnecessaryHelpInfo_Help_SeparateAttributeList(string builtInsWithoutHelp)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutHelp}})]
                {|#0:[BuiltInCommandHelpInfo(BuiltInCommandHandlers.Help, "")]|}
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            new DiagnosticResult("ARGP0051", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("BuiltInCommandHandlers.Help")
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutHelp), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_UnnecessaryHelpInfo_Help_SameAttributeList(string builtInsWithoutHelp)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutHelp}}), {|#0:BuiltInCommandHelpInfo(BuiltInCommandHandlers.Help, "")|}]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            new DiagnosticResult("ARGP0051", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("BuiltInCommandHandlers.Help")
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutVersion), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_UnnecessaryHelpInfo_Version_SeparateAttributeList(string builtInsWithoutVersion)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutVersion}})]
                {|#0:[BuiltInCommandHelpInfo(BuiltInCommandHandlers.Version, "")]|}
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            new DiagnosticResult("ARGP0051", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("BuiltInCommandHandlers.Version")
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.BuiltInsWithoutVersion), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_UnnecessaryHelpInfo_Version_SameAttributeList(string builtInsWithoutVersion)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser(BuiltInCommandHandlers = {{builtInsWithoutVersion}}), {|#0:BuiltInCommandHelpInfo(BuiltInCommandHandlers.Version, "")|}]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            new DiagnosticResult("ARGP0051", DiagnosticSeverity.Hidden)
                .WithLocation(0)
                .WithArguments("BuiltInCommandHandlers.Version")
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.SingleNonDefaultBuiltInCommandHandlers), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_TwoDuplicates(string handler)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                [{|#0:BuiltInCommandHelpInfo|}({{handler}}, "1")]
                [{|#1:BuiltInCommandHelpInfo|}({{handler}}, "2")]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0052")
                .WithLocation(0)
                .WithArguments(handler),
            DiagnosticResult
                .CompilerError("ARGP0052")
                .WithLocation(1)
                .WithArguments(handler),
        ]);
    }

    [Theory]
    [MemberData(nameof(CommonTestData.SingleNonDefaultBuiltInCommandHandlers), MemberType = typeof(CommonTestData))]
    public async Task SpecialCommandHandlers_BuiltInCommandHelpInfo_ThreeDuplicates(string handler)
    {
        var source = $$"""
            partial class C
            {
                [GeneratedArgumentParser]
                [{|#0:BuiltInCommandHelpInfo|}({{handler}}, "1")]
                [{|#1:BuiltInCommandHelpInfo|}({{handler}}, "2")]
                [{|#2:BuiltInCommandHelpInfo|}({{handler}}, "3")]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0052")
                .WithLocation(0)
                .WithArguments(handler),
            DiagnosticResult
                .CompilerError("ARGP0052")
                .WithLocation(1)
                .WithArguments(handler),
            DiagnosticResult
                .CompilerError("ARGP0052")
                .WithLocation(2)
                .WithArguments(handler),
        ]);
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
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ErrorMessageFormatProvider_TypeSpecifier_Valid()
    {
        var source = """
            partial class C
            {
                [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof(C))]
                public static partial ParseResult<EmptyOptions> {|CS8795:ParseArguments|}(string[] args);
            }
            """;

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source);
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

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0053")
                .WithLocation(0)
                .WithArguments("C[]")
        ]);
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

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerError("ARGP0053")
                .WithLocation(0)
                .WithArguments("int")
        ]);
    }
}
