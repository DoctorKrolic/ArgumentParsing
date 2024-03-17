using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ArgumentParsing.Tests.Unit.Utilities;

public sealed class CSharpCodeFixTest<TAnalyzer, TCodeFixProvider> : CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
    where TAnalyzer: DiagnosticAnalyzer, new()
    where TCodeFixProvider: CodeFixProvider, new()
{
    public LanguageVersion LanguageVersion { get; init; }

    protected override ParseOptions CreateParseOptions()
    {
        return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
    }
}
