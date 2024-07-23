namespace ArgumentParsing;

/// <summary>
/// Built-in special command handlers' flags
/// </summary>
[Flags]
public enum BuiltInCommandHandlers : byte
{
    /// <summary>
    /// Represents no built-in command handlers
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents built-in <c>--help</c> command handler
    /// </summary>
    Help = 1,

    /// <summary>
    /// Represents built-in <c>--version</c> command handler
    /// </summary>
    Version = 2,
}
