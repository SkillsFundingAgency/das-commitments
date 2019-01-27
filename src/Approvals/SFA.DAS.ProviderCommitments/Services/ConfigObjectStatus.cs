namespace SFA.DAS.ProviderCommitments.Services
{
    public enum ConfigObjectStatus
    {
        /// <summary>
        ///     Initial state - only whilst loading
        /// </summary>
        Undefined,

        /// <summary>
        ///     Single type found and de-serialised  okay
        /// </summary>
        Okay,

        /// <summary>
        ///     A type of the specified name could not be found
        /// </summary>
        TypeNotFound,

        /// <summary>
        ///     Multiple types of that name could be found. If this happens it means that there are
        ///     multiple types of that name declared. In that case specify the full name of the
        ///     config type (including name space).
        /// </summary>
        AmbiguousType,

        /// <summary>
        ///     The type was found but the content could not be deserialised into that type.
        /// </summary>
        CouldNotBeDeserialised
    }
}