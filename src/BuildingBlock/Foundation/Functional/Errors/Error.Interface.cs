using BuildingBlock.Functional.Metadata;

namespace BuildingBlock.Functional.Errors;

public interface IError : IHasMetadata
{
    ErrorType Type { get; }
    string Code { get; }
    string Message { get; }
}