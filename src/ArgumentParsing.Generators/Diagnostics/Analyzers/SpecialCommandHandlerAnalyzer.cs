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
            DiagnosticDescriptors.SpecialCommandHandlerShouldBeClass,
            DiagnosticDescriptors.SpecialCommandHandlerMustHaveAliases,
            DiagnosticDescriptors.InvalidSpecialCommandAlias,
            DiagnosticDescriptors.AliasShouldStartWithDash);

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
                SpecialCommandAliasesAttributeType = comp.SpecialCommandAliasesAttributeType(),
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
        var declaration = location.SourceTree?.GetRoot(context.CancellationToken).FindNode(location.SourceSpan) as BaseTypeDeclarationSyntax;
        var diagnosticLocation = declaration?.Identifier.GetLocation() ?? location;

        if (type.TypeKind == TypeKind.Struct)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SpecialCommandHandlerShouldBeClass,
                    diagnosticLocation));
        }

        var aliasesAttribute = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(knownTypes.SpecialCommandAliasesAttributeType, SymbolEqualityComparer.Default) == true);
        var constructorArg = aliasesAttribute?.ConstructorArguments[0];

        if (aliasesAttribute is null ||
            constructorArg is null or { IsNull: true } or { Values.IsEmpty: true })
        {
            var diagType = aliasesAttribute is null
                ? SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.NoAttribute
                : (constructorArg is { IsNull: true }
                    ? SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.NullValues
                    : SpecialCommandHandlerMustHaveAliasesDiagnosticTypes.EmptyValues);

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SpecialCommandHandlerMustHaveAliases,
                    diagnosticLocation,
                    properties: ImmutableDictionary.CreateRange([new KeyValuePair<string, string?>("Type", diagType)])));
        }
        else
        {
            var aliasSyntaxes = ((AttributeSyntax?)aliasesAttribute.ApplicationSyntaxReference?.GetSyntax())?.ArgumentList?.Arguments;

            for (var i = 0; i < constructorArg.Value.Values.Length; i++)
            {
                var aliasValue = (string?)constructorArg.Value.Values[i].Value;
                if (!aliasValue.IsValidName(allowDashPrefix: true))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidSpecialCommandAlias,
                            aliasSyntaxes?[i].GetLocation() ?? diagnosticLocation,
                            aliasValue));
                }
                else if (aliasValue[0] != '-')
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.AliasShouldStartWithDash,
                            aliasSyntaxes?[i].GetLocation() ?? diagnosticLocation));
                }
            }
        }
    }

    public static class SpecialCommandHandlerMustHaveAliasesDiagnosticTypes
    {
        public const string NoAttribute = "NoAttribute";
        public const string NullValues = "NullValues";
        public const string EmptyValues = "EmptyValues";
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol? ISpecialCommandHandlerType { get; init; }

        public required INamedTypeSymbol? SpecialCommandAliasesAttributeType { get; init; }
    }
}
