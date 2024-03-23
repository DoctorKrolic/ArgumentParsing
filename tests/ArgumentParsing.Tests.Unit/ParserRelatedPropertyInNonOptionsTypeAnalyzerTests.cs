using ArgumentParsing.CodeFixes;
using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;

namespace ArgumentParsing.Tests.Unit;

public sealed class ParserRelatedPropertyInNonOptionsTypeAnalyzerTests : AnalyzerTestBase<ParserRelatedPropertyInNonOptionsTypeAnalyzer>
{
    [Fact]
    public async Task TestSingleAttributeInNonOptionsType()
    {
        var source = """
            using System.Collections.Immutable;

            class Options
            {
                [Option]
                public int {|ARGP0035:Option|} { get; set; }

                [Parameter(0)]
                public int {|ARGP0035:Parameter|} { get; set; }

                [RemainingParameters]
                public ImmutableArray<int> {|ARGP0035:RemainingParameters|} { get; set; }

                public int PropertyWithoutAttributes { get; set; }
            }
            """;

        var fixedSource = """
            using System.Collections.Immutable;

            [OptionsType]
            class Options
            {
                [Option]
                public int Option { get; set; }

                [Parameter(0)]
                public int Parameter { get; set; }

                [RemainingParameters]
                public ImmutableArray<int> RemainingParameters { get; set; }

                public int PropertyWithoutAttributes { get; set; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AnnotateContainingTypeWithOptionsTypeAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task TestMultipleAttributesInNonOptionsType()
    {
        var source = """
            using System.Collections.Immutable;

            class Options
            {
                [Option]
                [Parameter(0)]
                [RemainingParameters]
                public int {|ARGP0035:Property|} { get; set; }

                public int PropertyWithoutAttributes { get; set; }
            }
            """;

        var fixedSource = """
            using System.Collections.Immutable;

            [OptionsType]
            class Options
            {
                [Option]
                [Parameter(0)]
                [RemainingParameters]
                public int Property { get; set; }

                public int PropertyWithoutAttributes { get; set; }
            }
            """;

        await VerifyAnalyzerWithCodeFixAsync<AnnotateContainingTypeWithOptionsTypeAttributeCodeFixProvider>(source, fixedSource);
    }

    [Fact]
    public async Task TestNoDiagnosticsInOptionsType()
    {
        var source = """
            using System.Collections.Immutable;

            [OptionsType]
            class Options
            {
                [Option]
                public int Option { get; set; }

                [Parameter(0)]
                public int Parameter { get; set; }

                [RemainingParameters]
                public ImmutableArray<int> RemainingParameters { get; set; }

                public int PropertyWithoutAttributes { get; set; }
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
