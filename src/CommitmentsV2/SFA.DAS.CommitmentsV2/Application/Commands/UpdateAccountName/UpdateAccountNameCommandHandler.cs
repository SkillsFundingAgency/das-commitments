using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName
{
    public class UpdateAccountNameCommandHandler : IRequestHandler<UpdateAccountNameCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public UpdateAccountNameCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task Handle(UpdateAccountNameCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);

            account.UpdateName(request.Name, request.Created);
        }
    }
}