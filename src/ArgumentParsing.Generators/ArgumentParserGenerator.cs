using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

[Generator]
public sealed partial class ArgumentParserGenerator : IIncrementalGenerator
{
    public const string GeneratedArgumentParserAttributeName = "ArgumentParsing.GeneratedArgumentParserAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var extractedInfosProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GeneratedArgumentParserAttributeName,
                static (node, _) => node is MethodDeclarationSyntax,
                ExtractMainInfo)
            .Where(info => info != default);

        var infoFromCompilation = context.CompilationProvider
            .Select(ExtractInfoFromCompilation);

        var environmentInfo = infoFromCompilation
            .Select((info, _) => info.EnvironmentInfo);

        var argumentParserInfos = extractedInfosProvider
            .Select((info, _) => info.ArgumentParserInfo!)
            .Combine(environmentInfo);

        context.RegisterSourceOutput(argumentParserInfos, EmitArgumentParser);

        var assemblyVersionInfo = infoFromCompilation
            .Select((info, _) => info.AssemblyVersionInfo);

        var optionsHelpInfos = extractedInfosProvider
            .Select((info, _) => info.OptionsHelpInfo!)
            .Combine(assemblyVersionInfo);

        context.RegisterSourceOutput(optionsHelpInfos, EmitHelpCommandHandler);

        // We want to generate `--version` command handler only if there is at least 1 argument parser method defined.
        // This ugly hack will result in correct assembly version info if the condition is true and `null` otherwise.
        // Obviously, we can then detect `null` on the generation step to emit handler conditionally
        var assemblyVersionInfoForCommandGeneration = argumentParserInfos
            .Select((_, _) => 0)
            .Collect()
            .Combine(assemblyVersionInfo)
            .Select((tup, _) => tup.Left.Length > 0 ? tup.Right : null);

        context.RegisterSourceOutput(assemblyVersionInfoForCommandGeneration, EmitVersionCommandHandler);
    }
}
