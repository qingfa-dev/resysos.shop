using BuildingBlock.Contracts.Filtering;

namespace BuildingBlock.Contracts.Querying;

/// <summary>
/// Recursive-descent parser for the filter grammar. It consumes the token stream produced by
/// <see cref="FilterLexer"/> and builds a <see cref="FilterNode"/> tree. Lexer failures are
/// propagated unchanged as <c>filter.lex.*</c> errors; grammar failures become a
/// <c>query.invalid_filter</c> error carrying the position. Malformed input never throws.
/// </summary>
/// <remarks>
/// <para>Grammar:</para>
/// <code>
/// FilterNode := OrNode
/// OrNode     := AndNode ( 'or' AndNode )*
/// AndNode    := UnaryNode ( ('and' | ',') UnaryNode )*
/// UnaryNode  := 'not' UnaryNode | Primary
/// Primary    := '(' FilterNode ')' | Condition
/// Condition  := Field Op Value
///             | Field 'between' Value 'and' Value
///             | Field ('in'|'notin') '(' Value (',' Value)* ')'
/// </code>
/// </remarks>
public static partial class FilterParser
{
    #region Parse

    /// <summary>Parses filter text into a filter expression tree.</summary>
    /// <param name="text">The raw filter text. Null, empty or whitespace is malformed.</param>
    /// <returns>The root node, or a validation error.</returns>
    public static Result<FilterNode> Parse(string? text)
    {
        var lexed = FilterLexer.Tokenize(text);
        if (lexed.IsFailure)
        {
            return Result.Failure<FilterNode>(lexed.Errors);
        }

        IReadOnlyList<FilterToken> tokens = lexed.Value;
        int index = 0;

        var node = ParseOr(tokens, ref index);
        if (node.IsFailure)
        {
            return node;
        }

        if (index < tokens.Count && tokens[index].Kind != FilterTokenKind.End)
        {
            return Fail(tokens, index, "the expression has trailing input");
        }

        return node;
    }

    #endregion

    #region Grammar

    private static Result<FilterNode> ParseOr(IReadOnlyList<FilterToken> tokens, ref int index)
    {
        var first = ParseAnd(tokens, ref index);
        if (first.IsFailure)
        {
            return first;
        }

        List<FilterNode>? children = null;
        while (IsWord(tokens, index, Or))
        {
            index++;
            var next = ParseAnd(tokens, ref index);
            if (next.IsFailure)
            {
                return next;
            }

            children ??= [first.Value];
            children.Add(next.Value);
        }

        return children is null
            ? first
            : Result.Success<FilterNode>(new FilterGroup
            {
                Operator = FilterGroupOperator.Or,
                Children = children
            });
    }

    private static Result<FilterNode> ParseAnd(IReadOnlyList<FilterToken> tokens, ref int index)
    {
        var first = ParseUnary(tokens, ref index);
        if (first.IsFailure)
        {
            return first;
        }

        List<FilterNode>? children = null;
        while (IsWord(tokens, index, And) || IsKind(tokens, index, FilterTokenKind.Comma))
        {
            index++;
            var next = ParseUnary(tokens, ref index);
            if (next.IsFailure)
            {
                return next;
            }

            children ??= [first.Value];
            children.Add(next.Value);
        }

        return children is null
            ? first
            : Result.Success<FilterNode>(new FilterGroup
            {
                Operator = FilterGroupOperator.And,
                Children = children
            });
    }

    private static Result<FilterNode> ParseUnary(IReadOnlyList<FilterToken> tokens, ref int index)
    {
        if (!IsWord(tokens, index, Not))
        {
            return ParsePrimary(tokens, ref index);
        }

        index++;
        var child = ParseUnary(tokens, ref index);
        return child.IsFailure
            ? child
            : Result.Success<FilterNode>(new FilterNegation { Child = child.Value });
    }

    private static Result<FilterNode> ParsePrimary(IReadOnlyList<FilterToken> tokens, ref int index)
    {
        if (!IsKind(tokens, index, FilterTokenKind.OpenParen))
        {
            return ParseCondition(tokens, ref index);
        }

        index++;
        var inner = ParseOr(tokens, ref index);
        if (inner.IsFailure)
        {
            return inner;
        }

        if (!IsKind(tokens, index, FilterTokenKind.CloseParen))
        {
            return Fail(tokens, index, "a closing parenthesis was expected");
        }

        index++;
        return inner;
    }

    #endregion

    #region Condition

    private static Result<FilterNode> ParseCondition(IReadOnlyList<FilterToken> tokens, ref int index)
    {
        if (!IsKind(tokens, index, FilterTokenKind.Field))
        {
            return Fail(tokens, index, "a field name was expected");
        }

        FilterToken fieldToken = tokens[index];
        index++;

        var field = FilterGuard.ValidField(fieldToken.Text, fieldToken.Text);
        if (field.IsFailure)
        {
            return Result.Failure<FilterNode>(field.Errors);
        }

        if (!IsKind(tokens, index, FilterTokenKind.Word)
            && !IsKind(tokens, index, FilterTokenKind.Symbol))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no operator");
        }

