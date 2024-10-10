using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;

public class GetAccountTransferStatusQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetAccountTransferStatusQuery, GetAccountTransferStatusQueryResult>
{
    public async Task<GetAccountTransferStatusQueryResult> Handle(GetAccountTransferStatusQuery request, CancellationToken cancellationToken)
    {
        var receiver = await dbContext.Value.Apprenticeships
            .Include(a => a.Cohort)
            .AnyAsync(a => a.Cohort.EmployerAccountId == request.AccountId &&
                           a.PaymentStatus != PaymentStatus.Completed &&
                           a.PaymentStatus != PaymentStatus.Withdrawn &&
                           a.Cohort.TransferSenderId.HasValue, cancellationToken: cancellationToken);

        var sender = await dbContext.Value.Apprenticeships
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