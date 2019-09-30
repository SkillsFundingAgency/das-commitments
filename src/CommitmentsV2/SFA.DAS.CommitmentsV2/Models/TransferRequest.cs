using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class TransferRequest : Entity
    {
        public TransferRequest()
        {
        }

        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string TrainingCourses { get; set; }
        public decimal Cost { get; set; }
        public byte Status { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? FundingCap { get; set; }
        public virtual Cohort Cohort { get; set; }
    }
}
