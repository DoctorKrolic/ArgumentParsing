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

    public static INamedTypeSymbol? IsExternalInitType(this Compilation compilation) => compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.IsExternalInit");

    public static INamedTypeSymbol? ISpecialCommandHandlerType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.ISpecialCommandHandler");

    public static INamedTypeSymbol? SpecialCommandAliasesAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.SpecialCommandAliasesAttribute");

    public static INamedTypeSymbol? HelpInfoAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.Help.HelpInfoAttribute");

    public static INamedTypeSymbol? HelpTextGeneratorAttributeType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.SpecialCommands.Help.HelpTextGeneratorAttribute");

    public static INamedTypeSymbol? ParseErrorCollectionType(this Compilation compilation) => compilation.GetTypeByMetadataName("ArgumentParsing.Results.Errors.ParseErrorCollection");
}
