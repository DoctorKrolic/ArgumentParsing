namespace ArgumentParsing.Generators.Models;

internal sealed record EnvironmentInfo(
    bool CanUseOptimalSpanBasedAlgorithm,
    bool HasStringStartsWithCharOverload,
    bool ForceDefaultVersionCommand);
