namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="QueryGuard"/>.</summary>
public sealed class QueryGuardTests
{
    #region Request

    [Fact]
    public void Complete_WithRequest_ShouldSucceed()
    {
        var request = QueryRequest.PageOnly();

        QueryGuard.Complete(request).Value.Should().BeSameAs(request);
    }

    [Fact]
    public void Complete_WithNull_ShouldReturnError()
    {
        QueryGuard.Complete(null).Errors.Should().ContainSingle()
            .Which.Should().Be(QueryConstant.Constraints.Request.Required.Error);
    }

    #endregion
}
