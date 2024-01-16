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
}
