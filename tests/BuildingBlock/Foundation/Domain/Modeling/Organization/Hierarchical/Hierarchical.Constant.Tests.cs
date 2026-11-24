using BuildingBlock.Domain.Modeling.Organization.Hierarchical;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Hierarchical;

/// <summary>Tests for <see cref="HierarchicalConstant"/> covering constant values and errors.</summary>
public sealed class HierarchicalConstantTests
{
    [Fact]
    public void FailureCodes_ShouldHaveExpectedCodes()
    {
        HierarchicalConstant.Entity.Required.Error.Code.Should().Be("hierarchical.entity.required");
        HierarchicalConstant.Parent.Empty.Error.Code.Should().Be("hierarchical.parent.empty");
        HierarchicalConstant.Parent.Self.Error.Code.Should().Be("hierarchical.parent.self_parent");
        HierarchicalConstant.Parent.Cycle.Error.Code.Should().Be("hierarchical.parent.cycle_detected");
        HierarchicalConstant.Level.Max.DepthExceeded.Code.Should().Be("hierarchical.depth.exceeded");
        HierarchicalConstant.Level.Range.Error.Code.Should().Be("hierarchical.level.out_of_range");
        HierarchicalConstant.Parent.MaxPathLength.Error.Code.Should().Be("hierarchical.path.too_long");
    }

    [Fact]
    public void Constants_ShouldMatchNestedConstraints()
    {
        HierarchicalConstant.Level.Min.Value.Should().Be(0);
        HierarchicalConstant.Level.Max.Value.Should().Be(32);
        HierarchicalConstant.Parent.MaxPathLength.Value.Should().Be(1024);
        HierarchicalConstant.Defaults.RootLevel.Should().Be(0);
        HierarchicalConstant.Defaults.PathSeparator.Should().Be('/');
        HierarchicalConstant.Patterns.Path.Should().Be(@"^[^/]+(?:/[^/]+)*$");
    }
}
