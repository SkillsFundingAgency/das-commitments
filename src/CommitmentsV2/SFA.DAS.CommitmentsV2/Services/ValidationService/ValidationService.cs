using FluentValidation;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService(
    ILogger<ValidationService> logger,
    IOverlapCheckService overlapService,
    IAcademicYearDateProvider academicYearDateProvider,
    IProviderRelationshipsApiClient providerRelationshipsApiClient,
    IEmployerAgreementService employerAgreementService,
    IUlnValidator ulnValidator,
    ILinkGenerator urlHelper) : IValidationService
{
    private static readonly InlineValidator<string> NameValidator = new()
    {
        validator => validator.RuleFor(name => name).ValidFreeTextCharacters()
    };
}
