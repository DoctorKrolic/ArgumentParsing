using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace ArgumentParsing.Tests.Unit.Utilities;

public sealed class CSharpSourceGeneratorTest<TGenerator> : CSharpSourceGeneratorTest<TGenerator, XUnitVerifier>
    where TGenerator : new()
{
    public LanguageVersion LanguageVersion { get; init; }

    protected override ParseOptions CreateParseOptions()
    {
        return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
    }
}
