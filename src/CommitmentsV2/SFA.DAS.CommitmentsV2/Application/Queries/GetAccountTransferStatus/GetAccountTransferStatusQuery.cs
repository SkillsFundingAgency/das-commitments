using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus
{
    public class GetAccountTransferStatusQuery : IRequest<GetTransferStatusQueryResult>
    {
        public long AccountId { get; set; }
    }

    public class GetTransferStatusQueryResult
    {
        public bool IsTransferReceiver { get; set; }
        public bool IsTransferSender { get; set; }
    }

    public class GetTransferStatusQueryHandler : IRequestHandler<GetAccountTransferStatusQuery, GetTransferStatusQueryResult>
    {
        public GetTransferStatusQueryHandler()
        {
            
        }

        public Task<GetTransferStatusQueryResult> Handle(GetAccountTransferStatusQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

}
