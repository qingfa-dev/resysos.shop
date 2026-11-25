namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for <see cref="QuerySpecificationMapper"/>.</summary>
public sealed class QuerySpecificationMapperTests
{
    #region Mapping

    [Fact]
    public void Instance_ShouldBeShared()
    {
        QuerySpecificationMapper.Instance.Should().BeSameAs(QuerySpecificationMapper.Instance);
    }

    [Fact]
    public void Map_EmptyRequest_ShouldMatchAll()
    {
        var result = QuerySpecificationMapper.Instance.Map<Person>(Mapping.Request());

        result.IsSuccess.Should().BeTrue();
        result.Value.Criteria.Should().NotBeNull();
        result.Value.IsSatisfiedBy(People.Ada).Should().BeTrue();
        result.Value.IsSatisfiedBy(People.Noah).Should().BeTrue();
    }

    [Fact]
    public void Map_WithFilters_ShouldBuildCriteria()
    {
        var request = Mapping.Request(filters:
        [
            Mapping.Filter("age", FilterOperator.Gte, "18"),
            Mapping.Filter("status", FilterOperator.Eq, "Active")
        ]);

        var specification = QuerySpecificationMapper.Instance.Map<Person>(request).Value;

        specification.IsSatisfiedBy(People.Ada).Should().BeTrue();
        specification.IsSatisfiedBy(People.Noah).Should().BeTrue();
        specification.IsSatisfiedBy(People.Grace).Should().BeFalse();
        specification.IsSatisfiedBy(People.Alan).Should().BeFalse();
    }

    [Fact]
    public void Map_WithNullRequest_ShouldReturnRequiredError()
    {
        var result = QuerySpecificationMapper.Instance.Map<Person>(null);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Request.Required.Error);
    }

    [Fact]
    public void Map_ShouldExposeOrderingExpressions()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "name", Direction = SortDirection.Descending },
            new SortDescriptor { Field = "age" }
        ]);

        var specification = QuerySpecificationMapper.Instance.Map<Person>(request).Value;

        specification.OrderBy.Should().HaveCount(2);
        specification.OrderBy[0].Descending.Should().BeTrue();
        specification.OrderBy[1].Descending.Should().BeFalse();
    }

    #endregion

    #region Safety

    [Fact]
    public void Map_InvalidRequest_ShouldNotThrow()
    {
        var request = Mapping.Request(
            filters: [Mapping.Filter("nickname", FilterOperator.Eq, "A")],
            sorts: [new SortDescriptor { Field = "nickname" }]);

        var act = () => QuerySpecificationMapper.Instance.Map<Person>(request);

        act.Should().NotThrow().Which.IsFailure.Should().BeTrue();
    }

    #endregion
}
