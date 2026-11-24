using System.Diagnostics.CodeAnalysis;

namespace BuildingBlock.Functional.Results;

public static class ResultGuard
{
    #region Parameters

    public static void ParameterRequired(
        [NotNull] Delegate? parameter,
        string parameterName)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(
                parameterName,
                ResultConstant.Constraints.Parameter.Required.ToString());
        }
    }

    public static void ParameterRequired<T>(
        [NotNull] T? value,
        string parameterName)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                parameterName,
                ResultConstant.Constraints.Parameter.Required.ToString());
        }
    }

    #endregion

    #region Errors

    public static void ErrorsRequired(IEnumerable<Error>? errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(
                nameof(errors),
                ResultConstant.Constraints.Errors.Required.ToString());
        }
    }

    public static void ErrorsNotEmpty(IEnumerable<Error>? errors)
    {
        ErrorsRequired(errors);

        if (!errors!.Any())
        {
            throw new ArgumentException(
                ResultConstant.Constraints.Errors.Empty.ToString(),
                nameof(errors));
        }
    }

    public static void ErrorsValid(IEnumerable<Error>? errors)
    {
        ErrorsNotEmpty(errors);

        if (errors!.Any(error => error.Equals(default(Error))))
        {
            throw new ArgumentException(
                ResultConstant.Constraints.Errors.ContainsNull.ToString(),
                nameof(errors));
        }
    }

    #endregion

    #region Value

    public static void ValueAccessible(bool isSuccess)
    {
        if (!isSuccess)
        {
            throw new InvalidOperationException(
                ResultConstant.Constraints.Value.Invalid.ToString());
        }
    }

    public static T EnsureSuccess<T>(bool isSuccess, T value)
    {
        ValueAccessible(isSuccess);

        return value;
    }

    #endregion
}
