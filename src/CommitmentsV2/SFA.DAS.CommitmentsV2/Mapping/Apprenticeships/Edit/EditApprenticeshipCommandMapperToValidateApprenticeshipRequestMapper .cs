//using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
//using SFA.DAS.CommitmentsV2.Data;
//using SFA.DAS.CommitmentsV2.Data.Extensions;
//using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
//using SFA.DAS.CommitmentsV2.Extensions;
//using SFA.DAS.CommitmentsV2.Shared.Interfaces;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.Edit
//{
//    public class EditApprenticeshipCommandMapperToValidateApprenticeshipRequestMapper : IMapper<EditApprenticeshipCommand, EditApprenticeshipValidationRequest>
//    {
//        private Lazy<ProviderCommitmentsDbContext> _providerDbContext;
//        private ICurrentDateTime _currentDateTime;

//        public EditApprenticeshipCommandMapperToValidateApprenticeshipRequestMapper(Lazy<ProviderCommitmentsDbContext> providerDbContext, ICurrentDateTime currentDateTime)
//        {
//            _providerDbContext = providerDbContext;
//            _currentDateTime = currentDateTime;
//        }

//        public async Task<EditApprenticeshipValidationRequest> Map(EditApprenticeshipCommand request)
//        {
//            var source = request.EditApprenticeshipRequest;
//            var validationRequest = new EditApprenticeshipValidationRequest();

//            var apprenticeship = await _providerDbContext.Value.GetApprenticeshipAggregate(source.ApprenticeshipId, CancellationToken.None);

//            validationRequest.CourseCode = GetValue(source.CourseCode, apprenticeship.CourseCode);
//            validationRequest.FirstName = GetValue(source.FirstName, apprenticeship.FirstName);
//            validationRequest.LastName = GetValue(source.LastName, apprenticeship.LastName);
//            validationRequest.EmployerReference = GetValue(source.EmployerReference, apprenticeship.EmployerRef);
//            validationRequest.ULN = GetValue(source.ULN, apprenticeship.Uln);
//            validationRequest.DateOfBirth = source.DateOfBirth ?? apprenticeship.DateOfBirth;
//            validationRequest.EndDate = source.EndDate ?? apprenticeship.EndDate;
//            validationRequest.StartDate = source.StartDate ?? apprenticeship.StartDate;
//            validationRequest.Cost = source.Cost ?? apprenticeship.PriceHistory.GetPrice(_currentDateTime.UtcNow);

//            return validationRequest;
//        }

//        private string GetValue(string sourceString, string apprenticeshipString)
//        {
//            if (!string.IsNullOrWhiteSpace(sourceString))
//                return sourceString;

//            return apprenticeshipString;
//        }
//    }
//}
