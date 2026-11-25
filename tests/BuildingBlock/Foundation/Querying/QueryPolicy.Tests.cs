namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryPolicy"/>.</summary>
public sealed class QueryPolicyTests
{
    #region Defaults

    [Fact]
    public void Defaults_ShouldDenyByDefault()
    {
        var policy = new QueryPolicy();

        policy.Exposure.Should().Be(QueryExposure.DenyByDefault);
        policy.Registry.Should().NotBeNull();
    }

    [Fact]
    public void Defaults_ShouldMatchDocumentedCaps()
    {
        var policy = new QueryPolicy();

        policy.DefaultPageSize.Should().Be(20);
        policy.MaxPageSize.Should().Be(200);
        policy.MaxFilterDepth.Should().Be(5);
        policy.MaxFilterConditions.Should().Be(50);
    }

    #endregion

    #region State

    [Fact]
    public void State_ShouldBeSettable()
    {
        var registry = new QueryFieldRegistry();

        var policy = new QueryPolicy
        {
            Exposure = QueryExposure.AllowByDefault,
            DefaultPageSize = 40,
            MaxPageSize = 50,
            MaxFilterDepth = 3,
            MaxFilterConditions = 10,
            Registry = registry
        };

        policy.Exposure.Should().Be(QueryExposure.AllowByDefault);
        policy.DefaultPageSize.Should().Be(40);
        policy.MaxPageSize.Should().Be(50);
        policy.MaxFilterDepth.Should().Be(3);
        policy.MaxFilterConditions.Should().Be(10);
        policy.Registry.Should().BeSameAs(registry);
    }

    #endregion
}
