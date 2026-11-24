using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Identifiable;

/// <summary>Specifications for <see cref="IdentifiableExtensionSpecExtensions"/> covering extension behavior.</summary>
public sealed class IdentifiableExtensionSpec
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
    public void EnsureIdentified_ValidGuid_ShouldPreserveEntity()
    {
        var entity = new GuidEntity { Id = Guid.NewGuid() };

        var result = Success(entity)
            .EnsureIdentified<GuidEntity, Guid>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureIdentified_DefaultGuid_ShouldFail()
    {
        var result = Success(new GuidEntity())
            .EnsureIdentified<GuidEntity, Guid>();

        ShouldFailWith(result, IdRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureIdentified_InvalidStringId_ShouldFail(string? id)
    {
        var entity = new StringEntity { Id = id! };

        var result = Result<StringEntity>.Success(entity)
            .EnsureIdentified<StringEntity, string>();

        ShouldFailWith(result, IdRequired);
    }

    [Fact]
    public void EnsureIdentified_ValidInt_ShouldPreserveEntity()
    {
        var entity = new IntEntity { Id = 7 };

        var result = Result<IntEntity>.Success(entity)
            .EnsureIdentified<IntEntity, int>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(entity);
    }

    [Fact]
    public void EnsureIdentified_InputFailure_ShouldPropagateError()
    {
        var input = Result<GuidEntity>.Failure(
            Error.Validation("test.fail", "boom"));

        var result = input.EnsureIdentified<GuidEntity, Guid>();

        ShouldFailWith(result, "test.fail");
    }

}
