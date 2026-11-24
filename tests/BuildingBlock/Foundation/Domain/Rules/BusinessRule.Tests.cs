namespace BuildingBlock.Domain.Unit.Test.Rules;

/// <summary>Tests for <see cref="BusinessRule"/>.</summary>
public sealed class BusinessRuleTests
{
    [Fact]
    public void Constructor_NullCondition_ShouldThrow()
    {
        var act = () => new BusinessRule<object>(
            null!,
            () => Error.Validation("rule.invalid", "invalid"));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullErrorFactory_ShouldThrow()
    {
        var act = () => new BusinessRule<object>(() => true, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
