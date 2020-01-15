
using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class GetApprenticeshipRequest
    {
        public long? AccountId { get; set; }
        public long? ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public string SortField { get; set; }
        
        public bool ReverseSort { get; set; }

        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}