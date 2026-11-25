using BuildingBlock.Specification.Abstract;

namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for the railway-only <see cref="IRepository{TAggregate, TId}"/> contract.</summary>
public sealed class RepositoryInterfaceTests
{
    [Fact]
    #region Lookups

    public async Task FindAsync_ExistingAggregate_ShouldSucceed()
    {
        IRepository<Person, Guid> repository = new FakeRepository();
        var id = Guid.NewGuid();

        var result = await repository.FindAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public async Task FindAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        IRepository<Person> repository = new FakeRepository();
        var id = Guid.Empty;

        var result = await repository.FindAsync(id);

        result.IsFailure.Should().BeTrue();
        var error = result.Errors.Should().ContainSingle().Which;
        error.Code.Should().Be(RepositoryConstant.Constraints.NotFound.Error.Code);
        error.Message.Should().Be($"{nameof(Person)} with id '{id}' was not found.");
    }

    [Fact]
    #endregion
    #region Contracts

    public void GuidRepository_ShouldBeTheGenericRepositoryContract()
    {
        IRepository<Person> repository = new FakeRepository();

        repository.Should().BeAssignableTo<IRepository<Person, Guid>>();
    }

    [Fact]
    #endregion
    #region Writes

    public async Task AddAsync_NullAggregate_ShouldReturnErrorWithoutThrowing()
    {
        var repository = new FakeRepository();

        var result = await repository.AddAsync(null!);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(RepositoryConstant.Constraints.Aggregate.Required.Error);
    }

    [Fact]
    #endregion
    #region Queries

    public async Task QueryOperations_ShouldReturnResults()
    {
        IRepository<Person> repository = new FakeRepository();
        ISpecification<Person> specification = SpecificationExtensions.Create<Person>(_ => true).Value;

        (await repository.ExistsAsync(Guid.NewGuid())).Value.Should().BeTrue();
        (await repository.ListAsync(specification)).Value.Should().BeEmpty();
        (await repository.CountAsync(specification)).Value.Should().Be(0);
        (await repository.FirstOrDefaultAsync(specification)).IsFailure.Should().BeTrue();
    }

    #endregion
    #region Helper Types

    private sealed class FakeRepository : IRepository<Person>
    {
        public Task<Result<Person>> FindAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(RepositoryGuard.EnsureFound<Person, Guid>(
                id == Guid.Empty ? null : new Person { Id = id },
                id));

        public Task<Result<Person>> AddAsync(Person aggregate, CancellationToken cancellationToken = default)
            => Task.FromResult(RepositoryGuard.NotNull(aggregate));

        public Task<Result<Person>> UpdateAsync(Person aggregate, CancellationToken cancellationToken = default)
            => Task.FromResult(RepositoryGuard.NotNull(aggregate));

        public Task<Result<Person>> RemoveAsync(Person aggregate, CancellationToken cancellationToken = default)
            => Task.FromResult(RepositoryGuard.NotNull(aggregate));

        public Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(id != Guid.Empty));

        public Task<Result<IReadOnlyList<Person>>> ListAsync(
            ISpecification<Person> specification,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<Person>>(Array.Empty<Person>()));

        public Task<Result<Person>> FirstOrDefaultAsync(
            ISpecification<Person> specification,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<Person>(
                RepositoryConstant.Constraints.NotFound.For<Person, Guid>(Guid.Empty)));

        public Task<Result<int>> CountAsync(
            ISpecification<Person> specification,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(0));
    }
    #endregion
}
