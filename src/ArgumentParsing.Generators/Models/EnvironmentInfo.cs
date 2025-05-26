namespace ArgumentParsing.Generators.Models;

internal sealed class EnvironmentInfo : IEquatable<EnvironmentInfo?>
{
    private readonly EnvironmentInfoFlags _flags;

    public bool CanUseOptimalSpanBasedAlgorithm => _flags.HasFlag(EnvironmentInfoFlags.CanUseOptimalSpanBasedAlgorithm);

    public bool HasStringStartsWithCharOverload => _flags.HasFlag(EnvironmentInfoFlags.HasStringStartsWithCharOverload);

    public EnvironmentInfo(bool canUseOptimalSpanBasedAlgorithm, bool hasStringStartsWithCharOverload)
    {
        if (canUseOptimalSpanBasedAlgorithm)
            _flags |= EnvironmentInfoFlags.CanUseOptimalSpanBasedAlgorithm;

        if (hasStringStartsWithCharOverload)
            _flags |= EnvironmentInfoFlags.HasStringStartsWithCharOverload;
    }

    public void Deconstruct(out bool canUseOptimalSpanBasedAlgorithm, out bool hasStringStartsWithCharOverload)
    {
        canUseOptimalSpanBasedAlgorithm = CanUseOptimalSpanBasedAlgorithm;
        hasStringStartsWithCharOverload = HasStringStartsWithCharOverload;
    }

    public bool Equals(EnvironmentInfo? other)
        => other is not null && _flags == other._flags;

    public override bool Equals(object? obj)
        => Equals(obj as EnvironmentInfo);

    public static bool operator ==(EnvironmentInfo? left, EnvironmentInfo? right)
        => EqualityComparer<EnvironmentInfo>.Default.Equals(left!, right!);

    public static bool operator !=(EnvironmentInfo? left, EnvironmentInfo? right)
        => !(left == right);

    public override int GetHashCode()
        => HashCode.Combine(_flags);

    public override string ToString()
        => $"{GetType().Name} {{ CanUseOptimalSpanBasedAlgorithm = {CanUseOptimalSpanBasedAlgorithm}, HasStringStartsWithCharOverload = {HasStringStartsWithCharOverload} }}";

    [Flags]
    private enum EnvironmentInfoFlags : byte
    {
        None = 0,
        CanUseOptimalSpanBasedAlgorithm = 1,
        HasStringStartsWithCharOverload = 2
    }
}
