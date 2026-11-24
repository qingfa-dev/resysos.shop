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

    #region Values

    private static bool TryReadValue(string raw, out string value, out bool quoted)
    {
        value = string.Empty;
        quoted = false;

        if (raw.Length == 0)
        {
            return false;
        }

        if (raw.Length >= 2 && raw[0] == '\'' && raw[^1] == '\'')
        {
            quoted = true;
            value = raw[1..^1];
            return true;
        }

        value = raw;
        return true;
    }

    #endregion

    #region Fields

    private static bool IsFieldCharacter(char current)
        => char.IsLetterOrDigit(current) || current is '_' or '.';

    #endregion
}
