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
}
