namespace ArgumentParsing.Tests.Functional.Utils;

public static class MyErrorMessageFormatProvider
{
    public const string UnknownOptionError = "[Error]: Unknown option '{0}' in argument '{1}'";
    public const string UnrecognizedArgumentError = "[Error]: Unrecognized argument '{0}'";
    public const string OptionValueIsNotProvidedError = "[Error]: No option value is provided after argument '{0}'";
    public const string DuplicateOptionError = "[Error]: Duplicate option '{0}'";
    public const string MissingRequiredOptionError_OnlyShortOptionName = "[Error]: Missing required option with short name '{0}'";
    public const string MissingRequiredOptionError_OnlyLongOptionName = "[Error]: Missing required option with long name '{0}'";
    public const string MissingRequiredOptionError_BothOptionNames = "[Error]: Missing required option '{0}' ('{1}')";
    public const string BadOptionValueFormatError = "[Error]: Value '{0}' is in incorrect format for option '{1}'";
    public const string FlagOptionValueError = "[Error]: Flag option '{0}' does not accept a value";
    public const string BadParameterValueFormatError = "[Error]: Value '{0}' is in incorrect format for parameter '{1}' (parameter index {2})";
    public const string MissingRequiredParameterError = "[Error]: Missing required parameter '{0}' (parameter index {1})";
    public const string BadRemainingParameterValueFormatError = "[Error]: Value '{0}' is in incorrect format for parameter at index {1}";
}
