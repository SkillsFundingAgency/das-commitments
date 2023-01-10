using MediatR;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest
{
    public class ResolveOverlappingTrainingDateRequestCommand : IRequest
    {
        public long? ApprenticeshipId { get; set; }
        public OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
    }
}