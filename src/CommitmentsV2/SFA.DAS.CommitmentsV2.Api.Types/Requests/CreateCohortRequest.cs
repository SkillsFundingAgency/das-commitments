using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateCohortRequest : IName, IUln
    {
        public string UserId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? BirthDay { get; set; }
        public int? BirthMonth { get; set; }
        public int? BirthYear { get; set; }
        public string Uln { get; set; }
        public string CourseCode { get; set; }
        public int? Cost { get; set; }
        public int? CourseStartMonth { get; set; }
        public int? CourseStartYear { get; set; }
        public int? CourseEndMonth { get; set; }
        public int? CourseEndYear { get; set; }
        public string OriginatorReference { get; set; }
        public Guid ReservationId { get; set; }
    }
}
