using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class TransferRequest : Entity
    {
        public static async Task<TransferRequest> CreateForCohort(ProviderCommitmentsDbContext db, long cohortId, ITrainingProgrammeLookup trainingProgrammeLookup, CancellationToken cancellationToken)
        {
            var cohort = await db.Cohorts
                .Include(c => c.Apprenticeships)
                .SingleAsync(c => c.Id == cohortId, cancellationToken: cancellationToken);

            var transferRequest = new TransferRequest(cohort);
            var request = await GetApprenticeshipSummaries(cohort, trainingProgrammeLookup);

            transferRequest.TrainingCourses = request.JsonList;
            transferRequest.Cost = request.Cost;
            transferRequest.FundingCap = request.Cap;
            transferRequest.Status = (byte) TransferApprovalStatus.Pending;

            return transferRequest;
        }

        public static async Task<TransferRequest> GetExistingTransferRequest(ProviderCommitmentsDbContext db, long transferRequestId, CancellationToken cancellationToken)
        {
            try
            {
                return await db.TransferRequests.Include(c=>c.Cohort).SingleAsync(c => c.Id == transferRequestId, cancellationToken);
            }
            catch (Exception e)
            {
                throw e;
            }
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

        private static async Task<(string JsonList, decimal Cost, int Cap)> GetApprenticeshipSummaries(Cohort cohort, ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            //var fundingBandCaps = Task.WhenAll(cohort.Apprenticeships.Select(async x=>new {x.Id, Cap = (await trainingProgrammeLookup.GetTrainingProgramme(x.CourseCode)).GetStatusOn(x.StartDate)  }))

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
