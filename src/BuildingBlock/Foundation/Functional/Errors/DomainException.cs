using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Errors;

public sealed class DomainException : Exception
{
    public Error Error { get; }

    public DomainException(Error error)
        : base(error.ToString())
    {
        Error = error;
    }

    public DomainException(Error error, Exception innerException)
        : base(error.ToString(), innerException)
    {
        Error = error;
    }
}