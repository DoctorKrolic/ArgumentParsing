#if NET6_0_OR_GREATER
using System.Collections.Immutable;
using System.Globalization;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class DateOnlyParsingTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('d')]
        public DateOnly DateOnlyOption { get; init; }

        [Option('n')]
        public DateOnly? NullableDateOnly { get; init; }

        [Option]
        public ImmutableArray<DateOnly> Dates { get; init; }

        [Parameter(0)]
        public DateOnly DateOnlyParam { get; init; }

        [RemainingParameters]
        public ImmutableArray<DateOnly> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlySpan<string> args);
    #endregion

    [Theory]
    [InlineData(new string[] { }, null, null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "03.03.2023" }, "03.03.2023", null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036", "-n", "12.09.1973" }, "01.12.2036", "12.09.1973", new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036", "--dates", "12.09.1973", "11.02.2027" }, "01.12.2036", null, new string[] { "12.09.1973", "11.02.2027" }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036", "--dates", "12.09.1973", "--", "11.02.2027" }, "01.12.2036", null, new string[] { "12.09.1973" }, "11.02.2027", new string[] { })]
    [InlineData(new string[] { "-n", "01.12.2036", "--dates", "12.09.1973", "--", "11.02.2027", "07.07.0707" }, null, "01.12.2036", new string[] { "12.09.1973" }, "11.02.2027", new string[] { "07.07.0707" })]
    public void ParseCorrectArguments(string[] args, string? dateOnlyOption, string? nullableDateOnlyOption, string[] dates, string? dateOnlyParam, string[] remainingParams)
    {
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(dateOnlyOption is null ? default : DateOnly.Parse(dateOnlyOption, CultureInfo.InvariantCulture), options.DateOnlyOption);
        Assert.Equal(nullableDateOnlyOption is null ? default(DateOnly?) : DateOnly.Parse(nullableDateOnlyOption, CultureInfo.InvariantCulture), options.NullableDateOnly);

        var datesAsserts = new Action<DateOnly>[dates.Length];

        for (var i = 0; i < datesAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            datesAsserts[i] = (dt) => Assert.Equal(DateOnly.Parse(dates[copy], CultureInfo.InvariantCulture), dt);
        }

        Assert.Collection(options.Dates, datesAsserts);

        Assert.Equal(dateOnlyParam is null ? default : DateOnly.Parse(dateOnlyParam, CultureInfo.InvariantCulture), options.DateOnlyParam);

        var remainingParametersAsserts = new Action<DateOnly>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (dt) => Assert.Equal(DateOnly.Parse(remainingParams[copy], CultureInfo.InvariantCulture), dt);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }
}
#endif
