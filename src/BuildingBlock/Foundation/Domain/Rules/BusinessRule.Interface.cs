namespace BuildingBlock.Domain.Rules;

/// <summary>
/// Interface for business rules that can be validated.
/// Business rules represent invariants that must always hold true.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Gets the error that will be returned if the rule is violated.
    /// </summary>
    Error Error { get; }

    /// <summary>
    /// Validates the rule.
    /// </summary>
    /// <returns>True if the rule is satisfied; otherwise false.</returns>
    bool IsValid();

    /// <summary>
    /// Validates the rule and throws if violated.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the rule is violated.</exception>
    void Validate()
    {
        if (!IsValid())
        {
            throw new DomainException(Error);
        }
    }
}

/// <summary>
/// Base record for business rules with a descriptive message.
/// </summary>
public abstract record BusinessRule : IBusinessRule
{
    /// <inheritdoc />
    public abstract Error Error { get; }

    /// <inheritdoc />
    public abstract bool IsValid();
}
