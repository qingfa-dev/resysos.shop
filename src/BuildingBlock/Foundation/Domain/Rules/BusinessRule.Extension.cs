namespace BuildingBlock.Domain.Rules;

/// <summary>
/// Extension methods for business rules.
/// </summary>
public static class BusinessRuleExtensions
{
    /// <summary>
    /// Creates a business rule from a condition and error.
    /// </summary>
    public static IBusinessRule Rule(Func<bool> condition, Error error)
        => new BusinessRule<object>(condition, () => error);

    /// <summary>
    /// Creates a business rule from a condition and error factory.
    /// </summary>
    public static IBusinessRule Rule(Func<bool> condition, Func<Error> errorFactory)
        => new BusinessRule<object>(condition, errorFactory);

    /// <summary>
    /// Validates all rules and collects errors.
    /// </summary>
    public static IReadOnlyList<Error> ValidateAll(this IEnumerable<IBusinessRule> rules)
        => rules.Where(r => !r.IsValid()).Select(r => r.Error).ToList();

    /// <summary>Validates a rule and returns a result instead of throwing.</summary>
    /// <param name="rule">The rule to validate.</param>
    /// <returns>Unit when the rule holds, or the rule's error.</returns>
    public static Result<Unit> ToResult(this IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.IsValid()
            ? Result.Success()
            : Result.Failure<Unit>(rule.Error);
    }

    /// <summary>Validates every rule and aggregates the violations into one result.</summary>
    /// <param name="rules">The rules to validate.</param>
    /// <returns>Unit when every rule holds, or the aggregated errors.</returns>
    public static Result<Unit> ToResult(this IEnumerable<IBusinessRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        IReadOnlyList<Error> errors = rules.ValidateAll();

        return errors.Count == 0
            ? Result.Success()
            : Result.Failure<Unit>(errors);
    }

    /// <summary>
    /// Validates all rules and throws if any are violated.
    /// </summary>
    public static void ValidateAllAndThrow(this IEnumerable<IBusinessRule> rules)
    {
        var errors = rules.ValidateAll();
        if (errors.Count > 0)
        {
            throw new DomainException(Error.Aggregate(errors));
        }
    }
}