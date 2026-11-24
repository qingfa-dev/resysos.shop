using BuildingBlock.Domain.Modeling.Foundation.Tenantable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Tenantable;

/// <summary>Specifications for <see cref="TenantableValidator"/> covering validation rules.</summary>
public class TenantableValidatorSpec
{
    #region Helper Methods

    private sealed class TestTenantable : ITenantable<Guid>
    {
        public Guid TenantId { get; set; }
    }

    private sealed class TestStringTenantable : ITenantable<string>
    {
        public string TenantId { get; set; } = string.Empty;
    }

    private sealed class TestIntTenantable : ITenantable<int>
    {
        public int TenantId { get; set; }
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<TestTenantable> Success(TestTenantable entity)
    {
        return Result<TestTenantable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateTenancy_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = TenantableValidator.ValidateTenancy<TestTenantable, Guid>(
            null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.entity.required");
    }

    [Fact]
    public void ValidateTenancy_GuidEmpty_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestTenantable { TenantId = Guid.Empty };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestTenantable, Guid>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenancy_ValidTenantId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTenantable { TenantId = Guid.NewGuid() };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestTenantable, Guid>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateTenantId_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = TenantableValidator.ValidateTenantId<TestTenantable, Guid>(null!, Guid.NewGuid());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.entity.required");
    }

    [Fact]
    public void ValidateTenantId_GuidEmpty_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestTenantable { TenantId = Guid.NewGuid() };

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestTenantable, Guid>(entity, Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenantId_ValidTenantId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTenantable();
        var tenantId = Guid.NewGuid();

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestTenantable, Guid>(entity, tenantId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateTenancy_BlankString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "   " };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestStringTenantable, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenancy_EmptyString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = string.Empty };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestStringTenantable, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenancy_NullString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = null! };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestStringTenantable, string>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenancy_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "tenant-1" };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestStringTenantable, string>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateTenancy_DefaultInt_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestIntTenantable { TenantId = 0 };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestIntTenantable, int>(entity);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenancy_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntTenantable { TenantId = 42 };

        // Act:
        var result = TenantableValidator.ValidateTenancy<TestIntTenantable, int>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateTenantId_NullString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "tenant-1" };

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestStringTenantable, string>(entity, null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenantId_BlankString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "tenant-1" };

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestStringTenantable, string>(entity, "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenantId_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringTenantable();

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestStringTenantable, string>(entity, "tenant-1");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateTenantId_DefaultInt_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestIntTenantable();

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestIntTenantable, int>(entity, 0);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void ValidateTenantId_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntTenantable();

        // Act:
        var result = TenantableValidator.ValidateTenantId<TestIntTenantable, int>(entity, 42);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    #endregion
}
