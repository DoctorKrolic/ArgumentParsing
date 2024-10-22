using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;
using ArgumentParsing.Tests.Functional.Utils;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class ErrorMessageFormatProviderTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options1
    {
        [Option]
        public string? Opt1 { get; init; }

        [Option]
        public int Opt2 { get; init; }

        [Option]
        public bool Flag { get; init; }
    }

    [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof(MyErrorMessageFormatProvider))]
    private static partial ParseResult<Options1> ParseArguments1(ReadOnlySpan<string> args);

    [OptionsType]
    private sealed class Options2
    {
        [Option('o', null)]
        public required string Opt1 { get; init; }

        [Option]
        public required string Opt2 { get; init; }

        [Option('r', "opt3")]
        public required string Opt3 { get; init; }
    }

    [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof(MyErrorMessageFormatProvider))]
    private static partial ParseResult<Options2> ParseArguments2(Span<string> args);

    [OptionsType]
    private sealed class Options3
    {
        [Parameter(0)]
        public required int Param { get; init; }

        [RemainingParameters]
        public ImmutableArray<TimeSpan> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser(ErrorMessageFormatProvider = typeof(MyErrorMessageFormatProvider))]
    private static partial ParseResult<Options3> ParseArguments3(string[] args);
    #endregion

    [Fact]
    public void UnknownOptionError()
    {
        var result = ParseArguments1(["-d", "a"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unknownOptionError = Assert.IsType<UnknownOptionError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.UnknownOptionError, unknownOptionError.OptionName, unknownOptionError.ContainingArgument), unknownOptionError.GetMessage());
    }

    [Fact]
    public void UnrecognizedArgumentError()
    {
        var result = ParseArguments1(["a"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.UnrecognizedArgumentError, unrecognizedArgumentError.Argument), unrecognizedArgumentError.GetMessage());
    }

    [Fact]
    public void OptionValueIsNotProvidedError()
    {
        var result = ParseArguments1(["--opt1"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var optionValueIsNotProvidedError = Assert.IsType<OptionValueIsNotProvidedError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.OptionValueIsNotProvidedError, optionValueIsNotProvidedError.PrecedingArgument), optionValueIsNotProvidedError.GetMessage());
    }

    [Fact]
    public void DuplicateOptionError()
    {
        var result = ParseArguments1(["--opt1", "a", "--opt1", "b"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var duplicateOptionError = Assert.IsType<DuplicateOptionError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.DuplicateOptionError, duplicateOptionError.OptionName), duplicateOptionError.GetMessage());
    }

    [Fact]
    public void MissingRequiredOptionError_OnlyShortOptionName()
    {
        var result = ParseArguments2(["--opt2", "a", "--opt3", "b"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.MissingRequiredOptionError_OnlyShortOptionName, missingRequiredOptionError.ShortOptionName), missingRequiredOptionError.GetMessage());
    }

    [Fact]
    public void MissingRequiredOptionError_OnlyLongOptionName()
    {
        var result = ParseArguments2(["-o", "a", "--opt3", "b"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.MissingRequiredOptionError_OnlyLongOptionName, missingRequiredOptionError.LongOptionName), missingRequiredOptionError.GetMessage());
    }

    [Fact]
    public void MissingRequiredOptionError_BothOptionNames()
    {
        var result = ParseArguments2(["-o", "a", "--opt2", "b"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredOptionError = Assert.IsType<MissingRequiredOptionError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.MissingRequiredOptionError_BothOptionNames, missingRequiredOptionError.ShortOptionName, missingRequiredOptionError.LongOptionName), missingRequiredOptionError.GetMessage());
    }

    [Fact]
    public void BadOptionValueFormatError()
    {
        var result = ParseArguments1(["--opt2", "a"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.BadOptionValueFormatError, badOptionValueFormatError.Value, badOptionValueFormatError.OptionName), badOptionValueFormatError.GetMessage());
    }

    [Fact]
    public void FlagOptionValueError()
    {
        var result = ParseArguments1(["--flag", "true"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var flagOptionValueError = Assert.IsType<FlagOptionValueError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.FlagOptionValueError, flagOptionValueError.OptionName), flagOptionValueError.GetMessage());
    }

    [Fact]
    public void BadParameterValueFormatError()
    {
        var result = ParseArguments3(["a"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badParameterValueFormatError = Assert.IsType<BadParameterValueFormatError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.BadParameterValueFormatError, badParameterValueFormatError.Value, badParameterValueFormatError.ParameterName, badParameterValueFormatError.ParameterIndex), badParameterValueFormatError.GetMessage());
    }

    [Fact]
    public void MissingRequiredParameterError()
    {
        var result = ParseArguments3([]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var missingRequiredParameterError = Assert.IsType<MissingRequiredParameterError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.MissingRequiredParameterError, missingRequiredParameterError.ParameterName, missingRequiredParameterError.ParameterIndex), missingRequiredParameterError.GetMessage());
    }

    [Fact]
    public void BadRemainingParameterValueFormatError()
    {
        var result = ParseArguments3(["2", "a"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(error);

        Assert.Equal(string.Format(MyErrorMessageFormatProvider.BadRemainingParameterValueFormatError, badRemainingParameterValueFormatError.Value, badRemainingParameterValueFormatError.ParameterIndex), badRemainingParameterValueFormatError.GetMessage());
    }
}
