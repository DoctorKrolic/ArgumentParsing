using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class ArgumentParserAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.InvalidParserParameterCount,
            DiagnosticDescriptors.InvalidArgsParameter,
            DiagnosticDescriptors.InvalidArgsParameterType,
            DiagnosticDescriptors.PreferArgsParameterName,
            DiagnosticDescriptors.ReturnTypeMustBeParseResult,
            DiagnosticDescriptors.InvalidOptionsType);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                GeneratedArgumentParserAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.GeneratedArgumentParserAttribute")!,
                ParseResultOfTType = comp.GetTypeByMetadataName("ArgumentParsing.Results.ParseResult`1")!,
            };

            context.RegisterSymbolAction(context => AnalyzeParserSignature(context, knownTypes), SymbolKind.Method);
        });
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol GeneratedArgumentParserAttributeType { get; init; }

        public required INamedTypeSymbol ParseResultOfTType { get; init; }
    }
}
