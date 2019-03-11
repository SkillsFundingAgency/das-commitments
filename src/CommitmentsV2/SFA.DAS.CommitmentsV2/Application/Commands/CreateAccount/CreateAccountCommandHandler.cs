using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount
{
    public class CreateAccountCommandHandler : RequestHandler<CreateAccountCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public CreateAccountCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        protected override void Handle(CreateAccountCommand request)
        {
            var account = new Account(request.AccountId, request.HashedId, request.PublicHashedId, request.Name, request.Created);

            _db.Value.Accounts.Add(account);
        }
    }
}