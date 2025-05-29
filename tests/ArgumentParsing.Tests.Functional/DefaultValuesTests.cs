using System.Collections.Immutable;
using System.Globalization;
using ArgumentParsing.Results;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class DefaultValuesTests
{
    private const string StringOptionDefaultValue = "Default value";

    #region OptionsAndParser
    [OptionsType]
    private sealed class Options
    {
        [Option('i')]
        public int IntOption { get; set; } = 2;

        [Option('s')]
        public string StringOption { get; set; } = StringOptionDefaultValue;

#if NET8_0_OR_GREATER
        [Option('l')]
        public long InitLongOption { get; init; } = 45L;
#endif

        [Option('c')]
        public ImmutableArray<int> CollectionOption { get; set; } = [1, 2, 3];

        [Parameter(0)]
        public TimeSpan Param1 { get; set; } = TimeSpan.FromSeconds(1);

#if NET8_0_OR_GREATER
        [Parameter(1)]
        public DateOnly Param2 { get; init; } = DateOnly.MinValue;

        [Parameter(2)]
#else
        [Parameter(1)]
#endif
        public DayOfWeek Param3 { get; set; } = DayOfWeek.Monday;

        [RemainingParameters]
        public ImmutableArray<string> RemainingParameters
        {
            get;
#if NET8_0_OR_GREATER
            init;
#else
            set;
#endif
        } = ["one", "two", "three"];
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<Options> ParseArguments(List<string> args);
    #endregion

    [Fact]
    public void KeepDefaultValuesIfNoOverridesProvided()
    {
        var result = ParseArguments([]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(2, options.IntOption);
        Assert.Equal(StringOptionDefaultValue, options.StringOption);

#if NET8_0_OR_GREATER
        Assert.Equal(45L, options.InitLongOption);
#endif

        Assert.Collection(options.CollectionOption,
            static e1 => Assert.Equal(1, e1),
            static e2 => Assert.Equal(2, e2),
            static e3 => Assert.Equal(3, e3));

        Assert.Equal(TimeSpan.FromSeconds(1), options.Param1);

#if NET8_0_OR_GREATER
        Assert.Equal(DateOnly.MinValue, options.Param2);
#endif

        Assert.Equal(DayOfWeek.Monday, options.Param3);

        Assert.Collection(options.RemainingParameters,
            static e1 => Assert.Equal("one", e1),
            static e2 => Assert.Equal("two", e2),
            static e3 => Assert.Equal("three", e3));
    }

    [Fact]
    public void OverrideDefaultValues()
    {
        List<string> args = ["-i", "5", "-s", "abc", "-c", "3", "8", "9"];
#if NET8_0_OR_GREATER
        args.Add("-l");
        args.Add("200");
#endif

        args.Add("--");
        args.Add("00:10:00");

#if NET8_0_OR_GREATER
        args.Add("10.10.1995");
#endif

        args.Add("Friday");

        args.Add("new");
        args.Add("remaining");
        args.Add("parameters");

        var result = ParseArguments(args);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(5, options.IntOption);
        Assert.Equal("abc", options.StringOption);

#if NET8_0_OR_GREATER
        Assert.Equal(200, options.InitLongOption);
#endif

        Assert.Collection(options.CollectionOption,
            static e1 => Assert.Equal(3, e1),
            static e2 => Assert.Equal(8, e2),
            static e3 => Assert.Equal(9, e3));

        Assert.Equal(TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), options.Param1);

#if NET8_0_OR_GREATER
        Assert.Equal(DateOnly.Parse("10.10.1995", CultureInfo.InvariantCulture), options.Param2);
#endif

        Assert.Equal(DayOfWeek.Friday, options.Param3);

        Assert.Collection(options.RemainingParameters,
            static e1 => Assert.Equal("new", e1),
            static e2 => Assert.Equal("remaining", e2),
            static e3 => Assert.Equal("parameters", e3));
    }
}
