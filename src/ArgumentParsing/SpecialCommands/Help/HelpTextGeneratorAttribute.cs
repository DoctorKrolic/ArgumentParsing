namespace ArgumentParsing.SpecialCommands.Help;

/// <summary>
/// Used to specify help text generator method
/// </summary>
/// <remarks>
/// This method is used in <c>ExecuteDefaults</c> implementation to print help text with parser errors
/// and in default <c>--help</c> special command implementation
/// </remarks>
/// <param name="generatorType">Type of help text generator</param>
/// <param name="methodName">Name of method in generator type, which produces help text</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HelpTextGeneratorAttribute(Type generatorType, string methodName) : Attribute
{
    /// <summary>
    /// Type of help text generator
    /// </summary>
    public Type GeneratorType { get; } = generatorType;

    /// <summary>
    /// Name of method in generator type, which produces help text
    /// </summary>
    public string MethodName { get; } = methodName;
}
