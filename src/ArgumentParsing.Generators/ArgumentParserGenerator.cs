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
            .Select((info, _) => info.ArgumentParserInfo!);

        context.RegisterSourceOutput(argumentParserInfos.Combine(environmentInfo), EmitArgumentParser);

        var assemblyVersionInfo = infoFromCompilation
            .Select((info, _) => info.AssemblyVersionInfo);

        var optionsHelpInfos = extractedInfosProvider
            .Where(info => !info.ArgumentParserInfo!.SpecialCommandHandlersInfos.HasValue)
            .Select((info, _) => info.OptionsHelpInfo!)
            .Combine(assemblyVersionInfo);

        context.RegisterSourceOutput(optionsHelpInfos, EmitHelpCommandHandler);

        // Candidate for `Any` API: https://github.com/dotnet/roslyn/issues/59690
        var hasAnyParsersWithDefaultHandlers = argumentParserInfos
            .Where(a => !a.SpecialCommandHandlersInfos.HasValue)
            .Collect()
            .Select((parsers, _) => !parsers.IsEmpty);

        context.RegisterSourceOutput(assemblyVersionInfo.Combine(hasAnyParsersWithDefaultHandlers), EmitVersionCommandHandler);
    }
}
