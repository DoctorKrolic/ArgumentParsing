using System.Collections.Immutable;
using System.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static readonly SymbolDisplayFormat s_qualifiedNameFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    private static (ArgumentParserInfo? ArgumentParserInfo, OptionsHelpInfo? OptionsHelpInfo) ExtractMainInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var argumentParserMethodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var argumentParserMethodSymbol = (IMethodSymbol)context.TargetSymbol;

        SimpleParameterInfo? parameterInfo = null;
        INamedTypeSymbol? validOptionsType = null;

        if (argumentParserMethodSymbol.Parameters is not [var singleParameter])
        {
            return default;
        }

        var singleParameterSyntax = argumentParserMethodSyntax.ParameterList.Parameters[0];

        if (singleParameter.IsParams ||
            singleParameter.RefKind != RefKind.None ||
            singleParameter.ScopedKind != ScopedKind.None ||
            argumentParserMethodSymbol.IsExtensionMethod)
        {
            return default;
        }

        var singleParameterType = singleParameter.Type;

        if (!singleParameterType.IsEnumerableCollectionOfStrings())
        {
            return default;
        }

        parameterInfo = new(singleParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), singleParameter.Name);

        var returnTypeSyntax = argumentParserMethodSyntax.ReturnType;
        var returnType = argumentParserMethodSymbol.ReturnType;

        var compilation = context.SemanticModel.Compilation;

        if (returnType is not INamedTypeSymbol { TypeArguments: [var optionsType] } namedReturnType ||
            !namedReturnType.ConstructedFrom.Equals(compilation.ParseResultOfTType(), SymbolEqualityComparer.Default))
        {
            return default;
        }

        if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType ||
            !namedOptionsType.Constructors.Any(c => c.Parameters.Length == 0) ||
            !namedOptionsType.GetAttributes().Any(a => a.AttributeClass?.Equals(compilation.OptionsTypeAttributeType(), SymbolEqualityComparer.Default) == true))
        {
            return default;
        }

        validOptionsType = namedOptionsType;

        cancellationToken.ThrowIfCancellationRequested();

        var (optionsInfo, optionsHelpInfo) = ExtractInfoFromOptionsType(validOptionsType, compilation, cancellationToken);

        if (optionsHelpInfo is null)
        {
            return default;
        }

        var methodInfo = new ArgumentParserMethodInfo(
            argumentParserMethodSyntax.Modifiers.ToString(),
            argumentParserMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            argumentParserMethodSymbol.Name,
            parameterInfo!);

        var argumentParserInfo = new ArgumentParserInfo(
            HierarchyInfo.From(argumentParserMethodSymbol.ContainingType),
            methodInfo,
            optionsInfo!);

        return (argumentParserInfo, optionsHelpInfo);
    }

    private static (OptionsInfo? OptionsInfo, OptionsHelpInfo? OptionsHelpInfo) ExtractInfoFromOptionsType(INamedTypeSymbol optionsType, Compilation compilation, CancellationToken cancellationToken)
    {
        var optionsBuilder = ImmutableArray.CreateBuilder<OptionInfo>();
        var optionsHelpBuilder = ImmutableArray.CreateBuilder<OptionHelpInfo>();

        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var parameterMap = new Dictionary<int, ParameterInfo>();
        var parameterHelpDescriptionsMap = new Dictionary<ParameterInfo, string?>();
        var parametersProperties = new Dictionary<ParameterInfo, IPropertySymbol>();

        var declaredRemainingParameters = false;
        RemainingParametersInfo? remainingParametersInfo = null;
        RemainingParametersHelpInfo? remainingParametersHelpInfo = null;

        foreach (var member in optionsType.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (member is IFieldSymbol { IsRequired: true })
            {
                return default;
            }

            if (member is not IPropertySymbol property)
            {
                continue;
            }

            var isOption = false;
            var isParameter = false;
            var hasRequiredAttribute = false;
            var hasLongNameFromAttribute = false;

            char? shortName = null;
            string? longName = null;

            var parameterIndex = 0;
            string? parameterName = null;

            var isRemainingParameters = false;

            string? helpDescription = null;

            var optionAttributeType = compilation.OptionAttributeType();
            var parameterAttributeType = compilation.ParameterAttributeType();
            var remainingParametersAttributeType = compilation.RemainingParametersAttributeType();
            var requiredAttributeType = compilation.SystemComponentModelDataAnnotationsRequiredAttributeType();
            var helpInfoAttributeType = compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.Help.HelpInfoAttribute")!;

            foreach (var attr in property.GetAttributes())
            {
                var attributeClass = attr.AttributeClass;

                if (attributeClass is null)
                {
                    continue;
                }

                if (attributeClass.Equals(requiredAttributeType, SymbolEqualityComparer.Default))
                {
                    hasRequiredAttribute = true;
                    continue;
                }

                if (attributeClass.Equals(remainingParametersAttributeType, SymbolEqualityComparer.Default))
                {
                    isRemainingParameters = true;
                    continue;
                }

                if (attributeClass.Equals(helpInfoAttributeType, SymbolEqualityComparer.Default) &&
                    attr.ConstructorArguments.Length > 0)
                {
                    helpDescription = (string?)attr.ConstructorArguments[0].Value;
                    continue;
                }

                var isOptionAttribute = attributeClass.Equals(optionAttributeType, SymbolEqualityComparer.Default);
                var isParameterAttribute = attributeClass.Equals(parameterAttributeType, SymbolEqualityComparer.Default);

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

            var countOfParserRelatedAttributes = (isOption ? 1 : 0) + (isParameter ? 1 : 0) + (isRemainingParameters ? 1 : 0);

            if (countOfParserRelatedAttributes == 0)
            {
                if (property.IsRequired)
                {
                    return default;
                }

                continue;
            }
            else if (countOfParserRelatedAttributes > 1)
            {
                return default;
            }

            if (property is not { DeclaredAccessibility: >= Accessibility.Internal, SetMethod.DeclaredAccessibility: >= Accessibility.Internal })
            {
                return default;
            }

            var propertyName = property.Name;
            var propertyType = property.Type;
            var isRequired = hasRequiredAttribute || property.IsRequired;

            if (isOption)
            {
                if (shortName.HasValue)
                {
                    var snv = shortName.Value;

                    if (!char.IsLetter(snv) || !seenShortNames.Add(snv))
                    {
                        return default;
                    }
                }

                if (!hasLongNameFromAttribute)
                {
                    longName = propertyName.ToKebabCase();
                }

                if (!shortName.HasValue && longName is null)
                {
                    return default;
                }

                if (longName is not null)
                {
                    if (!char.IsLetter(longName[0]) || !longName.Replace("-", string.Empty).All(char.IsLetterOrDigit) || !seenLongNames.Add(longName))
                    {
                        return default;
                    }
                }

                var (parseStrategy, nullableUnderlyingType, sequenceType, sequenceUnderlyingType) = GetParseStrategyForOption(propertyType, compilation);

                if (parseStrategy == ParseStrategy.None)
                {
                    return default;
                }

                if (isRequired && parseStrategy == ParseStrategy.Flag && nullableUnderlyingType is null)
                {
                    return default;
                }

                optionsBuilder.Add(new(
                    propertyName,
                    sequenceUnderlyingType ?? propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    shortName,
                    longName,
                    parseStrategy,
                    isRequired,
                    nullableUnderlyingType,
                    sequenceType));

                optionsHelpBuilder.Add(new(
                    shortName,
                    longName,
                    isRequired,
                    helpDescription));
            }
            else if (isParameter)
            {
                var hasParameter = parameterMap.ContainsKey(parameterIndex);

                if (parameterIndex < 0 || hasParameter)
                {
                    return default;
                }

                parameterName ??= propertyName.ToKebabCase();

                if (!char.IsLetter(parameterName[0]) || !parameterName.Replace("-", string.Empty).All(char.IsLetterOrDigit))
                {
                    return default;
                }

                var (parseStrategy, nullableUnderlyingType) = GetParseStrategyForParameter(propertyType);
                if (parseStrategy == ParseStrategy.None)
                {
                    return default;
                }

                if (!hasParameter)
                {
                    var parameterInfo = new ParameterInfo(
                        parameterName,
                        propertyName,
                        propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        parseStrategy,
                        isRequired,
                        nullableUnderlyingType);

                    parameterMap.Add(parameterIndex, parameterInfo);
                    parameterHelpDescriptionsMap.Add(parameterInfo, helpDescription);
                    parametersProperties.Add(parameterInfo, property);
                }
            }
            else
            {
                Debug.Assert(isRemainingParameters);

                if (declaredRemainingParameters)
                {
                    return default;
                }

                declaredRemainingParameters = true;

                var (parseStrategy, _, sequenceType, sequenceUnderlyingType) = GetParseStrategyForOption(propertyType, compilation);
                if (parseStrategy == ParseStrategy.None || sequenceType == SequenceType.None)
                {
                    return default;
                }

                remainingParametersInfo = new(
                    propertyName,
                    sequenceUnderlyingType!,
                    parseStrategy,
                    sequenceType);

                remainingParametersHelpInfo = new(helpDescription);
            }
        }

        var lastSeenIndex = 0;
        var parametersBuilder = ImmutableArray.CreateBuilder<ParameterInfo>();
        var parametersHelpBuilder = ImmutableArray.CreateBuilder<ParameterHelpInfo>();

        foreach (var pair in parameterMap.OrderBy(pair => pair.Key))
        {
            var index = pair.Key;

            if (index > (lastSeenIndex + 1))
            {
                return default;
            }

            var parameterInfo = pair.Value;
            parametersBuilder.Add(parameterInfo);
            parametersHelpBuilder.Add(new(parameterInfo.Name, parameterInfo.IsRequired, parameterHelpDescriptionsMap[parameterInfo]));
            lastSeenIndex = index;
        }

        var canNextParameterBeRequired = true;

        foreach (var info in parametersBuilder)
        {
            if (info.IsRequired)
            {
                if (!canNextParameterBeRequired)
                {
                    return default;
                }
            }
            else
            {
                canNextParameterBeRequired = false;
            }
        }

        var qualifiedName = optionsType.ToDisplayString(s_qualifiedNameFormat);
        var optionsInfo = new OptionsInfo(
            qualifiedName,
            optionsType.DeclaredAccessibility >= Accessibility.Internal,
            optionsBuilder.ToImmutable(),
            parametersBuilder.ToImmutable(),
            remainingParametersInfo);

        var optionsHelpInfo = new OptionsHelpInfo(
            qualifiedName,
            optionsHelpBuilder.ToImmutable(),
            parametersHelpBuilder.ToImmutable(),
            remainingParametersHelpInfo);

        return (optionsInfo, optionsHelpInfo);

        static (ParseStrategy, string? NullableUnderlyingType, SequenceType, string? SequenceUnderlyingType) GetParseStrategyForOption(ITypeSymbol type, Compilation compilation)
        {
            string? nullableUnderlyingType = null;

            var sequenceType = SequenceType.None;
            string? sequenceUnderlyingType = null;

            if (type is INamedTypeSymbol namedType)
            {
                var iEnumerableOfTType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
                var iReadOnlyCollectionOfTType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T);
                var iReadOnlyListOfTType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T);
                var immutableArrayType = compilation.ImmutableArrayOfTType();

                var constructedFrom = namedType.ConstructedFrom;
                var isImmutableArray = constructedFrom.Equals(immutableArrayType, SymbolEqualityComparer.Default);

                if (isImmutableArray ||
                    constructedFrom.Equals(iEnumerableOfTType, SymbolEqualityComparer.Default) ||
                    constructedFrom.Equals(iReadOnlyCollectionOfTType, SymbolEqualityComparer.Default) ||
                    constructedFrom.Equals(iReadOnlyListOfTType, SymbolEqualityComparer.Default))
                {
                    sequenceType = isImmutableArray ? SequenceType.ImmutableArray : SequenceType.List;
                    type = namedType.TypeArguments[0];
                    sequenceUnderlyingType = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
                else if (namedType is { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var nullableUnderlyingTypeSymbol] })
                {
                    nullableUnderlyingType = nullableUnderlyingTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    type = nullableUnderlyingTypeSymbol;
                }
            }

            var parseStrategy = type.GetPrimaryParseStrategy();
            return (parseStrategy, nullableUnderlyingType, sequenceType, sequenceUnderlyingType);
        }

        static (ParseStrategy, string? NullableUnderlyingType) GetParseStrategyForParameter(ITypeSymbol type)
        {
            string? nullableUnderlyingType = null;

            if (type is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var nullableUnderlyingTypeSymbol] })
            {
                nullableUnderlyingType = nullableUnderlyingTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                type = nullableUnderlyingTypeSymbol;
            }

            var parseStrategy = type.GetPrimaryParseStrategy();
            return (parseStrategy, nullableUnderlyingType);
        }
    }

    private static (EnvironmentInfo EnvironmentInfo, AssemblyVersionInfo AssemblyVersionInfo) ExtractInfoFromCompilation(Compilation compilation, CancellationToken cancellationToken)
    {
        var canUseOptimalSpanBasedAlgorithm = CanUseOptimalSpanBasedAlgorithm(compilation);

        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        var hasStringStartsWithCharOverload = stringType.GetMembers("StartsWith").Any(s => s is IMethodSymbol { Parameters: [{ Type.SpecialType: SpecialType.System_Char }] });

        cancellationToken.ThrowIfCancellationRequested();

        var environmentInfo = new EnvironmentInfo(canUseOptimalSpanBasedAlgorithm, hasStringStartsWithCharOverload);

        var assembly = compilation.Assembly;
        var assemblyVersionInfo = new AssemblyVersionInfo(
            assembly.Name,
            assembly.Identity.Version);

        return (environmentInfo, assemblyVersionInfo);

        static bool CanUseOptimalSpanBasedAlgorithm(Compilation compilation)
        {
            var spanType = compilation.GetTypeByMetadataName("System.Span`1");
            if (spanType is null)
            {
                return false;
            }

            var readOnlySpanType = compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
            if (readOnlySpanType is null)
            {
                return false;
            }

            var rangeType = compilation.GetTypeByMetadataName("System.Range");
            if (rangeType is null)
            {
                return false;
            }

            var memoryExtensionsType = compilation.GetTypeByMetadataName("System.MemoryExtensions");
            if (memoryExtensionsType is null)
            {
                return false;
            }

            var charType = compilation.GetSpecialType(SpecialType.System_Char);
            var readOnlySpanOfCharType = readOnlySpanType.Construct(charType);

            if (!memoryExtensionsType.GetMembers("AsSpan")
                .Any(s => s is IMethodSymbol { Parameters: [{ Type.SpecialType: SpecialType.System_String }, { Type.SpecialType: SpecialType.System_Int32 }] } method &&
                          method.ReturnType.Equals(readOnlySpanOfCharType, SymbolEqualityComparer.Default)))
            {
                return false;
            }

            var spanOfRangeType = spanType.Construct(rangeType);

            if (!memoryExtensionsType.GetMembers("Split")
                .Any(s => s is IMethodSymbol { Parameters: [{ Type: var firstParameterType }, { Type: var secondParameterType }, { Type.SpecialType: SpecialType.System_Char }, { IsOptional: true }] } &&
                          firstParameterType.Equals(readOnlySpanOfCharType, SymbolEqualityComparer.Default) &&
                          secondParameterType.Equals(spanOfRangeType, SymbolEqualityComparer.Default)))
            {
                return false;
            }

            // We have required members for span-based algorithm (we checked for most of them before)
            return true;
        }
    }
}
