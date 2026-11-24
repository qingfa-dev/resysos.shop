namespace BuildingBlock.Domain.Rules;

/// <summary>
/// Business rule that validates a condition with a custom error.
/// </summary>
/// <typeparam name="T">Dummy type parameter for generic factory methods.</typeparam>
public sealed record BusinessRule<T> : BusinessRule
    where T : notnull
{
    private readonly Func<bool> _condition;
    private readonly Func<Error> _errorFactory;

    /// <summary>
    /// Initializes a new business rule.
    /// </summary>
    /// <param name="condition">The condition to validate.</param>
    /// <param name="errorFactory">Factory for creating the error when violated.</param>
    public BusinessRule(Func<bool> condition, Func<Error> errorFactory)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        _errorFactory = errorFactory ?? throw new ArgumentNullException(nameof(errorFactory));
    }

    /// <inheritdoc />
    public override Error Error => _errorFactory();

    /// <inheritdoc />
    public override bool IsValid() => _condition();
}