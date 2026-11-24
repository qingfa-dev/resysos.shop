using BuildingBlock.Domain.Modeling.Foundation.Tenantable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Tenantable;

/// <summary>Tests for <see cref="TenantableConstant"/> covering constant values and errors.</summary>
public sealed class TenantableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        TenantableConstant.Entity.Required.Error.Code.Should().Be("tenantable.entity.required");
        TenantableConstant.Tenant.Required.Error.Code.Should().Be("tenantable.tenant_id.required");
    }

    [Fact]
    public void Constants_ShouldExposeExpectedDefaults()
    {
        TenantableConstant.Defaults.EmptyTenantId.Should().Be(Guid.Empty);
    }
}
