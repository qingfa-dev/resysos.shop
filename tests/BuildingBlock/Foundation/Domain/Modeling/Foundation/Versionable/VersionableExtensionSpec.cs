using BuildingBlock.Domain.Modeling.Foundation.Versionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Versionable;

/// <summary>Specifications for <see cref="VersionableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class VersionableExtensionSpec
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
    public void BumpVersion_ZeroVersion_ShouldIncrementToOne()
    {
        // Arrange:
        var entity = new TestVersionable();

        // Act:
        var result = Success(entity).BumpVersion();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Version.Should().Be(1);
    }

    [Fact]
    public void BumpVersion_NegativeVersion_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestVersionable { Version = -1 };

        // Act:
        var result = Success(entity).BumpVersion();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.negative");
        entity.Version.Should().Be(-1);
    }

    [Fact]
    public void BumpVersion_MaxVersion_ShouldFailOverflowWithoutMutation()
    {
        // Arrange:
        var entity = new TestVersionable { Version = long.MaxValue };

        // Act:
        var result = Success(entity).BumpVersion();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.overflow");
        entity.Version.Should().Be(long.MaxValue);
    }

    [Fact]
    public void EnsureVersionMatch_Mismatch_ShouldFailConcurrencyConflict()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 5 };

        // Act:
        var result = Success(entity).EnsureVersionMatch(4);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.concurrency_conflict");
    }

    [Fact]
    public void EnsureVersionMatch_Match_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 5 };

        // Act:
        var result = Success(entity).EnsureVersionMatch(5);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void InitializeVersion_ShouldResetToInitial()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 42 };

        // Act:
        var result = Success(entity).InitializeVersion();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Version.Should().Be(VersionableConstant.Defaults.InitialVersion);
    }

    [Fact]
    public void EnsureVersioned_ValidVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 7 };

        // Act:
        var result = Success(entity).EnsureVersioned();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureVersioned_NegativeVersion_ShouldFailOutOfRange()
    {
        // Arrange:
        var entity = new TestVersionable { Version = -1 };

        // Act:
        var result = Success(entity).EnsureVersioned();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("versionable.version.out_of_range");
    }

    [Fact]
    public void BumpVersion_AtMaxMinusOne_ShouldSucceedToMax()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Version.Max.Value - VersionableConstant.Defaults.Increment };

        // Act:
        var result = Success(entity).BumpVersion();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Version.Should().Be(VersionableConstant.Version.Max.Value);
    }

    [Fact]
    public void BumpVersion_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestVersionable { Version = 5 };
        var input = Result<TestVersionable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.BumpVersion();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.Version.Should().Be(5);
    }

    [Fact]
    public void EnsureVersioned_MaxVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Version.Max.Value };

        // Act:
        var result = Success(entity).EnsureVersioned();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureVersioned_MinVersion_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Version.Min.Value };

        // Act:
        var result = Success(entity).EnsureVersioned();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureVersioned_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestVersionable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.EnsureVersioned();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void EnsureVersionMatch_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestVersionable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.EnsureVersionMatch(5);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void InitializeVersion_AlreadyInitial_ShouldStayInitial()
    {
        // Arrange:
        var entity = new TestVersionable { Version = VersionableConstant.Defaults.InitialVersion };

        // Act:
        var result = Success(entity).InitializeVersion();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Version.Should().Be(VersionableConstant.Defaults.InitialVersion);
    }

    [Fact]
    public void InitializeVersion_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestVersionable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.InitializeVersion();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    #endregion
}
