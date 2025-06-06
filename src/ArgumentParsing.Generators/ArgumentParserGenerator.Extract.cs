using System.Collections.Immutable;
using System.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static readonly SymbolDisplayFormat s_qualifiedNameFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    private static ArgumentParserInfo? ExtractArgumentParserInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var argumentParserMethodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var argumentParserMethodSymbol = (IMethodSymbol)context.TargetSymbol;

        SimpleParameterInfo? parameterInfo = null;
        INamedTypeSymbol? validOptionsType = null;

        var comp = context.SemanticModel.Compilation;
        var genArgParserAttrType = comp.GetTypeByMetadataName(GeneratedArgumentParserAttributeName);

        var genArgParserAttrData = context.Attributes.First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, genArgParserAttrType));
        var builtInCommandHandlers = BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version;
        var additionalCommandHandlerInfosBuilder = ImmutableArray.CreateBuilder<AdditionalCommandHandlerInfo>();

        var namedArgs = genArgParserAttrData.NamedArguments;

        string? errorMessageFormatProvider = null;
        if (namedArgs.FirstOrDefault(static n => n.Key == "ErrorMessageFormatProvider") is { Key: not null, Value: { } errorMessageFormatProviderVal })
        {
            if (errorMessageFormatProviderVal.Kind == TypedConstantKind.Error ||
                !errorMessageFormatProviderVal.IsNull && errorMessageFormatProviderVal.Value is not INamedTypeSymbol { TypeKind: not TypeKind.Error, SpecialType: SpecialType.None })
            {
                return null;
            }

            errorMessageFormatProvider = ((INamedTypeSymbol?)errorMessageFormatProviderVal.Value)?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        if (namedArgs.FirstOrDefault(static n => n.Key == "BuiltInCommandHandlers").Value is { Value: byte builtInHandlersByte })
        {
            builtInCommandHandlers = (BuiltInCommandHandlers)builtInHandlersByte;
        }

        var registeredCommands = new HashSet<string>();

        if (builtInCommandHandlers.HasFlag(BuiltInCommandHandlers.Help))
        {
            registeredCommands.Add("--help");
        }

        if (builtInCommandHandlers.HasFlag(BuiltInCommandHandlers.Version))
        {
            registeredCommands.Add("--version");
        }

        if (namedArgs.FirstOrDefault(static n => n.Key == "AdditionalCommandHandlers").Value is { IsNull: false, Values: { IsDefault: false } additionalCommandHandlers })
        {
            var iSpecialCommandHandlerType = comp.ISpecialCommandHandlerType();
            var specialCommandAliasesAttributeType = comp.SpecialCommandAliasesAttributeType();
            var helpInfoAttributeType = comp.HelpInfoAttributeType();

            foreach (var commandHandler in additionalCommandHandlers)
            {
                if (commandHandler.Value is not INamedTypeSymbol commandHandlerType ||
                    !commandHandlerType.AllInterfaces.Any(i => i.Equals(iSpecialCommandHandlerType, SymbolEqualityComparer.Default)) ||
                    !commandHandlerType.Constructors.Any(static c => c.DeclaredAccessibility >= Accessibility.Internal && c.Parameters.IsEmpty))
                {
                    return null;
                }

                var commandHandlerAttributes = commandHandlerType.GetAttributes();
                var aliasesAttrData = commandHandlerAttributes
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, specialCommandAliasesAttributeType));

                if (aliasesAttrData is null)
                {
                    return null;
                }

                var firstConstructorArg = aliasesAttrData.ConstructorArguments.First();
                if (firstConstructorArg.IsNull || firstConstructorArg.Values.IsEmpty)
                {
                    return null;
                }

                var aliasesBuilder = ImmutableArray.CreateBuilder<string>();
                foreach (var alias in firstConstructorArg.Values)
                {
                    if (alias is not { IsNull: false, Value: string aliasVal } || !aliasVal.IsValidName(allowDashPrefix: true))
                    {
                        return null;
                    }

                    if (!registeredCommands.Add(aliasVal))
                    {
                        return null;
                    }

                    aliasesBuilder.Add(aliasVal);
                }

                var helpInfoAttrData = commandHandlerAttributes
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, helpInfoAttributeType));
                var commandHandlerHelpDescription = helpInfoAttrData is null
                    ? null
                    : (string?)helpInfoAttrData.ConstructorArguments.FirstOrDefault().Value;

                additionalCommandHandlerInfosBuilder.Add(new(
                    commandHandlerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    aliasesBuilder.ToImmutable(),
                    commandHandlerHelpDescription));
            }
        }

        var attributes = argumentParserMethodSymbol.GetAttributes();
        var builtInHelpDescriptions = new Dictionary<BuiltInCommandHandlers, string>(attributes.Length - 1);
        var builtInCommandHelpInfoAttributeType = comp.BuiltInCommandHelpInfoAttributeType();

        foreach (var attribute in attributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, builtInCommandHelpInfoAttributeType) ||
                attribute.ConstructorArguments is not [{ Value: byte firstCtorArgValByte }, { Value: string commandHelpDescription }])
            {
                continue;
            }

            var firstConstructorVal = (BuiltInCommandHandlers)firstCtorArgValByte;

            if (firstConstructorVal is not (BuiltInCommandHandlers.Help or BuiltInCommandHandlers.Version) ||
                builtInHelpDescriptions.ContainsKey(firstConstructorVal))
            {
                return null;
            }

            builtInHelpDescriptions.Add(firstConstructorVal, commandHelpDescription);
        }

        if (argumentParserMethodSymbol.Parameters is not [var singleParameter])
        {
            return null;
        }

        var singleParameterSyntax = argumentParserMethodSyntax.ParameterList.Parameters[0];

        if (singleParameter.IsParams ||
            singleParameter.RefKind != RefKind.None ||
            singleParameter.ScopedKind != ScopedKind.None ||
            argumentParserMethodSymbol.IsExtensionMethod)
        {
            return null;
        }

        var singleParameterType = singleParameter.Type;

        if (!singleParameterType.IsEnumerableCollectionOfStrings())
        {
            return null;
        }

        parameterInfo = new(singleParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), singleParameter.Name);

        var returnTypeSyntax = argumentParserMethodSyntax.ReturnType;
        var returnType = argumentParserMethodSymbol.ReturnType;

        if (returnType is not INamedTypeSymbol { TypeArguments: [var optionsType] } namedReturnType ||
            !namedReturnType.ConstructedFrom.Equals(comp.ParseResultOfTType(), SymbolEqualityComparer.Default))
        {
            return null;
        }

        if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType ||
            !namedOptionsType.Constructors.Any(static c => c.DeclaredAccessibility >= Accessibility.Internal && c.Parameters.IsEmpty) ||
            !namedOptionsType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, comp.OptionsTypeAttributeType())))
        {
            return null;
        }

        validOptionsType = namedOptionsType;

        cancellationToken.ThrowIfCancellationRequested();

        var optionsInfo = ExtractOptionsInfo(validOptionsType, comp, cancellationToken);

        if (optionsInfo is null)
        {
            return null;
        }

        var methodInfo = new ArgumentParserMethodInfo(
            argumentParserMethodSyntax.Modifiers.ToString(),
            argumentParserMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            argumentParserMethodSymbol.Name,
            parameterInfo);

        var builtInCommandInfos = ImmutableArray.CreateBuilder<BuiltInCommandInfo>(initialCapacity: 2);

        if (builtInCommandHandlers.HasFlag(BuiltInCommandHandlers.Help))
        {
            builtInHelpDescriptions.TryGetValue(BuiltInCommandHandlers.Help, out var helpCommandDescription);
            builtInCommandInfos.Add(new(BuiltInCommandHandlers.Help, helpCommandDescription));
        }

        if (builtInCommandHandlers.HasFlag(BuiltInCommandHandlers.Version))
        {
            builtInHelpDescriptions.TryGetValue(BuiltInCommandHandlers.Version, out var versionCommandDescription);
            builtInCommandInfos.Add(new(BuiltInCommandHandlers.Version, versionCommandDescription));
        }

        var argumentParserInfo = new ArgumentParserInfo(
            HierarchyInfo.From(argumentParserMethodSymbol.ContainingType),
            methodInfo,
            optionsInfo,
            errorMessageFormatProvider,
            builtInCommandInfos.ToImmutable(),
            additionalCommandHandlerInfosBuilder.ToImmutable());

        return argumentParserInfo;
    }

    private static OptionsInfo? ExtractOptionsInfo(INamedTypeSymbol optionsType, Compilation comp, CancellationToken cancellationToken)
    {
        HelpTextGeneratorInfo? helpTextGeneratorInfo = null;

        if (optionsType.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, comp.HelpTextGeneratorAttributeType())) is { } helpTextGeneratorAttribute)
        {
            var firstArg = helpTextGeneratorAttribute.ConstructorArguments[0];
            var secondArg = helpTextGeneratorAttribute.ConstructorArguments[1];

            if (firstArg.Value is not INamedTypeSymbol { TypeKind: not TypeKind.Error, SpecialType: SpecialType.None, IsUnboundGenericType: false } helpTextGeneratorType ||
                secondArg.Value is not string methodName)
            {
                return null;
            }

            if (helpTextGeneratorType.GetMembers(methodName).FirstOrDefault(m => m is IMethodSymbol
                {
                    IsStatic: true,
                    ReturnType.SpecialType: SpecialType.System_String,
                    Parameters: [{ HasExplicitDefaultValue: true, ExplicitDefaultValue: null, Type: var parameterType }]
                } && m.HasMinimalAccessibility(Accessibility.Internal) && parameterType.Equals(comp.ParseErrorCollectionType(), SymbolEqualityComparer.Default)) is not IMethodSymbol helpTextGeneratorMethod)
            {
                return null;
            }

            helpTextGeneratorInfo = new(
                helpTextGeneratorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                methodName,
                helpTextGeneratorMethod.Parameters[0].Name);
        }

        var optionsBuilder = ImmutableArray.CreateBuilder<OptionInfo>();

        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var parameterMap = new Dictionary<int, ParameterInfo>();

        var declaredRemainingParameters = false;
        RemainingParametersInfo? remainingParametersInfo = null;

        foreach (var member in optionsType.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (member is IFieldSymbol { IsRequired: true })
            {
                return null;
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
            var helpInfoAttributeType = comp.HelpInfoAttributeType();

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
                    return null;
                }

                continue;
            }
            else if (countOfParserRelatedAttributes > 1)
            {
                return null;
            }

            if (property is not { DeclaredAccessibility: >= Accessibility.Internal, SetMethod.DeclaredAccessibility: >= Accessibility.Internal })
            {
                return null;
            }

            var propertyName = property.Name;
            var propertyType = property.Type;
            var isRequired = hasRequiredAttribute || property.IsRequired;
            var defaultValueStrategy = DefaultValueStrategy.None;

            if (property.Locations.First() is { IsInSource: true } propertySourceLocation)
            {
                var compUnit = propertySourceLocation.SourceTree.GetCompilationUnitRoot(cancellationToken);
                var propertyDeclarationSyntax = (PropertyDeclarationSyntax)compUnit.FindNode(propertySourceLocation.SourceSpan);

                if (propertyDeclarationSyntax.Initializer is not null)
                {
                    if (property.SetMethod.IsInitOnly)
                    {
                        if (comp.UnsafeAccessorAttributeType() is null)
                        {
                            return null;
                        }

                        defaultValueStrategy = DefaultValueStrategy.UnsafeAccessor;
                    }
                    else
                    {
                        defaultValueStrategy = DefaultValueStrategy.Setter;
                    }
                }
            }

            if (isOption)
            {
                if (shortName.HasValue)
                {
                    var snv = shortName.Value;

                    if (!char.IsLetter(snv) || !seenShortNames.Add(snv))
                    {
                        return null;
                    }
                }

                if (!hasLongNameFromAttribute)
                {
                    longName = propertyName.ToKebabCase();
                }

                if (!shortName.HasValue && longName is null)
                {
                    return null;
                }

                if (longName is not null)
                {
                    if (!longName.IsValidName() || !seenLongNames.Add(longName))
                    {
                        return null;
                    }
                }

                var (parseStrategy, isNullable, sequenceType, baseType) = GetParseStrategy(propertyType, comp);

                if (parseStrategy == ParseStrategy.None)
                {
                    return null;
                }

                if (isRequired && parseStrategy == ParseStrategy.Flag && !isNullable)
                {
                    return null;
                }

                optionsBuilder.Add(new(
                    propertyName,
                    baseType,
                    shortName,
                    longName,
                    parseStrategy,
                    isRequired,
                    isNullable,
                    sequenceType,
                    defaultValueStrategy,
                    helpDescription));
            }
            else if (isParameter)
            {
                var hasParameter = parameterMap.ContainsKey(parameterIndex);

                if (parameterIndex < 0 || hasParameter)
                {
                    return null;
                }

                parameterName ??= propertyName.ToKebabCase();

                if (!parameterName.IsValidName())
                {
                    return null;
                }

                var (parseStrategy, isNullable, sequenceType, baseType) = GetParseStrategy(propertyType, comp);
                if (parseStrategy == ParseStrategy.None || sequenceType != SequenceType.None)
                {
                    return null;
                }

                if (!hasParameter)
                {
                    var parameterInfo = new ParameterInfo(
                        parameterName,
                        propertyName,
                        baseType,
                        parseStrategy,
                        isRequired,
                        isNullable,
                        defaultValueStrategy,
                        helpDescription);

                    parameterMap.Add(parameterIndex, parameterInfo);
                }
            }
            else
            {
                Debug.Assert(isRemainingParameters);

                if (declaredRemainingParameters)
                {
                    return null;
                }

                declaredRemainingParameters = true;

                var (parseStrategy, _, sequenceType, baseType) = GetParseStrategy(propertyType, comp);
                if (parseStrategy == ParseStrategy.None || sequenceType == SequenceType.None)
                {
                    return null;
                }

                remainingParametersInfo = new(
                    propertyName,
                    baseType,
                    propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    parseStrategy,
                    sequenceType,
                    defaultValueStrategy,
                    helpDescription);
            }
        }

        var lastSeenIndex = 0;
        var parametersBuilder = ImmutableArray.CreateBuilder<ParameterInfo>();

        foreach (var pair in parameterMap.OrderBy(pair => pair.Key))
        {
            var index = pair.Key;

            if (index > (lastSeenIndex + 1))
            {
                return null;
            }

            var parameterInfo = pair.Value;
            parametersBuilder.Add(parameterInfo);
            lastSeenIndex = index;
        }

        var canNextParameterBeRequired = true;

        foreach (var info in parametersBuilder)
        {
            if (info.IsRequired)
            {
                if (!canNextParameterBeRequired)
                {
                    return null;
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

        return optionsInfo;

        static (ParseStrategy, bool IsNullable, SequenceType, string BaseType) GetParseStrategy(ITypeSymbol type, Compilation compilation)
        {
            var isNullable = false;
            var sequenceType = SequenceType.None;

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
                }
                else if (namedType is { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var nullableUnderlyingTypeSymbol] })
                {
                    isNullable = true;
                    type = nullableUnderlyingTypeSymbol;
                }
            }

            var parseStrategy = type.GetPrimaryParseStrategy(compilation);
            return (parseStrategy, isNullable, sequenceType, type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
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
