using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit.Utilities;

public abstract class AnalyzerTestBase<TAnalyzer>
    where TAnalyzer: DiagnosticAnalyzer, new()
{
    protected static Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
        => VerifyAnalyzerAsync(source, [], languageVersion, referenceAssemblies);

    protected static async Task VerifyAnalyzerAsync(string source, DiagnosticResult[] diagnostics, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider>()
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
            LanguageVersion = languageVersion,
            ReferenceAssemblies = referenceAssemblies ?? ReferenceAssemblies.Net.Net80,
        };

        test.ExpectedDiagnostics.AddRange(diagnostics);

        await test.RunAsync();
    }
}