        FilterToken operatorToken = tokens[index];
        index++;

        return operatorToken.Kind == FilterTokenKind.Symbol
            ? ParseSymbolCondition(fieldToken, operatorToken, tokens, ref index)
            : ParseWordCondition(fieldToken, operatorToken, tokens, ref index);
    }

    private static Result<FilterNode> ParseSymbolCondition(
        FilterToken fieldToken,
        FilterToken operatorToken,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
    {
        if (operatorToken.Text == "|=")
        {
            return ParsePipeList(fieldToken, tokens, ref index);
        }

        FilterOperator op = operatorToken.Text switch
        {
            "==" => FilterOperator.Eq,
            "!=" => FilterOperator.Ne,
            "<>" => FilterOperator.Ne,
            ">" => FilterOperator.Gt,
            ">=" => FilterOperator.Gte,
            "<" => FilterOperator.Lt,
            "<=" => FilterOperator.Lte,
            "@=" => FilterOperator.Contains,
            "^=" => FilterOperator.StartsWith,
            "$=" => FilterOperator.EndsWith,
            _ => FilterOperator.Eq
        };

        return ParseScalar(fieldToken, op, caseInsensitive: false, tokens, ref index);
    }

    private static Result<FilterNode> ParseWordCondition(
        FilterToken fieldToken,
        FilterToken operatorToken,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
        => operatorToken.Text.ToLowerInvariant() switch
        {
            "isnull" => Condition(fieldToken, FilterOperator.IsNull),
            "isnotnull" => Condition(fieldToken, FilterOperator.IsNotNull),
            "between" => ParseBetween(fieldToken, tokens, ref index),
            "in" => ParseList(fieldToken, FilterOperator.In, tokens, ref index),
            "notin" => ParseList(fieldToken, FilterOperator.NotIn, tokens, ref index),
            "any" or "all" => Fail(tokens, index, "collection operators are not supported"),
            "eq" => ParseScalar(fieldToken, FilterOperator.Eq, caseInsensitive: false, tokens, ref index),
            "ieq" => ParseScalar(fieldToken, FilterOperator.Eq, caseInsensitive: true, tokens, ref index),
            "neq" => ParseScalar(fieldToken, FilterOperator.Ne, caseInsensitive: false, tokens, ref index),
            "ineq" => ParseScalar(fieldToken, FilterOperator.Ne, caseInsensitive: true, tokens, ref index),
            "gt" => ParseScalar(fieldToken, FilterOperator.Gt, caseInsensitive: false, tokens, ref index),
            "igt" => ParseScalar(fieldToken, FilterOperator.Gt, caseInsensitive: true, tokens, ref index),
            "gte" => ParseScalar(fieldToken, FilterOperator.Gte, caseInsensitive: false, tokens, ref index),
            "igte" => ParseScalar(fieldToken, FilterOperator.Gte, caseInsensitive: true, tokens, ref index),
            "lt" => ParseScalar(fieldToken, FilterOperator.Lt, caseInsensitive: false, tokens, ref index),
            "ilt" => ParseScalar(fieldToken, FilterOperator.Lt, caseInsensitive: true, tokens, ref index),
            "lte" => ParseScalar(fieldToken, FilterOperator.Lte, caseInsensitive: false, tokens, ref index),
            "ilte" => ParseScalar(fieldToken, FilterOperator.Lte, caseInsensitive: true, tokens, ref index),
            "contains" => ParseScalar(fieldToken, FilterOperator.Contains, caseInsensitive: false, tokens, ref index),
            "icontains" => ParseScalar(fieldToken, FilterOperator.Contains, caseInsensitive: true, tokens, ref index),
            "startswith" => ParseScalar(fieldToken, FilterOperator.StartsWith, caseInsensitive: false, tokens, ref index),
            "istartswith" => ParseScalar(fieldToken, FilterOperator.StartsWith, caseInsensitive: true, tokens, ref index),
            "endswith" => ParseScalar(fieldToken, FilterOperator.EndsWith, caseInsensitive: false, tokens, ref index),
            "iendswith" => ParseScalar(fieldToken, FilterOperator.EndsWith, caseInsensitive: true, tokens, ref index),
            _ => FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no operator")
        };

    #endregion

    #region Values

    private static Result<FilterNode> ParseScalar(
        FilterToken fieldToken,
        FilterOperator op,
        bool caseInsensitive,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
    {
        if (!IsValue(tokens, index))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no value");
        }

        FilterToken valueToken = tokens[index];
        index++;

        if (valueToken.Kind == FilterTokenKind.Value
            && valueToken.Text.Equals(Null, StringComparison.OrdinalIgnoreCase))
        {
            return op switch
            {
                FilterOperator.Eq => Condition(fieldToken, FilterOperator.IsNull),
                FilterOperator.Ne => Condition(fieldToken, FilterOperator.IsNotNull),
                _ => Fail(tokens, index, $"'{fieldToken.Text} null' only works with 'eq' or 'neq'")
            };
        }

        return Condition(fieldToken, op, caseInsensitive, value: valueToken.Text);
    }

    private static Result<FilterNode> ParseBetween(
        FilterToken fieldToken,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
    {
        if (!IsValue(tokens, index))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text} between' has no lower bound");
        }

        string lower = tokens[index].Text;
        index++;

        if (!IsWord(tokens, index, And))
        {
            return Fail(tokens, index, $"'{fieldToken.Text} between' expects 'and' between the bounds");
        }

        index++;

        if (!IsValue(tokens, index))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text} between' has no upper bound");
        }

        string upper = tokens[index].Text;
        index++;

        return Condition(fieldToken, FilterOperator.Between, values: [lower, upper]);
    }

    private static Result<FilterNode> ParseList(
        FilterToken fieldToken,
        FilterOperator op,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
    {
        if (!IsKind(tokens, index, FilterTokenKind.OpenParen))
        {
            return Fail(tokens, index, $"'{fieldToken.Text}' expects '(' after the operator");
        }

        index++;

        if (!IsValue(tokens, index))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no values");
        }

        List<string> values = [];
        while (true)
        {
            values.Add(tokens[index].Text);
            index++;

            if (IsKind(tokens, index, FilterTokenKind.Comma))
            {
                index++;
                if (!IsValue(tokens, index))
                {
                    return Fail(tokens, index, $"'{fieldToken.Text}' expects a value after ','");
                }

                continue;
            }

            if (IsKind(tokens, index, FilterTokenKind.CloseParen))
            {
                index++;
                break;
            }

            return Fail(tokens, index, $"'{fieldToken.Text}' expects ',' or ')'");
        }

        return Condition(fieldToken, op, values: values);
    }

    private static Result<FilterNode> ParsePipeList(
        FilterToken fieldToken,
        IReadOnlyList<FilterToken> tokens,
        ref int index)
    {
        if (!IsValue(tokens, index))
        {
            return FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no values after '|='");
        }

        List<string> values = [];
        foreach (string part in tokens[index].Text.Split(FilterConstant.Defaults.ValueSeparator))
        {
            string value = part.Trim();
            if (value.Length > 0)
            {
                values.Add(value);
            }
        }

        index++;

        return values.Count == 0
            ? FailWithField(tokens, index, fieldToken.Text, $"'{fieldToken.Text}' has no values after '|='")
            : Condition(fieldToken, FilterOperator.In, values: values);
    }

    #endregion

    #region Helpers

    private const string Null = "null";

    private const string And = "and";

    private const string Or = "or";

    private const string Not = "not";

    private static Result<FilterNode> Condition(
        FilterToken fieldToken,
        FilterOperator op,
        bool caseInsensitive = false,
        string? value = null,
        IReadOnlyList<string>? values = null)
        => Result.Success<FilterNode>(new FilterCondition
        {
            Field = fieldToken.Text,
            Operator = op,
            Value = value,
            Values = values ?? [],
            CaseInsensitive = caseInsensitive
        });

    private static bool IsKind(IReadOnlyList<FilterToken> tokens, int index, FilterTokenKind kind)
        => index < tokens.Count && tokens[index].Kind == kind;

    private static bool IsValue(IReadOnlyList<FilterToken> tokens, int index)
        => IsKind(tokens, index, FilterTokenKind.Value) || IsKind(tokens, index, FilterTokenKind.QuotedValue);

    private static bool IsWord(IReadOnlyList<FilterToken> tokens, int index, string word)
        => IsKind(tokens, index, FilterTokenKind.Word)
        && string.Equals(tokens[index].Text, word, StringComparison.OrdinalIgnoreCase);

    private static Result<FilterNode> Fail(IReadOnlyList<FilterToken> tokens, int index, string message)
        => Result.Failure<FilterNode>(
            FilterParserConstant.Constraints.Parser.Malformed(Position(tokens, index), message));

    private static Result<FilterNode> FailWithField(
        IReadOnlyList<FilterToken> tokens,
        int index,
        string field,
        string message)
        => Result.Failure<FilterNode>(
            FilterParserConstant.Constraints.Parser.Malformed(Position(tokens, index), message)
                .WithField(field));

    private static int Position(IReadOnlyList<FilterToken> tokens, int index)
        => index < tokens.Count
            ? tokens[index].Position
            : tokens.Count > 0
                ? tokens[^1].Position
                : 0;

    #endregion
}
