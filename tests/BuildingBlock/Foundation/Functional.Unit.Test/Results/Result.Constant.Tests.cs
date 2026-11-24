namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultConstantTests
{
    private static readonly Error[] Constraints =
    [
        ResultConstant.Constraints.Errors.Required,
        ResultConstant.Constraints.Errors.Empty,
        ResultConstant.Constraints.Errors.ContainsNull,
        ResultConstant.Constraints.Value.Required,
        ResultConstant.Constraints.Value.Invalid,
        ResultConstant.Constraints.Filter.Unsatisfied,
    ];

    [Theory]
    [InlineData(0, "result.errors.required", "Errors are required")]
    [InlineData(1, "result.errors.empty", "A failure result must contain at least one error.")]
    [InlineData(2, "result.errors.contains_null", "A failure result cannot contain a null error.")]
    [InlineData(3, "result.value.required", "Value is required")]
    [InlineData(4, "result.value.invalid", "A failed result does not contain a value. Inspect Errors or use Match.")]
    [InlineData(5, "result.filter.unsatisfied", "The value did not satisfy the filter predicate.")]
    public void Constraint_HasExpectedMetadata(int index, string code, string message)
    {
        var error = Constraints[index];

        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0, "result.errors.required")]
    [InlineData(1, "result.errors.empty")]
    [InlineData(2, "result.errors.contains_null")]
    [InlineData(3, "result.value.required")]
    [InlineData(4, "result.value.invalid")]
    [InlineData(5, "result.filter.unsatisfied")]
    public void Constraint_ToString_ContainsCodeAndMessage(int index, string code)
    {
        var error = Constraints[index];

        error.ToString().Should().Contain(code);
        error.ToString().Should().Contain(error.Message);
    }
}
