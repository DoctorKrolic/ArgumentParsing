namespace ArgumentParsing.Results.Errors;

internal static class DefaultErrorMessageFormats
{
    public const string UnknownOptionError = "Unknown option '{0}' in argument '{1}'";
    public const string UnrecognizedArgumentError = "Unrecognized argument '{0}'";
    public const string OptionValueIsNotProvidedError = "No option value is provided after argument '{0}'";
}
