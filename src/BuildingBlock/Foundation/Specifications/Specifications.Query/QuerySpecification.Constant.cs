namespace BuildingBlock.Specifications.Query;

/// <summary>Unified constants and error definitions for query-to-specification mapping.</summary>
public static partial class QuerySpecificationConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for query-to-specification mapping.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the query request.</summary>
        public static class Request
        {
            /// <summary>Error when the query request is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.request.required",
                    message: "A query request is required.");
            }
        }

        /// <summary>Constraints and error definitions for raw query parameters.</summary>
        public static class Parameters
        {
            /// <summary>Error when the query parameters are missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.parameters.required",
                    message: "Query parameters are required.");
            }
        }

        /// <summary>Constraints and error definitions for the mapper.</summary>
        public static class Mapper
        {
            /// <summary>Error when the mapper is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.mapper.required",
                    message: "A query specification mapper is required.");
            }
        }

        /// <summary>Constraints and error definitions for filter and sort fields.</summary>
        public static class Field
        {
            /// <summary>Error when a field name is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.field.required",
                    message: "A field name is required.");
            }

            /// <summary>Error when a field name does not resolve to a public property.</summary>
            public static class Unknown
            {
                public const string Code = "query.specification.field.unknown";

                /// <summary>Creates a validation error for an unresolved field.</summary>
                /// <param name="type">The entity type name.</param>
                /// <param name="field">The unresolved field path.</param>
                /// <returns>The validation error.</returns>
                public static Error For(string type, string field) => Error.Validation(
                    Code,
                    $"'{field}' is not a public property of '{type}'.");
            }
        }

        /// <summary>Constraints and error definitions for filter values.</summary>
        public static class Value
        {
            /// <summary>Error when an operator needs a value but none was supplied.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.value.required",
                    message: "A filter value is required for this operator.");
            }

            /// <summary>Error when a raw value cannot be converted to the field type.</summary>
            public static class Invalid
            {
                public const string Code = "query.specification.value.invalid";

                /// <summary>Creates a validation error for an unconvertible value.</summary>
                /// <param name="field">The field the value belongs to.</param>
                /// <param name="value">The raw value.</param>
                /// <param name="type">The field type.</param>
                /// <returns>The validation error.</returns>
                public static Error For(string field, string value, Type type) => Error.Validation(
                    Code,
                    $"'{value}' is not a valid {type.Name} value for '{field}'.");
            }
        }

        /// <summary>Constraints and error definitions for filter operators.</summary>
        public static class Operator
        {
            /// <summary>Error when an operator cannot be applied to a field type.</summary>
            public static class Unsupported
            {
                public const string Code = "query.specification.operator.unsupported";

                /// <summary>Creates a not-supported error for an operator and field type.</summary>
                /// <param name="operator">The unsupported operator.</param>
                /// <param name="field">The field the operator applies to.</param>
                /// <param name="type">The field type.</param>
                /// <returns>The not-supported error.</returns>
                public static Error For(FilterOperator @operator, string field, Type type) => Error.NotSupported(
                    Code,
                    $"Operator '{@operator}' is not supported for '{field}' of type '{type.Name}'.");
            }
        }

        /// <summary>Constraints and error definitions for sorting.</summary>
        public static class Sort
        {
            /// <summary>Constraints and error definitions for the sort direction.</summary>
            public static class Direction
            {
                /// <summary>Error when a sort direction is not a defined value.</summary>
                public static class Invalid
                {
                    public const string Code = "query.specification.sort.direction.invalid";

                    /// <summary>Creates a validation error for an undefined sort direction.</summary>
                    /// <param name="direction">The undefined direction.</param>
                    /// <returns>The validation error.</returns>
                    public static Error For(SortDirection direction) => Error.Validation(
                        Code,
                        $"'{direction}' is not a valid sort direction.");
                }
            }
        }

        /// <summary>Constraints and error definitions for paging.</summary>
        public static class Paging
        {
            /// <summary>Error when the paging request is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.paging.required",
                    message: "A paging request is required.");
            }
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for query-to-specification mapping.</summary>
    public static class Defaults
    {
        /// <summary>Character separating the segments of a nested field path.</summary>
        public const char FieldSeparator = '.';
    }

    #endregion
}
