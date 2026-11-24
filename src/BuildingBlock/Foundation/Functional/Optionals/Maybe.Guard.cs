namespace BuildingBlock.Functional.Optionals;

public static class MaybeGuard
{
    #region Value

    public static void ValueRequired<T>(T value, string parameterName = "value")
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                parameterName,
                MaybeConstant.Constraints.Value.Required.ToString());
        }
    }

    #endregion

    #region Parameter

    public static void ParameterRequired(Delegate? parameter, string parameterName)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(
                parameterName,
                MaybeConstant.Constraints.Parameter.Required.ToString());
        }
    }

    #endregion
}
