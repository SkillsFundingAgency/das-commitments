using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers;
using SFA.DAS.CommitmentsV2.Mapping.Reservations;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.ReservationsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Services;

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
        services.AddSingleton(s=> s.GetRequiredService<IReservationsApiClientFactory>().CreateClient());

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

    public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services)
    {
        services.AddDbContext<ProviderCommitmentsDbContext>((sp, options) =>
        {
            var dbConnection = DatabaseExtensions.GetSqlConnection(sp.GetService<CommitmentsV2Configuration>().DatabaseConnectionString);
            options.UseSqlServer(dbConnection);
        });

        services.AddScoped(provider => new Lazy<ProviderCommitmentsDbContext>(provider.GetService<ProviderCommitmentsDbContext>()));
        services.AddScoped<IProviderCommitmentsDbContext>(c => c.GetService<ProviderCommitmentsDbContext>());

        return services;
    }

    public static IServiceCollection AddCurrentDateTimeService(this IServiceCollection services, IConfiguration config)
    {
        var commitmentsConfiguration = config.Get<CommitmentsV2Configuration>();
        if (DateTime.TryParse(commitmentsConfiguration.CurrentDateTime, out var overrideValue))
        {
            services.AddTransient<ICurrentDateTime>(s => new CurrentDateTime(overrideValue));
        }
        else
        {
            services.AddTransient<ICurrentDateTime, CurrentDateTime>();
        }

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
        services.AddTransient<IOverlapCheckService, OverlapCheckService>();
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
        services.AddTransient<ITrainingProgrammeLookup, TrainingProgrammeLookup>();

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
            .Where(type => type.GetInterfaces().ToList().Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>)));

        foreach (var mapperType in mappingTypes)
        {
            var mapperInterface = mapperType.GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>));

            services.AddTransient(mapperInterface, mapperType);
        }

        return services;
    }
}