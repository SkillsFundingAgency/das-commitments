using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipDetailsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse.ApprenticeshipDetailsResponse;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipsResponseMapper : IMapper<GetApprenticeshipsQueryResult, GetApprenticeshipsResponse>
    {
        public Task<GetApprenticeshipsResponse> Map(GetApprenticeshipsQueryResult source)
        {
            return Task.FromResult(new GetApprenticeshipsResponse
            {
                TotalApprenticeshipsFound = source.TotalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = source.TotalApprenticeshipsWithAlertsFound,
                Apprenticeships = source.Apprenticeships.Select(MapApprenticeship),
                TotalApprenticeships = source.TotalApprenticeships,
                PageNumber = source.PageNumber
            });
        }

        private ApprenticeshipDetailsResponse MapApprenticeship(GetApprenticeshipsQueryResult.ApprenticeshipDetails source)
        {
            return new ApprenticeshipDetailsResponse
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                EmployerName = source.EmployerName,
                ProviderName = source.ProviderName,
                ProviderId = source.ProviderId,
                CourseName = source.CourseName,
                DeliveryModel = source.DeliveryModel ?? DeliveryModel.Normal,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                PauseDate = source.PauseDate,
                DateOfBirth = source.DateOfBirth,
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CohortReference = source.CohortReference,
                PaymentStatus = source.PaymentStatus,
                ApprenticeshipStatus = source.ApprenticeshipStatus,
                Alerts = source.Alerts,
                TotalAgreedPrice = source.TotalAgreedPrice,
                AccountLegalEntityId = source.AccountLegalEntityId,
                ConfirmationStatus = source.ConfirmationStatus,
                TransferSenderId = source.TransferSenderId,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                CourseCode = source.CourseCode,
                Cost = source.Cost,
                PledgeApplicationId = source.PledgeApplicationId
            };
        }
    }
}
