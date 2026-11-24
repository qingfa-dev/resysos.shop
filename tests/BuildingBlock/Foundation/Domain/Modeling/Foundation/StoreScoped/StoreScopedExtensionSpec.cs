using BuildingBlock.Domain.Modeling.Foundation.StoreScoped;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.StoreScoped;

/// <summary>Specifications for <see cref="StoreScopedExtensionSpecExtensions"/> covering extension behavior.</summary>
public class StoreScopedExtensionSpec
{
    #region Helper Methods

    private sealed class TestStoreScoped : IStoreScoped<Guid>
    {
        public Guid StoreId { get; set; }
    }

    private sealed class TestStringStoreScoped : IStoreScoped<string>
    {
        public string StoreId { get; set; } = string.Empty;
    }

    private sealed class TestIntStoreScoped : IStoreScoped<int>
    {
        public int StoreId { get; set; }
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<T> Success<T>(T entity)
    {
        return Result<T>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void EnsureStoreScoped_WithStoreId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStoreScoped { StoreId = Guid.NewGuid() };

        // Act:
        var result = Success(entity).EnsureStoreScoped<TestStoreScoped, Guid>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureStoreScoped_DefaultStoreId_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStoreScoped();

        // Act:
        var result = Success(entity).EnsureStoreScoped<TestStoreScoped, Guid>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void SetStoreId_ValidStoreId_ShouldSetStoreId()
    {
        // Arrange:
        var entity = new TestStoreScoped();
        var storeId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetStoreId<TestStoreScoped, Guid>(storeId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.StoreId.Should().Be(storeId);
    }

    [Fact]
    public void SetStoreId_GuidEmpty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var keep = Guid.NewGuid();
        var entity = new TestStoreScoped { StoreId = keep };

        // Act:
        var result = Success(entity).SetStoreId<TestStoreScoped, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
        entity.StoreId.Should().Be(keep);
    }

    [Fact]
    public void EnsureStoreScoped_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestStoreScoped>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.EnsureStoreScoped<TestStoreScoped, Guid>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void EnsureStoreScoped_BlankString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "   " };

        // Act:
        var result = Result<TestStringStoreScoped>.Success(entity).EnsureStoreScoped<TestStringStoreScoped, string>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void EnsureStoreScoped_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "store-1" };

        // Act:
        var result = Result<TestStringStoreScoped>.Success(entity).EnsureStoreScoped<TestStringStoreScoped, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void SetStoreId_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestStoreScoped>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.SetStoreId<TestStoreScoped, Guid>(Guid.NewGuid());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void SetStoreId_NullString_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "keep-me" };

        // Act:
        var result = Result<TestStringStoreScoped>.Success(entity).SetStoreId<TestStringStoreScoped, string>(null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
        entity.StoreId.Should().Be("keep-me");
    }

    [Fact]
    public void SetStoreId_BlankString_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "keep-me" };

        // Act:
        var result = Result<TestStringStoreScoped>.Success(entity).SetStoreId<TestStringStoreScoped, string>("   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
        entity.StoreId.Should().Be("keep-me");
    }

    [Fact]
    public void SetStoreId_ValidString_ShouldSetStoreId()
    {
        // Arrange:
        var entity = new TestStringStoreScoped();

        // Act:
        var result = Result<TestStringStoreScoped>.Success(entity).SetStoreId<TestStringStoreScoped, string>("store-1");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.StoreId.Should().Be("store-1");
    }

    #endregion
}
