using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit.Utilities;

public abstract class AnalyzerTestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected static Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null, CompilerDiagnostics compilerDiagnostics = CompilerDiagnostics.Errors)
        => VerifyAnalyzerAsync(source, [], languageVersion, referenceAssemblies, compilerDiagnostics);

    protected static Task VerifyAnalyzerAsync(string source, DiagnosticResult[] diagnostics, LanguageVersion languageVersion = LanguageVersion.Latest, ReferenceAssemblies referenceAssemblies = null, CompilerDiagnostics compilerDiagnostics = CompilerDiagnostics.Errors)
        => VerifyAnalyzerWithCodeFixAsync<EmptyCodeFixProvider>(source, fixedSource: null, diagnostics, languageVersion, referenceAssemblies, compilerDiagnostics);

    protected static Task VerifyAnalyzerWithCodeFixAsync<TCodeFix>(
        string source,
        string fixedSource,
        LanguageVersion languageVersion = LanguageVersion.Latest,
        ReferenceAssemblies referenceAssemblies = null,
        CompilerDiagnostics compilerDiagnostics = CompilerDiagnostics.Errors,
        int codeActionIndex = 0)
        where TCodeFix : CodeFixProvider, new()
    {
        return VerifyAnalyzerWithCodeFixAsync<TCodeFix>(source, fixedSource, [], languageVersion, referenceAssemblies, compilerDiagnostics, codeActionIndex);
    }

    protected static async Task VerifyAnalyzerWithCodeFixAsync<TCodeFix>(
        string source,
        string fixedSource,
        DiagnosticResult[] diagnostics,
        LanguageVersion languageVersion = LanguageVersion.Latest,
        ReferenceAssemblies referenceAssemblies = null,
        CompilerDiagnostics compilerDiagnostics = CompilerDiagnostics.Errors,
        int codeActionIndex = 0)
        where TCodeFix : CodeFixProvider, new()
    {
        var usings = """
            global using ArgumentParsing;
            global using ArgumentParsing.Results;
            global using ArgumentParsing.Results.Errors;
            global using ArgumentParsing.SpecialCommands;
            global using ArgumentParsing.SpecialCommands.Help;
            global using System;
            """;

        var emptyOptions = """
            using ArgumentParsing;

            [OptionsType]
            class EmptyOptions { }
            """;

        var mainLibraryReference = MetadataReference.CreateFromFile(typeof(GeneratedArgumentParserAttribute).Assembly.Location);
        var systemComponentModelDataAnnotationsLibraryReference = MetadataReference.CreateFromFile(typeof(RequiredAttribute).Assembly.Location);

        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix>()
        {
            TestState =
            {
                Sources =
                {
                    source,
                    emptyOptions
                },
                AdditionalReferences =
                {
                    mainLibraryReference,
                    systemComponentModelDataAnnotationsLibraryReference,
                }
            },
            CodeActionIndex = codeActionIndex,
            CompilerDiagnostics = compilerDiagnostics,
            LanguageVersion = languageVersion,
            ReferenceAssemblies = referenceAssemblies ?? ReferenceAssemblies.Net.Net80,
        };

        if (languageVersion >= LanguageVersion.CSharp10)
        {
            test.TestState.Sources.Add(usings);
        }

        test.ExpectedDiagnostics.AddRange(diagnostics);

        if (fixedSource is not null)
        {
            test.FixedState.Sources.Add(fixedSource);
            test.FixedState.Sources.Add(emptyOptions);
            if (languageVersion >= LanguageVersion.CSharp10)
            {
                test.FixedState.Sources.Add(usings);
            }

            test.FixedState.AdditionalReferences.Add(mainLibraryReference);
            test.FixedState.AdditionalReferences.Add(systemComponentModelDataAnnotationsLibraryReference);
        }

        await test.RunAsync();
    }
}
