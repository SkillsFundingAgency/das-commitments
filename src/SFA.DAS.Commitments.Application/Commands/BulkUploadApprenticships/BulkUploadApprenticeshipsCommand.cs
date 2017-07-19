using System.Collections.Generic;
using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
        public IEnumerable<Apprenticeship> Apprenticeships { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
