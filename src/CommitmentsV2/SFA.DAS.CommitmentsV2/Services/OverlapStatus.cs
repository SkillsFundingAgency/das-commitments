using System;

namespace SFA.DAS.CommitmentsV2.Services
{
    /// <summary>
    ///     A flag enum that describes how the proposed start and end dates overlap another
    ///     apprenticeship.
    /// </summary>
    /// <remarks>
    ///     Aggregating this result to a set:
    ///     The <see cref="DateEmbrace"/> and <see cref="DateWithin"/> values describes the 
    ///     proposed apprenticeship with regards to a single apprenticeship, not a set.
    ///     So for example, if an apprentice had two existing apprenticeships that ran from
    ///     Jan-19 to Mar-19 and Jun-19 to Aug-19 then proposing an apprenticeship that
    ///     ran from Mar-19 Jul-19 would *not* set the DateWithin. This is because even though
    ///     the existing apprenticeships run from Jan-Aug and the proposed apprenticeship runs
    ///     from Mar-Jul the flag only indicates the situation with regards to a *single*
    ///     apprenticeship, not the set.
    /// </remarks>
    [Flags]
    public enum OverlapStatus
    {
        /// <summary>
        ///     No overlaps exist
        /// </summary>
        None = 0,

        /// <summary>
        ///     The proposed start date overlaps with an existing apprenticeship
        ///     For example:
        ///         Existing apprenticeship runs Mar-19 to Oct-19
        ///         Proposed apprenticeship runs Sep-19 to Dec-19
        ///     The start of the proposed apprenticeship overlaps the existing apprenticeship.
        /// </summary>
        OverlappingStartDate = 1,

        /// <summary>
        ///     The proposed end date overlaps with an existing apprenticeship
        ///     For example:
        ///         Existing apprenticeship runs Mar-19 to Oct-19
        ///         Proposed apprenticeship runs Jan-19 to Apr-19
        ///     The end of the proposed apprenticeship overlaps the existing apprenticeship.
        /// </summary>
        OverlappingEndDate = 2,

        /// <summary>
        ///     The proposed start date and end date completely embrace (contain) an existing apprenticeship
        ///     For example:
        ///         Existing apprenticeship runs Mar-19 to Oct-19
        ///         Proposed apprenticeship runs Jan-19 to Nov-19
        ///     The existing apprenticeship is completely contained within the proposed apprenticeship.
        /// </summary>
        DateEmbrace = 4,

        /// <summary>
        ///     The proposed start date and end date are completely within an existing apprenticeship
        ///     For example:
        ///         Existing apprenticeship runs Mar-19 to Oct-19
        ///         Proposed apprenticeship runs Apr-19 to Jun-19
        ///     The proposed apprenticeship is completely contained within the existing apprenticeship.
        /// </summary>
        DateWithin = 8,

        /// <summary>
        ///     Short cut to <see cref="OverlappingStartDate"/> and <see cref="OverlappingEndDate"/>.
        /// </summary>
        OverlappingDates = OverlappingStartDate | OverlappingEndDate,

        /// <summary>
        ///     Indicates that the problem could be fixed by adjusting the proposed start date.
        /// </summary>
        ProblemWithStartDate = OverlappingStartDate | DateEmbrace | DateWithin,

        /// <summary>
        ///     Indicates that the problem could be fixed by adjusting the proposed end date.
        /// </summary>
        ProblemWithEndDate = OverlappingEndDate | DateEmbrace | DateWithin
    }
}