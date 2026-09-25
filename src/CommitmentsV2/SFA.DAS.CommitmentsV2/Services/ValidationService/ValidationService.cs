using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService(
    ILogger<ValidationService> logger,
    IOverlapCheckService overlapService,
    IAcademicYearDateProvider academicYearDateProvider,
    IProviderRelationshipsApiClient providerRelationshipsApiClient,
    IEmployerAgreementService employerAgreementService,
    IUlnValidator ulnValidator) : IValidationService
{
}