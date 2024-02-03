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
        (var qualifiedName, var optionInfos, var parameterInfos) = optionsInfo;

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
            writer.WriteLine(info.SequenceType switch
            {
                SequenceType.None => $"{info.Type} {info.PropertyName}_val = default({info.Type});",
                SequenceType.List => $"global::System.Collections.Generic.List<{info.Type}> {info.PropertyName}_builder = new();",
                SequenceType.ImmutableArray => $"global::System.Collections.Immutable.ImmutableArray<{info.Type}>.Builder {info.PropertyName}_builder = global::System.Collections.Immutable.ImmutableArray.CreateBuilder<{info.Type}>();",
                _ => throw new InvalidOperationException("Unreachable"),
            });
        }

        cancellationToken.ThrowIfCancellationRequested();

        var hasAnyOptions = optionInfos.Length > 0;

        writer.WriteLine();
        writer.WriteLine("int state = 0;");
        if (hasAnyOptions)
        {
            writer.WriteLine("int seenOptions = 0;");
        }
        writer.WriteLine("global::System.Collections.Generic.HashSet<global::ArgumentParsing.Results.Errors.ParseError> errors = null;");
        writer.WriteLine("global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];");
        writer.WriteLine("global::System.ReadOnlySpan<char> latestOptionName = global::System.ReadOnlySpan<char>.Empty;");
        if (hasAnyOptions)
        {
            writer.WriteLine("string previousArgument = null;");
        }
        var hasSequenceOptions = optionInfos.Any(static i => i.SequenceType != SequenceType.None);
        if (hasSequenceOptions)
        {
            writer.WriteLine("int optionSource = 0;");
        }
        writer.WriteLine();

        writer.WriteLine($"foreach (string arg in {method.ArgsParameterInfo.Name})");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> val;");
        writer.WriteLine();
        writer.WriteLine("bool hasLetters = global::System.Linq.Enumerable.Any(arg, char.IsLetter);");
        writer.WriteLine("bool startsOption = hasLetters && arg.Length > 1 && arg.StartsWith('-');");
        writer.WriteLine();
        if (hasAnyOptions)
        {
            writer.WriteLine("if (state > 0 && startsOption)");
            writer.OpenBlock();
            writer.WriteLine("errors ??= new();");
            writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));");
            writer.WriteLine("state = 0;");
            writer.CloseBlock();
            writer.WriteLine();
        }
        writer.WriteLine("if (hasLetters && arg.StartsWith(\"--\"))");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg, 2);");
        writer.WriteLine("int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');");
        writer.WriteLine();
        if (hasSequenceOptions)
        {
            writer.WriteLine("optionSource = 2;");
            writer.WriteLine();
        }
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
                writer.WriteLine($"state = {(info.NullableUnderlyingType is null ? "-10" : (int.MinValue + i))};");
            }
            else
            {
                writer.WriteLine($"state = {(info.SequenceType != SequenceType.None ? (int.MinValue + i) : i + 1)};");
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
        if (hasSequenceOptions)
        {
            writer.WriteLine("optionSource = 1;");
            writer.WriteLine();
        }
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
                writer.WriteLine($"state = {(info.NullableUnderlyingType is null ? "-10" : (int.MinValue + i))};");
            }
            else
            {
                writer.WriteLine($"state = {(info.SequenceType != SequenceType.None ? (int.MinValue + i) : i + 1)};");
            }
            writer.WriteLine($"seenOptions |= 0b{usageCode.ToString()};");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.WriteLine("default:");
        writer.Ident++;
        if (optionInfos.Any(static i => i.ParseStrategy == ParseStrategy.Flag || i.SequenceType != SequenceType.None))
        {
            writer.WriteLine($"if (state <= -10)");
            writer.OpenBlock();
            if (optionInfos.Any(static i => i.NullableUnderlyingType is not null || i.SequenceType != SequenceType.None))
            {
                writer.WriteLine("val = slice.Slice(i);");
            }
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
        if (hasSequenceOptions)
        {
            writer.WriteLine("optionSource = 0;");
        }
        writer.WriteLine();

        writer.WriteLine("decodeValue:", identDelta: -1);
        writer.WriteLine("switch (state)");
        writer.OpenBlock();
        if (optionInfos.Any(static i => i.ParseStrategy == ParseStrategy.Flag && i.NullableUnderlyingType is null))
        {
            writer.WriteLine("case -10:");
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

            var propertyName = info.PropertyName;
            var parseStrategy = info.ParseStrategy;
            var nullableUnderlyingType = info.NullableUnderlyingType;
            var sequenceType = info.SequenceType;

            if (parseStrategy == ParseStrategy.Flag && nullableUnderlyingType is null)
            {
                continue;
            }

            writer.WriteLine($"case {(parseStrategy == ParseStrategy.Flag || sequenceType != SequenceType.None ? (int.MinValue + i) : (i + 1))}:");
            writer.Ident++;
            if (sequenceType != SequenceType.None)
            {
                writer.WriteLine($"{info.Type} {propertyName}_val = default({info.Type});");
            }
            switch (parseStrategy)
            {
                case ParseStrategy.String:
                    writer.WriteLine($"{propertyName}_val = val.ToString();");
                    break;
                case ParseStrategy.Integer:
                case ParseStrategy.Float:
                    if (info.NullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{nullableUnderlyingType} {propertyName}_underlying = default({nullableUnderlyingType});");
                    }
                    var numberStyles = parseStrategy == ParseStrategy.Integer ? "global::System.Globalization.NumberStyles.Integer" : "global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands";
                    writer.WriteLine($"if (!{nullableUnderlyingType ?? info.Type}.TryParse(val, {numberStyles}, global::System.Globalization.CultureInfo.InvariantCulture, out {propertyName}{(nullableUnderlyingType is not null ? "_underlying" : "_val")}))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    if (nullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{propertyName}_val = {propertyName}_underlying;");
                    }
                    break;
                case ParseStrategy.Flag when nullableUnderlyingType is not null:
                    writer.WriteLine($"bool {propertyName}_underlying = default({nullableUnderlyingType});");
                    writer.WriteLine($"if (!bool.TryParse(val, out {propertyName}_underlying))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    writer.WriteLine($"{propertyName}_val = {propertyName}_underlying;");
                    break;
                case ParseStrategy.Enum:
                    if (info.NullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{nullableUnderlyingType} {propertyName}_underlying = default({nullableUnderlyingType});");
                    }
                    writer.WriteLine($"if (!global::System.Enum.TryParse<{nullableUnderlyingType ?? info.Type}>(val, out {propertyName}{(nullableUnderlyingType is not null ? "_underlying" : "_val")}))");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    if (nullableUnderlyingType is not null)
                    {
                        writer.WriteLine($"{propertyName}_val = {propertyName}_underlying;");
                    }
                    break;
                case ParseStrategy.Char:
                    writer.WriteLine($"if (val.Length == 1)");
                    writer.OpenBlock();
                    writer.WriteLine($"{propertyName}_val = val[0];");
                    writer.CloseBlock();
                    writer.WriteLine("else");
                    writer.OpenBlock();
                    writer.WriteLine("errors ??= new();");
                    writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.BadOptionValueFormatError(val.ToString(), latestOptionName.ToString()));");
                    writer.CloseBlock();
                    break;
            }
            if (sequenceType != SequenceType.None)
            {
                writer.WriteLine($"{propertyName}_builder.Add({propertyName}_val);");
                writer.WriteLine("if (optionSource > 0)");
                writer.OpenBlock();
                writer.WriteLine("state = 0;");
                writer.CloseBlock();
                writer.WriteLine("continue;");
            }
            else
            {
                writer.WriteLine("break;");
            }
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
        if (hasAnyOptions)
        {
            writer.WriteLine("previousArgument = arg;");
        }
        else
        {
            writer.WriteLine(";");
        }
        writer.CloseBlock();

        if (hasAnyOptions)
        {
            writer.WriteLine();
            writer.WriteLine("if (state > 0)");
            writer.OpenBlock();
            writer.WriteLine("errors ??= new();");
            writer.WriteLine("errors.Add(new global::ArgumentParsing.Results.Errors.OptionValueIsNotProvidedError(previousArgument));");
            writer.CloseBlock();
        }

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
            writer.WriteLine($"{info.PropertyName} = {info.PropertyName}{(info.SequenceType != SequenceType.None ? "_builder" : "_val")}{(info.SequenceType == SequenceType.ImmutableArray ? ".ToImmutable()" : string.Empty)},");
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
