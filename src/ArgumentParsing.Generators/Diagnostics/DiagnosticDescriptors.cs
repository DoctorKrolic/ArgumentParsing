using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Diagnostics;

internal static class DiagnosticDescriptors
{
    private const string ArgumentParsingCategoryName = "ArgumentParsing";

    public static readonly DiagnosticDescriptor InvalidParserParameterCount = new(
        id: "ARGP0001",
        title: "Invalid parameter count for argument parser method",
        messageFormat: "Argument parser must have exactly 1 parameter, represented as an enumerable collection of strings",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidArgsParameter = new(
        id: "ARGP0002",
        title: "Invalid parameter of an argument parser",
        messageFormat: "Parameter of an argument parser must be a simple parameter",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidArgsParameterType = new(
        id: "ARGP0003",
        title: "Invalid type of an argument parser's parameter",
        messageFormat: "Parameter of an argument parser must an enumerable collection of strings",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PreferArgsParameterName = new(
        id: "ARGP0004",
        title: "Prefer name 'args' for argument parser's parameter",
        messageFormat: "Follow the convention and prefer 'args' as a name for argument parser's parameter",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ReturnTypeMustBeParseResult = new(
        id: "ARGP0005",
        title: "Invalid return type",
        messageFormat: "Return type of an argument parser must be a ParseResult<T>",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidOptionsType = new(
        id: "ARGP0006",
        title: "Invalid options type",
        messageFormat: "Options type must be a non-special class or struct with a parameterless constructor",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RequiredFieldInOptionsTypeIsNotAllowed = new(
        id: "ARGP0007",
        title: "Required field is not allowed",
        messageFormat: "Required fields are not allowed in options type",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RequiredPropertyMustBeOptionOrValue = new(
        id: "ARGP0008",
        title: "Required property must be option or value",
        messageFormat: "Required property must be marked wither with 'Option' or 'Value' attribute",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyIsNotAccessible = new(
        id: "ARGP0009",
        title: "Property is not accessible",
        messageFormat: "In order for the property to be used in argument parsing it must have at least 'internal' accessibility",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyMustHaveAccessibleSetter = new(
        id: "ARGP0010",
        title: "Property must have accessible setter",
        messageFormat: "In order for the property to be used in argument parsing it must have a setter with at least 'internal' accessibility",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Reserve ARGP0011 to suggest to use `init` for option property if possible (will be implemented as an analyzer)

    public static readonly DiagnosticDescriptor InvalidShortName = new(
        id: "ARGP0012",
        title: "Invalid short name of an option",
        messageFormat: "Short name '{0}' is invalid. Short name of an option must be either empty (null) or a letter",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidLongName = new(
        id: "ARGP0013",
        title: "Invalid long name of an option",
        messageFormat: "Long name '{0}' is invalid. Long name of an option must be either empty (null) or start with a letter and contain only letters, digits and possibly '-' as separators",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateShortName = new(
        id: "ARGP0014",
        title: "Option with such short name is already defined",
        messageFormat: "Option with short name '{0}' is already defined",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateLongName = new(
        id: "ARGP0015",
        title: "Option with such long name is already defined",
        messageFormat: "Option with long name '{0}' is already defined",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidPropertyType = new(
        id: "ARGP0016",
        title: "Invalid property type",
        messageFormat: "Type '{0}' is invalid for property used in argument parsing",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
