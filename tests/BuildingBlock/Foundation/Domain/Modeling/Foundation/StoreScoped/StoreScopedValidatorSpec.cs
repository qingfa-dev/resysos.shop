using BuildingBlock.Domain.Modeling.Foundation.StoreScoped;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.StoreScoped;

/// <summary>Specifications for <see cref="StoreScopedValidator"/> covering validation rules.</summary>
public class StoreScopedValidatorSpec
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
    public void ValidateStoreScope_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<
            TestStoreScoped,
            Guid>(null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.entity.required");
    }

    [Fact]
    public void ValidateStoreScope_GuidEmpty_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStoreScoped { StoreId = Guid.Empty };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStoreScoped, Guid>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreScope_ValidStoreId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStoreScoped { StoreId = Guid.NewGuid() };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStoreScoped, Guid>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateStoreId_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStoreScoped, Guid>(null!, Guid.NewGuid());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.entity.required");
    }

    [Fact]
    public void ValidateStoreId_GuidEmpty_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStoreScoped { StoreId = Guid.NewGuid() };

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStoreScoped, Guid>(entity, Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreId_ValidStoreId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStoreScoped();
        var storeId = Guid.NewGuid();

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStoreScoped, Guid>(entity, storeId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateStoreScope_BlankString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "   " };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStringStoreScoped, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreScope_EmptyString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = string.Empty };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStringStoreScoped, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreScope_NullString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = null! };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStringStoreScoped, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreScope_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "store-1" };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestStringStoreScoped, string>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateStoreScope_DefaultInt_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestIntStoreScoped { StoreId = 0 };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestIntStoreScoped, int>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreScope_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntStoreScoped { StoreId = 42 };

        // Act:
        var result = StoreScopedValidator.ValidateStoreScope<TestIntStoreScoped, int>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateStoreId_NullString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "store-1" };

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStringStoreScoped, string>(entity, null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreId_BlankString_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestStringStoreScoped { StoreId = "store-1" };

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStringStoreScoped, string>(entity, "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreId_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringStoreScoped();

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestStringStoreScoped, string>(entity, "store-1");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateStoreId_DefaultInt_ShouldFailStoreIdRequired()
    {
        // Arrange:
        var entity = new TestIntStoreScoped();

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestIntStoreScoped, int>(entity, 0);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void ValidateStoreId_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntStoreScoped();

        // Act:
        var result = StoreScopedValidator.ValidateStoreId<TestIntStoreScoped, int>(entity, 42);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    #endregion
}
