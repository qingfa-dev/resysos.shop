namespace BuildingBlock.Specifications.Query;

/// <summary>Guards for query-to-specification mapping. Failures stay on the railway.</summary>
public static class QuerySpecificationGuard
{
    #region Request

    /// <summary>Checks that a query request was supplied.</summary>
    /// <param name="request">The candidate request.</param>
    /// <returns>The request, or a validation error.</returns>
    public static Result<QueryRequest> RequestRequired(QueryRequest? request)
        => request is null
            ? Result.Failure<QueryRequest>(
                QuerySpecificationConstant.Constraints.Request.Required.Error)
            : Result.Success(request);

    #endregion

    #region Mapper

    /// <summary>Checks that a mapper was supplied.</summary>
    /// <param name="mapper">The candidate mapper.</param>
    /// <returns>The mapper, or a validation error.</returns>
    public static Result<IQuerySpecificationMapper> MapperRequired(
        IQuerySpecificationMapper? mapper)
        => mapper is null
            ? Result.Failure<IQuerySpecificationMapper>(
                QuerySpecificationConstant.Constraints.Mapper.Required.Error)
            : Result.Success(mapper);

    #endregion

    #region Parameters

    /// <summary>Checks that raw query parameters were supplied.</summary>
    /// <param name="parameters">The candidate query parameters.</param>
    /// <returns>The parameters, or a validation error.</returns>
    public static Result<QueryParameters> ParametersRequired(QueryParameters? parameters)
        => parameters is null
            ? Result.Failure<QueryParameters>(
                QuerySpecificationConstant.Constraints.Parameters.Required.Error)
            : Result.Success(parameters);

    #endregion

    #region Paging

    /// <summary>Checks that a paging request was supplied.</summary>
    /// <param name="paging">The candidate paging request.</param>
    /// <returns>The paging request, or a validation error.</returns>
    public static Result<PagedRequest> PagingRequired(PagedRequest? paging)
        => paging is null
            ? Result.Failure<PagedRequest>(
                QuerySpecificationConstant.Constraints.Paging.Required.Error)
            : Result.Success(paging);

    #endregion

    #region Value

    /// <summary>Converts a raw filter value to the underlying field type.</summary>
    /// <param name="raw">The raw value.</param>
    /// <param name="targetType">The field type, which may be nullable.</param>
    /// <param name="field">The field the value belongs to.</param>
    /// <returns>The converted value, or a validation error.</returns>
    public static Result<object> ConvertValue(string? raw, Type targetType, string field)
    {
        if (raw is null)
        {
            return Result.Failure<object>(
                QuerySpecificationConstant.Constraints.Value.Required.Error);
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
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime dateTime)
                ? Result.Success<object>(dateTime)
                : Invalid(field, raw, type);
        }

        if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTimeOffset offset)
                ? Result.Success<object>(offset)
                : Invalid(field, raw, type);
        }

        if (type == typeof(DateOnly))
        {
            return DateOnly.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly date)
                ? Result.Success<object>(date)
                : Invalid(field, raw, type);
        }

        if (type == typeof(TimeOnly))
        {
            return TimeOnly.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out TimeOnly time)
                ? Result.Success<object>(time)
                : Invalid(field, raw, type);
        }

        if (type == typeof(TimeSpan))
        {
            return TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out TimeSpan span)
                ? Result.Success<object>(span)
                : Invalid(field, raw, type);
        }

        try
        {
            object converted = Convert.ChangeType(raw, type, CultureInfo.InvariantCulture);
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
            QuerySpecificationConstant.Constraints.Value.Invalid.For(field, raw, type));

    #endregion
}
