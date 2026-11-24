using System.Linq.Expressions;

namespace BuildingBlock.Specification.Unit.Tests;

#region Entities

internal sealed class Person : IIdentifiable<Guid>
{
    public Guid Id { get; init; }

    public int Age { get; init; }

    public string Name { get; init; } = string.Empty;
}

#endregion
#region Specifications

internal sealed class AdultSpecification : Specification<Person>
{
    public override Expression<Func<Person, bool>> Criteria => person => person.Age >= 18;
}

internal sealed class NamedAdaSpecification : Specification<Person>
{
    public override Expression<Func<Person, bool>> Criteria => person => person.Name == "Ada";
}

internal sealed class BareSpecification : ISpecification<Person>
{
    public Expression<Func<Person, bool>> Criteria => _ => true;

    public List<Expression<Func<Person, object>>> Includes { get; } = new();

    public List<string> IncludeStrings { get; } = new();

    public List<(Expression<Func<Person, object>> KeySelector, bool Descending)> OrderBy { get; } = new();

    public int? Skip => null;

    public int? Take => null;

    public bool IsPagingEnabled => false;

    public bool IsTrackingEnabled => true;

    public bool IsSatisfiedBy(Person entity) => true;
}

internal sealed class ShapedSpecification : Specification<Person>
{
    public override Expression<Func<Person, bool>> Criteria => _ => true;

    public void AddIncludePath(Expression<Func<Person, object>> include) => AddInclude(include);

    public void AddIncludePath(string include) => AddInclude(include);

    public void AddAscending(Expression<Func<Person, object>> keySelector) => AddOrderBy(keySelector);

    public void AddDescending(Expression<Func<Person, object>> keySelector) => AddOrderByDescending(keySelector);

    public void Page(int skip, int take) => ApplyPaging(skip, take);

    public void NoTracking() => DisableTracking();
}

#endregion
#region Factories

internal static class People
{
    public static Person Adult => new() { Id = Guid.NewGuid(), Age = 30, Name = "Ada" };

    public static Person Child => new() { Id = Guid.NewGuid(), Age = 10, Name = "Ada" };

    public static Person Other => new() { Id = Guid.NewGuid(), Age = 30, Name = "Grace" };
#endregion
}
