namespace BuildingBlock.Domain.Unit.Test.Rules;

/// <summary>Specifications for <see cref="IBusinessRule"/>, <see cref="BusinessRule"/>, and rule helpers.</summary>
public sealed class BusinessRuleSpec
{
    #region Single rule

    [Fact]
    public void Rule_SatisfiedCondition_ShouldBeValid()
    {
        var rule = BusinessRuleExtensions.Rule(
            () => true,
            Error.Validation("rule.invalid", "invalid"));

        rule.IsValid().Should().BeTrue();
        rule.Error.Code.Should().Be("rule.invalid");
    }

    [Fact]
    public void Rule_ViolatedCondition_ShouldBeInvalid()
    {
        var error = Error.Validation("rule.invalid", "invalid");
        var rule = BusinessRuleExtensions.Rule(() => false, error);

        rule.IsValid().Should().BeFalse();
        rule.Error.Should().Be(error);
    }

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

    [Fact]
    public void ErrorFactory_ShouldBeEvaluatedOnAccess()
    {
        var calls = 0;
        var rule = BusinessRuleExtensions.Rule(
            () => false,
            () =>
            {
                calls++;
                return Error.Validation("rule.invalid", "invalid");
            });

        rule.Error.Should().NotBeNull();
        rule.Error.Should().NotBeNull();

        calls.Should().Be(2);
    }

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

    #endregion

    #region Collections

    [Fact]
    public void ValidateAll_ShouldReturnOnlyViolatedErrors()
    {
        var first = Error.Validation("rule.first", "first");
        var second = Error.Validation("rule.second", "second");

        var errors = new[]
        {
            BusinessRuleExtensions.Rule(() => true, Error.Validation("rule.ok", "ok")),
            BusinessRuleExtensions.Rule(() => false, first),
            BusinessRuleExtensions.Rule(() => false, second)
        }.ValidateAll();

        errors.Should().HaveCount(2);
        errors.Should().Contain(first);
        errors.Should().Contain(second);
    }

    [Fact]
    public void ValidateAll_NoViolations_ShouldReturnEmpty()
    {
        var errors = new[]
        {
            BusinessRuleExtensions.Rule(() => true, Error.Validation("rule.ok", "ok"))
        }.ValidateAll();

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAllAndThrow_NoViolations_ShouldNotThrow()
    {
        var rules = new[]
        {
            BusinessRuleExtensions.Rule(() => true, Error.Validation("rule.ok", "ok"))
        };

        var act = () => rules.ValidateAllAndThrow();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAllAndThrow_Violations_ShouldThrowAggregate()
    {
        var rules = new[]
        {
            BusinessRuleExtensions.Rule(() => false, Error.Validation("rule.first", "first")),
            BusinessRuleExtensions.Rule(() => false, Error.Validation("rule.second", "second"))
        };

        var act = () => rules.ValidateAllAndThrow();

        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("error.multiple_violations");
    }

    #endregion
}
