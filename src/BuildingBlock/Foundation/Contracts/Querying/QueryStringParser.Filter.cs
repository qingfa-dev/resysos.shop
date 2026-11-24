using BuildingBlock.Contracts.Filtering;

namespace BuildingBlock.Contracts.Querying;

/// <summary>Filter-clause parsing for <see cref="QueryStringParser"/>.</summary>
public static partial class QueryStringParser
{
    #region Operators

    private static readonly string[] SymbolOperators =
    [
        "==", "!=", ">=", "<=", "@=", "^=", "$=", "|=", ">", "<"
    ];

    #endregion

    #region Clause

    private static Result<FilterDescriptor> ParseClause(string clause)
    {
        string text = clause.Trim();
        if (text.Length == 0)
        {
            return InvalidFilter("A filter clause is empty.");
        }

        int length = 0;
        while (length < text.Length && IsFieldCharacter(text[length]))
        {
            length++;
        }

        var field = FilterGuard.ValidField(text[..length], text);
        if (field.IsFailure)
        {
            return Result.Failure<FilterDescriptor>(field.Errors);
        }

        string remainder = text[length..].TrimStart();
        if (remainder.Length == 0)
        {
            return InvalidFilter($"'{field.Value}' has no operator.")
                .MapError(error => error.WithField(field.Value));
        }

        if (TryReadNullCheck(field.Value, remainder, out Result<FilterDescriptor> nullCheck))
        {
            return nullCheck;
        }

        foreach (string symbol in SymbolOperators)
        {
            if (!remainder.StartsWith(symbol, StringComparison.Ordinal))
            {
                continue;
            }

            string raw = remainder[symbol.Length..].Trim();
            return BuildDescriptor(field.Value, ToOperator(symbol), raw);
        }

        return InvalidFilter($"'{remainder}' is not a known operator for '{field.Value}'.")
            .MapError(error => error.WithField(field.Value));
    }

    #endregion

    #region Descriptor

    private static Result<FilterDescriptor> BuildDescriptor(
        string field,
        FilterOperator @operator,
        string raw)
    {
        if (@operator is FilterOperator.In)
        {
            List<string> values = [];
            foreach (string part in raw.Split(FilterConstant.Defaults.ValueSeparator))
            {
                if (TryReadValue(part.Trim(), out string value, out _))
                {
                    values.Add(value);
                }
            }

            return values.Count == 0
                ? InvalidFilter($"'{field}' has no values after '|='.")
                    .MapError(error => error.WithField(field))
                : Result.Success(new FilterDescriptor
                {
                    Field = field,
                    Operator = FilterOperator.In,
                    Values = values
                });
        }

        if (!TryReadValue(raw, out string text, out bool quoted))
        {
            return InvalidFilter($"'{field}' has no value.")
                .MapError(error => error.WithField(field));
        }

        if (!quoted && text.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            if (@operator is FilterOperator.Eq)
            {
                return Result.Success(new FilterDescriptor
                {
                    Field = field,
                    Operator = FilterOperator.IsNull
                });
            }

            if (@operator is FilterOperator.Ne)
            {
                return Result.Success(new FilterDescriptor
                {
                    Field = field,
                    Operator = FilterOperator.IsNotNull
                });
            }

            return InvalidFilter($"'{field} null' only works with '==' or '!='.")
                .MapError(error => error.WithField(field));
        }

        return Result.Success(new FilterDescriptor
        {
            Field = field,
            Operator = @operator,
            Value = text
        });
    }

    private static bool TryReadNullCheck(
        string field,
        string remainder,
        out Result<FilterDescriptor> result)
    {
        foreach ((string word, FilterOperator @operator) in new[]
        {
            ("isnotnull", FilterOperator.IsNotNull),
            ("isnull", FilterOperator.IsNull)
        })
        {
            if (!remainder.StartsWith(word, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string trailing = remainder[word.Length..].Trim();
            result = trailing.Length == 0
                ? Result.Success(new FilterDescriptor { Field = field, Operator = @operator })
                : InvalidFilter($"'{field} {word}' takes no value.")
                    .MapError(error => error.WithField(field));

            return true;
        }

        result = Result.Success(new FilterDescriptor { Field = field });
        return false;
    }

    #endregion

    #region Mapping

    private static FilterOperator ToOperator(string symbol)
        => symbol switch
        {
            "==" => FilterOperator.Eq,
            "!=" => FilterOperator.Ne,
            ">" => FilterOperator.Gt,
            ">=" => FilterOperator.Gte,
            "<" => FilterOperator.Lt,
            "<=" => FilterOperator.Lte,
            "@=" => FilterOperator.Contains,
            "^=" => FilterOperator.StartsWith,
            "$=" => FilterOperator.EndsWith,
            "|=" => FilterOperator.In,
            _ => FilterOperator.Eq
        };

    private static Result<FilterDescriptor> InvalidFilter(string message)
        => Result.Failure<FilterDescriptor>(FilterConstant.Constraints.Clause.Invalid(message));

    #endregion
}
