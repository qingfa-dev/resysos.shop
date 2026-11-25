namespace BuildingBlock.Querying.Compilation;

/// <summary>
/// Converts a raw filter value to the CLR type of the field it compares against, using the
/// invariant culture. A value that cannot be converted stays on the railway as
/// <c>query.specification.value.invalid</c>; conversion never throws.
/// </summary>
/// <remarks>
/// This is the querying engine's own copy of the conversion proven in the specification provider.
/// The engine cannot reference that project, so the logic is ported rather than shared. It keeps
/// the same error code so behavior and diagnostics match.
/// </remarks>
public static class QueryValueConverter
{
    #region Conversion

    /// <summary>Converts a raw filter value to the underlying field type.</summary>
    /// <param name="raw">The raw value.</param>
    /// <param name="targetType">The field type, which may be nullable.</param>
    /// <param name="field">The field the value belongs to.</param>
    /// <returns>The converted value, or a validation error.</returns>
    public static Result<object> Convert(string? raw, Type targetType, string field)
    {
        if (raw is null)
        {
            return Result.Failure<object>(
                QueryValueConverterConstant.Constraints.Value.Required.Error);
        }

        Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (type == typeof(string))
        {
            return Result.Success<object>(raw);
        }

        if (type.IsEnum)
        {
            return Enum.TryParse(type, raw, ignoreCase: true, out object? parsed) && parsed is not null
                ? Result.Success(parsed)
                : Invalid(field, raw, type);
        }

        if (type == typeof(Guid))
        {
            return Guid.TryParse(raw, out Guid guid)
                ? Result.Success<object>(guid)
                : Invalid(field, raw, type);
        }

        if (type == typeof(bool))
        {
            return bool.TryParse(raw, out bool boolean)
                ? Result.Success<object>(boolean)
                : Invalid(field, raw, type);
        }

        if (type == typeof(char))
        {
            return char.TryParse(raw, out char character)
                ? Result.Success<object>(character)
                : Invalid(field, raw, type);
        }

        if (type == typeof(DateTime))
        {
            return DateTime.TryParse(
                raw,
                QueryValueConverterConstant.Defaults.Culture,
                QueryValueConverterConstant.Defaults.RoundtripStyles,
                out DateTime dateTime)
                ? Result.Success<object>(dateTime)
                : Invalid(field, raw, type);
        }

        if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.TryParse(
                raw,
                QueryValueConverterConstant.Defaults.Culture,
                QueryValueConverterConstant.Defaults.RoundtripStyles,
                out DateTimeOffset offset)
                ? Result.Success<object>(offset)
                : Invalid(field, raw, type);
        }

        if (type == typeof(DateOnly))
        {
            return DateOnly.TryParse(
                raw,
                QueryValueConverterConstant.Defaults.Culture,
                QueryValueConverterConstant.Defaults.DateStyles,
                out DateOnly date)
                ? Result.Success<object>(date)
                : Invalid(field, raw, type);
        }

        if (type == typeof(TimeOnly))
        {
            return TimeOnly.TryParse(
                raw,
                QueryValueConverterConstant.Defaults.Culture,
                QueryValueConverterConstant.Defaults.DateStyles,
                out TimeOnly time)
                ? Result.Success<object>(time)
                : Invalid(field, raw, type);
        }

        if (type == typeof(TimeSpan))
        {
            return TimeSpan.TryParse(raw, QueryValueConverterConstant.Defaults.Culture, out TimeSpan span)
                ? Result.Success<object>(span)
                : Invalid(field, raw, type);
        }

        try
        {
            object converted = System.Convert.ChangeType(
                raw,
                type,
                QueryValueConverterConstant.Defaults.Culture);
            return Result.Success(converted);
        }
        catch (FormatException)
        {
            return Invalid(field, raw, type);
        }
        catch (InvalidCastException)
        {
            return Invalid(field, raw, type);
        }
        catch (OverflowException)
        {
            return Invalid(field, raw, type);
        }
    }

    #endregion

    #region Helpers

    private static Result<object> Invalid(string field, string raw, Type type)
        => Result.Failure<object>(
            QueryValueConverterConstant.Constraints.Value.Invalid.For(field, raw, type));

    #endregion
}
