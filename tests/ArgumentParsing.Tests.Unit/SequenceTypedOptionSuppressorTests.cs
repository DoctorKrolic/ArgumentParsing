using ArgumentParsing.Generators.Diagnostics.Suppressors;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class SequenceTypedOptionSuppressorTests : AnalyzerTestBase<SequenceTypedOptionSuppressor>
{
    [Fact]
    public async Task DoNotSuppressNormalDiagnostic()
    {
        var source = """
            #nullable enable

            class C
            {
                public string {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: false);
    }

    [Fact]
    public async Task DoNotSuppressNormalDiagnosticInOptionsType()
    {
        var source = """
            #nullable enable

            [OptionsType]
            class MyOptions
            {
                public string {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: false);
    }

    [Theory]
    [InlineData("IEnumerable")]
    [InlineData("IReadOnlyCollection")]
    [InlineData("IReadOnlyList")]
    public async Task DoNotSuppressOptionDiagnosticInNonOptionsType(string sequenceBaseType)
    {
        var source = $$"""
            using System.Collections.Generic;

            #nullable enable

            class MyOptions
            {
                [Option]
                public {{sequenceBaseType}}<string> {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: false);
    }

    [Fact]
    public async Task DoNotSuppressNonSequenceOptionDiagnosticInOptionsType()
    {
        var source = """
            using System.Collections.Generic;

            #nullable enable

            [OptionsType]
            class MyOptions
            {
                [Option]
                public string {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: false);
    }

    [Theory]
    [InlineData("IEnumerable")]
    [InlineData("IReadOnlyCollection")]
    [InlineData("IReadOnlyList")]
    public async Task SuppressSequenceOptionDiagnosticInOptionsType(string sequenceBaseType)
    {
        var source = $$"""
            using System.Collections.Generic;

            #nullable enable

            [OptionsType]
            class MyOptions
            {
                [Option]
                public {{sequenceBaseType}}<string> {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: true);
    }

    private static Task VerifyNullabilitySuppression(string source, bool isSuppressed)
        => VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerWarning("CS8618")
                .WithLocation(0)
                .WithLocation(0)
                .WithIsSuppressed(isSuppressed)
        ],
        compilerDiagnostics: CompilerDiagnostics.Warnings);
}
