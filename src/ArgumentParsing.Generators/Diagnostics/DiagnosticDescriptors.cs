using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Diagnostics;

public static class DiagnosticDescriptors
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
        title: "Invalid type of argument parser's parameter",
        messageFormat: "Parameter of argument parser must an enumerable collection of strings",
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
        title: "Invalid parser's return type",
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
        title: "Required field in options type is not allowed",
        messageFormat: "Required field is not allowed in options type",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RequiredPropertyMustParticipateInArgumentParsing = new(
        id: "ARGP0008",
        title: "Required property must participate in argument parsing",
        messageFormat: "Required property must participate in argument parsing, meaning that it must be marked wither with 'Option', 'Parameter' or 'RemainingParameters' attribute",
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

    public static readonly DiagnosticDescriptor PreferInitPropertyAccessor = new(
        id: "ARGP0011",
        title: "Prefer 'init' property accessor",
        messageFormat: "Prefer 'init' property accessor for parser-related properties",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidShortName = new(
        id: "ARGP0012",
        title: "Invalid short name of an option",
        messageFormat: "Short name '{0}' is invalid",
        description: "Short name of an option must be either empty (null) or a letter.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidLongName = new(
        id: "ARGP0013",
        title: "Invalid long name of an option",
        messageFormat: "Long name '{0}' is invalid",
        description: "Long name of an option must be either empty (null) or start with a letter and contain only letters, digits and possibly '-' as separators.",
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

    public static readonly DiagnosticDescriptor InvalidOptionPropertyType = new(
        id: "ARGP0016",
        title: "Invalid type of option property",
        messageFormat: "Type '{0}' is invalid for option property",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Implement as an analyzer:
    // ARGP0017 - suggest changing `[Required]` attribute to `required` property in case it is an option

    public static readonly DiagnosticDescriptor UnnecessaryRequiredAttribute = new(
        id: "ARGP0018",
        title: "Unnecessary [Required] attribute",
        messageFormat: "[Required] attribute is unnecessary since the property is already required",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary]);

    public static readonly DiagnosticDescriptor RequiredBoolOption = new(
        id: "ARGP0019",
        title: "Invalid required boolean option",
        messageFormat: "Having a required boolean option doesn't make sense",
        description: "Since boolean option is either set or not, making it required results in the only valid way, where the value of an option is always true, so it doesn't make sense to declare such option then.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RequiredNullableOption = new(
        id: "ARGP0020",
        title: "Required nullable option",
        messageFormat: "Having a required nullable option defeats the purpose of its nullability",
        description: "Since nullable option is null only if it is not present in arguments, making it required means it looses this ability.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PreferImmutableArrayAsSequenceType = new(
        id: "ARGP0021",
        title: "Prefer ImmutableArray<T> as a sequence type",
        messageFormat: "Prefer ImmutableArray<T> type for sequence options and remaining parameters",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NegativeParameterIndex = new(
        id: "ARGP0022",
        title: "Negative parameter index",
        messageFormat: "Parameter index cannot be negative",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateParameterIndex = new(
        id: "ARGP0023",
        title: "Parameter with this index is already defined",
        messageFormat: "Parameter with index '{0}' is already defined",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidParameterPropertyType = new(
        id: "ARGP0024",
        title: "Invalid type of parameter property",
        messageFormat: "Type '{0}' is invalid for parameter property",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingParameterWithIndex = new(
        id: "ARGP0025",
        title: "Missing parameter with index",
        messageFormat: "Parameter with index '{0}' is missing",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingParametersWithIndexes = new(
        id: "ARGP0026",
        title: "Missing parameters with indexes",
        messageFormat: "Parameters with indexes from '{0}' to '{1}' are missing",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RequiredCanOnlyBeFirstNParametersInARow = new(
        id: "ARGP0027",
        title: "Invalid required parameter",
        messageFormat: "Only first several parameters in a row can be required",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidParameterName = new(
        id: "ARGP0028",
        title: "Invalid parameter name",
        messageFormat: "Parameter name '{0}' is invalid",
        description: "Parameter name must start with a letter and contain only letters, digits and possibly '-' as separators.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateRemainingParameters = new(
        id: "ARGP0029",
        title: "Remaining parameters are already defined",
        messageFormat: "Property for remaining parameters is already defined",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidRemainingParametersPropertyType = new(
        id: "ARGP0030",
        title: "Invalid type of remaining parameters property",
        messageFormat: "Type '{0}' is invalid for remaining parameters property",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ARGP0031 - fade out `required` keyword or `[Required]` attribute for remaining parameters property. Implement when we can suppress nullability warning on that property

    public static readonly DiagnosticDescriptor TooLowAccessibilityOfOptionsType = new(
        id: "ARGP0032",
        title: "Options type has too low accessibility",
        messageFormat: "Options type has too low accessibility, so default '--help' command handler won't be generated for it",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoOptionNames = new(
        id: "ARGP0033",
        title: "Option doesn't have any name",
        messageFormat: "Option doesn't have both short and long name",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OptionsTypeMustBeAnnotatedWithAttribute = new(
        id: "ARGP0034",
        title: "Option type must be annotated with [OptionsType] attribute",
        messageFormat: "Option type must be annotated with [OptionsType] attribute",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ParserRelatedPropertyInNonOptionsType = new(
        id: "ARGP0035",
        title: "Usage of parser-related property in non-options type",
        messageFormat: "Property is annotated with parser-related attribute, but the containing type is not annotated with [OptionsType] attribute",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyCannotHaveMultipleParserRoles = new(
        id: "ARGP0036",
        title: "Property cannot have multiple parser roles",
        messageFormat: "Property cannot have multiple parser roles, meaning it cannot be annotated with multiple parser-related attributes",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
