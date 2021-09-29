using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus
{
    public class GetAccountTransferStatusQueryHandler : IRequestHandler<GetAccountTransferStatusQuery, GetAccountTransferStatusQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountTransferStatusQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountTransferStatusQueryResult> Handle(GetAccountTransferStatusQuery request, CancellationToken cancellationToken)
        {

            var receiver = await _dbContext.Value.Apprenticeships
                .Include(a => a.Cohort)
                .AnyAsync(a => a.Cohort.EmployerAccountId == request.AccountId &&
                               a.PaymentStatus != PaymentStatus.Completed &&
                               a.PaymentStatus != PaymentStatus.Withdrawn &&
                               a.Cohort.TransferSenderId.HasValue, cancellationToken: cancellationToken);

            var sender = await _dbContext.Value.Apprenticeships
                .Include(a => a.Cohort)
                .AnyAsync(a => a.Cohort.TransferSenderId == request.AccountId &&
                               a.PaymentStatus != PaymentStatus.Completed &&
                               a.PaymentStatus != PaymentStatus.Withdrawn, cancellationToken: cancellationToken);

            return new GetAccountTransferStatusQueryResult
            {
                IsTransferSender = sender,
                IsTransferReceiver = receiver
            };
        }
    }
}