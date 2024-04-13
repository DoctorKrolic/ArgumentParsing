using System.Collections.Immutable;
using System.Globalization;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class TimeSpanParsingTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('t')]
        public TimeSpan TimeSpanOption { get; init; }

        [Option('n')]
        public TimeSpan? NullableTimeSpan { get; init; }

        [Option]
        public ImmutableArray<TimeSpan> TimeSpans { get; init; }

        [Parameter(0)]
        public TimeSpan TimeSpanParam { get; init; }

        [RemainingParameters]
        public ImmutableArray<TimeSpan> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlySpan<string> args);
    #endregion

    [Theory]
    [InlineData(new string[] { }, null, null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "18:00:00" }, "18:00:00", null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "-n", "00:00:00" }, "15:25:49", "00:00:00", new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "--time-spans", "00:00:00", "02:03:18" }, "15:25:49", null, new string[] { "00:00:00", "02:03:18" }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "--time-spans", "00:00:00", "--", "02:03:18" }, "15:25:49", null, new string[] { "00:00:00" }, "02:03:18", new string[] { })]
    [InlineData(new string[] { "-n", "15:25:49", "--time-spans", "00:00:00", "--", "02:03:18", "07:07:07" }, null, "15:25:49", new string[] { "00:00:00" }, "02:03:18", new string[] { "07:07:07" })]
    public void ParseCorrectArguments(string[] args, string? timeSpanOption, string? nullableTimeSpanOption, string[] dates, string? dateTimeParam, string[] remainingParams)
    {
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(timeSpanOption is null ? default : TimeSpan.Parse(timeSpanOption, CultureInfo.InvariantCulture), options.TimeSpanOption);
        Assert.Equal(nullableTimeSpanOption is null ? default(TimeSpan?) : TimeSpan.Parse(nullableTimeSpanOption, CultureInfo.InvariantCulture), options.NullableTimeSpan);

        var timeSpansAsserts = new Action<TimeSpan>[dates.Length];

        for (var i = 0; i < timeSpansAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            timeSpansAsserts[i] = (ts) => Assert.Equal(TimeSpan.Parse(dates[copy], CultureInfo.InvariantCulture), ts);
        }

        Assert.Collection(options.TimeSpans, timeSpansAsserts);

        Assert.Equal(dateTimeParam is null ? default : TimeSpan.Parse(dateTimeParam, CultureInfo.InvariantCulture), options.TimeSpanParam);

        var remainingParametersAsserts = new Action<TimeSpan>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (ts) => Assert.Equal(TimeSpan.Parse(remainingParams[copy], CultureInfo.InvariantCulture), ts);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }
}
