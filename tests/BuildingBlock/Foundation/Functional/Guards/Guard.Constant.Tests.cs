namespace BuildingBlock.Functional.Unit.Test.Guards;

/// <summary>Specifications for <see cref="GuardConstant"/> covering error codes.</summary>
public sealed class GuardConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        GuardConstant.Null.Error.Code.Should().Be("guard.null");
        GuardConstant.Empty.Error.Code.Should().Be("guard.empty");
        GuardConstant.Default.Error.Code.Should().Be("guard.default");
        GuardConstant.Negative.Error.Code.Should().Be("guard.negative");
        GuardConstant.ZeroOrNegative.Error.Code.Should().Be("guard.zero_or_negative");
        GuardConstant.OutOfRange.Error.Code.Should().Be("guard.out_of_range");
        GuardConstant.EmptyCollection.Error.Code.Should().Be("guard.collection.empty");
    }

    [Fact]
    public void Errors_ShouldBeValidationErrors()
    {
        GuardConstant.Null.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.Empty.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.Default.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.Negative.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.ZeroOrNegative.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.OutOfRange.Error.Type.Should().Be(ErrorType.Validation);
        GuardConstant.EmptyCollection.Error.Type.Should().Be(ErrorType.Validation);
    }
}
