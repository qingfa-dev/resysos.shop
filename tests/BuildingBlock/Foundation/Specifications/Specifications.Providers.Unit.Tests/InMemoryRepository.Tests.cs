namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for the Result-only <see cref="InMemoryRepository{TAggregate, TId}"/>.</summary>
public sealed class InMemoryRepositoryTests
{
    #region Factory

    [Fact]
    public void Create_ShouldSucceed()
    {
        InMemoryRepository<Person, Guid>.Create().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_NullEvaluator_ShouldReturnError()
    {
        InMemoryRepository<Person, Guid>.Create(null!).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Evaluator.Required.Error);
    }

    #endregion

    #region Lookups

    [Fact]
    public async Task FindAsync_ExistingAggregate_ShouldSucceed()
    {
        var repository = CreateRepository();
        var person = PersonWithId(Guid.NewGuid(), 30);
        (await repository.AddAsync(person)).IsSuccess.Should().BeTrue();

        var result = await repository.FindAsync(person.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(person);
    }

    [Fact]
    public async Task FindAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = CreateRepository();
        var id = Guid.NewGuid();

        var result = await repository.FindAsync(id);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    [Fact]
    public async Task FindAsync_DefaultId_ShouldReturnError()
    {
        var repository = CreateRepository();

        (await repository.FindAsync(Guid.Empty)).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Id.Required.Error);
    }

    #endregion

    #region Writes

    [Fact]
    public async Task AddAsync_ShouldSucceed()
    {
        var repository = CreateRepository();
        var person = PersonWithId(Guid.NewGuid());

        var result = await repository.AddAsync(person);

        result.IsSuccess.Should().BeTrue();
        (await repository.ExistsAsync(person.Id)).Value.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_Duplicate_ShouldReturnConflictError()
    {
        var repository = CreateRepository();
        var person = PersonWithId(Guid.NewGuid());
        await repository.AddAsync(person);

        var result = await repository.AddAsync(person);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.already_exists");
    }

    [Fact]
    public async Task AddAsync_NullAggregate_ShouldReturnError()
    {
        var repository = CreateRepository();

        (await repository.AddAsync(null!)).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Aggregate.Required.Error);
    }

    [Fact]
    public async Task AddAsync_DefaultId_ShouldReturnError()
    {
        var repository = CreateRepository();

        (await repository.AddAsync(PersonWithId(Guid.Empty))).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Id.Required.Error);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAggregate_ShouldReplaceValue()
    {
        var repository = CreateRepository();
        var id = Guid.NewGuid();
        await repository.AddAsync(PersonWithId(id, 10));

        var updated = await repository.UpdateAsync(PersonWithId(id, 40));

        updated.IsSuccess.Should().BeTrue();
        (await repository.FindAsync(id)).Value.Age.Should().Be(40);
    }

    [Fact]
    public async Task UpdateAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = CreateRepository();

        var result = await repository.UpdateAsync(PersonWithId(Guid.NewGuid()));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    [Fact]
    public async Task RemoveAsync_ExistingAggregate_ShouldSucceed()
    {
        var repository = CreateRepository();
        var person = PersonWithId(Guid.NewGuid());
        await repository.AddAsync(person);

        var result = await repository.RemoveAsync(person);

        result.IsSuccess.Should().BeTrue();
        (await repository.ExistsAsync(person.Id)).Value.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = CreateRepository();

        var result = await repository.RemoveAsync(PersonWithId(Guid.NewGuid()));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    [Fact]
    public async Task Seed_ShouldOverwriteAndClear_ShouldEmptyStore()
    {
        var repository = CreateRepository();
        var id = Guid.NewGuid();

        repository.Seed(PersonWithId(id, 10)).IsSuccess.Should().BeTrue();
        repository.Seed(PersonWithId(id, 20)).IsSuccess.Should().BeTrue();
        (await repository.FindAsync(id)).Value.Age.Should().Be(20);

        repository.Clear().IsSuccess.Should().BeTrue();
        (await repository.ExistsAsync(id)).Value.Should().BeFalse();
    }

    #endregion

    #region Queries

    [Fact]
    public async Task ListAndCount_ShouldApplySpecification()
    {
        var repository = CreateRepository();
        repository.Seed(PersonWithId(Guid.NewGuid(), 10));
        repository.Seed(PersonWithId(Guid.NewGuid(), 20));
        repository.Seed(PersonWithId(Guid.NewGuid(), 30));
        var specification = SpecificationExtensions.Create<Person>(person => person.Age >= 20).Value
            .OrderBy(person => person.Age).Value;

        var list = await repository.ListAsync(specification);
        var count = await repository.CountAsync(specification);

        list.IsSuccess.Should().BeTrue();
        list.Value.Select(person => person.Age).Should().Equal(20, 30);
        count.Value.Should().Be(2);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchOrNotFound()
    {
        var repository = CreateRepository();
        repository.Seed(PersonWithId(Guid.NewGuid(), 20));
        repository.Seed(PersonWithId(Guid.NewGuid(), 10));
        var ascending = SpecificationExtensions.Create<Person>(_ => true).Value
            .OrderBy(person => person.Age).Value;
        var empty = SpecificationExtensions.Create<Person>(_ => false).Value;

        (await repository.FirstOrDefaultAsync(ascending)).Value.Age.Should().Be(10);
        (await repository.FirstOrDefaultAsync(empty)).Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.no_match");
    }

    #endregion

    #region Helpers

    private static InMemoryRepository<Person, Guid> CreateRepository()
        => InMemoryRepository<Person, Guid>.Create().Value;

    private static Person PersonWithId(Guid id, int age = 30)
        => new() { Id = id, Name = $"Age {age}", Age = age };

    #endregion
}
