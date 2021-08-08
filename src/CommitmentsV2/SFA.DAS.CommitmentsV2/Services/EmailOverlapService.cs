using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class EmailOverlapService : IEmailOverlapService
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<EmailOverlapService> _logger;

        public EmailOverlapService(IDbContextFactory dbContextFactory, ILogger<EmailOverlapService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<List<OverlappingEmail>> GetOverlappingEmails(EmailToValidate emailToValidate, long? cohortId, CancellationToken cancellationToken)
        {
            using var db = _dbContextFactory.CreateDbContext();

            var emailParam = new SqlParameter("@Email", emailToValidate.Email);
            var startDateParam = new SqlParameter("@StartDate", emailToValidate.StartDate);
            var endDateParam = new SqlParameter("@EndDate", emailToValidate.EndDate);
            var apprenticeshipIdParam = new SqlParameter("@ApprenticeshipId", emailToValidate.ApprenticeshipId);
            apprenticeshipIdParam.Value ??= DBNull.Value;

            var cohortIdParam = new SqlParameter("@CohortId", cohortId);
            cohortIdParam.Value ??= DBNull.Value;

            try
            {

                var query = db.OverlappingEmails.FromSql(
                    "EXEC CheckForOverlappingEmails @Email, @StartDate, @EndDate, @ApprenticeshipId, @CohortId", emailParam, startDateParam, endDateParam, apprenticeshipIdParam, cohortIdParam);

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Calling Stored Procedure CheckForOverlappingEmails", e);
                throw;
            }
        }

    }
}