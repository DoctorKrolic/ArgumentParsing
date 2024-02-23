namespace ArgumentParsing.SpecialCommands;

/// <summary>
/// Indicates a special command handler.
/// Most common special commands are <c>--help</c> and <c>--version</c>
/// </summary>
public interface ISpecialCommandHandler
{
    /// <summary>
    /// Handles the command
    /// </summary>
    /// <returns>An exit code as a result of running the command</returns>
    int HandleCommand();
}
