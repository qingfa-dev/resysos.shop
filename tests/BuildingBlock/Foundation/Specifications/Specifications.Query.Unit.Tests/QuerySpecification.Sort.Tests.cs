namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for sort mapping.</summary>
public sealed class QuerySpecificationSortTests
{
    #region Direction

    [Fact]
    public void Ascending_ShouldOrderSmallestFirst()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "age", Direction = SortDirection.Ascending }
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Alan", "Noah", "Ada", "Grace");
    }

    [Fact]
    public void Descending_ShouldOrderLargestFirst()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "age", Direction = SortDirection.Descending }
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Grace", "Ada", "Noah", "Alan");
    }

    [Fact]
    public void DefaultDirection_ShouldBeAscending()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "age" }
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Alan", "Noah", "Ada", "Grace");
    }

    #endregion

    #region Composition

    [Fact]
    public void MultipleSorts_ShouldApplyInOrder()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "status", Direction = SortDirection.Ascending },
            new SortDescriptor { Field = "age", Direction = SortDirection.Descending }
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Alan", "Ada", "Noah", "Grace");
    }

    [Fact]
    public void NestedField_ShouldOrderByLeaf()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "address.city", Direction = SortDirection.Ascending }
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Noah", "Ada", "Alan", "Grace");
    }

    [Fact]
    public void NestedField_WithNullParent_ShouldNotThrow()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "address.city" }
        ]);

        var act = () => Mapping.Evaluate(request);

        act.Should().NotThrow();
    }

    #endregion

    #region Invalid Input

    [Fact]
    public void UnknownField_ShouldReturnFieldError()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "nickname" }
        ]);

        var result = Mapping.Evaluate(request);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.field.unknown");
    }

    [Fact]
    public void UndefinedDirection_ShouldReturnDirectionError()
    {
        var request = Mapping.Request(sorts:
        [
            new SortDescriptor { Field = "age", Direction = (SortDirection)99 }
        ]);

        var result = Mapping.Evaluate(request);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.sort.direction.invalid");
    }

    [Fact]
    public void InvalidSorts_ShouldNotThrow()
    {
        var cases = new[]
        {
            new SortDescriptor { Field = "nickname" },
            new SortDescriptor { Field = "age", Direction = (SortDirection)99 }
        };

        foreach (SortDescriptor sort in cases)
        {
            var act = () => Mapping.Evaluate(Mapping.Request(sorts: [sort]));

            act.Should().NotThrow();
        }
    }

    #endregion
}
