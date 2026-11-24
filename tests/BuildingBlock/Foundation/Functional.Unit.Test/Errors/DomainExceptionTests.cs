namespace BuildingBlock.Domain.Unit.Test.Functional.Errors;

using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Constructor_SetsErrorAndMessage()
    {
        var error = Error.Validation("validation.failed", "Invalid input");

        var exception = new DomainException(error);

        exception.Error.Should().Be(error);
        exception.Message.Should().Be(error.ToString());
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithInnerException_SetsInnerExceptionAndMessage()
    {
        var error = Error.NotFound("resource.not_found", "Missing");
        var inner = new InvalidOperationException("root cause");

        var exception = new DomainException(error, inner);

        exception.Error.Should().Be(error);
        exception.Message.Should().Be(error.ToString());
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void DomainException_IsException()
    {
        var exception = new DomainException(Error.Create());

        exception.Should().BeAssignableTo<Exception>();
    }
}
