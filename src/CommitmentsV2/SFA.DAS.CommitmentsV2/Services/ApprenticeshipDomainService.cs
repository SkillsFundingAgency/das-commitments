using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeshipDomainService : IApprenticeshipDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<ApprenticeshipDomainService> _logger;

        public ApprenticeshipDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApprenticeshipDomainService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        //private readonly IAcademicYearDateProvider _academicYearDateProvider;
        //private readonly ILogger<CohortDomainService> _logger;
        //private readonly IUlnValidator _ulnValidator;
        //private readonly IReservationValidationService _reservationValidationService;
        //private readonly IOverlapCheckService _overlapCheckService;
        //private readonly IAuthenticationService _authenticationService;
        //private readonly ICurrentDateTime _currentDateTime;
        //private readonly IEmployerAgreementService _employerAgreementService;
        //private readonly IEncodingService _encodingService;
        //private readonly IAccountApiClient _accountApiClient;

        public async Task<Apprenticeship> GetApprenticeshipById(long apprenticeshipId)
        {
            try {
                return await _dbContext.Value.Apprenticeships.SingleOrDefaultAsync(x => x.Id == apprenticeshipId);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Error in ApprenticeshipDomainService.GetApprentceshipById: {e.Message}");
                throw;
            }
        }
    }
}
