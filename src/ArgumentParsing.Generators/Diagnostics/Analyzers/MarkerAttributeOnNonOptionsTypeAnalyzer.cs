using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkerAttributeOnNonOptionsTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.MarkerAttributeOnNonOptionsType);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                HelpTextGeneratorAttributeType = comp.HelpTextGeneratorAttributeType(),
                OptionsTypeAttributeType = comp.OptionsTypeAttributeType(),
            };

            context.RegisterSymbolAction(context => AnalyzeType(context, knownTypes), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var attributes = type.GetAttributes();
        var helpTextGeneratorAttribute = attributes.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.HelpTextGeneratorAttributeType));
        if (helpTextGeneratorAttribute is null)
        {
            return;
        }

        if (!attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, knownTypes.OptionsTypeAttributeType)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MarkerAttributeOnNonOptionsType,
                    helpTextGeneratorAttribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? type.Locations.First(),
                    "HelpTextGenerator"));
        }
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? HelpTextGeneratorAttributeType { get; init; }

        public required INamedTypeSymbol? OptionsTypeAttributeType { get; init; }
    }
}
