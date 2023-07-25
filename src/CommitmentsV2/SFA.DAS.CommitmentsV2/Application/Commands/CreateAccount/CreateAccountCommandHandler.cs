using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public CreateAccountCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = new Account(request.AccountId, request.HashedId, request.PublicHashedId, request.Name,
                request.Created);

            await _db.Value.Accounts.AddAsync(account, cancellationToken);
        }
    }
}