using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.EditValidation
{
    public class ValidateApprenticeshipForEditCommandToValidateApprenticeshipRequestMapper : IMapper<ValidateApprenticeshipForEditCommand, EditApprenticeshipValidationRequest>
    {
        public Task<EditApprenticeshipValidationRequest> Map(ValidateApprenticeshipForEditCommand request)
        {
            return Task.FromResult(new EditApprenticeshipValidationRequest
            {
                ProviderId = request.ProviderId,
                EmployerAccountId = request.EmployerAccountId,
                ApprenticeshipId = request.ApprenticeshipId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                ULN = request.ULN,
                Cost = request.Cost,
                EmployerReference = request.EmployerReference,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TrainingCode = request.TrainingCode
            });
        }
    }
}
