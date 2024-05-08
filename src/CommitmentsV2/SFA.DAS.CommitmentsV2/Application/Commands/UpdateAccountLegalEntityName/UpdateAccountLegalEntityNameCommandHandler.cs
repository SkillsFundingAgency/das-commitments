using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName
{
    public class UpdateAccountLegalEntityNameCommandHandler : IRequestHandler<UpdateAccountLegalEntityNameCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public UpdateAccountLegalEntityNameCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task Handle(UpdateAccountLegalEntityNameCommand request, CancellationToken cancellationToken)
        {
            var accountLegalEntity = await _db.Value.AccountLegalEntities.IgnoreQueryFilters().SingleAsync(a => a.Id == request.AccountLegalEntityId, cancellationToken);

            accountLegalEntity.UpdateName(request.Name, request.Created);
        }
    }
}