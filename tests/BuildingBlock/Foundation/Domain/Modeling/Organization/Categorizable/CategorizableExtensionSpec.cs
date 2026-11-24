using BuildingBlock.Domain.Modeling.Organization.Categorizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Categorizable;

/// <summary>Specifications for <see cref="CategorizableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class CategorizableExtensionSpec
{
    #region Helper Methods

    private sealed class TestCategorizable : ICategorizable<string>
    {
        public ICollection<string> Categories { get; } = new List<string>();
    }

    private sealed class TestCategorizableGuid : ICategorizable<Guid>
    {
        public ICollection<Guid> Categories { get; } = new List<Guid>();
    }

    /// <summary>Creates a successful result for the test entity.</summary>
    private static Result<TestCategorizable> Success(
        TestCategorizable entity)
    {
        return Result<TestCategorizable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void AddCategory_NewCategory_ShouldAddCategory()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).AddCategory("shoes");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().ContainSingle().Which.Should().Be("shoes");
    }

    [Fact]
    public void AddCategory_DuplicateCategory_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).AddCategory("shoes");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "categorizable.category.duplicate");
        entity.Categories.Should().ContainSingle();
    }

    [Fact]
    public void RemoveCategory_AssignedCategory_ShouldRemoveCategory()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).RemoveCategory("shoes");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCategory_MissingCategory_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).RemoveCategory("boots");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "categorizable.category.not_found");
        entity.Categories.Should().ContainSingle();
    }

    [Fact]
    public void ClearCategories_AssignedCategories_ShouldRemoveAll()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).ClearCategories<TestCategorizable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void AddCategory_TrimmedInput_ShouldStoreTrimmed()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).AddCategory("  shoes  ");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().ContainSingle().Which.Should().Be("shoes");
    }

    [Fact]
    public void AddCategory_NullCategory_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).AddCategory((string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.required");
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCategory_TrimmedInput_ShouldRemove()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).RemoveCategory("  shoes  ");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void AddCategories_AllValid_ShouldAddAll()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).AddCategories<TestCategorizable, string>(new[] { "shoes", "boots" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Count.Should().Be(2);
        entity.Categories.Should().Contain("shoes");
        entity.Categories.Should().Contain("boots");
    }

    [Fact]
    public void AddCategories_DuplicateOfExisting_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("shoes");

        // Act:
        var result = Success(entity).AddCategories<TestCategorizable, string>(new[] { "boots", "shoes" });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.duplicate");
        entity.Categories.Count.Should().Be(1);
        entity.Categories.Should().Contain("shoes");
        entity.Categories.Should().NotContain("boots");
    }

    [Fact]
    public void AddCategories_NullItem_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).AddCategories<TestCategorizable, string>(new[] { "boots", null! });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.required");
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void AddCategories_SecondItemTooLong_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        var longName = new string('x', CategorizableConstant.Category.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).AddCategories<TestCategorizable, string>(new[] { "ok", longName });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.too_long");
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void AddCategories_ExceedingLimit_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        for (var i = 0; i < CategorizableConstant.Category.MaxCount.Value - 1; i++)
        {
            entity.Categories.Add($"cat-{i}");
        }

        // Act:
        var result = Success(entity).AddCategories<TestCategorizable, string>(new[] { "extra-a", "extra-b" });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.limit_exceeded");
        entity.Categories.Count.Should().Be(CategorizableConstant.Category.MaxCount.Value - 1);
        entity.Categories.Should().NotContain("extra-a");
        entity.Categories.Should().NotContain("extra-b");
    }

    [Fact]
    public void SetCategories_Replace_ShouldReplaceAll()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("old");

        // Act:
        var result = Success(entity).SetCategories<TestCategorizable, string>(new[] { "a", "b" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Count.Should().Be(2);
        entity.Categories.Should().Contain("a");
        entity.Categories.Should().Contain("b");
        entity.Categories.Should().NotContain("old");
    }

    [Fact]
    public void SetCategories_Duplicates_ShouldDedup()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).SetCategories<TestCategorizable, string>(new[] { "  a  ", "a", "b" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Count.Should().Be(2);
        entity.Categories.Should().Contain("a");
        entity.Categories.Should().Contain("b");
    }

    [Fact]
    public void SetCategories_ExceedingLimit_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCategorizable();
        Success(entity).AddCategory("keep");
        var oversized = Enumerable.Range(0, CategorizableConstant.Category.MaxCount.Value + 1)
            .Select(i => $"cat-{i}")
            .ToList();

        // Act:
        var result = Success(entity).SetCategories<TestCategorizable, string>(oversized);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.limit_exceeded");
        entity.Categories.Should().ContainSingle().Which.Should().Be("keep");
    }

    [Fact]
    public void ClearCategories_Empty_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = Success(entity).ClearCategories<TestCategorizable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void AddCategory_GuidValid_ShouldAdd()
    {
        // Arrange:
        var entity = new TestCategorizableGuid();
        var id = Guid.NewGuid();

        // Act:
        var result = Result<TestCategorizableGuid>.Success(entity).AddCategory(id);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Categories.Should().ContainSingle().Which.Should().Be(id);
    }

    #endregion
}
