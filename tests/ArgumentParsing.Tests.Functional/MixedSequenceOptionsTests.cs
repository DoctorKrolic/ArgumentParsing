using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedSequenceOptionsTests
{
    #region OptionsAndParser
    private sealed class Options
    {
        [Option('s')]
        public IEnumerable<string> Strings { get; init; } = null!;

        [Option('i')]
        public IReadOnlyCollection<int> Ints { get; init; } = null!;

        [Option('d')]
        public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; init; } = null!;
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IReadOnlyList<string> args);
    #endregion

    [Theory]
    [InlineData("", null, null, null)]
    [InlineData("-s -i -d", null, null, null)]
    [InlineData("-s a -i 1 -d Monday", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday })]
    [InlineData("-sa -i1 -dMonday", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday })]
    [InlineData("-s a b -i 1 2 -d Monday Sunday", new string[] { "a", "b" }, new int[] { 1, 2 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday })]
    [InlineData("-s a b c -i 1 2 3 -d Monday Sunday Friday", new string[] { "a", "b", "c" }, new int[] { 1, 2, 3 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Friday })]
    [InlineData("--strings --ints --days-of-week", null, null, null)]
    [InlineData("--strings a --ints 1 --days-of-week Monday", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday })]
    [InlineData("--strings=a --ints=1 --days-of-week=Monday", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday })]
    [InlineData("--strings a b --ints 1 2 --days-of-week Monday Sunday", new string[] { "a", "b" }, new int[] { 1, 2 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday })]
    [InlineData("--strings a b c --ints 1 2 3 --days-of-week Monday Sunday Friday", new string[] { "a", "b", "c" }, new int[] { 1, 2, 3 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Friday })]
    public void ParseCorrectArguments(string argsString, string[]? strings, int[]? ints, DayOfWeek[]? daysOfWeek)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        strings ??= [];
        ints ??= [];
        daysOfWeek ??= [];

        var stringsAsserts = new Action<string>[strings.Length];
        var intsAsserts = new Action<int>[ints.Length];
        var daysOfWeekAsserts = new Action<DayOfWeek>[daysOfWeek.Length];

        for (var i = 0; i < stringsAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            stringsAsserts[i] = (s) => Assert.Equal(strings[copy], s);
        }

        for (var i = 0; i < intsAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            intsAsserts[i] = (i) => Assert.Equal(ints[copy], i);
        }

        for (var i = 0; i < daysOfWeekAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            daysOfWeekAsserts[i] = (d) => Assert.Equal(daysOfWeek[copy], d);
        }

        Assert.Collection(strings, stringsAsserts);
        Assert.Collection(ints, intsAsserts);
        Assert.Collection(daysOfWeek, daysOfWeekAsserts);

        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("-sa b", "b")]
    [InlineData("--days-of-week=Thursday Wednesday", "Wednesday")]
    public void UnrecognizedArgumentError(string argsString, string unrecognizedArgument)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var unrecognizedArgumentError = Assert.IsType<UnrecognizedArgumentError>(error);

        Assert.Equal(unrecognizedArgument, unrecognizedArgumentError.Argument);
    }

    [Theory]
    [InlineData("-s a b -s", "s")]
    [InlineData("--ints --ints", "ints")]
    [InlineData("-d Monday -s --days-of-week", "days-of-week")]
    [InlineData("--strings a --ints 5 -d -s b", "s")]
    [InlineData("--strings=a --ints 5 -d -sb", "s")]
    [InlineData("--ints=5 --ints=10", "ints")]
    public void DuplicateOptionError(string argsString, string optionName)
    {
        var args = argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);

        var error = Assert.Single(errors);
        var duplicateOptionError = Assert.IsType<DuplicateOptionError>(error);

        Assert.Equal(optionName, duplicateOptionError.OptionName);
    }
}
