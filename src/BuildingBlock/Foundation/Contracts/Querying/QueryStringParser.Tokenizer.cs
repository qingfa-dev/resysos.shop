namespace BuildingBlock.Contracts.Querying;

/// <summary>Shared text helpers for <see cref="QueryStringParser"/>.</summary>
public static partial class QueryStringParser
{
    #region Clauses

    private static IEnumerable<string> SplitClauses(string text)
    {
        int start = 0;
        bool quoted = false;

        for (int index = 0; index < text.Length; index++)
        {
            char current = text[index];
            if (current == '\'')
            {
                quoted = !quoted;
                continue;
            }

            if (current == ',' && !quoted)
            {
                yield return text[start..index];
                start = index + 1;
            }
        }

        yield return text[start..];
    }

    #endregion
}
