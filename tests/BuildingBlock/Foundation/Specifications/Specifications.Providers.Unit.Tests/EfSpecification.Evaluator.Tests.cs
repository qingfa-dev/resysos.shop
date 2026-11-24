using Microsoft.Extensions.Options;

namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="EfSpecificationEvaluator"/>.</summary>
public sealed class EfSpecificationEvaluatorTests
{
    #region Factory

    [Fact]
    public void Create_NullOptions_ShouldReturnError()
    {
        EfSpecificationEvaluator.Create(null!).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Options.Required.Error);
    }

    [Fact]
    public void Create_ValidOptions_ShouldSucceed()
    {
        EfSpecificationEvaluator.Create(Options.Create(new EfSpecificationOptions()))
            .IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Validation

    [Fact]
    public void Apply_NullQuery_ShouldReturnError()
    {
        var evaluator = CreateEvaluator().Value;
        var specification = SpecificationExtensions.Create<PersonEntity>(_ => true).Value;

        evaluator.Apply(null!, specification).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Query.Required.Error);
    }

    [Fact]
    public void Apply_NullSpecification_ShouldReturnError()
    {
        var evaluator = CreateEvaluator().Value;

        evaluator.Apply(People.Query().Take(0), null!).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Specification.Required.Error);
    }

    #endregion

    #region Evaluation

    [Fact]
    public void Apply_ShouldFilterByCriteria()
    {
        using var context = CreateContext(
            withFilter: false,
            Entity(10),
            Entity(20),
            Entity(30));
        var evaluator = CreateEvaluator().Value;
        var specification = SpecificationExtensions
            .Create<PersonEntity>(person => person.Age >= 20)
            .Value;

        var result = evaluator.Apply(context.Entities, specification).Value.ToList();

        result.Select(person => person.Age).Should().Equal(20, 30);
    }

    [Fact]
    public void Apply_ShouldOrderAscendingAndDescending()
    {
        using var context = CreateContext(withFilter: false, Entity(30), Entity(10), Entity(20));
        var evaluator = CreateEvaluator().Value;
        var ascending = SpecificationExtensions.Create<PersonEntity>(_ => true).Value
            .OrderBy(person => person.Age).Value;
        var descending = SpecificationExtensions.Create<PersonEntity>(_ => true).Value
            .OrderBy(person => person.Age, descending: true).Value;

        var ascendingAges = evaluator.Apply(context.Entities, ascending).Value
            .Select(person => person.Age).ToList();
        var descendingAges = evaluator.Apply(context.Entities, descending).Value
            .Select(person => person.Age).ToList();

        ascendingAges.Should().Equal(10, 20, 30);
        descendingAges.Should().Equal(30, 20, 10);
    }

    [Fact]
    public void Apply_ShouldPageAfterOrdering()
    {
        using var context = CreateContext(
            withFilter: false,
            Entity(40),
            Entity(10),
            Entity(30),
            Entity(20));
        var evaluator = CreateEvaluator().Value;
        var specification = SpecificationExtensions.Create<PersonEntity>(_ => true).Value
            .OrderBy(person => person.Age).Value
            .WithPaging(page: 2, pageSize: 2).Value;

        var result = evaluator.Apply(context.Entities, specification).Value.ToList();

        result.Select(person => person.Age).Should().Equal(30, 40);
    }

    [Fact]
    public void Apply_AsNoTracking_ShouldNotTrackEntities()
    {
        using var context = CreateContext(withFilter: false, Entity(10), Entity(20));
        var evaluator = CreateEvaluator().Value;
        var specification = SpecificationExtensions.Create<PersonEntity>(_ => true).Value
            .AsNoTracking().Value;

        var result = evaluator.Apply(context.Entities, specification).Value.ToList();

        result.Should().HaveCount(2);
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    #endregion

    #region Options

    [Fact]
    public void Apply_IgnoreQueryFilters_ShouldIncludeFilteredEntities()
    {
        using var context = CreateContext(
            withFilter: true,
            Entity(10),
            Entity(20, deleted: true));
        var specification = SpecificationExtensions.Create<PersonEntity>(_ => true).Value;

        var filtered = CreateEvaluator().Value
            .Apply(context.Entities, specification).Value.ToList();
        var unfiltered = CreateEvaluator(ignoreQueryFilters: true).Value
            .Apply(context.Entities, specification).Value.ToList();

        filtered.Should().ContainSingle();
        unfiltered.Should().HaveCount(2);
    }

    #endregion

    #region Helpers

    private static Result<EfSpecificationEvaluator> CreateEvaluator(bool ignoreQueryFilters = false)
        => EfSpecificationEvaluator.Create(Options.Create(
            new EfSpecificationOptions { IgnoreQueryFilters = ignoreQueryFilters }));

    private static PersonEntity Entity(int age, bool deleted = false)
        => new() { Id = Guid.NewGuid(), Name = $"Age {age}", Age = age, IsDeleted = deleted };

    private static TestDbContextBase CreateContext(bool withFilter, params PersonEntity[] entities)
    {
        TestDbContextBase context = withFilter ? new FilteredDbContext() : new PlainDbContext();
        context.Entities.AddRange(entities);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return context;
    }

    private static DbContextOptions BuildOptions()
        => new DbContextOptionsBuilder()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class PersonEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public bool IsDeleted { get; set; }
    }

    private abstract class TestDbContextBase : DbContext
    {
        protected TestDbContextBase(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PersonEntity> Entities => Set<PersonEntity>();
    }

    private sealed class PlainDbContext : TestDbContextBase
    {
        public PlainDbContext() : base(BuildOptions())
        {
        }
    }

    private sealed class FilteredDbContext : TestDbContextBase
    {
        public FilteredDbContext() : base(BuildOptions())
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonEntity>().HasQueryFilter(entity => !entity.IsDeleted);
        }
    }

    #endregion
}
