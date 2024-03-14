using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit.Utilities;

public abstract class AnalyzerTestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected static Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
        => VerifyAnalyzerAsync(source, [], languageVersion, referenceAssemblies);

    protected static Task VerifyAnalyzerAsync(string source, DiagnosticResult[] diagnostics, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
        => VerifyAnalyzerWithCodeFixAsync<EmptyCodeFixProvider>(source, fixedSource: null, diagnostics, languageVersion, referenceAssemblies);

    protected static Task VerifyAnalyzerWithCodeFixAsync<TCodeFix>(string source, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
        where TCodeFix : CodeFixProvider, new()
    {
        return VerifyAnalyzerWithCodeFixAsync<TCodeFix>(source, fixedSource, [], languageVersion, referenceAssemblies);
    }

    protected static async Task VerifyAnalyzerWithCodeFixAsync<TCodeFix>(string source, string fixedSource, DiagnosticResult[] diagnostics, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null)
        where TCodeFix : CodeFixProvider, new()
    {
        var additionalDoc = """
            global using ArgumentParsing;
            global using ArgumentParsing.Results;
            global using System;

            class EmptyOptions { }
            """;

        var mainLibraryReference = MetadataReference.CreateFromFile(typeof(GeneratedArgumentParserAttribute).Assembly.Location);

        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix>()
        {
            TestState =
            {
                Sources =
                {
                    source,
                    additionalDoc,
                },
                AdditionalReferences =
                {
                    mainLibraryReference,
                }
            },
            LanguageVersion = languageVersion,
            ReferenceAssemblies = referenceAssemblies ?? ReferenceAssemblies.Net.Net80,
        };

        test.ExpectedDiagnostics.AddRange(diagnostics);

        if (fixedSource is not null)
        {
            test.FixedState.Sources.Add(fixedSource);
            test.FixedState.Sources.Add(additionalDoc);
            test.FixedState.AdditionalReferences.Add(mainLibraryReference);
        }

        await test.RunAsync();
    }
}
