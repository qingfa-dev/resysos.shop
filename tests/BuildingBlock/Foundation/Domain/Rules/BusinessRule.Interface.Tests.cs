namespace BuildingBlock.Domain.Unit.Test.Rules;

/// <summary>Tests for <see cref="IBusinessRule"/>.</summary>
public sealed class BusinessRuleInterfaceTests
{
    [Fact]
    public void Validate_SatisfiedRule_ShouldNotThrow()
    {
        var rule = BusinessRuleExtensions.Rule(
            () => true,
            Error.Validation("rule.invalid", "invalid"));

        var act = () => rule.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ViolatedRule_ShouldThrowDomainException()
    {
        var error = Error.Validation("rule.invalid", "invalid");
        var rule = BusinessRuleExtensions.Rule(() => false, error);

        var act = () => rule.Validate();

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(error);
    }
}
