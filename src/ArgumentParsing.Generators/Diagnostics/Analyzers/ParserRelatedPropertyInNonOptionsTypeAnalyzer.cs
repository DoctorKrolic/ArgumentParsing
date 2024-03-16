using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParserRelatedPropertyInNonOptionsTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.ParserRelatedPropertyInNonOptionsType);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                OptionsTypeAttributeType = comp.OptionsTypeAttributeType(),
                OptionAttributeType = comp.OptionAttributeType(),
                ParameterAttributeType = comp.ParameterAttributeType(),
                RemainingParametersAttributeType = comp.RemainingParametersAttributeType(),
            };

            context.RegisterSymbolAction(context => AnalyzeProperty(context, knownTypes), SymbolKind.Property);
        });
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var property = (IPropertySymbol)context.Symbol;

        if (!property.GetAttributes()
            .Any(a => a.AttributeClass?.Equals(knownTypes.OptionAttributeType, SymbolEqualityComparer.Default) == true ||
                      a.AttributeClass?.Equals(knownTypes.ParameterAttributeType, SymbolEqualityComparer.Default) == true ||
                      a.AttributeClass?.Equals(knownTypes.RemainingParametersAttributeType, SymbolEqualityComparer.Default) == true))
        {
            return;
        }

        if (property.ContainingType.GetAttributes()
            .Any(a => a.AttributeClass?.Equals(knownTypes.OptionsTypeAttributeType, SymbolEqualityComparer.Default) == true))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ParserRelatedPropertyInNonOptionsType, property.Locations.First()));
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? OptionsTypeAttributeType { get; init; }

        public required INamedTypeSymbol? OptionAttributeType { get; init; }

        public required INamedTypeSymbol? ParameterAttributeType { get; init; }

        public required INamedTypeSymbol? RemainingParametersAttributeType { get; init; }
    }
}
