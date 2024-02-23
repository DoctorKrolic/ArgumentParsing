using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace ArgumentParsing.Results.Errors;

/// <summary>
/// Represents an immutable collection of errors, occurred during argument parsing
/// </summary>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(DebugView))]
public sealed class ParseErrorCollection : IReadOnlyCollection<ParseError>
{
    private readonly HashSet<ParseError> _errors;

    /// <inheritdoc/>
    public int Count => _errors.Count;

    private ParseErrorCollection(HashSet<ParseError> errors)
    {
        _errors = errors;
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Enumerator GetEnumerator() => new(_errors.GetEnumerator());

    /// <inheritdoc/>
    IEnumerator<ParseError> IEnumerable<ParseError>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Wraps <see cref="ParseErrorCollection"/> around <see cref="HashSet{T}"/> of <see cref="ParseError"/>
    /// without copying elements of the <see cref="HashSet{T}"/>
    /// </summary>
    /// <remarks>
    /// This method is hidden from the IDE and is intended to be used in generated code only.
    /// Thus it is not considered a part of public API and can be changed/removed in any release without notice
    /// </remarks>
    /// <param name="errors">Hash set of errors</param>
    /// <returns>Constructed <see cref="ParseErrorCollection"/></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ParseErrorCollection AsErrorCollection(HashSet<ParseError> errors)
        => new(errors);

    /// <summary>
    /// Enumerates parse errors of the error collection
    /// </summary>
    public struct Enumerator : IEnumerator<ParseError>
    {
        private HashSet<ParseError>.Enumerator _enumerator;

        /// <inheritdoc/>
        public ParseError Current => _enumerator.Current;

        internal Enumerator(HashSet<ParseError>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

        /// <inheritdoc/>
        public bool MoveNext() => _enumerator.MoveNext();

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        object IEnumerator.Current => Current;

        /// <inheritdoc/>
        void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
    }

    private class DebugView(ParseErrorCollection errors)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ParseError[] Items => errors.ToArray();
    }
}
