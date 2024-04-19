namespace ArgumentParsing.SpecialCommands.Version;

/// <summary>
/// Forces generation of default <c>--version</c> special command implementation for an assembly
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GenerateDefaultVersionSpecialCommandAttribute : Attribute
{
}
