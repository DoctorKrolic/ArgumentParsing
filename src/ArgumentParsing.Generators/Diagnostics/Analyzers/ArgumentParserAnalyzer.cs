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
            DiagnosticDescriptors.InvalidOptionsType,
            DiagnosticDescriptors.RequiredFieldInOptionsTypeIsNotAllowed,
            DiagnosticDescriptors.RequiredPropertyMustParticipateInArgumentParsing,
            DiagnosticDescriptors.PropertyIsNotAccessible,
            DiagnosticDescriptors.PropertyMustHaveAccessibleSetter,
            // ARGP0011
            DiagnosticDescriptors.InvalidShortName,
            DiagnosticDescriptors.InvalidLongName,
            DiagnosticDescriptors.DuplicateShortName,
            DiagnosticDescriptors.DuplicateLongName,
            DiagnosticDescriptors.InvalidOptionPropertyType,
            // ARGP0017
            // ARGP0018
            DiagnosticDescriptors.RequiredBoolOption,
            DiagnosticDescriptors.RequiredNullableOption,
            // ARGP0021
            DiagnosticDescriptors.NegativeParameterIndex,
            DiagnosticDescriptors.DuplicateParameterIndex,
            DiagnosticDescriptors.InvalidParameterPropertyType,
            DiagnosticDescriptors.MissingParameterWithIndex,
            DiagnosticDescriptors.MissingParametersWithIndexes,
            DiagnosticDescriptors.RequiredCanOnlyBeFirstNParametersInARow,
            DiagnosticDescriptors.InvalidParameterName);

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
                OptionAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.OptionAttribute")!,
                ParameterAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.ParameterAttribute")!,
                RemainingParametersAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.RemainingParametersAttribute")!,
                RequiredAttributeType = comp.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.RequiredAttribute")!,
                IEnumerableOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T),
                IReadOnlyCollectionOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T),
                IReadOnlyListOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T),
                ImmutableArrayOfTType = comp.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1"),
            };

            context.RegisterSymbolAction(context => AnalyzeParserSignature(context, knownTypes), SymbolKind.Method);
        });
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol GeneratedArgumentParserAttributeType { get; init; }

        public required INamedTypeSymbol ParseResultOfTType { get; init; }

        public required INamedTypeSymbol OptionAttributeType { get; init; }

        public required INamedTypeSymbol ParameterAttributeType { get; init; }

        public required INamedTypeSymbol RemainingParametersAttributeType { get; init; }

        public required INamedTypeSymbol RequiredAttributeType { get; init; }

        public required INamedTypeSymbol IEnumerableOfTType { get; init; }

        public required INamedTypeSymbol IReadOnlyCollectionOfTType { get; init; }

        public required INamedTypeSymbol IReadOnlyListOfTType { get; init; }

        public required INamedTypeSymbol? ImmutableArrayOfTType { get; init; }
    }
}
