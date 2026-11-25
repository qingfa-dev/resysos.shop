using System.Text;

namespace BuildingBlock.Contracts.Querying;

/// <summary>
/// Single left-to-right lexer for the filter grammar. Turns filter text into a flat token
/// stream for the filter parser. Malformed input fails on the railway with a
/// <c>filter.lex.*</c> error and never throws.
/// </summary>
public static partial class FilterLexer
{
    #region Tokenize

    /// <summary>Tokenizes filter text into a flat token stream terminated by an end token.</summary>
    /// <param name="text">The raw filter text. Null, empty or whitespace yields only an end token.</param>
    /// <returns>The token stream, or a <c>filter.lex.*</c> validation error.</returns>
    public static Result<IReadOnlyList<FilterToken>> Tokenize(string? text)
    {
        string source = text ?? string.Empty;
        List<FilterToken> tokens = [];
        Stack<bool> valueLists = new();
        bool expectValue = false;
        bool pipeList = false;
        int betweenStage = 0;
        int index = 0;

        while (index < source.Length)
        {
            char current = source[index];

            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (pipeList)
            {
                Result<string> pipe = ReadPipeList(source, index, out int next);
                if (pipe.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<FilterToken>>(pipe.Errors);
                }

                tokens.Add(new FilterToken(FilterTokenKind.Value, pipe.Value, index));
                pipeList = false;
                betweenStage = 0;
                expectValue = false;
                index = next;
                continue;
            }

            if (current == Quote)
            {
                Result<string> quoted = ReadQuoted(source, index, out int next);
                if (quoted.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<FilterToken>>(quoted.Errors);
                }

                tokens.Add(new FilterToken(FilterTokenKind.QuotedValue, quoted.Value, index));
                betweenStage = betweenStage == 1 ? 2 : 0;
                expectValue = false;
                index = next;
                continue;
            }

            if (current == OpenParen)
            {
                tokens.Add(new FilterToken(FilterTokenKind.OpenParen, "(", index));
                valueLists.Push(expectValue);
                index++;
                continue;
            }

            if (current == CloseParen)
            {
                tokens.Add(new FilterToken(FilterTokenKind.CloseParen, ")", index));
                if (valueLists.Count > 0)
                {
                    valueLists.Pop();
                }

                betweenStage = 0;
                expectValue = false;
                index++;
                continue;
            }

            if (current == Comma)
            {
                tokens.Add(new FilterToken(FilterTokenKind.Comma, ",", index));
                expectValue = valueLists.Count > 0 && valueLists.Peek();
                index++;
                continue;
            }

            if (IsSymbolStart(current) && !(expectValue && IsValueCharacter(current)))
            {
                string? symbol = MatchSymbol(source, index);
                if (symbol is null)
                {
                    return Failure(current, index);
                }

                tokens.Add(new FilterToken(FilterTokenKind.Symbol, symbol, index));
                betweenStage = 0;
                expectValue = true;
                pipeList = symbol == InOperator;
                index += symbol.Length;
                continue;
            }

            if (IsIdentifierCharacter(current) || (expectValue && IsValueCharacter(current)))
            {
                int start = index;
                while (index < source.Length
                    && (expectValue ? IsValueCharacter(source[index]) : IsBareCharacter(source[index], expectValue)))
                {
                    index++;
                }

                string bare = source[start..index];
                if (expectValue)
                {
                    tokens.Add(new FilterToken(FilterTokenKind.Value, bare, start));
                    betweenStage = betweenStage == 1 ? 2 : 0;
                    expectValue = false;
                }
                else if (TryGetWordCategory(bare, out WordCategory category))
                {
                    tokens.Add(new FilterToken(FilterTokenKind.Word, bare, start));
                    ApplyWord(bare, category, ref expectValue, ref betweenStage);
                }
                else
                {
                    tokens.Add(new FilterToken(FilterTokenKind.Field, bare, start));
                    betweenStage = 0;
                    expectValue = false;
                }

                continue;
            }

            return Failure(current, index);
        }

        tokens.Add(new FilterToken(FilterTokenKind.End, string.Empty, source.Length));
        return Result.Success<IReadOnlyList<FilterToken>>(tokens);
    }

    #endregion

    #region Characters

    private const char Quote = '\'';

    private const char ValueSeparator = '|';

    private const char OpenParen = '(';

    private const char CloseParen = ')';

    private const char Comma = ',';

    private static readonly string[] Symbols =
    [
        "==", "!=", "<>", ">=", "<=", "@=", "^=", "$=", "|=", ">", "<"
    ];

    private static bool IsSymbolStart(char current)
        => current is '=' or '!' or '<' or '>' or '@' or '^' or '$' or '|';

    private static bool IsIdentifierCharacter(char current)
        => char.IsLetterOrDigit(current) || current is '_' or '.';

    private static bool IsBareCharacter(char current, bool expectValue)
        => IsIdentifierCharacter(current) || (expectValue && current == ValueSeparator);

    // Value position is intentionally wider than field position so legacy unquoted values
    // (GUIDs, dates/times, ISO-8601 stamps, ...) still lex as one token. Field position stays
    // on IsBareCharacter/IsIdentifierCharacter.
    private static bool IsValueCharacter(char current)
        => IsIdentifierCharacter(current)
            || current is '|' or '-' or ':' or '+' or '/' or '@' or '#' or '%' or '&' or '*' or '~' or '?' or '=';

