#if NET7_0_OR_GREATER
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ArgumentParsing.Results;
using ArgumentParsing.Results.Errors;

namespace ArgumentParsing.Tests.Functional;

public sealed partial class GenericParsableTypesTests
{
    #region OptionsAndParser
    [OptionsType]
    private sealed class SpanParsableOptions
    {
        [Option]
        public MySpanParsableType Option { get; init; }

        [Option]
        public ImmutableArray<MySpanParsableType> SequenceOption { get; init; }

        [Parameter(0)]
        public MySpanParsableType Parameter { get; init; }

        [RemainingParameters]
        public ImmutableArray<MySpanParsableType> RemainingParameters { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<SpanParsableOptions> ParseSpanOptionArguments(ReadOnlySpan<string> args);

    [DebuggerDisplay("MySpanParsableType \\{ Data = {Data} \\}")]
    public readonly struct MySpanParsableType : ISpanParsable<MySpanParsableType>
    {
        public int Data { get; init; }

        public static MySpanParsableType Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => default;

        public static MySpanParsableType Parse(string s, IFormatProvider? provider)
            => Parse(s.AsSpan(), provider);

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out MySpanParsableType result)
        {
            if (!int.TryParse(s, provider, out var data))
            {
                result = default;
                return false;
            }

            result = new() { Data = data };
            return true;
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MySpanParsableType result)
            => TryParse(s.AsSpan(), provider, out result);
    }

    [OptionsType]
    private sealed class ParsableOptions
    {
        [Option]
        public MyParsableType Option { get; init; }

        [Option]
        public ImmutableArray<MyParsableType> SequenceOption { get; init; }

        [Parameter(0)]
        public MyParsableType Parameter { get; init; }

        [RemainingParameters]
        public ImmutableArray<MyParsableType> RemainingParameters { get; init; }
    }

    [GeneratedArgumentParser]
    private static partial ParseResult<ParsableOptions> ParseOptionArguments(string[] args);

    [DebuggerDisplay("MyParsableType \\{ Data = {Data} \\}")]
    public readonly struct MyParsableType : IParsable<MyParsableType>
    {
        public int Data { get; init; }

        public static MyParsableType Parse(string s, IFormatProvider? provider) => default;

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MyParsableType result)
        {
            if (!int.TryParse(s, provider, out var data))
            {
                result = default;
                return false;
            }

            result = new() { Data = data };
            return true;
        }
    }
    #endregion

    [Fact]
    public void ParseSpanOptions()
    {
        var result = ParseSpanOptionArguments(["--option", "1", "--sequence-option", "2", "3", "--", "4", "5", "6"]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(1, options.Option.Data);
        Assert.Collection(options.SequenceOption,
            static o1 => Assert.Equal(2, o1.Data),
            static o2 => Assert.Equal(3, o2.Data));
        Assert.Equal(4, options.Parameter.Data);
        Assert.Collection(options.RemainingParameters,
            static p1 => Assert.Equal(5, p1.Data),
            static p2 => Assert.Equal(6, p2.Data));

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Fact]
    public void BadOptionValueFormatError_SpanParsableOptions()
    {
        var result = ParseSpanOptionArguments(["--option", "a", "--sequence-option", "b", "c", "--", "d", "e", "f"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);
        Assert.Collection(errors,
            static e1 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e1);
                Assert.Equal("option", badOptionValueFormatError.OptionName);
                Assert.Equal("a", badOptionValueFormatError.Value);
            },
            static e2 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e2);
                Assert.Equal("sequence-option", badOptionValueFormatError.OptionName);
                Assert.Equal("b", badOptionValueFormatError.Value);
            },
            static e3 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e3);
                Assert.Equal("sequence-option", badOptionValueFormatError.OptionName);
                Assert.Equal("c", badOptionValueFormatError.Value);
            },
            static e4 =>
            {
                var badParameterValueFormatError = Assert.IsType<BadParameterValueFormatError>(e4);
                Assert.Equal(0, badParameterValueFormatError.ParameterIndex);
                Assert.Equal("parameter", badParameterValueFormatError.ParameterName);
                Assert.Equal("d", badParameterValueFormatError.Value);
            },
            static e5 =>
            {
                var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(e5);
                Assert.Equal(1, badRemainingParameterValueFormatError.ParameterIndex);
                Assert.Equal("e", badRemainingParameterValueFormatError.Value);
            },
            static e6 =>
            {
                var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(e6);
                Assert.Equal(2, badRemainingParameterValueFormatError.ParameterIndex);
                Assert.Equal("f", badRemainingParameterValueFormatError.Value);
            });
    }

    [Fact]
    public void ParseOptions()
    {
        var result = ParseOptionArguments(["--option", "1", "--sequence-option", "2", "3", "--", "4", "5", "6"]);

        Assert.Equal(ParseResultState.ParsedOptions, result.State);

        var options = result.Options;
        Assert.NotNull(options);

        Assert.Equal(1, options.Option.Data);
        Assert.Collection(options.SequenceOption,
            static o1 => Assert.Equal(2, o1.Data),
            static o2 => Assert.Equal(3, o2.Data));
        Assert.Equal(4, options.Parameter.Data);
        Assert.Collection(options.RemainingParameters,
            static p1 => Assert.Equal(5, p1.Data),
            static p2 => Assert.Equal(6, p2.Data));

        Assert.Null(result.Errors);
        Assert.Null(result.SpecialCommandHandler);
    }

    [Fact]
    public void BadOptionValueFormatError_ParsableOptions()
    {
        var result = ParseOptionArguments(["--option", "a", "--sequence-option", "b", "c", "--", "d", "e", "f"]);

        Assert.Equal(ParseResultState.ParsedWithErrors, result.State);

        Assert.Null(result.Options);

        var errors = result.Errors;
        Assert.NotNull(errors);
        Assert.Collection(errors,
            static e1 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e1);
                Assert.Equal("option", badOptionValueFormatError.OptionName);
                Assert.Equal("a", badOptionValueFormatError.Value);
            },
            static e2 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e2);
                Assert.Equal("sequence-option", badOptionValueFormatError.OptionName);
                Assert.Equal("b", badOptionValueFormatError.Value);
            },
            static e3 =>
            {
                var badOptionValueFormatError = Assert.IsType<BadOptionValueFormatError>(e3);
                Assert.Equal("sequence-option", badOptionValueFormatError.OptionName);
                Assert.Equal("c", badOptionValueFormatError.Value);
            },
            static e4 =>
            {
                var badParameterValueFormatError = Assert.IsType<BadParameterValueFormatError>(e4);
                Assert.Equal(0, badParameterValueFormatError.ParameterIndex);
                Assert.Equal("parameter", badParameterValueFormatError.ParameterName);
                Assert.Equal("d", badParameterValueFormatError.Value);
            },
            static e5 =>
            {
                var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(e5);
                Assert.Equal(1, badRemainingParameterValueFormatError.ParameterIndex);
                Assert.Equal("e", badRemainingParameterValueFormatError.Value);
            },
            static e6 =>
            {
                var badRemainingParameterValueFormatError = Assert.IsType<BadRemainingParameterValueFormatError>(e6);
                Assert.Equal(2, badRemainingParameterValueFormatError.ParameterIndex);
                Assert.Equal("f", badRemainingParameterValueFormatError.Value);
            });
    }
}
#endif
