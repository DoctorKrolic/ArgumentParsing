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
        messageFormat: "Invalid options type",
        description: "Options type must be a non-special class or struct with an accessible parameterless constructor.",
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
        messageFormat: "Prefer 'init' accessor for parser-related property '{0}'",
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

    public static readonly DiagnosticDescriptor UseRequiredProperty = new(
        id: "ARGP0017",
        title: "Use required property",
        messageFormat: "Use required property instead of [Required] attribute to make parser-related member required",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

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

    public static readonly DiagnosticDescriptor RequiredRemainingParameters = new(
        id: "ARGP0031",
        title: "Required remaining parameters",
        messageFormat: "Making remaining parameters required is not necessary since they are always assigned by the parser",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary]);

    public static readonly DiagnosticDescriptor TooLowAccessibilityOfOptionsType = new(
        id: "ARGP0032",
        title: "Options type has too low accessibility",
        messageFormat: "Options type has too low accessibility, so 'ExecuteDefaults' method won't be generated for it",
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

    public static readonly DiagnosticDescriptor ParserArgumentIsASet = new(
        id: "ARGP0037",
        title: "Parser argument is a set",
        messageFormat: "Argument type of a parser is a set",
        description: "Since order of enumeration in sets is not strictly defined, passing as set as arguments collection may lead to unexpected parse results.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SpecialCommandHandlerShouldBeClass = new(
        id: "ARGP0038",
        title: "Special command handler should be a class",
        messageFormat: "Make special command handler a class for better performance",
        description: "Special command handlers are used as interfaces in argument parsing, thus having a special command handler as a struct results in unnecessary boxing without any benefits. Consider changing command handler declaration to be a class.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SpecialCommandHandlerMustHaveAliases = new(
        id: "ARGP0039",
        title: "Special command handler must declare aliases",
        messageFormat: "Special command handler must declare at least one alias",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidSpecialCommandAlias = new(
        id: "ARGP0040",
        title: "Invalid alias of a special command",
        messageFormat: "Special command alias '{0}' is invalid",
        description: "Alias of a special command must contain only letters, digits and dashes.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AliasShouldStartWithDash = new(
        id: "ARGP0041",
        title: "Alias of a special command should start with a dash",
        messageFormat: "Alias of a special command should start with a dash or 2 dashes",
        description: "Special command aliases should start with dashes. If you want a normal command, you should use a verb.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SpecialCommandHandlerMustHaveAccessibleParameterlessConstructor = new(
        id: "ARGP0042",
        title: "Special command handler must have an accessible parameterless constructor",
        messageFormat: "Special command handler must have an accessible parameterless constructor",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidSpecialCommandHandlerTypeSpecifier = new(
        id: "ARGP0043",
        title: "Invalid special command handler type specifier",
        messageFormat: "'{0}' is an invalid special command handler specifier",
        description: "Special command handler specifier must be a non-null type, which implements 'ISpecialCommandHandler' interface.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MarkerAttributeOnNonOptionsType = new(
        id: "ARGP0044",
        title: "Usage of marker attribute on non-options type",
        messageFormat: "[{0}] attribute is applied to non-options type",
        description: "This marker attribute only has effect when applied to an options type. Consider annotating target type with [OptionsType] attribute.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidHelpTextGeneratorTypeSpecifier = new(
        id: "ARGP0045",
        title: "Invalid help text generator type specifier",
        messageFormat: "'{0}' is an invalid help text generator specifier",
        description: "Help text generator specifier must be a non-null non-special named type. In case it is a generic type, it must have all its type arguments specified.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidIdentifierName = new(
        id: "ARGP0046",
        title: "Invalid identifier name",
        messageFormat: "'{0}' is an invalid identifier name",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CannotFindHelpTextGeneratorMethod = new(
        id: "ARGP0047",
        title: "Cannot find help text generator method",
        messageFormat: "Unable to find accessible method '{0}' with the expected signature",
        description: "Valid help text generator method is a static method with return type 'string' and a single parameter of 'ParseErrorCollection' type with explicit default value of 'null'.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OptionsTypeHasHelpTextGeneratorButNoHelpCommandHandlerInParser = new(
        id: "ARGP0048",
        title: "Associated options type has a custom help text generator, but no '--help' special command handler found for the parser",
        messageFormat: "Associated options type '{0}' has a custom help text generator, but no '--help' special command handler found for the parser",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateSpecialCommand = new(
        id: "ARGP0049",
        title: "Duplicate special command",
        messageFormat: "Special command '{0}' is already defined for this parser",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BuiltInCommandHelpInfoNeedsSpecificHandler = new(
        id: "ARGP0050",
        title: "'[BuiltInCommandHelpInfo]' attribute needs one specific command handler",
        messageFormat: "'[BuiltInCommandHelpInfo]' attribute needs one specific command handler as its first constructor argument",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnnecessaryBuiltInCommandHelpInfo = new(
        id: "ARGP0051",
        title: "Unnecessary [BuiltInCommandHelpInfo] attribute",
        messageFormat: "[BuiltInCommandHelpInfo] attribute for '{0}' is unnecessary since the parser doesn't include this command",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary]);

    public static readonly DiagnosticDescriptor DuplicateBuiltInCommandHelpInfo = new(
        id: "ARGP0052",
        title: "Duplicate '[BuiltInCommandHelpInfo]' attribute",
        messageFormat: "Duplicate '[BuiltInCommandHelpInfo]' attribute for '{0}'",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidErrorMessageFormatProviderTypeSpecifier = new(
        id: "ARGP0053",
        title: "Invalid error message format provider type specifier",
        messageFormat: "'{0}' is an invalid error message format provider specifier",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ImplementISpanParsable = new(
        id: "ARGP0054",
        title: "Implement 'ISpanParsable' interface to improve performance",
        messageFormat: "For better performance in parsing member '{0}' of options type '{1}', implement the 'ISpanParsable' interface on '{2}'",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CannotHaveInitAccessorWithADefaultValue = new(
        id: "ARGP0055",
        title: "'init' accessor cannot be used here",
        messageFormat: "'init' accessor cannot be used for parser-related property '{0}', use 'set' accessor instead",
        description: "Parser uses unsafe accessors to conditionally assign an init-only property with a default value. Current runtime doesn't support unsafe accessors therefore 'init' cannot be used.",
        category: ArgumentParsingCategoryName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
