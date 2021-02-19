using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority
{
    public class UpdateProviderPaymentsPriorityCommandHandler : AsyncRequestHandler<UpdateProviderPaymentsPriorityCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<UpdateProviderPaymentsPriorityCommandHandler> _logger;

        public UpdateProviderPaymentsPriorityCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateProviderPaymentsPriorityCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        protected override async Task Handle(UpdateProviderPaymentsPriorityCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Value.Accounts
                .Include(a => a.CustomProviderPaymentPriorities)
                .SingleAsync(a => a.Id == request.AccountId, cancellationToken);

            var updatedCustomProviderPaymentPriorities = request.ProviderPaymentPriorityUpdateItems.Select(r => new CustomProviderPaymentPriority
            {
                EmployerAccountId = request.AccountId,
                ProviderId = r.ProviderId,
                PriorityOrder = r.PriorityOrder
            }).ToList();

            UpdateDifferences(
                account,
                updatedCustomProviderPaymentPriorities,
                request.UserInfo);
        }

        private void UpdateDifferences(
            Account account,
            List<CustomProviderPaymentPriority> updatedPriorites,
            UserInfo userInfo)
        {
            var currentPriorities = account.CustomProviderPaymentPriorities.ToList();

            var changedPriorities = currentPriorities
                .Where(w => updatedPriorites.Exists(e => e.ProviderId == w.ProviderId && e.PriorityOrder != w.PriorityOrder))
                .ToList();
            
            var removedPriorities = currentPriorities
                .Where(w => !updatedPriorites.Exists(e => e.ProviderId == w.ProviderId))
                .ToList();

            var addedPriorities = updatedPriorites
                .Where(w => !currentPriorities.Exists(e => e.ProviderId == w.ProviderId))
                .ToList();

            foreach (var item in changedPriorities)
            {
                var updatedPrority = updatedPriorites.First(f => f.ProviderId == item.ProviderId);
                account.UpdateCustomProviderPaymentPriority(item.ProviderId, updatedPrority.PriorityOrder, userInfo);
            }

            foreach (var item in removedPriorities)
            {
                account.RemoveCustomProviderPaymentPriority(() =>
                {
                    _db.Value.CustomProviderPaymentPriorities.Remove(item);
                    return item;
                }, userInfo);
            }

            foreach (var item in addedPriorities)
            {
                account.AddCustomProviderPaymentPriority(() =>
                {
                    _db.Value.CustomProviderPaymentPriorities.Add(item);
                    return item;
                }, userInfo);
            }

            account.NotifyCustomProviderPaymentPrioritiesChanged();

            _logger.LogInformation($"ProviderPaymentsPriority updated for AccountId : {account.Id}");
        }
    }
}