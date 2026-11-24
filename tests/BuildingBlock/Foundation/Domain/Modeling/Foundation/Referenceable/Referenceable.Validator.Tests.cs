using BuildingBlock.Domain.Modeling.Foundation.Referenceable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Referenceable;

/// <summary>Tests for <see cref="ReferenceableValidator"/> covering validation rules.</summary>
public class ReferenceableValidatorTests
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
    public void ValidateReference_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = ReferenceableValidator.ValidateReference(
            (TestReferenceable)null!,
            "ORD-1");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.entity.required");
    }

    [Fact]
    public void ValidateReference_NullReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, null);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.required");
    }

    [Fact]
    public void ValidateReference_WhitespaceReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.required");
    }

    [Fact]
    public void ValidateReference_TooLongReference_ShouldFailTooLong()
    {
        // Arrange:
        var entity = new TestReferenceable();
        var reference = new string('r', ReferenceableConstant.Reference.MaxLength.Value + 1);

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, reference);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.too_long");
    }

    [Fact]
    public void ValidateReference_LeadingSpaceReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, " ORD-1");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.untrimmed");
    }

    [Fact]
    public void ValidateReference_TrailingSpaceReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD-1 ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.untrimmed");
    }

    [Fact]
    public void ValidateReference_BadCharsetReference_ShouldFailInvalidCharset()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD@001!");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("referenceable.reference.invalid_charset");
    }

    [Fact]
    public void ValidateReference_ValidReference_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD-2024-0001");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    #endregion
}
