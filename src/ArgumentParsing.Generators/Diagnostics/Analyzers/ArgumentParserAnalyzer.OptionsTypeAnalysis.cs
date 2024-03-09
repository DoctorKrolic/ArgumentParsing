using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArgumentParsing.Generators.Diagnostics.Analyzers;

public partial class ArgumentParserAnalyzer
{
    private static void AnalyzeOptionsType(SymbolAnalysisContext context, INamedTypeSymbol optionsType, KnownTypes knownTypes)
    {
        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var firstPropertyOfShortNameWithNoError = new Dictionary<char, IPropertySymbol>();
        var firstPropertyOfLongNameWithNoError = new Dictionary<string, IPropertySymbol>();

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

                var propertyType = property.Type;
                var (possibleParseStrategy, isNullable) = GetPotentialParseStrategy(propertyType, knownTypes);

                if (!possibleParseStrategy.HasValue)
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

                var parseStrategy = possibleParseStrategy.Value;

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
        }

        static (ParseStrategy? PotentialParseStrategy, bool IsNullable) GetPotentialParseStrategy(ITypeSymbol type, KnownTypes knownTypes)
        {
            if (type is not INamedTypeSymbol namedType)
            {
                return default;
            }

            var isNullable = false;

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
                }
            }

            return (namedType.GetPotentialPrimaryParseStrategy(), isNullable);
        }
    }
}
