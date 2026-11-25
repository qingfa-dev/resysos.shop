namespace BuildingBlock.Persistence.Unit.Tests;

/// <summary>Tests for <see cref="RepositoryConstant"/> and <see cref="InMemoryRepositoryConstant"/>.</summary>
public sealed class RepositoryConstantTests
{
    #region Repository

    [Fact]
    public void AggregateRequired_ShouldHaveDocumentedIdentity()
    {
        var error = RepositoryConstant.Constraints.Aggregate.Required.Error;

        error.Code.Should().Be("repository.aggregate.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void IdRequired_ShouldHaveDocumentedIdentity()
        => RepositoryConstant.Constraints.Id.Required.Error.Code.Should().Be("repository.id.required");

    [Fact]
    public void PredicateRequired_ShouldHaveDocumentedIdentity()
        => RepositoryConstant.Constraints.Predicate.Required.Error.Code.Should().Be("repository.predicate.required");

    [Fact]
    public void NotFoundFor_ShouldNameAggregateAndId()
    {
        var id = Guid.NewGuid();

        var error = RepositoryConstant.Constraints.NotFound.For<Order, Guid>(id);

        error.Code.Should().Be("repository.not_found");
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Contain(nameof(Order)).And.Contain(id.ToString());
    }

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        RepositoryConstant.Defaults.Page.Should().Be(1);
        RepositoryConstant.Defaults.PageSize.Should().Be(50);
    }

    #endregion

    #region In-memory

    [Fact]
    public void InMemoryNotFound_ShouldHaveDocumentedIdentity()
        => InMemoryRepositoryConstant.Constraints.NotFound.NoMatch.Error.Code
            .Should().Be("in_memory_repository.no_match");

    [Fact]
    public void InMemoryConflict_ShouldNameAggregateAndId()
    {
        var id = Guid.NewGuid();

        var error = InMemoryRepositoryConstant.Constraints.Conflict.AlreadyExists<Order, Guid>(id);

        error.Code.Should().Be("in_memory_repository.already_exists");
        error.Type.Should().Be(ErrorType.Conflict);
        error.Message.Should().Contain(nameof(Order)).And.Contain(id.ToString());
    }

    #endregion
}
