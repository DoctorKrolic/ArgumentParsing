using ArgumentParsing.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentParsing.Generators;

[Generator]
public sealed partial class ArgumentParserGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var extractedInfosProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "ArgumentParsing.GeneratedArgumentParserAttribute",
                    static (node, _) => node is MethodDeclarationSyntax,
                    Extract);

        var diagnostics = extractedInfosProvider
            .Where(info => !info.Diagnostics.IsDefaultOrEmpty)
            .Select((info, _) => info.Diagnostics);

        context.ReportDiagnostics(diagnostics);

        var argumentParserInfos = extractedInfosProvider
            .Where(info => info.ArgumentParserInfo is not null)
            .Select((info, _) => info.ArgumentParserInfo!);

        context.RegisterSourceOutput(argumentParserInfos, EmitArgumentParser);

        var assemblyVersionInfos = extractedInfosProvider
            .Where(info => info.AssemblyVersionInfo is not null)
            .Select((info, _) => info.AssemblyVersionInfo!)
            .Collect();

        context.RegisterSourceOutput(assemblyVersionInfos, EmitVersionCommandHandlers);
    }
}
