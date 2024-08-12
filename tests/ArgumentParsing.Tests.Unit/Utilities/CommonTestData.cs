namespace ArgumentParsing.Tests.Unit.Utilities;

public static class CommonTestData
{
    public static TheoryData<string> BuiltInsWithoutHelp => new(
    [
        "BuiltInCommandHandlers.None",
        "BuiltInCommandHandlers.Version",
        "BuiltInCommandHandlers.None | BuiltInCommandHandlers.Version",
    ]);

    public static TheoryData<string> BuiltInsWithoutVersion => new(
    [
        "BuiltInCommandHandlers.None",
        "BuiltInCommandHandlers.Help",
        "BuiltInCommandHandlers.None | BuiltInCommandHandlers.Help",
    ]);

    public static TheoryData<string> InvalidHandlerForBuiltInCommandHelpInfo => new(
    [
        "BuiltInCommandHandlers.None",
        "BuiltInCommandHandlers.Help | BuiltInCommandHandlers.Version",
        "(BuiltInCommandHandlers)252",
    ]);
}
