using System.Collections.Immutable;
using System.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionsTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
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
            DiagnosticDescriptors.InvalidParameterName,
            DiagnosticDescriptors.DuplicateRemainingParameters,
            DiagnosticDescriptors.InvalidRemainingParametersPropertyType,
            DiagnosticDescriptors.TooLowAccessibilityOfOptionsType,
            DiagnosticDescriptors.NoOptionNames);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static context =>
        {
            var comp = context.Compilation;
            var knownTypes = new KnownTypes
            {
                OptionsTypeAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.OptionsTypeAttribute")!,
                OptionAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.OptionAttribute")!,
                ParameterAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.ParameterAttribute")!,
                RemainingParametersAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.RemainingParametersAttribute")!,
                RequiredAttributeType = comp.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.RequiredAttribute")!,
                IEnumerableOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T),
                IReadOnlyCollectionOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T),
                IReadOnlyListOfTType = comp.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T),
                ImmutableArrayOfTType = comp.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1"),
            };

            context.RegisterSymbolAction(context => AnalyzeOptionsType(context, knownTypes), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeOptionsType(SymbolAnalysisContext context, KnownTypes knownTypes)
    {
        var optionsType = (INamedTypeSymbol)context.Symbol;

        if (!optionsType.GetAttributes()
            .Any(a => a.AttributeClass?.Equals(knownTypes.OptionsTypeAttributeType, SymbolEqualityComparer.Default) == true))
        {
            return;
        }

        if (optionsType.DeclaredAccessibility < Accessibility.Internal)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.TooLowAccessibilityOfOptionsType, optionsType.Locations.First()));
        }

        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var firstPropertyOfShortNameWithNoError = new Dictionary<char, IPropertySymbol>();
        var firstPropertyOfLongNameWithNoError = new Dictionary<string, IPropertySymbol>();

        var seenParametersWithTheirRequirements = new Dictionary<int, bool>();
        var firstPropertyOfParameterIndexWithNoError = new Dictionary<int, IPropertySymbol>();
        var parametersProperties = new Dictionary<int, IPropertySymbol>();

        var declaredRemainingParameters = false;
        IPropertySymbol? remainingParametersProperty = null;

        foreach (var member in optionsType.GetMembers())
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (member is IFieldSymbol { IsRequired: true } && member.DeclaredAccessibility >= optionsType.DeclaredAccessibility)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.RequiredFieldInOptionsTypeIsNotAllowed, member.Locations.First()));

                continue;
            }

            if (member is not IPropertySymbol property)
            {
                continue;
            }

            var isOption = false;
            char? shortName = null;
            var hasLongNameFromAttribute = false;
            string? longName = null;

            var isParameter = false;
            var parameterIndex = 0;
            string? parameterName = null;

            var isRemainingParameters = false;

            var hasRequiredAttribute = false;

            foreach (var attr in property.GetAttributes())
            {
                var attrType = attr.AttributeClass;

                if (attrType is null)
                {
                    continue;
                }

                if (attrType.Equals(knownTypes.RequiredAttributeType, SymbolEqualityComparer.Default))
                {
                    hasRequiredAttribute = true;
                    continue;
                }

                if (attrType.Equals(knownTypes.RemainingParametersAttributeType, SymbolEqualityComparer.Default))
                {
                    isRemainingParameters = true;
                    continue;
                }

                var isOptionAttribute = attrType.Equals(knownTypes.OptionAttributeType, SymbolEqualityComparer.Default);
                var isParameterAttribute = attrType.Equals(knownTypes.ParameterAttributeType, SymbolEqualityComparer.Default);

                if (!isOptionAttribute && !isParameterAttribute)
                {
                    continue;
                }

                isOption |= isOptionAttribute;
                isParameter |= isParameterAttribute;

                foreach (var constructorArg in attr.ConstructorArguments)
                {
                    var argType = constructorArg.Type;
                    var argValue = constructorArg.Value;

                    if (argType is null)
                    {
                        continue;
                    }

                    if (isOptionAttribute)
                    {
                        if (argType.SpecialType == SpecialType.System_Char)
                        {
                            shortName = (char)argValue!;
                        }
                        else if (argType.SpecialType == SpecialType.System_String)
                        {
                            hasLongNameFromAttribute = true;
                            longName = (string?)argValue;
                        }
                    }
                    else if (isParameterAttribute)
                    {
                        if (argType.SpecialType == SpecialType.System_Int32)
                        {
                            parameterIndex = (int)argValue!;
                        }
                    }
                }

                if (isParameterAttribute)
                {
                    foreach (var namedArg in attr.NamedArguments)
                    {
                        if (namedArg.Key != "Name")
                            continue;

                        parameterName = (string?)namedArg.Value.Value;
                    }
                }
            }

            var propertyType = property.Type;
            var propertyLocation = property.Locations.First();

            if (!isOption && !isParameter && !isRemainingParameters)
            {
                if (property.IsRequired &&
                    property.SetMethod is not null &&
                    property.DeclaredAccessibility >= optionsType.DeclaredAccessibility &&
                    property.SetMethod.DeclaredAccessibility >= optionsType.DeclaredAccessibility)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.RequiredPropertyMustParticipateInArgumentParsing, propertyLocation));
                }

                continue;
            }

            if (property.DeclaredAccessibility < Accessibility.Internal)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.PropertyIsNotAccessible, propertyLocation));
            }
            else if (property is not { SetMethod.DeclaredAccessibility: >= Accessibility.Internal })
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.PropertyMustHaveAccessibleSetter,
                        property.SetMethod?.Locations.First() ?? propertyLocation));
            }

            var isRequired = hasRequiredAttribute || property.IsRequired;

            if (isOption)
            {
                if (shortName.HasValue)
                {
                    var snv = shortName.Value;

                    if (!char.IsLetter(snv))
                    {
                        // '\uffff' is a special value which is produced when user doesn't specify any char ('').
                        // This will be errored by the C# compiler, so we don't want to add our custom diagnostic
                        if (shortName.Value != '\uffff')
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.InvalidShortName, propertyLocation, snv));
                        }
                    }
                    else
                    {
                        if (seenShortNames.Add(snv))
                        {
                            firstPropertyOfShortNameWithNoError.Add(snv, property);
                        }
                        else
                        {
                            if (firstPropertyOfShortNameWithNoError.TryGetValue(snv, out var previousProperty))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        DiagnosticDescriptors.DuplicateShortName, previousProperty.Locations.First(), snv.ToString()));

                                firstPropertyOfShortNameWithNoError.Remove(snv);
                            }

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.DuplicateShortName, propertyLocation, snv.ToString()));
                        }
                    }
                }

                if (!hasLongNameFromAttribute)
                {
                    longName = property.Name.ToKebabCase();
                }

                if (!shortName.HasValue && longName is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.NoOptionNames, propertyLocation));
                }

                if (longName is not null)
                {
                    if (!char.IsLetter(longName[0]) || !longName.Replace("-", string.Empty).All(char.IsLetterOrDigit))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.InvalidLongName, propertyLocation, longName));
                    }
                    else
                    {
                        if (seenLongNames.Add(longName))
                        {
                            firstPropertyOfLongNameWithNoError.Add(longName, property);
                        }
                        else
                        {
                            if (firstPropertyOfLongNameWithNoError.TryGetValue(longName, out var previousProperty))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        DiagnosticDescriptors.DuplicateLongName, previousProperty.Locations.First(), longName));

                                firstPropertyOfLongNameWithNoError.Remove(longName);
                            }

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.DuplicateLongName, propertyLocation, longName));
                        }
                    }
                }

                var (parseStrategy, isNullable, _) = GetParseStrategy(propertyType, knownTypes);

                if (parseStrategy == ParseStrategy.None)
                {
                    if (propertyType.TypeKind != TypeKind.Error)
                    {
                        var propertySyntax = (BasePropertyDeclarationSyntax?)property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.InvalidOptionPropertyType,
                                propertySyntax?.Type.GetLocation() ?? propertyLocation,
                                propertyType));
                    }

                    continue;
                }

                if (isRequired && !isNullable && parseStrategy == ParseStrategy.Flag)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.RequiredBoolOption, propertyLocation));
                }

                if (isRequired && isNullable)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.RequiredNullableOption, propertyLocation));
                }
            }
            else if (isParameter)
            {
                var hasParameter = seenParametersWithTheirRequirements.ContainsKey(parameterIndex);

                if (parameterIndex < 0)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.NegativeParameterIndex, propertyLocation));
                }
                else
                {
                    if (hasParameter)
                    {
                        if (firstPropertyOfParameterIndexWithNoError.TryGetValue(parameterIndex, out var previousProperty))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.DuplicateParameterIndex,
                                    previousProperty.Locations.First(),
                                    parameterIndex.ToString()));

                            firstPropertyOfParameterIndexWithNoError.Remove(parameterIndex);
                        }

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateParameterIndex,
                                propertyLocation,
                                parameterIndex.ToString()));
                    }
                    else
                    {
                        firstPropertyOfParameterIndexWithNoError.Add(parameterIndex, property);
                    }
                }

                parameterName ??= property.Name.ToKebabCase();

                if (!char.IsLetter(parameterName[0]) || !parameterName.Replace("-", string.Empty).All(char.IsLetterOrDigit))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidParameterName, propertyLocation, parameterName));
                }

                var (parseStrategy, isNullable, isSequence) = GetParseStrategy(propertyType, knownTypes);
                if ((parseStrategy == ParseStrategy.None || isSequence) && propertyType.TypeKind != TypeKind.Error)
                {
                    var propertySyntax = (BasePropertyDeclarationSyntax?)property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidParameterPropertyType,
                            propertySyntax?.Type.GetLocation() ?? propertyLocation,
                            propertyType));
                }

                if (!hasParameter)
                {
                    seenParametersWithTheirRequirements.Add(parameterIndex, isRequired);
                    parametersProperties.Add(parameterIndex, property);
                }
            }
            else
            {
                Debug.Assert(isRemainingParameters);

                if (declaredRemainingParameters)
                {
                    if (remainingParametersProperty is not null)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateRemainingParameters, remainingParametersProperty.Locations.First()));

                        remainingParametersProperty = null;
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateRemainingParameters, propertyLocation));
                }
                else
                {
                    remainingParametersProperty = property;
                }

                declaredRemainingParameters = true;

                var (parseStrategy, isNullable, isSequence) = GetParseStrategy(propertyType, knownTypes);
                if ((parseStrategy == ParseStrategy.None || isNullable || !isSequence) && propertyType.TypeKind != TypeKind.Error)
                {
                    var propertySyntax = (BasePropertyDeclarationSyntax?)property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
                    var diagnosticLocation = propertySyntax?.Type.GetLocation() ?? propertyLocation;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidRemainingParametersPropertyType, diagnosticLocation));
                }
            }
        }

        var lastSeenParameterIndex = 0;
        var parameterRequirements = ImmutableArray.CreateBuilder<bool>();

        foreach (var pair in seenParametersWithTheirRequirements.OrderBy(static p => p.Key))
        {
            var index = pair.Key;

            if (index > (lastSeenParameterIndex + 1))
            {
                if (index - lastSeenParameterIndex == 2)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MissingParameterWithIndex,
                            optionsType.Locations.First(),
                            (index - 1).ToString()));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MissingParametersWithIndexes,
                            optionsType.Locations.First(),
                            (lastSeenParameterIndex + 1).ToString(),
                            (index - 1).ToString()));
                }
            }

            lastSeenParameterIndex = index;
            parameterRequirements.Add(pair.Value);
        }

        var canNextParameterBeRequired = true;

        for (var i = 0; i < parameterRequirements.Count; i++)
        {
            var isRequired = parameterRequirements[i];

            if (isRequired)
            {
                if (!canNextParameterBeRequired)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.RequiredCanOnlyBeFirstNParametersInARow,
                            parametersProperties[i].Locations.First()));
                }
            }
            else
            {
                canNextParameterBeRequired = false;
            }
        }

        static (ParseStrategy, bool IsNullable, bool IsSequence) GetParseStrategy(ITypeSymbol type, KnownTypes knownTypes)
        {
            if (type is not INamedTypeSymbol namedType)
            {
                return default;
            }

            var isNullable = false;
            var isSequence = false;

            if (namedType is { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var nullableUnderlyingType] })
            {
                if (nullableUnderlyingType is INamedTypeSymbol namedNullableUnderlyingType)
                {
                    namedType = namedNullableUnderlyingType;
                }
                else
                {
                    return default;
                }

                isNullable = true;
            }
            else
            {
                var constructedFrom = namedType.ConstructedFrom;

                if (constructedFrom.Equals(knownTypes.IEnumerableOfTType, SymbolEqualityComparer.Default) ||
                    constructedFrom.Equals(knownTypes.IReadOnlyCollectionOfTType, SymbolEqualityComparer.Default) ||
                    constructedFrom.Equals(knownTypes.IReadOnlyListOfTType, SymbolEqualityComparer.Default) ||
                    constructedFrom.Equals(knownTypes.ImmutableArrayOfTType, SymbolEqualityComparer.Default))
                {
                    if (namedType.TypeArguments is [INamedTypeSymbol namedSequenceUnderlyingType])
                    {
                        namedType = namedSequenceUnderlyingType;
                    }
                    else
                    {
                        return default;
                    }

                    isSequence = true;
                }
            }

            return (namedType.GetPrimaryParseStrategy(), isNullable, isSequence);
        }
    }

    private readonly struct KnownTypes
    {
        public required INamedTypeSymbol OptionsTypeAttributeType { get; init; }

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
