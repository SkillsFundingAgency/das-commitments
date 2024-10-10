using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;

public class UpdateProviderPaymentsPriorityCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateProviderPaymentsPriorityCommandHandler> logger)
    : IRequestHandler<UpdateProviderPaymentsPriorityCommand>
{
    public async Task Handle(UpdateProviderPaymentsPriorityCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating Provider Payment Priority for employer account {AccountId}", request.AccountId);

        var account = await db.Value.Accounts
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

        logger.LogInformation("Updated Provider Payment Priorities with {Count} providers for employer account {AccountId}", request.ProviderPaymentPriorityUpdateItems.Count, request.AccountId);
    }

    private void UpdateDifferences(
        Account account,
        List<CustomProviderPaymentPriority> updatedPriorities,
        UserInfo userInfo)
    {
        var currentPriorities = account.CustomProviderPaymentPriorities.ToList();

        var changedPriorities = currentPriorities
            .Where(w => updatedPriorities.Exists(e => e.ProviderId == w.ProviderId && e.PriorityOrder != w.PriorityOrder))
            .ToList();
            
        var removedPriorities = currentPriorities
            .Where(w => !updatedPriorities.Exists(e => e.ProviderId == w.ProviderId))
            .ToList();

        var addedPriorities = updatedPriorities
            .Where(w => !currentPriorities.Exists(e => e.ProviderId == w.ProviderId))
            .ToList();

        if (changedPriorities.Count != 0)
        {
            foreach (var item in changedPriorities)
            {
                var updatedPriority = updatedPriorities.First(f => f.ProviderId == item.ProviderId);
                account.UpdateCustomProviderPaymentPriority(item.ProviderId, updatedPriority.PriorityOrder, userInfo);
            }

            logger.LogInformation("Changed {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (removedPriorities.Count != 0)
        {
            foreach (var item in removedPriorities)
            {
                account.RemoveCustomProviderPaymentPriority(() =>
                {
                    db.Value.CustomProviderPaymentPriorities.Remove(item);
                    return item;
                }, userInfo);
            }

            logger.LogInformation("Removed {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (addedPriorities.Count != 0)
        {
            foreach (var item in addedPriorities)
            {
                account.AddCustomProviderPaymentPriority(() =>
                {
                    db.Value.CustomProviderPaymentPriorities.Add(item);
                    return item;
                }, userInfo);
            }

            logger.LogInformation("Added {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (changedPriorities.Count == 0 && removedPriorities.Count == 0 && addedPriorities.Count == 0)
        {
            return;
        }
        
        account.NotifyCustomProviderPaymentPrioritiesChanged();

        logger.LogInformation("Notified Provider Payment Priorities Updated for employer account {Id}", account.Id);
    }
}