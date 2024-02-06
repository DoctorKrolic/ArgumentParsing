namespace ArgumentParsing.Results.Errors;

internal static class DefaultErrorMessageFormats
{
    public const string UnknownOptionError = "Unknown option '{0}' in argument '{1}'";
    public const string UnrecognizedArgumentError = "Unrecognized argument '{0}'";
    public const string OptionValueIsNotProvidedError = "No option value is provided after argument '{0}'";
    public const string DuplicateOptionError = "Duplicate option '{0}'";
    public const string MissingRequiredOptionError_OneOptionName = "Missing required option '{0}'";
    public const string MissingRequiredOptionError_BothOptionNames = "Missing required option '{0}' ('{1}')";
    public const string BadOptionValueFormatError = "Value '{0}' is in incorrect format for option '{1}'";
    public const string FlagOptionValueError = "Flag option '{0}' does not accept a value";
    public const string BadParameterValueFormatError = "Value '{0}' is in incorrect format for parameter '{1}' (parameter index '{2}')";
    public const string MissingRequiredParameterError = "Missing required parameter '{0}' (parameter index '{1}')";
    public const string BadRemainingParameterValueFormatError = "Value '{0}' is in incorrect format for parameter at index '{1}'";
}
