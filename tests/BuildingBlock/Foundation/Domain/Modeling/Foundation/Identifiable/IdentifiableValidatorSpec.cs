using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Identifiable;

/// <summary>Specifications for <see cref="IdentifiableValidator"/> covering validation rules.</summary>
public sealed class IdentifiableValidatorSpec
{
    private const string IdRequired = "identifiable.id.required";
    private const string EntityRequired = "identifiable.entity.required";

    private sealed class GuidEntity : IIdentifiable<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class StringEntity : IIdentifiable<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class IntEntity : IIdentifiable<int>
    {
        public int Id { get; set; }
    }

    private static Result<T> Success<T>(T entity) =>
        Result<T>.Success(entity);

    private static void ShouldFailWith<T>(
        Result<T> result,
        string expectedCode)
    {
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().NotBeNull();
        result.Errors![0].Code.Should().Be(expectedCode);
    }

    // EnsureIdentified

    [Fact]
    public void ValidateIdentification_NullEntity_ShouldFail()
    {
        var result =
            IdentifiableValidator.ValidateIdentification<GuidEntity, Guid>(
                null!);

        ShouldFailWith(result, EntityRequired);
    }

    [Fact]
    public void ValidateIdentification_ValidGuid_ShouldPreserveEntity()
    {
        var entity = new GuidEntity { Id = Guid.NewGuid() };

        var result =
            IdentifiableValidator.ValidateIdentification<GuidEntity, Guid>(
                entity);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateIdentification_DefaultGuid_ShouldFail()
    {
        var result =
            IdentifiableValidator.ValidateIdentification<GuidEntity, Guid>(
                new GuidEntity());

        ShouldFailWith(result, IdRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateIdentification_InvalidStringId_ShouldFail(
        string? id)
    {
        var entity = new StringEntity { Id = id! };

        var result =
            IdentifiableValidator.ValidateIdentification<StringEntity, string>(
                entity);

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void ValidateIdentification_ValidStringId_ShouldPreserveEntity()
    {
        var entity = new StringEntity { Id = "entity-1" };

        var result =
            IdentifiableValidator.ValidateIdentification<StringEntity, string>(
                entity);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateIdentification_DefaultInt_ShouldFail()
    {
        var result =
            IdentifiableValidator.ValidateIdentification<IntEntity, int>(
                new IntEntity());

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void ValidateIdentification_ValidInt_ShouldPreserveEntity()
    {
        var entity = new IntEntity { Id = 42 };

        var result =
            IdentifiableValidator.ValidateIdentification<IntEntity, int>(
                entity);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateId_InvalidString_ShouldFail(string? id)
    {
        var result = IdentifiableValidator.ValidateId<string>(id!);

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void ValidateId_ValidString_ShouldReturnValue()
    {
        const string id = "entity-1";

        var result = IdentifiableValidator.ValidateId(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
    }

    [Fact]
    public void ValidateId_EmptyGuid_ShouldFail()
    {
        var result = IdentifiableValidator.ValidateId(Guid.Empty);

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void ValidateId_ValidGuid_ShouldReturnValue()
    {
        var id = Guid.NewGuid();

        var result = IdentifiableValidator.ValidateId(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
    }

    [Fact]
    public void ValidateId_DefaultInt_ShouldFail()
    {
        var result = IdentifiableValidator.ValidateId(0);

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void ValidateId_ValidInt_ShouldReturnValue()
    {
        const int id = 42;

        var result = IdentifiableValidator.ValidateId(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
    }

}
