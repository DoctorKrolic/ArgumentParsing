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

        var comp = context.SemanticModel.Compilation;
        var genArgParserAttrType = comp.GetTypeByMetadataName(GeneratedArgumentParserAttributeName)!;

        var genArgParserAttrData = context.Attributes.First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, genArgParserAttrType));
        ImmutableArray<SpecialCommandHandlerInfo>.Builder? specialCommandHandlerInfosBuilder = null;

        if (genArgParserAttrData.NamedArguments.FirstOrDefault(static n => n.Key == "SpecialCommandHandlers").Value is { IsNull: false, Values: { IsDefault: false } specialCommandHandlers })
        {
            specialCommandHandlerInfosBuilder = ImmutableArray.CreateBuilder<SpecialCommandHandlerInfo>();
            var iSpecialCommandHandlerType = comp.ISpecialCommandHandlerType();
            var specialCommandAliasesAttributeType = comp.SpecialCommandAliasesAttributeType();

            foreach (var commandHandler in specialCommandHandlers)
            {
                if (commandHandler.Value is not INamedTypeSymbol commandHandlerType ||
                    !commandHandlerType.AllInterfaces.Any(i => i.Equals(iSpecialCommandHandlerType, SymbolEqualityComparer.Default)) ||
                    !commandHandlerType.Constructors.Any(static c => c.DeclaredAccessibility >= Accessibility.Internal && c.Parameters.IsEmpty))
                {
                    return default;
                }

                var aliasesAttrData = commandHandlerType
                    .GetAttributes()
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, specialCommandAliasesAttributeType));

                if (aliasesAttrData is null)
                {
                    return default;
                }

                var firstConstructorArg = aliasesAttrData.ConstructorArguments.First();
                if (firstConstructorArg.IsNull || firstConstructorArg.Values.IsEmpty)
                {
                    return default;
                }

                var aliasesBuilder = ImmutableArray.CreateBuilder<string>();
                foreach (var alias in firstConstructorArg.Values)
                {
                    if (alias is not { IsNull: false, Value: string aliasVal } || !aliasVal.IsValidName(allowDashPrefix: true))
                    {
                        return default;
                    }

                    aliasesBuilder.Add(aliasVal);
                }

                specialCommandHandlerInfosBuilder.Add(new(
                    commandHandlerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    aliasesBuilder.ToImmutable()));
            }
        }

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

        if (returnType is not INamedTypeSymbol { TypeArguments: [var optionsType] } namedReturnType ||
            !namedReturnType.ConstructedFrom.Equals(comp.ParseResultOfTType(), SymbolEqualityComparer.Default))
        {
            return default;
        }

        if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType ||
            !namedOptionsType.Constructors.Any(static c => c.DeclaredAccessibility >= Accessibility.Internal && c.Parameters.IsEmpty) ||
            !namedOptionsType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, comp.OptionsTypeAttributeType())))
        {
            return default;
        }

        validOptionsType = namedOptionsType;

        cancellationToken.ThrowIfCancellationRequested();

        var (optionsInfo, optionsHelpInfo) = ExtractInfoFromOptionsType(validOptionsType, comp, cancellationToken);

        if (optionsInfo is null)
        {
            return default;
        }

        var methodInfo = new ArgumentParserMethodInfo(
            argumentParserMethodSyntax.Modifiers.ToString(),
            argumentParserMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            argumentParserMethodSymbol.Name,
            parameterInfo);

        var argumentParserInfo = new ArgumentParserInfo(
            HierarchyInfo.From(argumentParserMethodSymbol.ContainingType),
            methodInfo,
            optionsInfo,
            specialCommandHandlerInfosBuilder?.ToImmutable());

        return (argumentParserInfo, optionsHelpInfo);
    }

    private static (OptionsInfo? OptionsInfo, OptionsHelpInfo? OptionsHelpInfo) ExtractInfoFromOptionsType(INamedTypeSymbol optionsType, Compilation comp, CancellationToken cancellationToken)
    {
        HelpTextGeneratorInfo? helpTextGeneratorInfo = null;

        if (optionsType.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, comp.HelpTextGeneratorAttributeType())) is { } helpTextGeneratorAttribute)
        {
            var firstArg = helpTextGeneratorAttribute.ConstructorArguments[0];
            var secondArg = helpTextGeneratorAttribute.ConstructorArguments[1];

            if (firstArg.Value is not INamedTypeSymbol { TypeKind: not TypeKind.Error, SpecialType: SpecialType.None, IsUnboundGenericType: false } helpTextGeneratorType ||
                secondArg.Value is not string methodName)
            {
                return default;
            }

            if (helpTextGeneratorType.GetMembers(methodName).FirstOrDefault(m => m is IMethodSymbol
                {
                    IsStatic: true,
                    ReturnType.SpecialType: SpecialType.System_String,
                    Parameters: [{ HasExplicitDefaultValue: true, ExplicitDefaultValue: null, Type: var parameterType }]
                } && m.HasMinimalAccessibility(Accessibility.Internal) && parameterType.Equals(comp.ParseErrorCollectionType(), SymbolEqualityComparer.Default)) is not IMethodSymbol helpTextGeneratorMethod)
            {
                return default;
            }

            helpTextGeneratorInfo = new(
                helpTextGeneratorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                methodName,
                helpTextGeneratorMethod.Parameters[0].Name);
        }

        var optionsBuilder = ImmutableArray.CreateBuilder<OptionInfo>();
        var optionsHelpBuilder = ImmutableArray.CreateBuilder<OptionHelpInfo>();

        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var parameterMap = new Dictionary<int, ParameterInfo>();
        var parameterHelpDescriptionsMap = new Dictionary<ParameterInfo, string?>();

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

            var optionAttributeType = comp.OptionAttributeType();
            var parameterAttributeType = comp.ParameterAttributeType();
            var remainingParametersAttributeType = comp.RemainingParametersAttributeType();
            var requiredAttributeType = comp.SystemComponentModelDataAnnotationsRequiredAttributeType();
            var helpInfoAttributeType = comp.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.Help.HelpInfoAttribute")!;

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
                    if (!longName.IsValidName() || !seenLongNames.Add(longName))
                    {
                        return default;
                    }
                }

                var (parseStrategy, nullableUnderlyingType, sequenceType, sequenceUnderlyingType) = GetParseStrategy(propertyType, comp);

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

                if (!parameterName.IsValidName())
                {
                    return default;
                }

                var (parseStrategy, nullableUnderlyingType, sequenceType, _) = GetParseStrategy(propertyType, comp);
                if (parseStrategy == ParseStrategy.None || sequenceType != SequenceType.None)
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

                var (parseStrategy, _, sequenceType, sequenceUnderlyingType) = GetParseStrategy(propertyType, comp);
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
            optionsType.HasMinimalAccessibility(Accessibility.Internal),
            optionsBuilder.ToImmutable(),
            parametersBuilder.ToImmutable(),
            remainingParametersInfo,
            helpTextGeneratorInfo);

        var optionsHelpInfo = new OptionsHelpInfo(
            qualifiedName,
            optionsHelpBuilder.ToImmutable(),
            parametersHelpBuilder.ToImmutable(),
            remainingParametersHelpInfo,
            helpTextGeneratorInfo);

        return (optionsInfo, optionsHelpInfo);

        static (ParseStrategy, string? NullableUnderlyingType, SequenceType, string? SequenceUnderlyingType) GetParseStrategy(ITypeSymbol type, Compilation compilation)
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
    }

    private static (EnvironmentInfo EnvironmentInfo, AssemblyVersionInfo AssemblyVersionInfo) ExtractInfoFromCompilation(Compilation compilation, CancellationToken cancellationToken)
    {
        var canUseOptimalSpanBasedAlgorithm = CanUseOptimalSpanBasedAlgorithm(compilation);

        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        var hasStringStartsWithCharOverload = stringType.GetMembers("StartsWith").Any(s => s is IMethodSymbol { Parameters: [{ Type.SpecialType: SpecialType.System_Char }] });

        cancellationToken.ThrowIfCancellationRequested();

        var generateDefaultVersionSpecialCommandAttributeType = compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.Version.GenerateDefaultVersionSpecialCommandAttribute");
        var forceDefaultVersionCommand = compilation.Assembly.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, generateDefaultVersionSpecialCommandAttributeType));

        var environmentInfo = new EnvironmentInfo(canUseOptimalSpanBasedAlgorithm, hasStringStartsWithCharOverload, forceDefaultVersionCommand);

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
