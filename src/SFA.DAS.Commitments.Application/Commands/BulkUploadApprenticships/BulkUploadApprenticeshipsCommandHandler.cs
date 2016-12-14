using System;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommandHandler : AsyncRequestHandler<BulkUploadApprenticeshipsCommand>
    {
        protected override Task HandleCore(BulkUploadApprenticeshipsCommand message)
        {
            throw new NotImplementedException();
        }
    }
}
