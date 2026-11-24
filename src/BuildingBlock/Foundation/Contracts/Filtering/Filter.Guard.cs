namespace BuildingBlock.Contracts.Filtering;

/// <summary>Guards for filter clauses. Failures stay on the railway.</summary>
public static class FilterGuard
{
    #region Field

    /// <summary>Checks that a field name is a valid identifier path.</summary>
    /// <param name="field">The candidate field name.</param>
    /// <param name="clause">The original clause, used for the error message.</param>
    /// <returns>The field name, or a validation error.</returns>
    public static Result<string> ValidField(string field, string clause)
        => IsValidField(field)
            ? Result.Success(field)
            : Result.Failure<string>(FilterConstant.Constraints.Clause.Invalid(
                $"'{clause}' does not start with a valid field name."));

    #endregion

    #region Helpers

    private static bool IsValidField(string text)
    {
        if (text.Length == 0 || (!char.IsLetter(text[0]) && text[0] != '_'))
        {
            return false;
        }

        foreach (char current in text)
        {
            if (!char.IsLetterOrDigit(current) && current is not ('_' or '.'))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
