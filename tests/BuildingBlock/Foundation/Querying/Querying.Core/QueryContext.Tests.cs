namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryContext"/>.</summary>
public sealed class QueryContextTests
{
    #region Anonymous

    [Fact]
    public void Anonymous_ShouldHaveNoRoles() =>
        QueryContext.Anonymous.Roles.Should().BeEmpty();

    #endregion

    #region Create

    [Fact]
    public void Create_WithNullRoles_ShouldDefaultToEmpty()
    {
        var context = QueryContext.Create("user-1", null).Value;

        context.UserId.Should().Be("user-1");
        context.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithRoles_ShouldPreserveThem()
    {
        var context = QueryContext.Create(null, ["admin", "editor"]).Value;

        context.Roles.Should().BeEquivalentTo("admin", "editor");
    }

    [Fact]
    public void Create_WithBlankRole_ShouldFail()
    {
        var result = QueryContext.Create("user-1", ["admin", " "]);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.context.role.required");
    }

    #endregion
}
