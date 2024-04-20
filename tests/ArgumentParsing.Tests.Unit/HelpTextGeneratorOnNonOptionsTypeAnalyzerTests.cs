using ArgumentParsing.Generators.Diagnostics.Analyzers;
using ArgumentParsing.Tests.Unit.Utilities;

namespace ArgumentParsing.Tests.Unit;

public sealed class HelpTextGeneratorOnNonOptionsTypeAnalyzerTests : AnalyzerTestBase<HelpTextGeneratorOnNonOptionsTypeAnalyzer>
{
    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    public async Task NoDiagnosticsWhenOnOptionsType1(string typeKeyword)
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
    public async Task NoDiagnosticsWhenOnOptionsType2(string typeKeyword)
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
    public async Task NoDiagnosticsWhenOnOptionsType3(string typeKeyword)
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
    public async Task WarningOnNonOptionsType(string typeKeyword)
    {
        var source = $$"""
            [{|ARGP0044:HelpTextGenerator(default, default)|}]
            {{typeKeyword}} MyOptions
            {
            }
            """;

        await VerifyAnalyzerAsync(source);
    }
}
