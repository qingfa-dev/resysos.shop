namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryFieldRegistry"/>.</summary>
public sealed class QueryFieldRegistryTests
{
    #region Resolve

    [Fact]
    public void Resolve_RegisteredField_ShouldReturnField()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age", x => x.Age, FilterOperators.Gte | FilterOperators.Lte, sortable: true);

        registry.Resolve(typeof(Person), "age").Value.Name.Should().Be("age");
    }

    [Fact]
    public void Resolve_UnknownField_ShouldReturnSecurityError()
    {
        var registry = new QueryFieldRegistry();

        var result = registry.Resolve(typeof(Person), "passwordHash");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    [Fact]
    public void Resolve_ShouldMatchFieldNameCaseInsensitively()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age", x => x.Age);

        registry.Resolve(typeof(Person), "AGE").Value.Name.Should().Be("age");
    }

    [Fact]
    public void Resolve_UnknownEntityType_ShouldReturnSecurityError()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age", x => x.Age);

        var result = registry.Resolve(typeof(Address), "age");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    [Fact]
    public void Resolve_ShouldNeverFallBackToReflection()
    {
        var registry = new QueryFieldRegistry();

        // "Age" is a real CLR property of Person, but it was never registered.
        registry.Resolve(typeof(Person), "Age").IsFailure.Should().BeTrue();
    }

    #endregion

    #region Add

    [Fact]
    public void Add_ShouldCarryGrantsAndUnboxedAccessor()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age", x => x.Age, FilterOperators.Gte, sortable: true, searchable: true);

        var field = registry.Resolve(typeof(Person), "age").Value;

        field.Name.Should().Be("age");
        field.Filterable.Should().Be(FilterOperators.Gte);
        field.Sortable.Should().BeTrue();
        field.Selectable.Should().BeTrue();
        field.Searchable.Should().BeTrue();
        field.Accessor.Body.Should().BeAssignableTo<MemberExpression>()
            .Which.Member.Name.Should().Be(nameof(Person.Age));
        field.Accessor.ReturnType.Should().Be(typeof(int));
    }

    [Fact]
    public void Add_WithoutSelectable_ShouldDefaultToSelectable()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("name", x => x.Name);

        var field = registry.Resolve(typeof(Person), "name").Value;

        field.Selectable.Should().BeTrue();
        field.Filterable.Should().Be(FilterOperators.None);
        field.Sortable.Should().BeFalse();
        field.Searchable.Should().BeFalse();
    }

    #endregion

    #region Attributes

    [Fact]
    public void AddFromAttributes_DenyByDefault_ShouldExposeOnlyAttributedFields()
    {
        var registry = new QueryFieldRegistry();

        registry.AddFromAttributes(typeof(Product), QueryExposure.DenyByDefault);

        registry.Resolve(typeof(Product), "price").IsSuccess.Should().BeTrue();
        registry.Resolve(typeof(Product), "internalNote").IsFailure.Should().BeTrue();
        registry.Resolve(typeof(Product), "costBasis").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddFromAttributes_AllowByDefault_ShouldExposeUnannotatedButNotHidden()
    {
        var registry = new QueryFieldRegistry();

        registry.AddFromAttributes(typeof(Product), QueryExposure.AllowByDefault);

        registry.Resolve(typeof(Product), "internalNote").IsSuccess.Should().BeTrue();
        registry.Resolve(typeof(Product), "costBasis").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddFromAttributes_ShouldCarryAttributeGrants()
    {
        var registry = new QueryFieldRegistry();

        registry.AddFromAttributes(typeof(Product), QueryExposure.DenyByDefault);

        var price = registry.Resolve(typeof(Product), "price").Value;
        price.Filterable.Should().Be(FilterOperators.Gt | FilterOperators.Lte);
        price.Sortable.Should().BeTrue();
        price.Selectable.Should().BeTrue();

        registry.Resolve(typeof(Product), "name").Value.Searchable.Should().BeTrue();
    }

    [Fact]
    public void AddFromAttributes_Generic_ShouldMirrorTypeOverload()
    {
        var registry = new QueryFieldRegistry();

        registry.AddFromAttributes<Product>(QueryExposure.DenyByDefault);

        registry.Resolve(typeof(Product), "price").IsSuccess.Should().BeTrue();
    }

    #endregion
}
