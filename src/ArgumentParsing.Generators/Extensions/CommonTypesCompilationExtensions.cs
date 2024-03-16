using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators.Extensions;

public static class CommonTypesCompilationExtensions
{
    public static INamedTypeSymbol? ParseResultOfTType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.Results.ParseResult`1");

    public static INamedTypeSymbol? OptionsTypeAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.OptionsTypeAttribute");

    public static INamedTypeSymbol? OptionAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.OptionAttribute");

    public static INamedTypeSymbol? ParameterAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.ParameterAttribute");

    public static INamedTypeSymbol? RemainingParametersAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.RemainingParametersAttribute");

    public static INamedTypeSymbol? SystemComponentModelDataAnnotationsRequiredAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.RequiredAttribute");

    public static INamedTypeSymbol? ImmutableArrayOfTType(this Compilation compilation) => compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");
}
