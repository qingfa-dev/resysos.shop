using BuildingBlock.Domain.Modeling.Organization.Categorizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Categorizable;

/// <summary>Specifications for <see cref="CategorizableValidator"/> covering validation rules.</summary>
public class CategorizableValidatorSpec
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
    public void ValidateAddCategory_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            (TestCategorizable)null!,
            "shoes");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.entity.required");
    }

    [Fact]
    public void ValidateAddCategory_NullCategory_ShouldFailCategoryRequired()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            (string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.required");
    }

    [Fact]
    public void ValidateAddCategory_EmptyGuid_ShouldFailCategoryEmpty()
    {
        // Arrange:
        var entity = new TestCategorizableGuid();

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.empty");
    }

    [Fact]
    public void ValidateAddCategory_BlankName_ShouldFailCategoryRequired()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.required");
    }

    [Fact]
    public void ValidateAddCategory_TooLongName_ShouldFailCategoryTooLong()
    {
        // Arrange:
        var entity = new TestCategorizable();
        var longName = new string(
            'x',
            CategorizableConstant.Category.MaxLength.Value + 1);

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            longName);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.too_long");
    }

    [Fact]
    public void ValidateAddCategory_Duplicate_ShouldFailCategoryDuplicate()
    {
        // Arrange:
        var entity = new TestCategorizable();
        entity.Categories.Add("shoes");

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            "shoes");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.duplicate");
    }

    [Fact]
    public void ValidateAddCategory_LimitExceeded_ShouldFailCategoryLimitExceeded()
    {
        // Arrange:
        var entity = new TestCategorizable();
        for (var i = 0; i < CategorizableConstant.Category.MaxCount.Value; i++)
        {
            entity.Categories.Add($"cat-{i}");
        }

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            "one-more");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.limit_exceeded");
    }

    [Fact]
    public void ValidateAddCategory_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(
            entity,
            "shoes");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRemoveCategory_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = CategorizableValidator.ValidateRemoveCategory(
            (TestCategorizable)null!,
            "shoes");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.entity.required");
    }

    [Fact]
    public void ValidateRemoveCategory_NullCategory_ShouldFailCategoryRequired()
    {
        // Arrange:
        var entity = new TestCategorizable();

        // Act:
        var result = CategorizableValidator.ValidateRemoveCategory(
            entity,
            (string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.required");
    }

    [Fact]
    public void ValidateRemoveCategory_Missing_ShouldFailCategoryNotFound()
    {
        // Arrange:
        var entity = new TestCategorizable();
        entity.Categories.Add("shoes");

        // Act:
        var result = CategorizableValidator.ValidateRemoveCategory(
            entity,
            "boots");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("categorizable.category.not_found");
    }

    [Fact]
    public void ValidateRemoveCategory_Assigned_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCategorizable();
        entity.Categories.Add("shoes");

        // Act:
        var result = CategorizableValidator.ValidateRemoveCategory(
            entity,
            "shoes");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateAddCategory_GuidValid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCategorizableGuid();

        // Act:
        var result = CategorizableValidator.ValidateAddCategory(entity, Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
