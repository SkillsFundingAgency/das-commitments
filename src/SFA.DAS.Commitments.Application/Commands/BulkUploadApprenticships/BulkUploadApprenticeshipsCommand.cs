using System.Collections.Generic;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }

        public long CommitmentId { get; set; }

        public IList<Apprenticeship> Apprenticeships { get; set; }
    }
}
