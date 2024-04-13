using System.Collections.Immutable;
using System.Globalization;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class DateTimeParsingTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('d')]
        public DateTime DateTimeOption { get; init; }

        [Option('n')]
        public DateTime? NullableDateTime { get; init; }

        [Option]
        public ImmutableArray<DateTime> Dates { get; init; }

        [Parameter(0)]
        public DateTime DateTimeParam { get; init; }

        [RemainingParameters]
        public ImmutableArray<DateTime> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlySpan<string> args);
    #endregion

    [Theory]
    [InlineData(new string[] { }, null, null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "03.03.2023 18:00:00" }, "03.03.2023 18:00:00", null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036 15:25:49", "-n", "12.09.1973 00:00:00" }, "01.12.2036 15:25:49", "12.09.1973 00:00:00", new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036 15:25:49", "--dates", "12.09.1973 00:00:00", "11.02.2027 02:03:18" }, "01.12.2036 15:25:49", null, new string[] { "12.09.1973 00:00:00", "11.02.2027 02:03:18" }, null, new string[] { })]
    [InlineData(new string[] { "-d", "01.12.2036 15:25:49", "--dates", "12.09.1973 00:00:00", "--", "11.02.2027 02:03:18" }, "01.12.2036 15:25:49", null, new string[] { "12.09.1973 00:00:00" }, "11.02.2027 02:03:18", new string[] { })]
    [InlineData(new string[] { "-n", "01.12.2036 15:25:49", "--dates", "12.09.1973 00:00:00", "--", "11.02.2027 02:03:18", "07.07.0707 07:07:07" }, null, "01.12.2036 15:25:49", new string[] { "12.09.1973 00:00:00" }, "11.02.2027 02:03:18", new string[] { "07.07.0707 07:07:07" })]
    public void ParseCorrectArguments(string[] args, string? dateTimeOption, string? nullableDateTimeOption, string[] dates, string? dateTimeParam, string[] remainingParams)
    {
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(dateTimeOption is null ? default : DateTime.Parse(dateTimeOption, CultureInfo.InvariantCulture), options.DateTimeOption);
        Assert.Equal(nullableDateTimeOption is null ? default(DateTime?) : DateTime.Parse(nullableDateTimeOption, CultureInfo.InvariantCulture), options.NullableDateTime);

        var datesAsserts = new Action<DateTime>[dates.Length];

        for (var i = 0; i < datesAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            datesAsserts[i] = (dt) => Assert.Equal(DateTime.Parse(dates[copy], CultureInfo.InvariantCulture), dt);
        }

        Assert.Collection(options.Dates, datesAsserts);

        Assert.Equal(dateTimeParam is null ? default : DateTime.Parse(dateTimeParam, CultureInfo.InvariantCulture), options.DateTimeParam);

        var remainingParametersAsserts = new Action<DateTime>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (dt) => Assert.Equal(DateTime.Parse(remainingParams[copy], CultureInfo.InvariantCulture), dt);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }
}
