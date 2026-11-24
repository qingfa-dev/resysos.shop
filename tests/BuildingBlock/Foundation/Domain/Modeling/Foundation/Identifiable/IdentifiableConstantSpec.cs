using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Identifiable;

/// <summary>Specifications for <see cref="IdentifiableConstant"/> covering constant values and errors.</summary>
public sealed class IdentifiableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        IdentifiableConstant.Entity.Required.Error.Code.Should().Be("identifiable.entity.required");
        IdentifiableConstant.Id.Required.Error.Code.Should().Be("identifiable.id.required");
    }

    [Fact]
    public void Constants_ShouldExposeExpectedDefaults()
    {
        IdentifiableConstant.Defaults.EmptyId.Should().Be(Guid.Empty);
    }
}
