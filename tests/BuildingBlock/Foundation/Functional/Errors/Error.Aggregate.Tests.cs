namespace BuildingBlock.Domain.Unit.Test.Functional.Errors;

using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class ErrorAggregateTests
{
    [Fact]
    public void Aggregate_WithEmptyEnumerable_ReturnsEmptyAggregateError()
    {
        var result = Error.Aggregate(Array.Empty<Error>());

        result.Code.Should().Be("error.aggregate.empty");
        result.Message.Should().Be("Aggregate error with no errors");
        result.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Aggregate_WithNoArguments_ReturnsEmptyAggregateError()
    {
        var result = Error.Aggregate();

        result.Code.Should().Be("error.aggregate.empty");
        result.Message.Should().Be("Aggregate error with no errors");
        result.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Aggregate_WithNullEnumerable_ReturnsEmptyAggregateError()
    {
        IEnumerable<Error>? errors = null;

        var result = Error.Aggregate(errors!);

        result.Code.Should().Be("error.aggregate.empty");
        result.Message.Should().Be("Aggregate error with no errors");
        result.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Aggregate_WithSingleError_ReturnsThatErrorUnchanged()
    {
        var error = Error.Validation("validation.email", "Email is invalid").WithField("email");
        IEnumerable<Error> errors = new[] { error };

        var result = Error.Aggregate(errors);

        result.Should().Be(error);
        result.Metadata.Should().BeSameAs(error.Metadata);
    }

    [Fact]
    public void Aggregate_WithMultipleErrors_CombinesCodesAndMessagesInOrder()
    {
        var first = Error.Create("validation.email", "Email is invalid", ErrorType.Validation);
        var second = Error.Create("resource.not_found", "User not found", ErrorType.NotFound);
        var third = Error.Create("validation.password", "Password is required", ErrorType.Validation);

        var result = Error.Aggregate(first, second, third);

        result.Code.Should().Be("error.multiple_violations");
        result.Message.Should().Be(
            "Multiple validation errors: Email is invalid; User not found; Password is required");
        result.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Aggregate_WithMultipleErrors_RecordsSummaryMetadata()
    {
        var first = Error.Create("validation.email", "Email is invalid", ErrorType.Validation);
        var second = Error.Create("resource.not_found", "User not found", ErrorType.NotFound);

        var result = Error.Aggregate(first, second);

        result.Metadata.GetMetadata<int>("error_count").Should().Be(2);
        result.Metadata.GetMetadata("error_codes").Should().Be("validation.email, resource.not_found");
    }

    [Fact]
    public void Aggregate_WithMultipleErrors_RecordsEachErrorAsMetadata()
    {
        var first = Error.Create("validation.email", "Email is invalid", ErrorType.Validation);
        var second = Error.Create("resource.not_found", "User not found", ErrorType.NotFound);

        var result = Error.Aggregate(first, second);

        result.Metadata.GetMetadata("error_0_code").Should().Be(first.Code);
        result.Metadata.GetMetadata("error_0_message").Should().Be(first.Message);
        result.Metadata.GetMetadata("error_0_type").Should().Be("Validation");

        result.Metadata.GetMetadata("error_1_code").Should().Be(second.Code);
        result.Metadata.GetMetadata("error_1_message").Should().Be(second.Message);
        result.Metadata.GetMetadata("error_1_type").Should().Be("NotFound");

        result.Metadata.HasMetadata("error_2_code").Should().BeFalse();
    }

    [Fact]
    public void Aggregate_WithMultipleErrors_DoesNotMutateSourceErrors()
    {
        var first = Error.Create("validation.email", "Email is invalid", ErrorType.Validation);
        var second = Error.Create("resource.not_found", "User not found", ErrorType.NotFound);

        _ = Error.Aggregate(first, second);

        first.Code.Should().Be("validation.email");
        first.Message.Should().Be("Email is invalid");
        first.Type.Should().Be(ErrorType.Validation);
        first.Metadata.HasMetadata("error_0_code").Should().BeFalse();

        second.Code.Should().Be("resource.not_found");
        second.Message.Should().Be("User not found");
        second.Type.Should().Be(ErrorType.NotFound);
        second.Metadata.HasMetadata("error_1_code").Should().BeFalse();
    }

    [Fact]
    public void Aggregate_WithLazyEnumerable_EnumeratesSourceOnce()
    {
        var first = Error.Validation("validation.email", "Email is invalid");
        var second = Error.Validation("validation.password", "Password is required");
        var enumerationCount = 0;

        IEnumerable<Error> Errors()
        {
            enumerationCount++;
            yield return first;
            yield return second;
        }

        var result = Error.Aggregate(Errors());

        enumerationCount.Should().Be(1);
        result.Metadata.GetMetadata<int>("error_count").Should().Be(2);
    }

    [Fact]
    public void Aggregate_ParamsOverload_MatchesEnumerableOverload()
    {
        Error[] errors =
        [
            Error.Validation("validation.email", "Email is invalid"),
            Error.NotFound("resource.not_found", "User not found"),
        ];

        var viaParams = Error.Aggregate(errors);
        var viaEnumerable = Error.Aggregate((IEnumerable<Error>)errors);

        viaParams.Should().Be(viaEnumerable);
        viaParams.Metadata.GetMetadata("error_codes").Should()
            .Be(viaEnumerable.Metadata.GetMetadata("error_codes"));
    }
}
