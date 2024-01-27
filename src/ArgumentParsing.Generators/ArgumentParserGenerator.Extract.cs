using System.Collections.Immutable;
using System.Diagnostics;
using ArgumentParsing.Generators.Diagnostics;
using ArgumentParsing.Generators.Extensions;
using ArgumentParsing.Generators.Models;
using ArgumentParsing.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static (ArgumentParserInfo? ArgumentParserInfo, ImmutableEquatableArray<DiagnosticInfo> Diagnostics) Extract(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var argumentParserMethodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var argumentParserMethodSymbol = (IMethodSymbol)context.TargetSymbol;

        var diagnosticsBuilder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        SimpleParameterInfo? parameterInfo = null;
        INamedTypeSymbol? validOptionsType = null;

        var hasErrors = false;

        if (argumentParserMethodSymbol.Parameters is not [var singleParameter])
        {
            hasErrors = true;
            diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidParserParameterCount, argumentParserMethodSyntax.Identifier));
        }
        else
        {
            var singleParameterSyntax = argumentParserMethodSyntax.ParameterList.Parameters[0];

            if (singleParameter.IsParams ||
                singleParameter.RefKind != RefKind.None ||
                singleParameter.ScopedKind != ScopedKind.None ||
                argumentParserMethodSymbol.IsExtensionMethod)
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidArgsParameter, singleParameterSyntax));
            }

            var singleParameterType = singleParameter.Type;

            if (!IsEnumerableCollectionOfStrings(singleParameterType))
            {
                hasErrors = true;

                if (singleParameterType.TypeKind != TypeKind.Error)
                {
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidArgsParameterType, singleParameterSyntax.Type!));
                }
            }

            if (!hasErrors && singleParameter.Name != "args")
            {
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.PreferArgsParameterName, singleParameterSyntax.Identifier));
            }

            parameterInfo = new(singleParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), singleParameter.Name);

            static bool IsEnumerableCollectionOfStrings(ITypeSymbol type)
            {
                return HasMember(type, "GetEnumerator", static e => e is IMethodSymbol { Parameters.Length: 0 } getEnumeratorMethod && IsValidStringEnumerator(getEnumeratorMethod.ReturnType));
            }

            static bool IsValidStringEnumerator(ITypeSymbol type)
            {
                return HasMember(type, "MoveNext", static m => m is IMethodSymbol { Parameters.Length: 0, ReturnType.SpecialType: SpecialType.System_Boolean }) &&
                       HasMember(type, "Current", static c => c is IPropertySymbol { Type.SpecialType: SpecialType.System_String, GetMethod: not null });
            }

            static bool HasMember(ITypeSymbol type, string memberName, Func<ISymbol, bool> predicate)
            {
                if (type.GetMembers(memberName).Any(predicate))
                {
                    return true;
                }

                if (type.BaseType is { } baseType && HasMember(baseType, memberName, predicate))
                {
                    return true;
                }

                foreach (var @interface in type.AllInterfaces)
                {
                    if (HasMember(@interface, memberName, predicate))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        var returnTypeSyntax = argumentParserMethodSyntax.ReturnType;
        var returnType = argumentParserMethodSymbol.ReturnType;

        if (returnType is not INamedTypeSymbol { Name: "ParseResult", TypeArguments: [var optionsType], ContainingNamespace: { Name: "Results", ContainingNamespace: { Name: "ArgumentParsing", ContainingNamespace.IsGlobalNamespace: true } } })
        {
            hasErrors = true;

            if (returnType.TypeKind != TypeKind.Error)
            {
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.ReturnTypeMustBeParseResult, returnTypeSyntax));
            }
        }
        else
        {
            if (optionsType is not INamedTypeSymbol { SpecialType: SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct } namedOptionsType || !namedOptionsType.Constructors.Any(c => c.Parameters.Length == 0))
            {
                hasErrors = true;

                if (optionsType.TypeKind != TypeKind.Error)
                {
                    var errorNode = returnTypeSyntax is GenericNameSyntax { TypeArgumentList.Arguments: [var genericArgument] } genericName ? genericArgument : returnTypeSyntax;
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidOptionsType, errorNode));
                }
            }
            else
            {
                validOptionsType = namedOptionsType;
            }
        }

        if (validOptionsType is null)
        {
            return (null, diagnosticsBuilder.ToImmutable());
        }

        var (optionsInfo, optionsDiagnostics) = AnalyzeOptionsType(validOptionsType, context.SemanticModel.Compilation);
        hasErrors |= optionsInfo is null;
        diagnosticsBuilder.AddRange(optionsDiagnostics);

        if (hasErrors)
        {
            return (null, diagnosticsBuilder.ToImmutable());
        }

        Debug.Assert(parameterInfo is not null);

        var methodInfo = new ArgumentParserMethodInfo(
            argumentParserMethodSyntax.Modifiers.ToString(),
            argumentParserMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            argumentParserMethodSymbol.Name,
            parameterInfo!);

        var argumentParserInfo = new ArgumentParserInfo(
            HierarchyInfo.From(argumentParserMethodSymbol.ContainingType),
            methodInfo,
            optionsInfo!);

        return (argumentParserInfo, diagnosticsBuilder.ToImmutable());
    }

    private static (OptionsInfo? OptionsInfo, ImmutableArray<DiagnosticInfo> Diagnostics) AnalyzeOptionsType(INamedTypeSymbol optionsType, Compilation compilation)
    {
        var optionsBuilder = ImmutableArray.CreateBuilder<OptionInfo>();
        var diagnosticsBuilder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        var hasErrors = false;

        var seenShortNames = new HashSet<char>();
        var seenLongNames = new HashSet<string>();

        var firstPropertyOfShortNameWithNoError = new Dictionary<char, IPropertySymbol>();
        var firstPropertyOfLongNameWithNoError = new Dictionary<string, IPropertySymbol>();

        foreach (var member in optionsType.GetMembers())
        {
            if (member is IFieldSymbol { IsRequired: true })
            {
                hasErrors = true;

                if (member.DeclaredAccessibility >= optionsType.DeclaredAccessibility)
                {
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.RequiredFieldInOptionsTypeIsNotAllowed, member));
                }
            }

            if (member is not IPropertySymbol property)
            {
                continue;
            }

            var hasOptionAttribute = false;
            var hasRequiredAttribute = false;
            var hasLongNameFromAttribute = false;

            char? shortName = null;
            string? longName = null;

            var optionAttributeType = compilation.GetTypeByMetadataName("ArgumentParsing.OptionAttribute")!;
            var requiredAttributeType = compilation.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.RequiredAttribute")!;

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

                if (!attributeClass.Equals(optionAttributeType, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                hasOptionAttribute = true;

                foreach (var constructorArg in attr.ConstructorArguments)
                {
                    var argType = constructorArg.Type;
                    var argValue = constructorArg.Value;

                    if (argType is null)
                    {
                        continue;
                    }

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
            }

            if (!hasOptionAttribute)
            {
                if (property.IsRequired)
                {
                    hasErrors = true;

                    if (property.SetMethod is not null &&
                        property.DeclaredAccessibility >= optionsType.DeclaredAccessibility &&
                        property.SetMethod.DeclaredAccessibility >= optionsType.DeclaredAccessibility)
                    {
                        diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.RequiredPropertyMustBeOptionOrValue, property));
                    }
                }

                continue;
            }

            if (property.DeclaredAccessibility < Accessibility.Internal)
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.PropertyIsNotAccessible, property));
            }
            else if (property is not { SetMethod.DeclaredAccessibility: > Accessibility.Internal })
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.PropertyMustHaveAccessibleSetter, (ISymbol?)property.SetMethod ?? property));
            }

            var propertyName = property.Name;

            if (shortName.HasValue && !char.IsLetter(shortName.Value))
            {
                hasErrors = true;

                // '\uffff' is a special value which is produced when user doesn't specify any char ('').
                // This will be errored by the C# compiler, so we don't want to add our custom diagnostic
                if (shortName.Value != '\uffff')
                {
                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidShortName, property, shortName!.Value));
                }
            }
            else if (shortName.HasValue)
            {
                var sn = shortName.Value;

                if (seenShortNames.Add(sn))
                {
                    firstPropertyOfShortNameWithNoError.Add(sn, property);
                }
                else
                {
                    hasErrors = true;

                    if (firstPropertyOfShortNameWithNoError.TryGetValue(sn, out var previousProperty))
                    {
                        diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateShortName, previousProperty, sn.ToString()));
                        firstPropertyOfShortNameWithNoError.Remove(sn);
                    }

                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateShortName, property, sn.ToString()));
                }
            }

            if (!hasLongNameFromAttribute)
            {
                longName = propertyName.ToKebabCase();
            }

            if (longName is not null &&
                !(char.IsLetter(longName[0]) && longName.Replace("-", string.Empty).All(char.IsLetterOrDigit)))
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidLongName, property, longName!));
            }
            else if (longName is not null)
            {
                if (seenLongNames.Add(longName))
                {
                    firstPropertyOfLongNameWithNoError.Add(longName, property);
                }
                else
                {
                    hasErrors = true;

                    if (firstPropertyOfLongNameWithNoError.TryGetValue(longName, out var previousProperty))
                    {
                        diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateLongName, previousProperty, longName));
                        firstPropertyOfLongNameWithNoError.Remove(longName);
                    }

                    diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateLongName, property, longName));
                }
            }

            var possibleParseStrategy = GetPotentialParseStrategy(property.Type);

            if (!possibleParseStrategy.HasValue)
            {
                hasErrors = true;

                var propertySyntax = (BasePropertyDeclarationSyntax?)property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                var diagnosticLocation = propertySyntax?.Type.GetLocation() ?? property.Locations.First();

                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidOptionPropertyType, diagnosticLocation, property.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                continue;
            }

            var parseStrategy = possibleParseStrategy.Value;
            var isRequired = hasRequiredAttribute || property.IsRequired;

            if (isRequired && parseStrategy == ParseStrategy.Flag)
            {
                hasErrors = true;
                diagnosticsBuilder.Add(DiagnosticInfo.Create(DiagnosticDescriptors.RequiredBoolOption, property));
            }

            optionsBuilder.Add(new(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                shortName,
                longName,
                parseStrategy,
                isRequired));
        }

        if (hasErrors)
        {
            return (null, diagnosticsBuilder.ToImmutable());
        }

        var optionsInfo = new OptionsInfo(
            optionsType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
            optionsBuilder.ToImmutable());

        return (optionsInfo, diagnosticsBuilder.ToImmutable());

        static ParseStrategy? GetPotentialParseStrategy(ITypeSymbol type) => type switch
        {
            { TypeKind: TypeKind.Enum } => ParseStrategy.Enum,
            { SpecialType: SpecialType.System_String } => ParseStrategy.String,
            {
                SpecialType: SpecialType.System_Byte or
                             SpecialType.System_SByte or
                             SpecialType.System_Int16 or
                             SpecialType.System_UInt16 or
                             SpecialType.System_Int32 or
                             SpecialType.System_UInt32 or
                             SpecialType.System_Int64 or
                             SpecialType.System_UInt64
            } or { Name: "BigInteger", ContainingNamespace: { Name: "Numerics", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } => ParseStrategy.Integer,
            { SpecialType: SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal } => ParseStrategy.Float,
            { SpecialType: SpecialType.System_Boolean } => ParseStrategy.Flag,
            _ => null,
        };
    }
}
