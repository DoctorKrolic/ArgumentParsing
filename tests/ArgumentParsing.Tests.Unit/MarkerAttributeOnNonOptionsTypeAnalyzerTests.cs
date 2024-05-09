using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit;

public sealed class MarkerAttributeOnNonOptionsTypeAnalyzerTests : AnalyzerTestBase<MarkerAttributeOnNonOptionsTypeAnalyzer>
{
    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    public async Task HelpTextGenerator_NoDiagnosticsWhenOnOptionsType1(string typeKeyword)
    {
        var source = $$"""
            [OptionsType, HelpTextGenerator(default, default)]
            {{typeKeyword}} MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    public async Task HelpTextGenerator_NoDiagnosticsWhenOnOptionsType2(string typeKeyword)
    {
        var source = $$"""
            [OptionsType]
            [HelpTextGenerator(default, default)]
            {{typeKeyword}} MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    public async Task HelpTextGenerator_NoDiagnosticsWhenOnOptionsType3(string typeKeyword)
    {
        var source = $$"""
            [HelpTextGenerator(default, default)]
            partial {{typeKeyword}} MyOptions
            {
            }

            [OptionsType]
            partial {{typeKeyword}} MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    public async Task HelpTextGenerator_WarningOnNonOptionsType(string typeKeyword)
    {
        var source = $$"""
            [{|#0:HelpTextGenerator(default, default)|}]
            {{typeKeyword}} MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source,
        [
            DiagnosticResult
                .CompilerWarning("ARGP0044")
                .WithLocation(0)
                .WithArguments("HelpTextGenerator")
        ]);
    }
}
