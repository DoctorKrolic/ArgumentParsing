using System.Collections.Immutable;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class MixedSequenceOptionsTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        // Different types of sequences are intentional, do not replace them with immutable arrays

        [Option('s')]
        public IEnumerable<string> Strings { get; init; } = null!;

        [Option('i')]
        public IReadOnlyCollection<int> Ints { get; init; } = null!;

        [Option('d')]
        public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; init; } = null!;

        [Option('c')]
        public ImmutableArray<char> Chars { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(IReadOnlyList<string> args);
    #endregion

    [Theory]
    [InlineData("", null, null, null, null)]
    [InlineData("-s -i -d -c", null, null, null, null)]
    [InlineData("-s a -i 1 -d Monday -c c", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday }, new char[] { 'c' })]
    [InlineData("-sa -i1 -dMonday -c c", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday }, new char[] { 'c' })]
    [InlineData("-s a b -i 1 2 -d Monday Sunday -c c h", new string[] { "a", "b" }, new int[] { 1, 2 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday }, new char[] { 'c', 'h' })]
    [InlineData("-s a b c -i 1 2 3 -d Monday Sunday Friday -c c h a", new string[] { "a", "b", "c" }, new int[] { 1, 2, 3 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Friday }, new char[] { 'c', 'h', 'a' })]
    [InlineData("--strings --ints --days-of-week --chars", null, null, null, null)]
    [InlineData("--strings a --ints 1 --days-of-week Monday --chars c", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday }, new char[] { 'c' })]
    [InlineData("--strings=a --ints=1 --days-of-week=Monday --chars=c", new string[] { "a" }, new int[] { 1 }, new DayOfWeek[] { DayOfWeek.Monday }, new char[] { 'c' })]
    [InlineData("--strings a b --ints 1 2 --days-of-week Monday Sunday --chars c h", new string[] { "a", "b" }, new int[] { 1, 2 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday }, new char[] { 'c', 'h' })]
    [InlineData("--strings a b c --ints 1 2 3 --days-of-week Monday Sunday Friday --chars c h a", new string[] { "a", "b", "c" }, new int[] { 1, 2, 3 }, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Friday }, new char[] { 'c', 'h', 'a' })]
    public void ParseCorrectArguments(string argsString, string[]? strings, int[]? ints, DayOfWeek[]? daysOfWeek, char[]? chars)
    {
        var args = string.IsNullOrEmpty(argsString) ? [] : argsString.Split(' ');
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        strings ??= [];
        ints ??= [];
        daysOfWeek ??= [];
        chars ??= [];

        var stringsAsserts = new Action<string>[strings.Length];
        var intsAsserts = new Action<int>[ints.Length];
        var daysOfWeekAsserts = new Action<DayOfWeek>[daysOfWeek.Length];
        var charsAsserts = new Action<char>[chars.Length];

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

        for (var i = 0; i < charsAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            charsAsserts[i] = (c) => Assert.Equal(chars[copy], c);
        }

        Assert.Collection(strings, stringsAsserts);
        Assert.Collection(ints, intsAsserts);
        Assert.Collection(daysOfWeek, daysOfWeekAsserts);
        Assert.Collection(chars, charsAsserts);

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
