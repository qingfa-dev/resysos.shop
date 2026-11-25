namespace BuildingBlock.Contracts.Querying;

/// <summary>Constants and error definitions for <see cref="FilterLexer"/>.</summary>
public static class FilterLexerConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for filter lexing.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for the filter lexer.</summary>
        public static class Lexer
        {
            /// <summary>Error code for an unterminated quoted value.</summary>
            public const string UnterminatedQuoteCode = "filter.lex.unterminated_quote";

            /// <summary>Error code for a character the lexer does not recognise.</summary>
            public const string UnexpectedCharacterCode = "filter.lex.unexpected_character";

            /// <summary>Creates a validation error for an unterminated quoted value.</summary>
            /// <param name="position">The zero-based index of the opening quote.</param>
            /// <returns>The validation error.</returns>
            public static Error UnterminatedQuote(int position) => Error.Validation(
                UnterminatedQuoteCode,
                $"The filter expression has an unterminated quoted value at position {position}.");

            /// <summary>Creates a validation error for an unexpected character.</summary>
            /// <param name="character">The character that was not recognised.</param>
            /// <param name="position">The zero-based index of the character.</param>
            /// <returns>The validation error.</returns>
            public static Error UnexpectedCharacter(char character, int position) => Error.Validation(
                UnexpectedCharacterCode,
                $"The filter expression contains an unexpected character '{character}' at position {position}.");
        }
    }

    #endregion
}
