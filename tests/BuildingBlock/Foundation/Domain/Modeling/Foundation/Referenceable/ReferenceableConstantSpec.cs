using BuildingBlock.Domain.Modeling.Foundation.Referenceable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Referenceable;

/// <summary>Specifications for <see cref="ReferenceableConstant"/> covering constant values and errors.</summary>
public sealed class ReferenceableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        ReferenceableConstant.Entity.Required.Error.Code.Should().Be("referenceable.entity.required");
        ReferenceableConstant.Reference.Required.Error.Code.Should().Be("referenceable.reference.required");
        ReferenceableConstant.Reference.MaxLength.Error.Code.Should().Be("referenceable.reference.too_long");
        ReferenceableConstant.Reference.Trimmed.Error.Code.Should().Be("referenceable.reference.untrimmed");
        ReferenceableConstant.Reference.Charset.Error.Code.Should().Be("referenceable.reference.invalid_charset");
    }

    [Fact]
    public void Constants_ShouldMatchNestedDefinitions()
    {
        ReferenceableConstant.Reference.MinLength.Value.Should().Be(1);
        ReferenceableConstant.Reference.MaxLength.Value.Should().Be(128);
        ReferenceableConstant.Defaults.Empty.Should().Be(string.Empty);
        ReferenceableConstant.Reference.Trimmed.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        ReferenceableConstant.Reference.Charset.Pattern.Should().Be(@"^[A-Za-z0-9\-_/#:. ]+$");
    }
}
