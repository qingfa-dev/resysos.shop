using BuildingBlock.Domain.Modeling.Organization.Categorizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Categorizable;

/// <summary>Specifications for <see cref="CategorizableConstant"/> covering constant values and errors.</summary>
public sealed class CategorizableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        CategorizableConstant.Entity.Required.Error.Code.Should().Be("categorizable.entity.required");
        CategorizableConstant.Category.Required.Error.Code.Should().Be("categorizable.category.required");
        CategorizableConstant.Category.Empty.Error.Code.Should().Be("categorizable.category.empty");
        CategorizableConstant.Category.MaxLength.Error.Code.Should().Be("categorizable.category.too_long");
        CategorizableConstant.Category.MaxCount.Error.Code.Should().Be("categorizable.category.limit_exceeded");
        CategorizableConstant.Category.Duplicate.Error.Code.Should().Be("categorizable.category.duplicate");
        CategorizableConstant.Category.NotFound.Error.Code.Should().Be("categorizable.category.not_found");
    }

    [Fact]
    public void Constants_ShouldMatchNestedConstraints()
    {
        CategorizableConstant.Category.MaxLength.Value.Should().Be(128);
        CategorizableConstant.Category.MaxCount.Value.Should().Be(32);
        CategorizableConstant.Defaults.TrimValues.Should().BeTrue();
    }
}
