﻿using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.Edit;

public class ValidateApprenticeshipForEditRequestToValidateApprenticeshipForEditCommand : IMapper<ValidateApprenticeshipForEditRequest, ValidateApprenticeshipForEditCommand>
{
    public Task<ValidateApprenticeshipForEditCommand>  Map(ValidateApprenticeshipForEditRequest request)
    {
        return Task.FromResult(new ValidateApprenticeshipForEditCommand
        {
            ApprenticeshipValidationRequest = new EditApprenticeshipValidationRequest
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
                DeliveryModel = request.DeliveryModel,
                CourseCode = request.TrainingCode,
                Version = request.Version,
                Option = request.Option,
                ProviderReference = request.ProviderReference,
                Email = request.Email,
                EmploymentEndDate = request.EmploymentEndDate,
                EmploymentPrice = request.EmploymentPrice
            }
        });
    }
}