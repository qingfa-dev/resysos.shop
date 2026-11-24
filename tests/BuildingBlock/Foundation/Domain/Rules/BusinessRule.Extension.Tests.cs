namespace BuildingBlock.Domain.Unit.Test.Rules;

/// <summary>Tests for <see cref="BusinessRuleExtensions"/>.</summary>
public sealed class BusinessRuleExtensionTests
{
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

    [Fact]
    public void ToResult_Should_Return_Unit_For_A_Satisfied_Rule()
    {
        IBusinessRule rule = BusinessRuleExtensions.Rule(() => true, Error.Validation());

        rule.ToResult().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_Should_Return_The_Rule_Error()
    {
        IBusinessRule rule = BusinessRuleExtensions.Rule(
            () => false,
            Error.Validation("order.empty", "The order is empty."));

        Result<BuildingBlock.Functional.Results.Unit> result = rule.ToResult();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("order.empty");
    }

    [Fact]
    public void ToResult_Should_Aggregate_Every_Violation()
    {
        IBusinessRule[] rules =
        [
            BusinessRuleExtensions.Rule(() => false, Error.Validation("a", "a")),
            BusinessRuleExtensions.Rule(() => true, Error.Validation("b", "b")),
            BusinessRuleExtensions.Rule(() => false, Error.Validation("c", "c")),
        ];

        Result<BuildingBlock.Functional.Results.Unit> result = rules.ToResult();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void ToResult_Should_Succeed_For_No_Rules()
    {
        Array.Empty<IBusinessRule>().ToResult().IsSuccess.Should().BeTrue();
    }
}
