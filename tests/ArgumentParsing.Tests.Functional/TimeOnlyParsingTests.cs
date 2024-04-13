#if NET6_0_OR_GREATER
using System.Collections.Immutable;
using System.Globalization;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class TimeOnlyParsingTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('t')]
        public TimeOnly TimeOnlyOption { get; init; }

        [Option('n')]
        public TimeOnly? NullableTimeOnly { get; init; }

        [Option]
        public ImmutableArray<TimeOnly> Times { get; init; }

        [Parameter(0)]
        public TimeOnly TimeOnlyParam { get; init; }

        [RemainingParameters]
        public ImmutableArray<TimeOnly> RemainingParams { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(ReadOnlySpan<string> args);
    #endregion

    [Theory]
    [InlineData(new string[] { }, null, null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "18:00:00" }, "18:00:00", null, new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "-n", "00:00:00" }, "15:25:49", "00:00:00", new string[] { }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "--times", "00:00:00", "02:03:18" }, "15:25:49", null, new string[] { "00:00:00", "02:03:18" }, null, new string[] { })]
    [InlineData(new string[] { "-t", "15:25:49", "--times", "00:00:00", "--", "02:03:18" }, "15:25:49", null, new string[] { "00:00:00" }, "02:03:18", new string[] { })]
    [InlineData(new string[] { "-n", "15:25:49", "--times", "00:00:00", "--", "02:03:18", "07:07:07" }, null, "15:25:49", new string[] { "00:00:00" }, "02:03:18", new string[] { "07:07:07" })]
    public void ParseCorrectArguments(string[] args, string? timeOnlyOption, string? nullableTimeOnlyOption, string[] times, string? timeOnlyParam, string[] remainingParams)
    {
        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(timeOnlyOption is null ? default : TimeOnly.Parse(timeOnlyOption, CultureInfo.InvariantCulture), options.TimeOnlyOption);
        Assert.Equal(nullableTimeOnlyOption is null ? default(TimeOnly?) : TimeOnly.Parse(nullableTimeOnlyOption, CultureInfo.InvariantCulture), options.NullableTimeOnly);

        var timesAsserts = new Action<TimeOnly>[times.Length];

        for (var i = 0; i < timesAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            timesAsserts[i] = (to) => Assert.Equal(TimeOnly.Parse(times[copy], CultureInfo.InvariantCulture), to);
        }

        Assert.Collection(options.Times, timesAsserts);

        Assert.Equal(timeOnlyParam is null ? default : TimeOnly.Parse(timeOnlyParam, CultureInfo.InvariantCulture), options.TimeOnlyParam);

        var remainingParametersAsserts = new Action<TimeOnly>[remainingParams.Length];

        for (var i = 0; i < remainingParametersAsserts.Length; i++)
        {
            var copy = i; // Avoid closure
            remainingParametersAsserts[i] = (to) => Assert.Equal(TimeOnly.Parse(remainingParams[copy], CultureInfo.InvariantCulture), to);
        }

        Assert.Collection(options.RemainingParams, remainingParametersAsserts);

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }
}

#endif
