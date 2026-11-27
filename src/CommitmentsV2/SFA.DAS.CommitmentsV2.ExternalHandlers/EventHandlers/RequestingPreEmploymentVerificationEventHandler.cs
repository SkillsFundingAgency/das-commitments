using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.External.Messages.Events.HiringGrant;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class RequestingPreEmploymentVerificationEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDateTime,
    ILogger<RequestingPreEmploymentVerificationEventHandler> logger)
    : IHandleMessages<RequestingPreEmploymentVerification>
{
    public async Task Handle(RequestingPreEmploymentVerification message, IMessageHandlerContext context)
    {
        var db = dbContext.Value;
        var hiringPayment = await db.HiringPayments.FindAsync(message.ApprenticeshipId);
        if (hiringPayment != null)
        {
            hiringPayment.Status = HiringPaymentStatus.QualifiesForHiringPayment;
            hiringPayment.Updated = currentDateTime.UtcNow;
            logger.LogInformation("Restarting pre-employment verification for apprenticeship {ApprenticeshipId}", message.ApprenticeshipId);
            return;
        }

        var apprenticeship = await db.Apprenticeships
            .Include(a => a.Cohort)
            .SingleOrDefaultAsync(a => a.Id == message.ApprenticeshipId);

        if (apprenticeship == null)
        {
            throw new DomainException(nameof(apprenticeship), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
        }

        db.HiringPayments.Add(new HiringPayment
        {
            ApprenticeshipId = apprenticeship.Id,
            Ukprn = message.UKPRN ?? apprenticeship.Cohort.ProviderId,
            Uln = message.ULN?.ToString(CultureInfo.InvariantCulture) ?? apprenticeship.Uln,
            EmployerAccountId = message.EmployerAccountIdentifier ?? apprenticeship.Cohort.EmployerAccountId,
            Status = HiringPaymentStatus.QualifiesForHiringPayment,
            Created = currentDateTime.UtcNow
        });

        logger.LogInformation("Starting pre-employment verification for apprenticeship {ApprenticeshipId}", message.ApprenticeshipId);
    }
}
