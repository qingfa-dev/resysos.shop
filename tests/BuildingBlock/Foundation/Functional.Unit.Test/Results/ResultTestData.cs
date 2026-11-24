namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;

internal static class ResultTestData
{
    public static readonly Error ErrorA = Error.Validation("test.error_a", "Error A");
    public static readonly Error ErrorB = Error.Validation("test.error_b", "Error B");
    public static readonly Error ErrorC = Error.Validation("test.error_c", "Error C");

    public static CancellationToken AlreadyCanceled() => new(canceled: true);
}
