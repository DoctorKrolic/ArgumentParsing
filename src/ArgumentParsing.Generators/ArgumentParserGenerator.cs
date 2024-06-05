using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

[Generator]
public sealed partial class ArgumentParserGenerator : IIncrementalGenerator
{
    public const string GeneratedArgumentParserAttributeName = "ArgumentParsing.GeneratedArgumentParserAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var argumentParserInfos = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GeneratedArgumentParserAttributeName,
                static (node, _) => node is MethodDeclarationSyntax,
                ExtractArgumentParserInfo)
            .Where(info => info is not null)
            .Select((info, _) => info!);

        var infoFromCompilation = context.CompilationProvider
            .Select(ExtractInfoFromCompilation);

        var environmentInfo = infoFromCompilation
            .Select((info, _) => info.EnvironmentInfo);

        context.RegisterSourceOutput(argumentParserInfos.Combine(environmentInfo), EmitArgumentParser);

        var assemblyVersionInfo = infoFromCompilation
            .Select((info, _) => info.AssemblyVersionInfo);

        var optionsHelpInfos = argumentParserInfos
            .Where(info => !info.SpecialCommandHandlersInfos.HasValue)
            .Select((info, _) => info.OptionsInfo)
            .WithComparer(HelpOnlyOptionsInfoComparer.Instance)
            .Combine(assemblyVersionInfo);

        context.RegisterSourceOutput(optionsHelpInfos, EmitHelpCommandHandler);

        // Candidate for `Any` API: https://github.com/dotnet/roslyn/issues/59690
        var hasAnyParsersWithDefaultHandlers = argumentParserInfos
            .Where(a => !a.SpecialCommandHandlersInfos.HasValue)
            .Collect()
            .Select((parsers, _) => !parsers.IsEmpty);

        var forceDefaultVersionCommand = environmentInfo
            .Select((info, _) => info.ForceDefaultVersionCommand)
            .Combine(hasAnyParsersWithDefaultHandlers)
            .Select((pair, _) => pair.Left || pair.Right);

        context.RegisterSourceOutput(assemblyVersionInfo.Combine(forceDefaultVersionCommand), EmitVersionCommandHandler);
    }
}
