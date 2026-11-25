using BuildingBlock.Contracts.Filtering;

namespace BuildingBlock.Contracts.Querying;

/// <summary>Filter-clause parsing for <see cref="QueryStringParser"/>.</summary>
public static partial class QueryStringParser
{
    #region Parse

    private static Result<ParsedFilter> ParseFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Result.Success(new ParsedFilter(null, []));
        }

        var parsed = FilterParser.Parse(filter);
        if (parsed.IsFailure)
        {
            return Result.Failure<ParsedFilter>(parsed.Errors.Select(MapFilterError).ToList());
        }

        return Result.Success(new ParsedFilter(parsed.Value, Flatten(parsed.Value)));
    }

    #endregion

    #region Projection

    private static IReadOnlyList<FilterDescriptor> Flatten(FilterNode node)
    {
        if (node is FilterCondition condition)
        {
            return [ToDescriptor(condition)];
        }

        if (node is not FilterGroup { Operator: FilterGroupOperator.And } group)
        {
            return [];
        }

        List<FilterDescriptor> descriptors = [];
        foreach (FilterNode child in group.Children)
        {
            if (child is not FilterCondition leaf)
            {
                return [];
            }

            descriptors.Add(ToDescriptor(leaf));
        }

        return descriptors;
    }

    private static FilterDescriptor ToDescriptor(FilterCondition condition)
        => new()
        {
            Field = condition.Field,
            Operator = condition.Operator,
            Value = condition.Value,
            Values = condition.Values
        };

    #endregion

    #region Errors

    private static Error MapFilterError(Error error)
        => error.Code.StartsWith("filter.lex.", StringComparison.Ordinal)
            ? FilterConstant.Constraints.Clause.Invalid(error.Message)
            : error;

    #endregion

    #region State

    /// <summary>The parsed filter tree and its flat-AND descriptor projection.</summary>
    private sealed record ParsedFilter(FilterNode? Filter, IReadOnlyList<FilterDescriptor> Filters);

    #endregion
}
