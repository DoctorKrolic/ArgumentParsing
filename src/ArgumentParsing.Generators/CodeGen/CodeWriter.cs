using System.CodeDom.Compiler;

namespace ArgumentParsing.Generators.CodeGen;

internal sealed class CodeWriter
{
    private readonly StringWriter _sw;
    private readonly IndentedTextWriter _textWriter;

    public int Ident
    {
        get => _textWriter.Indent;
        set => _textWriter.Indent = value;
    }

    public CodeWriter()
    {
        _sw = new();
        _textWriter = new(_sw);
    }

    public void Write(string s)
    {
        _textWriter.Write(s);
    }

    public void WriteLine(string s, int identDelta = 0)
    {
        _textWriter.Indent += identDelta;
        _textWriter.WriteLine(s);
        _textWriter.Indent -= identDelta;
    }

    public void WriteLine()
    {
        _sw.WriteLine();
    }

    public void OpenBlock()
    {
        WriteLine("{");
        Ident++;
    }

    public void CloseBlock()
    {
        Ident--;
        WriteLine("}");
    }

    public void CloseRemainingBlocks()
    {
        while (_textWriter.Indent > 0)
        {
            CloseBlock();
        }
    }

    public override string ToString()
        => _sw.ToString();
}
