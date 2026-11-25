namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="RepositoryConstant"/>.</summary>
public sealed class RepositoryConstantTests
{
    #region Not Found

    [Fact]
    public void NotFound_ShouldHaveDocumentedIdentity()
    {
        var error = RepositoryConstant.Constraints.NotFound.Error;

        error.Code.Should().Be("repository.not_found");
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Be("The requested aggregate was not found.");
    }

    [Fact]
    public void NotFound_ErrorFactory_ShouldFormatAggregateAndId()
    {
        var id = Guid.NewGuid();

        var error = RepositoryConstant.Constraints.NotFound.For<Person, Guid>(id);

        error.Code.Should().Be(RepositoryConstant.Constraints.NotFound.Error.Code);
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Be($"{nameof(Person)} with id '{id}' was not found.");
    }

    #endregion
    #region Paging

    [Fact]
    public void PagingConstraints_ShouldMatchDocumentedValues()
    {
        RepositoryConstant.Constraints.Paging.Page.Min.Value.Should().Be(1);
        RepositoryConstant.Constraints.Paging.PageSize.Min.Value.Should().Be(1);
        RepositoryConstant.Constraints.Paging.PageSize.Max.Value.Should().Be(1000);
    }

    #endregion
    #region Required Errors

    [Fact]
    public void RequiredErrors_ShouldMatchDocumentedIdentity()
    {
        var aggregate = RepositoryConstant.Constraints.Aggregate.Required.Error;
        var id = RepositoryConstant.Constraints.Id.Required.Error;
        var paging = RepositoryConstant.Constraints.Paging.Page.Min.Error;

        aggregate.Code.Should().Be("repository.aggregate.required");
        aggregate.Type.Should().Be(ErrorType.Validation);

        id.Code.Should().Be("repository.id.required");
        id.Type.Should().Be(ErrorType.Validation);

        paging.Code.Should().Be("repository.paging.invalid");
        paging.Type.Should().Be(ErrorType.Validation);
        paging.Message.Should().Be("Page must be at least 1.");

        RepositoryConstant.Constraints.Paging.PageSize.Min.Error.Message
            .Should().Be("Page size must be at least 1.");
        RepositoryConstant.Constraints.Paging.PageSize.Max.Error.Message
            .Should().Be("Page size cannot exceed 1000.");
    }

    #endregion
    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        RepositoryConstant.Defaults.Page.Should().Be(1);
        RepositoryConstant.Defaults.PageSize.Should().Be(50);
    }
    #endregion
}
