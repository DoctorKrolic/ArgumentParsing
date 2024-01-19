using ArgumentParsing.Generators.Models;
using ArgumentParsing.Generators.Utils;
using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Extensions;

internal static class DiagnosticsExtension
{
    public static void ReportDiagnostics(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<ImmutableEquatableArray<DiagnosticInfo>> diagnostics)
    {
        context.RegisterSourceOutput(diagnostics, static (context, diagnostics) =>
        {
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic.ToDiagnostic());
            }
        });
    }
}
