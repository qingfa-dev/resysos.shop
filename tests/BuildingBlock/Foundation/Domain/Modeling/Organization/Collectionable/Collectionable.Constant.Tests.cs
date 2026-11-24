using BuildingBlock.Domain.Modeling.Organization.Collectionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Collectionable;

/// <summary>Tests for <see cref="CollectionableConstant"/> covering constant values and errors.</summary>
public sealed class CollectionableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        CollectionableConstant.Entity.Required.Error.Code.Should().Be("collectionable.entity.required");
        CollectionableConstant.Collection.Required.Error.Code.Should().Be("collectionable.collection.required");
        CollectionableConstant.Collection.Empty.Error.Code.Should().Be("collectionable.collection.empty");
        CollectionableConstant.Collection.MaxLength.Error.Code.Should().Be("collectionable.collection.too_long");
        CollectionableConstant.Collection.MaxCount.Error.Code.Should().Be("collectionable.collection.limit_exceeded");
        CollectionableConstant.Collection.Duplicate.Error.Code.Should().Be("collectionable.collection.duplicate");
        CollectionableConstant.Collection.NotFound.Error.Code.Should().Be("collectionable.collection.not_found");
    }

    [Fact]
    public void Constants_ShouldMatchNestedConstraints()
    {
        CollectionableConstant.Collection.MaxLength.Value.Should().Be(128);
        CollectionableConstant.Collection.MaxCount.Value.Should().Be(32);
        CollectionableConstant.Defaults.TrimValues.Should().BeTrue();
    }
}
