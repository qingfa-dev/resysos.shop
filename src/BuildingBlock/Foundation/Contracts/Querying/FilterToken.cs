namespace BuildingBlock.Contracts.Querying;

/// <summary>The lexical category of a filter token produced by <see cref="FilterLexer"/>.</summary>
public enum FilterTokenKind
{
    #region Tokens

    /// <summary>A field or identifier in field position, e.g. <c>address.city</c>.</summary>
    Field,

    /// <summary>A word operator in operator position, e.g. <c>eq</c>, <c>and</c>, <c>not</c>.</summary>
    Word,

    /// <summary>A symbol operator, e.g. <c>==</c>, <c>&gt;=</c>, <c>@=</c>.</summary>
    Symbol,

    /// <summary>A bare value in value position, e.g. <c>18</c>, <c>Ada</c>.</summary>
    Value,

    /// <summary>A single-quoted value with quotes stripped and <c>''</c> escapes resolved.</summary>
    QuotedValue,

    #endregion

    #region Structure

    /// <summary>An opening parenthesis.</summary>
    OpenParen,

    /// <summary>A closing parenthesis.</summary>
    CloseParen,

    /// <summary>A comma.</summary>
    Comma,

    /// <summary>The end of the input. Always the final token.</summary>
    End

    #endregion
}

/// <summary>A single lexical token produced by <see cref="FilterLexer"/>.</summary>
/// <param name="Kind">The lexical category.</param>
/// <param name="Text">
/// The token text. Quotes are stripped and <c>''</c> escapes resolved for quoted values; the end
/// token carries an empty string.
/// </param>
/// <param name="Position">The zero-based index in the source text where the token begins.</param>
public sealed record FilterToken(FilterTokenKind Kind, string Text, int Position);
