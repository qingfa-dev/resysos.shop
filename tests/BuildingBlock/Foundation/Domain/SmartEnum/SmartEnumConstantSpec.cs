using BuildingBlock.Domain.SmartEnums;

namespace BuildingBlock.Domain.Unit.Test.SmartEnums;

/// <summary>Specifications for <see cref="SmartEnumConstant"/> covering constant values and errors.</summary>
public sealed class SmartEnumConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        SmartEnumConstant.Id.Min.Error.Code.Should().Be("smart_enum.id.invalid");
        SmartEnumConstant.Name.Required.Error.Code.Should().Be("smart_enum.name.required");
        SmartEnumConstant.Name.MaxLength.Error.Code.Should().Be("smart_enum.name.too_long");
        SmartEnumConstant.Duplicate.Id.Error.Code.Should().Be("smart_enum.id.duplicate");
        SmartEnumConstant.Duplicate.Name.Error.Code.Should().Be("smart_enum.name.duplicate");
        SmartEnumConstant.Lookup.IdNotFound.Error.Code.Should().Be("smart_enum.id.not_found");
        SmartEnumConstant.Lookup.NameNotFound.Error.Code.Should().Be("smart_enum.name.not_found");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        SmartEnumConstant.Id.Min.Value.Should().Be(0);
        SmartEnumConstant.Name.MaxLength.Value.Should().Be(128);
    }
}
