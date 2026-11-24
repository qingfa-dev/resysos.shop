namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="InMemorySpecificationConstant"/>.</summary>
public sealed class InMemorySpecificationConstantTests
{
    #region Errors

    [Theory]
    [InlineData(0, "in_memory_specification.source.required")]
    [InlineData(1, "in_memory_specification.specification.required")]
    [InlineData(2, "in_memory_specification.evaluator.required")]
    [InlineData(3, "in_memory_repository.aggregate.required")]
    [InlineData(4, "in_memory_repository.id.required")]
    public void ProviderErrors_ShouldHaveDocumentedIdentity(int index, string code)
    {
        Error[] errors =
        [
            InMemorySpecificationConstant.Source.Required.Error,
            InMemorySpecificationConstant.Specification.Required.Error,
            InMemorySpecificationConstant.Evaluator.Required.Error,
            InMemorySpecificationConstant.Aggregate.Required.Error,
            InMemorySpecificationConstant.Id.Required.Error,
        ];

        errors[index].Code.Should().Be(code);
        errors[index].Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Factories

    [Fact]
    public void NotFoundFactory_ShouldFormatAggregateAndId()
    {
        var id = Guid.NewGuid();

        var error = InMemorySpecificationConstant.NotFound.For<Person, Guid>(id);

        error.Code.Should().Be("in_memory_repository.not_found");
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Be($"{nameof(Person)} with id '{id}' was not found.");
    }

    [Fact]
    public void ConflictFactory_ShouldFormatAggregateAndId()
    {
        var id = Guid.NewGuid();

        var error = InMemorySpecificationConstant.Conflict.AlreadyExists<Person, Guid>(id);

        error.Code.Should().Be("in_memory_repository.already_exists");
        error.Type.Should().Be(ErrorType.Conflict);
        error.Message.Should().Be($"{nameof(Person)} with id '{id}' already exists.");
    }

    [Fact]
    public void NoMatchError_ShouldHaveDocumentedIdentity()
    {
        var error = InMemorySpecificationConstant.NotFound.NoMatch.Error;

        error.Code.Should().Be("in_memory_repository.no_match");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    #endregion
}
