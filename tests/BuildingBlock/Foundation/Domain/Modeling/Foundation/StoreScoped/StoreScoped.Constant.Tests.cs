using BuildingBlock.Domain.Modeling.Foundation.StoreScoped;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.StoreScoped;

/// <summary>Tests for <see cref="StoreScopedConstant"/> covering constant values and errors.</summary>
public sealed class StoreScopedConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        StoreScopedConstant.Entity.Required.Error.Code.Should().Be("store_scoped.entity.required");
        StoreScopedConstant.Store.Required.Error.Code.Should().Be("store_scoped.store_id.required");
    }

    [Fact]
    public void Constants_ShouldExposeExpectedDefaults()
    {
        StoreScopedConstant.Defaults.EmptyStoreId.Should().Be(Guid.Empty);
    }
}
