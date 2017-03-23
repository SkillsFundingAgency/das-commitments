using System;
using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    [Validator(typeof(CreateApprenticeshipUpdateValidator))]
    public class CreateApprenticeshipUpdateCommand: IAsyncRequest
    {
        //todo:replace with api type
        public PendingApprenticeshipUpdatePlaceholder ApprenticeshipUpdate { get; set; }
        
    }
}
