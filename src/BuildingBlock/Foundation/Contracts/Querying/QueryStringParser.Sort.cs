using BuildingBlock.Contracts.Sorting;

namespace BuildingBlock.Contracts.Querying;

/// <summary>Sort-clause parsing for <see cref="QueryStringParser"/>.</summary>
public static partial class QueryStringParser
{
    #region Clause

    private static Result<SortDescriptor> ParseSortClause(string clause)
    {
        string text = clause.Trim();
        if (text.Length == 0)
        {
            return InvalidSort("A sort clause is empty.");
        }

        SortDirection direction = SortDirection.Ascending;
        if (text[0] == SortConstant.Defaults.DescendingPrefix)
        {
            direction = SortDirection.Descending;
            text = text[1..].Trim();
        }
        else if (text[0] == SortConstant.Defaults.AscendingPrefix)
        {
            text = text[1..].Trim();
        }

        int space = text.LastIndexOf(' ');
        if (space > 0)
        {
            string suffix = text[(space + 1)..];
            if (suffix.Equals(SortConstant.Defaults.AscendingSuffix, StringComparison.OrdinalIgnoreCase))
            {
                direction = SortDirection.Ascending;
                text = text[..space].Trim();
            }
            else if (suffix.Equals(SortConstant.Defaults.DescendingSuffix, StringComparison.OrdinalIgnoreCase))
            {
                direction = SortDirection.Descending;
                text = text[..space].Trim();
            }
        }

        var field = SortGuard.ValidField(text);
        if (field.IsFailure)
        {
            return Result.Failure<SortDescriptor>(field.Errors);
        }

        return Result.Success(new SortDescriptor { Field = field.Value, Direction = direction });
    }

    #endregion

    #region Errors

    private static Result<SortDescriptor> InvalidSort(string message)
        => Result.Failure<SortDescriptor>(SortConstant.Constraints.Clause.Invalid(message));

    #endregion
}
