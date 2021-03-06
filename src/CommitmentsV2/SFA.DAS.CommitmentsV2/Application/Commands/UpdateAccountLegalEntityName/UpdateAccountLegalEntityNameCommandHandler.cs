using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName
{
    public class UpdateAccountLegalEntityNameCommandHandler : AsyncRequestHandler<UpdateAccountLegalEntityNameCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public UpdateAccountLegalEntityNameCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        protected override async Task Handle(UpdateAccountLegalEntityNameCommand request, CancellationToken cancellationToken)
        {
            var accountLegalEntity = await _db.Value.AccountLegalEntities.IgnoreQueryFilters().SingleAsync(a => a.Id == request.AccountLegalEntityId, cancellationToken);

            accountLegalEntity.UpdateName(request.Name, request.Created);
        }
    }
}