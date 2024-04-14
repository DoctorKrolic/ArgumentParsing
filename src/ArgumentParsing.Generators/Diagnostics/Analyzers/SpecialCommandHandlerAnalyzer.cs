using System.Collections.Immutable;
using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpecialCommandHandlerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.SpecialCommandHandlerShouldBeClass);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                ISpecialCommandHandlerType = comp.ISpecialCommandHandlerType(),
            };

            context.RegisterSymbolAction(context => AnalyzeSpecialCommandHandler(context, knownTypes), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeSpecialCommandHandler(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || !type.AllInterfaces.Any(i => i.Equals(knownTypes.ISpecialCommandHandlerType, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var location = type.Locations.First();

        if (type.TypeKind == TypeKind.Struct)
        {
            var declaration = location.SourceTree?.GetRoot(context.CancellationToken).FindNode(location.SourceSpan) as BaseTypeDeclarationSyntax;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SpecialCommandHandlerShouldBeClass,
                    declaration?.Identifier.GetLocation() ?? location));
        }
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? ISpecialCommandHandlerType { get; init; }
    }
}
