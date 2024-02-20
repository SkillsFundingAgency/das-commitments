using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;

public class UpdateProviderPaymentsPriorityCommandHandler : IRequestHandler<UpdateProviderPaymentsPriorityCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly ILogger<UpdateProviderPaymentsPriorityCommandHandler> _logger;

    public UpdateProviderPaymentsPriorityCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateProviderPaymentsPriorityCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(UpdateProviderPaymentsPriorityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Provider Payment Priority for employer account {AccountId}", request.AccountId);

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

        _logger.LogInformation("Updated Provider Payment Priorities with {Count} providers for employer account {AccountId}", request.ProviderPaymentPriorityUpdateItems.Count, request.AccountId);
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

        if (changedPriorities.Any())
        {
            foreach (var item in changedPriorities)
            {
                var updatedPriority = updatedPriorites.First(f => f.ProviderId == item.ProviderId);
                account.UpdateCustomProviderPaymentPriority(item.ProviderId, updatedPriority.PriorityOrder, userInfo);
            }

            _logger.LogInformation("Changed {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (removedPriorities.Any())
        {
            foreach (var item in removedPriorities)
            {
                account.RemoveCustomProviderPaymentPriority(() =>
                {
                    _db.Value.CustomProviderPaymentPriorities.Remove(item);
                    return item;
                }, userInfo);
            }

            _logger.LogInformation("Removed {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (addedPriorities.Any())
        {
            foreach (var item in addedPriorities)
            {
                account.AddCustomProviderPaymentPriority(() =>
                {
                    _db.Value.CustomProviderPaymentPriorities.Add(item);
                    return item;
                }, userInfo);
            }

            _logger.LogInformation("Added {Count} Provider Payment Priorities for employer account {Id}", changedPriorities.Count, account.Id);
        }

        if (!changedPriorities.Any() && !removedPriorities.Any() && !addedPriorities.Any())
        {
            return;
        }
        
        account.NotifyCustomProviderPaymentPrioritiesChanged();

        _logger.LogInformation("Notified Provider Payment Priorities Updated for employer account {account.Id}", account.Id);
    }
}