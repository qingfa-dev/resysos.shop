using BuildingBlock.Domain.Modeling.Foundation.Referenceable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Referenceable;

/// <summary>Tests for <see cref="ReferenceableExtensions"/> covering extension behavior.</summary>
public class ReferenceableExtensionTests
{
    #region Helper Methods

    private sealed class TestReferenceable : IReferenceable
    {
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<TestReferenceable> Success(
        TestReferenceable entity)
    {
        return Result<TestReferenceable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetReference_ValidReference_ShouldSetReference()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = Success(entity).SetReference("ORD-2024-0001");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Reference.Should().Be("ORD-2024-0001");
    }

    [Fact]
    public void SetReference_WhitespaceReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference("   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.required");
        entity.Reference.Should().Be("keep-me");
    }

    [Fact]
    public void SetReference_TooLongReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };
        var reference = new string(
            'r',
            ReferenceableConstant.Reference.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).SetReference(reference);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.too_long");
        entity.Reference.Should().Be("keep-me");
    }

    [Fact]
    public void SetReference_NullReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference(null);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.required");
        entity.Reference.Should().Be("keep-me");
    }

    [Fact]
    public void SetReference_UntrimmedReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference(" ORD-1");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.untrimmed");
        entity.Reference.Should().Be("keep-me");
    }

    [Fact]
    public void SetReference_InvalidCharsetReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference("ORD@001!");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.invalid_charset");
        entity.Reference.Should().Be("keep-me");
    }

    [Fact]
    public void SetReference_AtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable();
        var reference = new string('r', ReferenceableConstant.Reference.MaxLength.Value);

        // Act:
        var result = Success(entity).SetReference(reference);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Reference.Should().Be(reference);
    }

    [Fact]
    public void SetReference_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.SetReference("ORD-1");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void ClearReference_NonEmpty_ShouldClearToEmpty()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-2024-0001" };

        // Act:
        var result = Success(entity).ClearReference();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
        entity.Reference.Should().Be(ReferenceableConstant.Defaults.Empty);
    }

    [Fact]
    public void ClearReference_AlreadyEmpty_ShouldStayEmpty()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = ReferenceableConstant.Defaults.Empty };

        // Act:
        var result = Success(entity).ClearReference();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Reference.Should().Be(string.Empty);
    }

    [Fact]
    public void ClearReference_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.ClearReference();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void EnsureReferenced_ValidReference_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-2024-0001" };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureReferenced_EmptyReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = string.Empty };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.required");
    }

    [Fact]
    public void EnsureReferenced_TooLongReference_ShouldFailTooLong()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = new string('r', ReferenceableConstant.Reference.MaxLength.Value + 1) };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.too_long");
    }

    [Fact]
    public void EnsureReferenced_UntrimmedReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-1 " };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.untrimmed");
    }

    [Fact]
    public void EnsureReferenced_InvalidCharsetReference_ShouldFailInvalidCharset()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD@001!" };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.invalid_charset");
    }

    [Fact]
    public void EnsureReferenced_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.EnsureReferenced();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    #endregion
}
