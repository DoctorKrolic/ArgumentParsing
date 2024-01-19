﻿using ArgumentParsing.Generators.CodeGen;
using ArgumentParsing.Generators.Models;
using Microsoft.CodeAnalysis;

namespace ArgumentParsing.Generators;

public partial class ArgumentParserGenerator
{
    private static void Emit(SourceProductionContext context, ArgumentParserInfo parserInfo)
    {
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

        writer.WriteLine();
        writer.WriteLine("int state = 0;");
        writer.WriteLine("global::System.Span<global::System.Range> longArgSplit = stackalloc global::System.Range[2];");
        writer.WriteLine();

        writer.WriteLine($"foreach (string arg in {method.ArgsParameterInfo.Name})");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> val;");
        writer.WriteLine();
        writer.WriteLine("if (arg.StartsWith(\"--\"))");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg).Slice(2);");
        writer.WriteLine("int written = global::System.MemoryExtensions.Split(slice, longArgSplit, '=');");
        writer.WriteLine();
        writer.WriteLine("switch (slice[longArgSplit[0]])");
        writer.OpenBlock();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            writer.WriteLine($"case \"{info.LongName}\":");
            writer.Ident++;
            writer.WriteLine($"state = {i + 1};");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("if (written == 2)");
        writer.OpenBlock();
        writer.WriteLine("val = slice[longArgSplit[1]];");
        writer.WriteLine("goto decodeValue;");
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("continue;");
        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("if (arg.StartsWith('-'))");
        writer.OpenBlock();
        writer.WriteLine("global::System.ReadOnlySpan<char> slice = global::System.MemoryExtensions.AsSpan(arg).Slice(1);");
        writer.WriteLine();
        writer.WriteLine("for (int i = 0; i < slice.Length; i++)");
        writer.OpenBlock();
        writer.WriteLine("if (state > 0)");
        writer.OpenBlock();
        writer.WriteLine("val = slice.Slice(i);");
        writer.WriteLine("goto decodeValue;");
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("switch (slice[i])");
        writer.OpenBlock();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            var info = optionInfos[i];

            writer.WriteLine($"case '{info.ShortName}':");
            writer.Ident++;
            writer.WriteLine($"state = {i + 1};");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.CloseBlock();
        writer.CloseBlock();
        writer.WriteLine();
        writer.WriteLine("continue;");
        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("val = global::System.MemoryExtensions.AsSpan(arg);");
        writer.WriteLine();

        writer.WriteLine("decodeValue:", identDelta: -1);
        writer.WriteLine("switch (state)");
        writer.OpenBlock();

        for (var i = 0; i < optionInfos.Length; i++)
        {
            writer.WriteLine($"case {i + 1}:");
            writer.Ident++;
            writer.WriteLine($"{optionInfos[i].PropertyName}_val = val.ToString();");
            writer.WriteLine("break;");
            writer.Ident--;
        }

        writer.CloseBlock();

        writer.WriteLine();
        writer.WriteLine("state = 0;");
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

        context.AddSource($"{optionsInfo.QualifiedTypeName}.g.cs", writer.ToString());
    }
}
