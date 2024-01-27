using ArgumentParsing.Generators.CodeGen;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static void Emit(SourceProductionContext context, ArgumentParserInfo parserInfo)
    {
        var cancellationToken = context.CancellationToken;

        (var hierarchy, var method, var optionsInfo) = parserInfo;
        (var qualifiedName, var optionInfos) = optionsInfo;

        var writer = new CodeWriter();

        writer.WriteLine("// <auto-generated/>");
        writer.WriteLine("#nullable disable");
        writer.WriteLine("#pragma warning disable");
        writer.WriteLine();

        var @namespace = hierarchy.Namespace;
        if (!string.IsNullOrEmpty(@namespace))
        {
            writer.WriteLine($"namespace {@namespace}");
            writer.OpenBlock();
        }

        foreach (var type in hierarchy.Hierarchy)
        {
            writer.WriteLine($"partial {type.GetTypeKeyword()} {type.QualifiedName}");
            writer.OpenBlock();
        }

        writer.WriteLine($"{method.Modifiers} {method.ReturnType} {method.Name}({method.ArgsParameterInfo.Type} {method.ArgsParameterInfo.Name})");
        writer.OpenBlock();

        foreach (var info in optionInfos)
        {
            writer.WriteLine($"{info.Type} {info.PropertyName}_val = default({info.Type});");
        }

        cancellationToken.ThrowIfCancellationRequested();

        writer.WriteLine();
        writer.WriteLine("int state = 0;");
        writer.WriteLine("int seenOptions = 0;");
        writer.WriteLine("global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;");
        writer.WriteLine("global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];");
        writer.WriteLine("global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;");
        writer.WriteLine("string previousArgument = null;");
        writer.WriteLine();

        writer.WriteLine($"foreach (string arg in {method.ArgsParameterInfo.Name})");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> val;");
        writer.WriteLine();
        writer.WriteLine("bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);");
        writer.WriteLine("bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');");
        writer.WriteLine();
        writer.WriteLine("if (state > 0 && startsOption)");
        writer.OpenBlock();
        writer.WriteLine("errors ??= new();");
        writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));");
        writer.WriteLine("state = 0;");
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("if (hasLetters && arg.StartsWith(\"--\"))");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);");
        writer.WriteLine("int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');");
        writer.WriteLine();
        writer.WriteLine("latestOptionName = slice[longArgSplit[0]];");
        writer.WriteLine("switch (latestOptionName)");
        writer.OpenBlock();

        cancellationToken.ThrowIfCancellationRequested();

        Span<char> usageCode = stackalloc char[optionInfos.Length];

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            if (info.LongName is null)
            {
                continue;
            }

            writer.WriteLine($"case \"{info.LongName}\":");
            writer.Ident++;
            usageCode.Fill('0');
            usageCode[^(i + 1)] = '1';
            writer.WriteLine($"if ((seenOptions & 0b{usageCode.ToString()}) > 0)");
            writer.OpenBlock();
            writer.WriteLine("errors ??= new();");
            writer.WriteLine($"errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError(\"{info.LongName}\"));");
            writer.CloseBlock();
            if (info.ParseStrategy == ParseStrategy.Flag)
            {
                writer.WriteLine($"{info.PropertyName}_val = true;");
                writer.WriteLine("state = -2;");
            }
            else
            {
                writer.WriteLine($"state = {i + 1};");
            }
            writer.WriteLine($"seenOptions |= 0b{usageCode.ToString()};");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.WriteLine("default:");
        writer.Ident++;
        writer.WriteLine("errors ??= new();");
        writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(latestOptionName.ToString(), arg));");
        writer.WriteLine("if (written == 1)");
        writer.OpenBlock();
        writer.WriteLine("state = -1;");
        writer.CloseBlock();
        writer.WriteLine("goto continueMainLoop;");
        writer.Ident--;

        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("if (written == 2)");
        writer.OpenBlock();
        writer.WriteLine("val = slice[longArgSplit[1]];");
        writer.WriteLine("goto decodeValue;");
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("goto continueMainLoop;");
        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("if (startsOption)");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 1);");
        writer.WriteLine();
        writer.WriteLine("for (int i = 0; i < slice.Length; i++)");
        writer.OpenBlock();
        writer.WriteLine("if (state > 0)");
        writer.OpenBlock();
        writer.WriteLine("val = slice.Slice(i);");
        writer.WriteLine("goto decodeValue;");
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("char shortOptionName = slice[i];");
        writer.WriteLine("latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i]);");
        writer.WriteLine("switch (shortOptionName)");
        writer.OpenBlock();

        cancellationToken.ThrowIfCancellationRequested();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            if (!info.ShortName.HasValue)
            {
                continue;
            }

            writer.WriteLine($"case '{info.ShortName}':");
            writer.Ident++;
            usageCode.Fill('0');
            usageCode[^(i + 1)] = '1';
            writer.WriteLine($"if ((seenOptions & 0b{usageCode.ToString()}) > 0)");
            writer.OpenBlock();
            writer.WriteLine("errors ??= new();");
            writer.WriteLine($"errors.Add(new global::ArgumentParsing.Results.Errors.DuplicateOptionError(\"{info.ShortName}\"));");
            writer.CloseBlock();
            if (info.ParseStrategy == ParseStrategy.Flag)
            {
                writer.WriteLine($"{info.PropertyName}_val = true;");
                writer.WriteLine("state = -2;");
            }
            else
            {
                writer.WriteLine($"state = {i + 1};");
            }
            writer.WriteLine($"seenOptions |= 0b{usageCode.ToString()};");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        var hasFlagOptions = optionInfos.Any(static i => i.ParseStrategy == ParseStrategy.Flag);
        
        writer.WriteLine("default:");
        writer.Ident++;
        if (hasFlagOptions)
        {
            writer.WriteLine("if (state == -2)");
            writer.OpenBlock();
            writer.WriteLine("latestOptionName = new global::System.ReadOnlySpan<char>(in slice[i - 1]);");
            writer.WriteLine("goto decodeValue;");
            writer.CloseBlock();
        }
        writer.WriteLine("errors ??= new();");
        writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.UnknownOptionError(shortOptionName.ToString(), arg));");
        writer.WriteLine("state = -1;");
        writer.WriteLine("goto continueMainLoop;");
        writer.Ident--;

        writer.CloseBlock();
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("goto continueMainLoop;");
        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("val = global::System.MemoryExtensions.AsSpan(arg);");
        writer.WriteLine();

        writer.WriteLine("decodeValue:", identDelta: -1);
        writer.WriteLine("switch (state)");
        writer.OpenBlock();
        if (hasFlagOptions)
        {
            writer.WriteLine("case -2:");
            writer.Ident++;
            writer.WriteLine("errors ??= new();");
            writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.FlagOptionValueError(latestOptionName.ToString()));");
            writer.WriteLine("break;");
            writer.Ident--;
        }
        writer.WriteLine("case -1:");
        writer.Ident++;
        writer.WriteLine("break;");
        writer.Ident--;

        cancellationToken.ThrowIfCancellationRequested();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            if (info.ParseStrategy == ParseStrategy.Flag)
            {
                continue;
            }

            writer.WriteLine($"case {i + 1}:");
            writer.Ident++;
            var parseStrategy = info.ParseStrategy;
            var nullableUnderlyingType = info.NullableUnderlyingType;
            switch (parseStrategy)
            {
                case ParseStrategy.String:
                    writer.WriteLine($"{info.PropertyName}_val = val.ToString();");
                    break;
                case ParseStrategy.Integer:
                case ParseStrategy.Float:
                    if (info.NullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{nullableUnderlyingType} {info.PropertyName}_underlying = default({nullableUnderlyingType});");
                    }
                    var numberStyles = parseStrategy == ParseStrategy.Integer ? "global::System.Globalization.NumberStyles.Integer" : "global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands";
                    writer.WriteLine($"if (!{nullableUnderlyingType ?? info.Type}.TryParse(val, {numberStyles}, global::System.Globalization.CultureInfo.InvariantCulture, out {info.PropertyName}{(nullableUnderlyingType is not null ? "_underlying" : "_val")}))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    if (nullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{info.PropertyName}_val = {info.PropertyName}_underlying;");
                    }
                    break;
                case ParseStrategy.Enum:
                    if (info.NullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{nullableUnderlyingType} {info.PropertyName}_underlying = default({nullableUnderlyingType});");
                    }
                    writer.WriteLine($"if (!global::System.Enum.TryParse<{nullableUnderlyingType ?? info.Type}>(val, out {info.PropertyName}{(nullableUnderlyingType is not null ? "_underlying" : "_val")}))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    if (nullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{info.PropertyName}_val = {info.PropertyName}_underlying;");
                    }
                    break;
                case ParseStrategy.Char:
                    if (info.NullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{nullableUnderlyingType} {info.PropertyName}_underlying = default({nullableUnderlyingType});");
                    }
                    writer.WriteLine($"if (!{nullableUnderlyingType ?? info.Type}.TryParse(val.ToString(), out {info.PropertyName}{(nullableUnderlyingType is not null ? "_underlying" : "_val")}))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    if (nullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{info.PropertyName}_val = {info.PropertyName}_underlying;");
                    }
                    break;
            }
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.WriteLine("default:");
        writer.Ident++;
        writer.WriteLine("errors ??= new();");
        writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.UnrecognizedArgumentError(arg));");
        writer.WriteLine("break;");
        writer.Ident--;

        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("state = 0;");

        writer.WriteLine();
        writer.WriteLine("continueMainLoop:", identDelta: -1);
        writer.WriteLine("previousArgument = arg;");
        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("if (state > 0)");
        writer.OpenBlock();
        writer.WriteLine("errors ??= new();");
        writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));");
        writer.CloseBlock();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            if (info.IsRequired)
            {
                writer.WriteLine();
                usageCode.Fill('0');
                usageCode[^(i + 1)] = '1';
                writer.WriteLine($"if ((seenOptions & 0b{usageCode.ToString()}) == 0)");
                writer.OpenBlock();
                writer.WriteLine("errors ??= new();");
                writer.WriteLine((info.ShortName, info.LongName) switch
                {
                    (not null, null) => $"errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError('{info.ShortName.Value}'));",
                    (null, not null) => $"errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError(\"{info.LongName}\"));",
                    (not null, not null) => $"errors.Add(new global::ArgumentParsing.Results.Errors.MissingRequiredOptionError('{info.ShortName.Value}', \"{info.LongName}\"));",
                    _ => throw new InvalidOperationException("Unreachable"),
                });
                writer.CloseBlock();
            }
        }

        writer.WriteLine();
        writer.WriteLine("if (errors != null)");
        writer.OpenBlock();
        writer.WriteLine($"return new {method.ReturnType}(global::ArgumentParsing.Results.Errors.ParseErrorCollection.AsErrorCollection(errors));");
        writer.CloseBlock();

        var optionsType = $"global::{qualifiedName}";
        writer.WriteLine();
        writer.WriteLine($"{optionsType} options = new {optionsType}");
        writer.OpenBlock();

        foreach (var info in optionInfos)
        {
            writer.WriteLine($"{info.PropertyName} = {info.PropertyName}_val,");
        }

        writer.Ident--;
        writer.WriteLine("};");
        writer.WriteLine();

        writer.WriteLine($"return new {method.ReturnType}(options);");
        writer.CloseBlock();

        writer.CloseRemainingBlocks();

        context.AddSource($"{optionsInfo.QualifiedTypeName}.g.cs", writer.ToString().Trim());
    }
}
