namespace BuildingBlock.Domain.SmartEnums;

public static class SmartEnumGuard
{
    #region Properties
    #region IDs
    public static Result<int> ValidateId(int id)
    {
        if (id < 0)
            return Result.Failure<int>(SmartEnumConstant.Id.Min.Error);

        return Result.Success(id);
    }
    #endregion

    #region Names
    public static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<string>(SmartEnumConstant.Name.Required.Error);

        if (name.Length > SmartEnumConstant.Name.MaxLength.Value)
            return Result.Failure<string>(SmartEnumConstant.Name.MaxLength.Error);

        return Result.Success(name);
    }
    #endregion

    #endregion
}