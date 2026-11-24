namespace BuildingBlock.Functional.Metadata;

public static class HasMetadataExtensions
{
    #region Presence

    public static bool HasMetadata(this IHasMetadata source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.HasMetadata();
    }

    public static bool HasMetadata(
        this IHasMetadata source,
        string key)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.HasMetadata(key);
    }

    public static bool HasMetadata(
        this IHasMetadata source,
        string key,
        out object? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.HasMetadata(key, out value);
    }
    #endregion
    #region Retrieval

    public static IMetadata GetMetadata(this IHasMetadata source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata;
    }

    public static object? GetMetadata(
        this IHasMetadata source,
        string key)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetMetadata(key);
    }

    public static T? GetMetadata<T>(
        this IHasMetadata source,
        string key)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetMetadata<T>(key);
    }

    public static bool TryGetMetadata(
        this IHasMetadata source,
        string key,
        out object? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.TryGetMetadata(key, out value);
    }

    public static bool TryGetMetadata<T>(
        this IHasMetadata source,
        string key,
        out T? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.TryGetMetadata(key, out value);
    }

    #endregion
    #region Composition

    public static T WithMetadata<T>(
        this T source,
        string key,
        object value)
        where T : IHasMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.WithMetadata(key, value);

        return source;
    }

    public static T WithMetadata<T>(
        this T source,
        IMetadata metadata)
        where T : IHasMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.WithMetadata(metadata);

        return source;
    }

    public static T WithMetadata<T>(
        this T source,
        params KeyValuePair<string, object>[] parameters)
        where T : IHasMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.WithMetadata(parameters);

        return source;
    }

    #endregion
    #region Removal

    public static T WithoutMetadata<T>(this T source)
        where T : IHasMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.WithoutMetadata();

        return source;
    }

    public static T WithoutMetadata<T>(
        this T source,
        string key)
        where T : IHasMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.WithoutMetadata(key);

        return source;
    }



    #endregion
    #region Absence

    public static bool WithoutMetadata(this IHasMetadata source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !source.Metadata.HasMetadata();
    }

    public static bool WithoutMetadata(
        this IHasMetadata source,
        string key)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !source.Metadata.HasMetadata(key);
    }
    #endregion
}