namespace BuildingBlock.Querying.Unit.Tests;

#region Entities

internal sealed class Address
{
    public string? Country { get; init; }

    public string? City { get; init; }
}

internal sealed class Person
{
    public int Id { get; init; }

    public int Age { get; init; }

    public int? Score { get; init; }

    public string Name { get; init; } = string.Empty;

    public Address? Address { get; init; }
}

#endregion

#region Attributed entities

/// <summary>An entity whose query exposure is declared with attributes.</summary>
internal sealed class Product
{
    [Queryable]
    public int Id { get; init; }

    [Queryable]
    [Filterable(Operators = FilterOperators.Eq | FilterOperators.Contains | FilterOperators.StartsWith)]
    [Sortable]
    [Searchable]
    public string Name { get; init; } = string.Empty;

    [Queryable]
    [Filterable(Operators = FilterOperators.Gt | FilterOperators.Lte)]
    [Sortable]
    public decimal Price { get; init; }

    [Hidden]
    public string CostBasis { get; init; } = string.Empty;

    public string InternalNote { get; init; } = string.Empty;
}

#endregion

#region Data

internal static class People
{
    public static readonly Person Ada = new()
    {
        Id = 1,
        Age = 30,
        Name = "Ada",
        Address = new Address { City = "London", Country = "UK" }
    };

    public static readonly Person Grace = new()
    {
        Id = 2,
        Age = 45,
        Name = "Grace",
        Address = new Address { City = "New York", Country = "US" }
    };

    public static readonly Person Alan = new()
    {
        Id = 3,
        Age = 17,
        Name = "Alan",
        Address = new Address { City = "London", Country = null }
    };

    public static IReadOnlyList<Person> All { get; } = [Ada, Grace, Alan];
}

#endregion

#region Registry

/// <summary>Builds a <see cref="QueryFieldRegistry"/> from a single accessor.</summary>
internal static class Registry
{
    /// <summary>Creates a registry holding one field mapped to an accessor.</summary>
    /// <typeparam name="T">The entity type the field belongs to.</typeparam>
    /// <param name="name">The external field name.</param>
    /// <param name="accessor">The member accessor that backs the field.</param>
    /// <returns>The registry containing the single field.</returns>
    public static QueryFieldRegistry With<T>(string name, Expression<Func<T, object?>> accessor)
    {
        var registry = new QueryFieldRegistry();
        registry.Add(typeof(T), name, accessor);
        return registry;
    }
}

#endregion

#region Policy

/// <summary>Builds a <see cref="QueryPolicy"/> whose registry contains a single field.</summary>
internal static class Policy
{
    /// <summary>Builds a permissive policy exposing every simple public property of an entity.</summary>
    /// <typeparam name="T">The entity type the policy targets.</typeparam>
    /// <returns>A policy whose registry exposes the entity's scalar properties.</returns>
    public static QueryPolicy For<T>()
    {
        var registry = new QueryFieldRegistry();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is null)
            {
                continue;
            }

            if (property.PropertyType != typeof(string) && property.PropertyType.IsClass)
            {
                continue;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var accessor = Expression.Lambda(Expression.Property(parameter, property), parameter);
            registry.Add(typeof(T), property.Name, accessor, FilterOperators.All, sortable: true);
        }

        return new QueryPolicy { Registry = registry };
    }

    public static QueryPolicy WithField<T>(
        string name,
        FilterOperators operators = FilterOperators.None,
        bool sortable = false,
        int maxFilterDepth = QueryPolicyConstant.Defaults.MaxFilterDepth,
        int maxFilterConditions = QueryPolicyConstant.Defaults.MaxFilterConditions)
    {
        var registry = new QueryFieldRegistry();
        registry.Add(typeof(T), name, Accessor(typeof(T), name), operators, sortable);

        return new QueryPolicy
        {
            Registry = registry,
            MaxFilterDepth = maxFilterDepth,
            MaxFilterConditions = maxFilterConditions
        };
    }

    private static LambdaExpression Accessor(Type entityType, string path)
    {
        var parameter = Expression.Parameter(entityType, "x");
        Expression current = parameter;
        Type currentType = entityType;

        foreach (string segment in path.Split('.'))
        {
            var property = currentType.GetProperty(
                segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw new InvalidOperationException($"No property '{segment}' on '{currentType.Name}'.");

            current = Expression.Property(current, property);
            currentType = property.PropertyType;
        }

        return Expression.Lambda(current, parameter);
    }
}

#endregion
