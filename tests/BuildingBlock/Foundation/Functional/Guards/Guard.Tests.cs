using BuildingBlock.Functional.Guards;

namespace BuildingBlock.Functional.Unit.Test.Guards;

/// <summary>Specifications for <see cref="Guard"/> covering result-based guard clauses.</summary>
public sealed class GuardTests
{
    #region Null and empty

    [Fact]
    public void AgainstNull_NullValue_ShouldFail()
    {
        var result = Guard.AgainstNull<string>(null);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.null");
    }

    [Fact]
    public void AgainstNull_Value_ShouldReturnValue()
    {
        var result = Guard.AgainstNull("value");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public void AgainstNull_CustomError_ShouldBeUsed()
    {
        var error = Error.Validation("custom.null", "custom");

        var result = Guard.AgainstNull<string>(null, error);

        result.Errors[0].Should().Be(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_BlankValue_ShouldFail(string? value)
    {
        var result = Guard.AgainstNullOrWhiteSpace(value);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.empty");
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_Value_ShouldReturnValue()
    {
        var result = Guard.AgainstNullOrWhiteSpace("value");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public void AgainstEmpty_NullCollection_ShouldFail()
    {
        var result = Guard.AgainstEmpty<string>(null);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.collection.empty");
    }

    [Fact]
    public void AgainstEmpty_EmptyCollection_ShouldFail()
    {
        var result = Guard.AgainstEmpty(Array.Empty<string>());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.collection.empty");
    }

    [Fact]
    public void AgainstEmpty_Collection_ShouldReturnCollection()
    {
        IReadOnlyCollection<string> values = new[] { "a", "b" };

        var result = Guard.AgainstEmpty(values);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(values);
    }

    #endregion

    #region Default and ranges

    [Fact]
    public void AgainstDefault_DefaultValue_ShouldFail()
    {
        var result = Guard.AgainstDefault(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.default");
    }

    [Fact]
    public void AgainstDefault_NonDefaultValue_ShouldReturnValue()
    {
        var value = Guid.NewGuid();

        var result = Guard.AgainstDefault(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void AgainstNegative_NegativeValue_ShouldFail()
    {
        var result = Guard.AgainstNegative(-1);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AgainstNegative_NonNegativeValue_ShouldReturnValue(int value)
    {
        var result = Guard.AgainstNegative(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgainstZeroOrNegative_NonPositiveValue_ShouldFail(int value)
    {
        var result = Guard.AgainstZeroOrNegative(value);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.zero_or_negative");
    }

    [Fact]
    public void AgainstZeroOrNegative_PositiveValue_ShouldReturnValue()
    {
        var result = Guard.AgainstZeroOrNegative(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(11)]
    public void AgainstOutOfRange_ValueOutsideRange_ShouldFail(int value)
    {
        var result = Guard.AgainstOutOfRange(value, 5, 10);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("guard.out_of_range");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(10)]
    public void AgainstOutOfRange_ValueInsideRange_ShouldReturnValue(int value)
    {
        var result = Guard.AgainstOutOfRange(value, 5, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    #endregion

    #region Predicates

    [Fact]
    public void Against_InvalidCondition_ShouldFail()
    {
        var error = Error.Validation("probe.invalid", "invalid");

        var result = Guard.Against(isInvalid: true, value: "value", error);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(error);
    }

    [Fact]
    public void Against_ValidCondition_ShouldReturnValue()
    {
        var error = Error.Validation("probe.invalid", "invalid");

        var result = Guard.Against(isInvalid: false, value: "value", error);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public void Ensure_PredicateRejectsValue_ShouldFail()
    {
        var error = Error.Validation("probe.invalid", "invalid");

        var result = Guard.Ensure("value", candidate => candidate.Length > 10, error);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(error);
    }

    [Fact]
    public void Ensure_PredicateAcceptsValue_ShouldReturnValue()
    {
        var error = Error.Validation("probe.invalid", "invalid");

        var result = Guard.Ensure("value", candidate => candidate.Length > 1, error);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    #endregion
}
