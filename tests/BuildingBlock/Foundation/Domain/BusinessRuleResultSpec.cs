namespace BuildingBlock.Domain.Unit.Test;

/// <summary>Specifications for the result-returning business-rule helpers.</summary>
public sealed class BusinessRuleResultSpec
{
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