    private static string? MatchSymbol(string source, int index)
    {
        foreach (string symbol in Symbols)
        {
            if (source.AsSpan(index).StartsWith(symbol, StringComparison.Ordinal))
            {
                return symbol;
            }
        }

        return null;
    }

    #endregion

    #region Quoted values

    private static Result<string> ReadQuoted(string source, int start, out int next)
    {
        StringBuilder value = new();
        int index = start + 1;

        while (index < source.Length)
        {
            char current = source[index];
            if (current != Quote)
            {
                value.Append(current);
                index++;
                continue;
            }

            if (index + 1 < source.Length && source[index + 1] == Quote)
            {
                value.Append(Quote);
                index += 2;
                continue;
            }

            next = index + 1;
            return Result.Success(value.ToString());
        }

        next = start;
        return Result.Failure<string>(FilterLexerConstant.Constraints.Lexer.UnterminatedQuote(start));
    }

    #endregion

    #region Pipe lists

    private const string InOperator = "|=";

    // The legacy `|=` (any-of) operator carries a `|`-separated value list. Bare values keep the
    // `|` inside a single Value token; this path lets quoted parts (`'new'|'open'`) stay in the
    // same list and join to one Value token so the parser can split it like the legacy dialect.
    private static Result<string> ReadPipeList(string source, int start, out int next)
    {
        StringBuilder value = new();
        int index = start;

        while (true)
        {
            if (index < source.Length && source[index] == Quote)
            {
                Result<string> quoted = ReadQuoted(source, index, out int afterQuote);
                if (quoted.IsFailure)
                {
                    next = start;
                    return quoted;
                }

                value.Append(quoted.Value);
                index = afterQuote;
            }
            else
            {
                int partStart = index;
                while (index < source.Length && IsPipePartCharacter(source[index]))
                {
                    index++;
                }

                value.Append(source[partStart..index]);
            }

            int probe = index;
            while (probe < source.Length && char.IsWhiteSpace(source[probe]))
            {
                probe++;
            }

            if (probe < source.Length && source[probe] == ValueSeparator)
            {
                value.Append(ValueSeparator);
                index = probe + 1;
                continue;
            }

            break;
        }

        next = index;
        return Result.Success(value.ToString());
    }

    private static bool IsPipePartCharacter(char current)
        => current != ValueSeparator && IsValueCharacter(current);

    #endregion

    #region Word operators

    private const string And = "and";

    private const string Between = "between";

    /// <summary>The position a word operator leaves the lexer in.</summary>
    private enum WordCategory
    {
        /// <summary>Expect a field next, e.g. <c>and</c>, <c>not</c>, <c>any</c>.</summary>
        Field,

        /// <summary>Expect a value next, e.g. <c>eq</c>, <c>between</c>, <c>in</c>.</summary>
        Value,

        /// <summary>Stay in operator position, e.g. <c>isnull</c>, <c>isnotnull</c>.</summary>
        Operator
    }

    private static readonly Dictionary<string, WordCategory> WordOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eq"] = WordCategory.Value,
        ["neq"] = WordCategory.Value,
        ["gt"] = WordCategory.Value,
        ["gte"] = WordCategory.Value,
        ["lt"] = WordCategory.Value,
        ["lte"] = WordCategory.Value,
        ["contains"] = WordCategory.Value,
        ["startswith"] = WordCategory.Value,
        ["endswith"] = WordCategory.Value,
        ["in"] = WordCategory.Value,
        ["notin"] = WordCategory.Value,
        ["between"] = WordCategory.Value,
        ["isnull"] = WordCategory.Operator,
        ["isnotnull"] = WordCategory.Operator,
        ["any"] = WordCategory.Field,
        ["all"] = WordCategory.Field,
        ["and"] = WordCategory.Field,
        ["or"] = WordCategory.Field,
        ["not"] = WordCategory.Field,
        ["ieq"] = WordCategory.Value,
        ["ineq"] = WordCategory.Value,
        ["igt"] = WordCategory.Value,
        ["igte"] = WordCategory.Value,
        ["ilt"] = WordCategory.Value,
        ["ilte"] = WordCategory.Value,
        ["icontains"] = WordCategory.Value,
        ["istartswith"] = WordCategory.Value,
        ["iendswith"] = WordCategory.Value
    };

    private static bool TryGetWordCategory(string text, out WordCategory category)
        => WordOperators.TryGetValue(text, out category);

    private static bool IsWord(string text, string word)
        => string.Equals(text, word, StringComparison.OrdinalIgnoreCase);

    private static void ApplyWord(
        string word,
        WordCategory category,
        ref bool expectValue,
        ref int betweenStage)
    {
        if (IsWord(word, Between))
        {
            betweenStage = 1;
            expectValue = true;
            return;
        }

        if (betweenStage == 2 && IsWord(word, And))
        {
            betweenStage = 3;
            expectValue = true;
            return;
        }

        betweenStage = 0;
        expectValue = category == WordCategory.Value;
    }

    #endregion

    #region Failures

    private static Result<IReadOnlyList<FilterToken>> Failure(char character, int position)
        => Result.Failure<IReadOnlyList<FilterToken>>(
            FilterLexerConstant.Constraints.Lexer.UnexpectedCharacter(character, position));

    #endregion
}
