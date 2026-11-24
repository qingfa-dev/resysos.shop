using BuildingBlock.Domain.Modeling.Foundation.Versionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Versionable;

/// <summary>Tests for <see cref="VersionableValidator"/> covering validation rules.</summary>
public class VersionableValidatorTests
{
    #region Helper Methods

    private sealed class TestVersionable : IVersionable
    {
        public long Version { get; set; }
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<TestVersionable> Success(TestVersionable entity)
    {
        return Result<TestVersionable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateBump_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = VersionableValidator.ValidateBump(
            (TestVersionable)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.entity.required");
    }

    [Fact]
    public void ValidateBump_NegativeVersion_ShouldFailNegative()
    {
        // Arrange:
        var entity = new TestVersionable { Version = -5 };

        // Act:
        var result = VersionableValidator.ValidateBump(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.negative");
    }

    [Fact]
    public void ValidateBump_MaxVersion_ShouldFailOverflow()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Version.Max.Value };

        // Act:
        var result = VersionableValidator.ValidateBump(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.overflow");
    }

    [Fact]
    public void ValidateBump_ValidVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 0 };

        // Act:
        var result = VersionableValidator.ValidateBump(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateVersion_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = VersionableValidator.ValidateVersion((TestVersionable)null!, 0);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.entity.required");
    }

    [Fact]
    public void ValidateVersion_NegativeVersion_ShouldFailOutOfRange()
    {
        // Arrange:
        var entity = new TestVersionable();

        // Act:
        var result = VersionableValidator.ValidateVersion(entity, -1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.out_of_range");
    }

    [Fact]
    public void ValidateVersion_MinVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable();

        // Act:
        var result = VersionableValidator.ValidateVersion(entity, VersionableConstant.Version.Min.Value);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateVersion_MaxVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable();

        // Act:
        var result = VersionableValidator.ValidateVersion(entity, VersionableConstant.Version.Max.Value);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateConcurrency_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = VersionableValidator.ValidateConcurrency((TestVersionable)null!, 0);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.entity.required");
    }

    [Fact]
    public void ValidateConcurrency_Mismatch_ShouldFailConcurrencyConflict()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 3 };

        // Act:
        var result = VersionableValidator.ValidateConcurrency(entity, 2);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.concurrency_conflict");
    }

    [Fact]
    public void ValidateConcurrency_Match_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 3 };

        // Act:
        var result = VersionableValidator.ValidateConcurrency(entity, 3);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateVersion_TooLarge_ShouldFailOutOfRange()
    {
        // Arrange:
        var entity = new TestVersionable();
        var tooLarge = VersionableConstant.Version.Max.Value;

        // Act:
        var validAtMax = VersionableValidator.ValidateVersion(entity, tooLarge);
        var invalidBelowMin = VersionableValidator.ValidateVersion(entity, VersionableConstant.Version.Min.Value - 1);

        // Assert:
        validAtMax.IsSuccess.Should().BeTrue();
        invalidBelowMin.IsFailure.Should().BeTrue();
        invalidBelowMin.Errors![0].Code.Should().Be("versionable.version.out_of_range");
    }

    [Fact]
    public void ValidateBump_AtMaxMinusOne_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Version.Max.Value - VersionableConstant.Defaults.Increment };

        // Act:
        var result = VersionableValidator.ValidateBump(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    #endregion
}
