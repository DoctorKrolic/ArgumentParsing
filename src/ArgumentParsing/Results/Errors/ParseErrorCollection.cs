using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace ArgumentParsing.Results.Errors;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(DebugView))]
public sealed class ParseErrorCollection : IReadOnlyCollection<ParseError>
{
    private readonly HashSet<ParseError> _errors;

    public int Count => _errors.Count;

    private ParseErrorCollection(HashSet<ParseError> errors)
    {
        _errors = errors;
    }

    public Enumerator GetEnumerator() => new(_errors.GetEnumerator());

    IEnumerator<ParseError> IEnumerable<ParseError>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ParseErrorCollection AsErrorCollection(HashSet<ParseError> errors)
        => new(errors);

    public struct Enumerator : IEnumerator<ParseError>
    {
        private HashSet<ParseError>.Enumerator _enumerator;

        public ParseError Current => _enumerator.Current;

        internal Enumerator(HashSet<ParseError>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public bool MoveNext() => _enumerator.MoveNext();

        public void Dispose() { }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
    }

    private class DebugView(ParseErrorCollection errors)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ParseError[] Items => errors.ToArray();
    }
}
