using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class DraftApprenticeshipCommandBaseMapper
    {
        public Task<T> Map<T>(AddDraftApprenticeshipRequest source) where T : DraftApprenticeshipCommandBase, new()
        {
            return Task.FromResult(new T()
            {
                UserId = source.UserId,
                ProviderId = source.ProviderId,
                CourseCode = source.CourseCode,
                DeliveryModel = source.DeliveryModel,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                OriginatorReference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                DateOfBirth = source.DateOfBirth,
                Uln = source.Uln,
                EmploymentEndDate = source.EmploymentEndDate,
                EmploymentPrice = source.EmploymentPrice,
                UserInfo = source.UserInfo,
                IgnoreStartDateOverlap = source.IgnoreStartDateOverlap,
                IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot
            });
        }
    }
}