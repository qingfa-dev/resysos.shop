namespace BuildingBlock.Functional.Results;

/// <summary>
/// Immutable Result implementation. The class is partial so each functional
/// concern can be implemented in its own source file.
/// </summary>
public partial class Result<T> :
    IResultMonad<T>,
    IResultFactory<Result<T>, T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
        Errors = Array.Empty<Error>();
    }

    private Result(IEnumerable<Error> errors)
    {
        ResultGuard.ErrorsValid(errors);

        _value = default;
        IsSuccess = false;
        Errors = Array.AsReadOnly(errors.ToArray());
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<Error> Errors { get; }

    public T Value
    {
        get
        {
            ResultGuard.ValueAccessible(IsSuccess);

            return _value!;
        }
    }

  
}
