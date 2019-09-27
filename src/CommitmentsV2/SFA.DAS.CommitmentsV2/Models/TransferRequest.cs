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
        public static async Task<TransferRequest> CreateForCohort(ProviderCommitmentsDbContext db, long cohortId, ITrainingProgrammeApiClient trainingProgrammeApiClient, CancellationToken cancellationToken)
        {
            var cohort = await db.Cohorts
                .Include(c => c.Apprenticeships)
                .Include(c => c.TransferRequests)
                .SingleAsync(c => c.Id == cohortId, cancellationToken: cancellationToken);

            if (cohort.TransferRequests.Count(x => x.Status == (byte) TransferApprovalStatus.Pending) > 0)
            {
                throw new DomainException("", $"A Pending Transfer Request already exists for this cohort {cohortId}");
            }

            var transferRequest = new TransferRequest(cohort);
            var request = await GetApprenticeshipSummaries(cohort, trainingProgrammeApiClient);

            transferRequest.TrainingCourses = request.JsonList;
            transferRequest.Cost = request.Cost;
            transferRequest.FundingCap = request.Cap;
            transferRequest.Status = (byte) TransferApprovalStatus.Pending;

            return transferRequest;
        }

        public static async Task<TransferRequest> GetExistingTransferRequest(ProviderCommitmentsDbContext db, long transferRequestId, CancellationToken cancellationToken)
        {
            return await db.TransferRequests.Include(c=>c.Cohort).SingleAsync(c => c.Id == transferRequestId, cancellationToken);
        }

        public TransferRequest()
        {
        }

        public TransferRequest(Cohort cohort) : this()
        {
            Cohort = cohort;
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

        private static async Task<(string JsonList, decimal Cost, int Cap)> GetApprenticeshipSummaries(Cohort cohort, ITrainingProgrammeApiClient trainingProgrammeApiClient)
        {
            var fundingBandCaps = Task.WhenAll(cohort.Apprenticeships.Select(async x => new
            {
                x.Id,
                Cap = (await trainingProgrammeApiClient.GetTrainingProgramme(x.CourseCode)).GetStatusOn(x.StartDate.Value)
            }));

            var summaries = cohort.Apprenticeships.GroupBy(a => new { a.CourseCode, a.CourseName })
                .Select(course => new
                {
                    course.Key.CourseCode,
                    course.Key.CourseName,
                    Apprentices = course.Count(),
                    Cost = course.Sum(a=>a.Cost??0)
                });

            var cost = summaries.Sum(x => x.Cost);
            var jsonList = summaries.Select(x => new {x.CourseName, x.Apprentices}).ToList();

            return await Task.FromResult((JsonConvert.SerializeObject(jsonList), cost ,2));
        }



    }
}
