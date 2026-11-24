using BuildingBlock.Domain.Modeling.Foundation.Tenantable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Tenantable;

/// <summary>Tests for <see cref="TenantableExtensions"/> covering extension behavior.</summary>
public class TenantableExtensionTests
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
    public void EnsureTenanted_WithTenantId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTenantable { TenantId = Guid.NewGuid() };

        // Act:
        var result = Success(entity).EnsureTenanted<TestTenantable, Guid>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureTenanted_DefaultTenantId_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestTenantable();

        // Act:
        var result = Success(entity).EnsureTenanted<TestTenantable, Guid>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void SetTenantId_ValidTenantId_ShouldSetTenantId()
    {
        // Arrange:
        var entity = new TestTenantable();
        var tenantId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetTenantId<TestTenantable, Guid>(tenantId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void SetTenantId_GuidEmpty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var keep = Guid.NewGuid();
        var entity = new TestTenantable { TenantId = keep };

        // Act:
        var result = Success(entity).SetTenantId<TestTenantable, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
        entity.TenantId.Should().Be(keep);
    }

    [Fact]
    public void EnsureTenanted_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestTenantable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.EnsureTenanted<TestTenantable, Guid>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void EnsureTenanted_BlankString_ShouldFailTenantIdRequired()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "   " };

        // Act:
        var result = Result<TestStringTenantable>.Success(entity).EnsureTenanted<TestStringTenantable, string>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void EnsureTenanted_ValidString_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "tenant-1" };

        // Act:
        var result = Result<TestStringTenantable>.Success(entity).EnsureTenanted<TestStringTenantable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void SetTenantId_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestTenantable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.SetTenantId<TestTenantable, Guid>(Guid.NewGuid());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void SetTenantId_NullString_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "keep-me" };

        // Act:
        var result = Result<TestStringTenantable>.Success(entity).SetTenantId<TestStringTenantable, string>(null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
        entity.TenantId.Should().Be("keep-me");
    }

    [Fact]
    public void SetTenantId_BlankString_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestStringTenantable { TenantId = "keep-me" };

        // Act:
        var result = Result<TestStringTenantable>.Success(entity).SetTenantId<TestStringTenantable, string>("   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("tenantable.tenant_id.required");
        entity.TenantId.Should().Be("keep-me");
    }

    [Fact]
    public void SetTenantId_ValidString_ShouldSetTenantId()
    {
        // Arrange:
        var entity = new TestStringTenantable();

        // Act:
        var result = Result<TestStringTenantable>.Success(entity).SetTenantId<TestStringTenantable, string>("tenant-1");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.TenantId.Should().Be("tenant-1");
    }

    #endregion
}
