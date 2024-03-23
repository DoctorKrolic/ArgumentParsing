using ArgumentParsing.Generators.Diagnostics.Suppressors;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class ParserRelatedPropertyMarkedWithRequiredAttributeSuppressorTests : AnalyzerTestBase<ParserRelatedPropertyMarkedWithRequiredAttributeSuppressor>
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
    [InlineData("Option")]
    [InlineData("Parameter(0)")]
    [InlineData("RemainingParameters")]
    public async Task DoNotSuppressParserRelatedDiagnosticInNonOptionsType(string markerAttributeContent)
    {
        var source = $$"""
            #nullable enable

            class MyOptions
            {
                [{{markerAttributeContent}}]
                public string {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: false);
    }

    [Theory]
    [InlineData("Option")]
    [InlineData("Parameter(0)")]
    [InlineData("RemainingParameters")]
    public async Task SuppressParserRelatedDiagnosticInOptionsType(string markerAttributeContent)
    {
        var source = $$"""
            #nullable enable

            [OptionsType]
            class MyOptions
            {
                [{{markerAttributeContent}}]
                public string {|#0:Prop|} { get; set; }
            }
            """;

        await VerifyNullabilitySuppression(source, isSuppressed: true);
    }

    private static Task VerifyNullabilitySuppression(string source, bool isSuppressed)
        => VerifyAnalyzerAsync(source,
        [
            DiagnosticResult.CompilerWarning("CS8618")
                .WithLocation(0)
                .WithLocation(0)
                .WithIsSuppressed(isSuppressed)
        ],
        compilerDiagnostics: CompilerDiagnostics.Warnings);
}
