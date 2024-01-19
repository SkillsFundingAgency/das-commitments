using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ModelBinding;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Http;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.Authorization.Features.Configuration;
using SFA.DAS.CommitmentsV2.Mapping;
using System.Linq;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using Microsoft.AspNetCore.Http;
using SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers;
using SFA.DAS.CommitmentsV2.Mapping.Reservations;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.EditValidation;
using SFA.DAS.ReservationsV2.Api.Client;
using SFA.DAS.CommitmentsV2.LinkGeneration;
//using Microsoft.AspNetCore.Authentication;

namespace SFA.DAS.CommitmentsV2.DependencyResolution;

public static class ServiceRegistrationExtensions
{

    public static IServiceCollection AddAcademicYearDateProviderServices(this IServiceCollection services)
    {
        services.AddSingleton<IAcademicYearDateProvider, AcademicYearDateProvider>();
        return services;
    }

    public static IServiceCollection AddApprovalsOuterApiServiceServices(this IServiceCollection services)
    {
        services.AddTransient<HttpClient>();
        services.AddSingleton<IApprovalsOuterApiClient, ApprovalsOuterApiClient>();

        return services;
    }

    public static IServiceCollection AddReservationsApiClient(this IServiceCollection services)
    {
        services.AddTransient<IReservationsApiClientFactory, ReservationsApiClientFactory>();
        services.AddSingleton(s=> (s.GetRequiredService<IReservationsApiClientFactory>()).CreateClient());

        return services;
    }



    public static IServiceCollection AddApprenticeshipSearchServices(this IServiceCollection services)
    {
        services.AddTransient<IApprenticeshipSearch, ApprenticeshipSearch>();
        services.AddTransient<IApprenticeshipSearchService<ApprenticeshipSearchParameters>, ApprenticeshipSearchService>();
        services.AddTransient<IApprenticeshipSearchService<OrderedApprenticeshipSearchParameters>, OrderedApprenticeshipSearchService>();
        services.AddTransient<IApprenticeshipSearchService<ReverseOrderedApprenticeshipSearchParameters>, ReverseOrderedApprenticeshipSearchService>();

        return services;
    }




    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddTransient<IApprenticeshipDomainService, ApprenticeshipDomainService>();
        services.AddTransient<ICohortDomainService, CohortDomainService>();
        services.AddTransient<IChangeOfPartyRequestDomainService, ChangeOfPartyRequestDomainService>();
        services.AddTransient<ITransferRequestDomainService, TransferRequestDomainService>();
        services.AddTransient<IApprenticeshipStatusSummaryService, ApprenticeshipStatusSummaryService>();
        services.AddTransient<IReservationValidationService, ReservationValidationService>();
        services.AddSingleton<IEmployerAgreementService, EmployerAgreementService>();
        services.AddTransient<IUlnUtilisationService, UlnUtilisationService>();
        services.AddTransient<IEmailOverlapService, EmailOverlapService>();
        services.AddTransient<IRplFundingCalculationService, RplFundingCalculationService>();
        services.AddTransient<IUlnValidator, UlnValidator>();
        services.AddTransient<IEditApprenticeshipValidationService, EditApprenticeshipValidationService>();
        services.AddTransient<IEmailOptionalService, EmailOptionalService>();
        services.AddTransient<IEmployerAlertSummaryEmailService, EmployerAlertSummaryEmailService>();
        services.AddTransient<IEmployerTransferRequestPendingEmailService, EmployerTransferRequestPendingEmailService>();
        services.AddTransient<IProviderAlertSummaryEmailService, ProviderAlertSummaryEmailService>();
        services.AddTransient<IOverlappingTrainingDateRequestDomainService, OverlappingTrainingDateRequestDomainService>();
        services.AddTransient<IResolveOverlappingTrainingDateRequestService, ResolveOverlappingTrainingDateRequestService>();
        services.AddTransient<IAddEpaToApprenticeshipService, AddEpaToApprenticeshipsService>();
        services.AddTransient<IDataLockUpdaterService, DataLockUpdaterService>();
        services.AddTransient<IFilterOutAcademicYearRollOverDataLocks, FilterOutAcademicYearRollOverDataLocks>();
        services.AddTransient<Learners.Validators.IUlnValidator, Learners.Validators.UlnValidator>();
        services.AddTransient<IFeatureTogglesService<FeatureToggle>, FeatureTogglesService<FeaturesConfiguration, FeatureToggle>>();

        return services;
    }


    public static IServiceCollection AddMappingServices(this IServiceCollection services)
    {
        services.AddTransient<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>, AddCohortCommandToDraftApprenticeshipDetailsMapper>();
        services.AddTransient<IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>, AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper>();
        services.AddTransient<IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse>, GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper>();
        services.AddTransient<IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse>, GetDraftApprenticeshipsResultMapper>();
        services.AddTransient<IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>, AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper>();
        services.AddTransient<IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand>, DeleteDraftApprenticeshipRequestToDeleteDraftApprenticeshipCommandMapper>();
        services.AddTransient<IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>, UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapper>();
        services.AddTransient<IOldMapper<ReservationValidationRequest, ReservationValidationMessage>, ReservationValidationRequestToValidationReservationMessageMapper>();
        services.AddTransient<IOldMapper<Reservations.Api.Types.ReservationValidationResult, Domain.Entities.Reservations.ReservationValidationResult>, ValidationResultToReservationValidationErrorMapper>();
        services.AddTransient<IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails>, UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper>();

        services.AddMappers();


        return services;
    }


    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        var mappingAssembly = typeof(ReservationValidationRequestToValidationReservationMessageMapper).Assembly;

        var mappingTypes = mappingAssembly
            .GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>)));

        foreach (var mapperType in mappingTypes)
        {
            var mapperInterface = mapperType.GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>));

            services.AddTransient(mapperInterface, mapperType);
        }

        return services;
    }






}