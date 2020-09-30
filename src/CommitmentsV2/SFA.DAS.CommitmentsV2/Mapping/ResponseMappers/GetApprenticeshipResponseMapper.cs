using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipResponseMapper : IMapper<GetApprenticeshipQueryResult, GetApprenticeshipResponse>
    {
        public Task<GetApprenticeshipResponse> Map(GetApprenticeshipQueryResult source)
        {
            return Task.FromResult(new GetApprenticeshipResponse
            {
                Id = source.Id,
                CohortId = source.CohortId,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                EmployerAccountId = source.EmployerAccountId,
                AccountLegalEntityId = source.AccountLegalEntityId,
                EmployerName = source.EmployerName,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.Uln,
                CourseCode = source.CourseCode,
                CourseName = source.CourseName,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.Reference,
                Status = source.Status,
                StopDate = source.StopDate,
                PauseDate = source.PauseDate,
                CompletionDate = source.CompletionDate,
                EndpointAssessorName = source.EndpointAssessorName,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                ContinuationOfId = source.ContinuationOfId,
                OriginalStartDate = source.OriginalStartDate,
                PreviousProviderId = source.PreviousProviderId,
                PreviousEmployerAccountId = source.PreviousEmployerAccountId,
                ApprenticeshipEmployerTypeOnApproval = source.ApprenticeshipEmployerTypeOnApproval
            });
        }
    }
}
