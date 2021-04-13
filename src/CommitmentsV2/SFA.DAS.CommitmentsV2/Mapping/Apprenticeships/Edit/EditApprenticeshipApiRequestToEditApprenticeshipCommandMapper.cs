//using SFA.DAS.CommitmentsV2.Api.Types.Requests;
//using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
//using SFA.DAS.CommitmentsV2.Shared.Interfaces;
//using System.Threading.Tasks;

//namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.Edit
//{
//    public class EditApprenticeshipApiRequestToEditApprenticeshipCommandMapper : IMapper<EditApprenticeshipApiRequest, EditApprenticeshipCommand>
//    {
//        public Task<EditApprenticeshipCommand> Map(EditApprenticeshipApiRequest request)
//        {
//            return Task.FromResult(new EditApprenticeshipCommand
//            {
//                ProviderId = request.ProviderId,
//                AccountId = request.AccountId,
//                ApprenticeshipId = request.ApprenticeshipId,
//                FirstName = request.FirstName,
//                LastName = request.LastName,
//                DateOfBirth = request.DateOfBirth,
//                ULN = request.ULN,
//                Cost = request.Cost,
//                EmployerReference = request.EmployerReference,
//                StartDate = request.StartDate,
//                EndDate = request.EndDate,
//                CourseCode = request.CourseCode
//            });
//        }
//    }
//}
