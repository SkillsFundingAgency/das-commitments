using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.Services;

public class WithDrawalNotificationToEmployerService(Lazy<ProviderCommitmentsDbContext> dbContext,
         IEncodingService encodingService,
         CommitmentsV2Configuration commitmentsV2Configuration,
         ILogger<WithDrawalNotificationToEmployerService> logger) : IWithDrawalNotificationToEmployerService
{
    public async Task SendWithdrawalNotificationToEmployer(long apprenticeshipId, IMessageHandlerContext context)
    {
        var apprenticeshipDetails = await dbContext.Value.
                                       Apprenticeships.AsNoTracking().Where(t => t.Id == apprenticeshipId).
                                       Select(
                                           x => new
                                           {
                                               x.Id,
                                               x.CourseName,
                                               Cohort = new
                                               {
                                                   x.Cohort.EmployerAccountId,
                                                   AccountLegalEntity = new
                                                   {
                                                       x.Cohort.AccountLegalEntity.Name
                                                   },
                                                   Provider = new
                                                   {
                                                       x.Cohort.Provider.Name
                                                   }
                                               }
                                           }).SingleOrDefaultAsync();

        if (apprenticeshipDetails is null)
        {
            logger.LogInformation("Apprenticeship details not found for apprenticeship id {apprenticeshipId}", apprenticeshipId);
            return;
        }

        var employerEncodedAccountId = encodingService.Encode(apprenticeshipDetails.Cohort.EmployerAccountId, EncodingType.AccountId);
        var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(apprenticeshipDetails.Cohort.EmployerAccountId,
         "LearnerWithdrawalNotificationToEmployer", new Dictionary<string, string>
         {
                {"employer_name", apprenticeshipDetails.Cohort.AccountLegalEntity.Name},
                {"course_name", apprenticeshipDetails.CourseName},
                {"provider_name", apprenticeshipDetails.Cohort.Provider.Name},
                {"url", $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}{employerEncodedAccountId}/apprentices" }
         });

        await context.Send(sendEmailToEmployerCommand);
        logger.LogInformation("Sent Learner Withdrawal Notification to Employer for apprenticeship id {apprenticeshipId}", apprenticeshipId);
    }
}