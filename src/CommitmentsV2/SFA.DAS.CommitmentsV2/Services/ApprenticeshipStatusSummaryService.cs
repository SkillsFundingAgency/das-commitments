using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeshipStatusSummaryService : IApprenticeshipStatusSummaryService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<ApprenticeshipStatusSummaryService> _logger;

        public ApprenticeshipStatusSummaryService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<ApprenticeshipStatusSummaryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long employerAccountId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Apprenticeship Status Summary for employer account {employerAccountId}");

            /*var employerAccountIdParam = new SqlParameter("@employerAccountId", employerAccountId);
         
            var results = await _dbContext.Value.ApprenticeshipStatusSummary
                             .FromSql("EXEC GetApprenticeshipStatusSummaries @employerAccountId", employerAccountIdParam)
                             .ToListAsync();*/

            var apprenticeshipSummary = await (from appren in _dbContext.Value.Apprenticeships
                                         join coh in _dbContext.Value.Cohorts on appren.CommitmentId equals coh.Id
                                         join ale in _dbContext.Value.AccountLegalEntities on coh.AccountLegalEntityId equals ale.Id
                                         where coh.EmployerAccountId == employerAccountId
                                         select new ApprenticeshipSummary 
                                         {
                                             AccountLegalEntityId = ale.LegalEntityId,
                                             OrganisationType = ale.OrganisationType,
                                             PaymentStatus = appren.PaymentStatus
                                         
                                         }).ToListAsync();

            var statusCount = apprenticeshipSummary.Count();

            var vasSummary = new GetApprenticeshipStatusSummaryQueryResult
            {
                LegalEntityIdentifier = apprenticeshipSummary.FirstOrDefault().AccountLegalEntityId,
                LegalEntityOrganisationType = apprenticeshipSummary.FirstOrDefault().OrganisationType,
                ActiveCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Active).Count(),
                WithdrawnCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Withdrawn).Count(),
                CompletedCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Completed).Count(),
                PausedCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Paused).Count(),
            };

            var test = vasSummary;


            var esfaSummary = new GetApprenticeshipStatusSummaryQueryResults
            {
               GetApprenticeshipStatusSummaryQueryResult = new List<GetApprenticeshipStatusSummaryQueryResult>
               {
                   new GetApprenticeshipStatusSummaryQueryResult
                   {
                       LegalEntityIdentifier = apprenticeshipSummary.FirstOrDefault().AccountLegalEntityId,
                       LegalEntityOrganisationType = apprenticeshipSummary.FirstOrDefault().OrganisationType,
                       ActiveCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Active).Count(),
                       WithdrawnCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Withdrawn).Count(),
                       CompletedCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Completed).Count(),
                       PausedCount = apprenticeshipSummary.Where(x => x.PaymentStatus == PaymentStatus.Paused).Count()
                   }
               }
              
            };

            return esfaSummary;



            /* var ganga = vasSummary;

             var vasCode = apprenticeshipSummary.GroupBy(a => new { a.AccountLegalEntityId, a.OrganisationType, a.PaymentStatus, a.Count });

             var courseSummary = apprenticeshipSummary.GroupBy(a => new { a.AccountLegalEntityId, a.OrganisationType, a.PaymentStatus, a.Count })
               .OrderBy(course => course.Key.AccountLegalEntityId)
               .Select(course => new GetApprenticeshipStatusSummaryQueryResult
               {
                  LegalEntityIdentifier = course.Key.AccountLegalEntityId,
                  PaymentStatus = course.Key.PaymentStatus,
                  LegalEntityOrganisationType = course.Key.OrganisationType                  
               });

             var test = courseSummary;

             var countSummary = new GetApprenticeshipStatusSummaryQueryResult
             {
                 ActiveCount = courseSummary.Select(x => x.PaymentStatus == PaymentStatus.Active).Count(),
                 WithdrawnCount = courseSummary.Select(x => x.PaymentStatus == PaymentStatus.Withdrawn).Count(),
                 CompletedCount = courseSummary.Select(x => x.PaymentStatus == PaymentStatus.Completed).Count(),
                 PausedCount = courseSummary.Select(x => x.PaymentStatus == PaymentStatus.Paused).Count(),
             };

             var test123 = countSummary.PausedCount;*/





            /*var apprenticeshipsStatusSummaries = new Dictionary<string, GetApprenticeshipStatusSummaryQueryResult>();

            foreach (var result in results)
            {
                var legalEntityId = result.LegalEntityId;
                var organisationType = result.LegalEntityOrganisationType;
                var paymentStatus = result.PaymentStatus;
                var count = result.Count;

                if (!apprenticeshipsStatusSummaries.ContainsKey(legalEntityId))
                {
                    apprenticeshipsStatusSummaries.Add(legalEntityId, new GetApprenticeshipStatusSummaryQueryResult
                    {
                        LegalEntityIdentifier = legalEntityId,
                        LegalEntityOrganisationType = organisationType
                    });
                }

                var apprenticeshipStatusSummary = apprenticeshipsStatusSummaries[legalEntityId];

                switch (result.PaymentStatus)
                {
                    //TODO : Remove later
                    case PaymentStatus.PendingApproval:
                        apprenticeshipStatusSummary.PendingApprovalCount = count;
                        break;
                    case PaymentStatus.Active:
                        apprenticeshipStatusSummary.ActiveCount = count;
                        break;
                    case PaymentStatus.Paused:
                        apprenticeshipStatusSummary.PausedCount = count;
                        break;
                    case PaymentStatus.Withdrawn:
                        apprenticeshipStatusSummary.WithdrawnCount = count;
                        break;
                    case PaymentStatus.Completed:
                        apprenticeshipStatusSummary.CompletedCount = count;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected payment status '{paymentStatus}' found when creating apprenticeship summary statuses");
                }
            }

            if (apprenticeshipsStatusSummaries != null)
            {
                _logger.LogInformation($"Retrieved Apprenticeship Status Summary for employer account {employerAccountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Apprenticeship Status Summary for employer account {employerAccountId}");
            }

            return new GetApprenticeshipStatusSummaryQueryResults
            {
                GetApprenticeshipStatusSummaryQueryResult = apprenticeshipsStatusSummaries.Values
            };*/
        }
    }

    public class ApprenticeshipSummary
    {
        public string AccountLegalEntityId { get; set; }

        //public SFA.DAS.Common.Domain.Types.OrganisationType OrganisationType { get;  set; }
        public  SFA.DAS.CommitmentsV2.Models.OrganisationType OrganisationType { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public int Count { get; set; }
    }
}
