using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.BulkUpload
{
    public class BulkUploadValidateQueryHandler : IRequestHandler<BulkUploadValidateQuery, BulkUploadResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IReservationsApiClient _reservationApiClient;
        private readonly IMediator _mediator;

        public BulkUploadValidateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IReservationsApiClient reservationsApiClient, IMediator mediator)
        {
            _dbContext = dbContext;
            _reservationApiClient = reservationsApiClient;
            _mediator = mediator;
        }

        public async Task<BulkUploadResponse> Handle(BulkUploadValidateQuery query, CancellationToken cancellationToken)
        {
            query.UserInfo = new UserInfo() { UserDisplayName = "xxx", UserEmail = "xxx@hotmail.com", UserId = "1" };
            var response = new BulkUploadResponse() { Results = new List<BulkCreateResult>() };
            await GetReservationErrors(query);
            var legalEntities = query.request.BulkUploadApprenticeships.Select(x => x.LegalEntityId).Distinct().ToList();

            foreach (var legalEntityId in legalEntities)
            {
                var legalEntity = _dbContext.Value.AccountLegalEntities.Include(x => x.Account).First(x => x.Id == legalEntityId);
                var apprentices = query.request.BulkUploadApprenticeships.Where(x => x.LegalEntityId == legalEntityId).ToList();

                Guid reservationId = Guid.Empty;

                foreach (var apprentice in apprentices)
                {

                    if (legalEntity.Account.LevyStatus == ApprenticeshipEmployerType.NonLevy)
                    {
                        reservationId = await _reservationApiClient.CreateReservationNonLevy(new Reservation
                        {
                            Id = Guid.NewGuid(),

                            StartDate = apprentice.StartDate.Value,
                            CourseId = apprentice.CourseCode,
                            AccountId = legalEntity.AccountId,

                            ProviderId = (uint?)apprentice.ProviderId,
                            AccountLegalEntityId = legalEntity.Id,
                            AccountLegalEntityName = legalEntity.Name,
                            IsLevyAccount = false
                        }, CancellationToken.None);
                    }
                    else
                    {
                        var reservationResult = await _reservationApiClient.BulkCreateReservations(legalEntity.Id,
                            new BulkCreateReservationsRequest
                            {
                                Count = 1,
                                TransferSenderId = null
                            }, CancellationToken.None);

                        reservationId = reservationResult.ReservationIds.First();
                    }

                    if (!string.IsNullOrEmpty(apprentice.CohortRef))
                    {
                        var cohort = _dbContext.Value.Cohorts.First(x => x.Reference == apprentice.CohortRef);

                        var command = new AddDraftApprenticeshipCommand
                        {
                            UserId = "1",
                            ProviderId = apprentice.ProviderId,
                            CourseCode = apprentice.CourseCode,
                            Cost = apprentice.Cost,
                            StartDate = apprentice.StartDate,
                            EndDate = apprentice.EndDate,
                            OriginatorReference = apprentice.OriginatorReference,
                            ReservationId = reservationId,
                            FirstName = apprentice.FirstName,
                            LastName = apprentice.LastName,
                            DateOfBirth = apprentice.DateOfBirth,
                            Uln = apprentice.ULN,
                            UserInfo = query.UserInfo,
                            CohortId = cohort.Id
                        };

                        var result = await _mediator.Send(command);

                        response.Results.Add(new BulkCreateResult { CohortReference = apprentice.CohortRef, CohortId = result.Id, ApprenticeName = apprentice.FirstName + " " + apprentice.LastName, LegalEntityId = legalEntity.Id, LegalEntityName = legalEntity.Name, AccountName = legalEntity.Account.Name });
                    }
                    else
                    {
                        // Move providerId to BulkuploadValidateQuery
                        var command = new AddCohortCommand(
                        legalEntity.AccountId,
                        legalEntity.Id,
                        apprentice.ProviderId,
                        apprentice.CourseCode,
                        apprentice.Cost,
                        apprentice.StartDate,
                        apprentice.EndDate,
                        apprentice.OriginatorReference,
                        reservationId,
                        apprentice.FirstName,
                        apprentice.LastName,
                        apprentice.DateOfBirth,
                        apprentice.ULN,
                        null,
                        query.UserInfo);
                        var result = await _mediator.Send(command);

                        response.Results.Add(new BulkCreateResult { CohortReference = result.Reference, CohortId = result.Id, ApprenticeName = apprentice.FirstName + " " + apprentice.LastName, LegalEntityId = legalEntity.Id, LegalEntityName = legalEntity.Name, AccountName = legalEntity.Account.Name });
                    }
                }
            }


            // Add property validations.

            // Add ULN validation errors.

            return response;
        }

        private async Task GetReservationErrors(BulkUploadValidateQuery query)
        {
            foreach (var legalEntityId in query.request.BulkUploadApprenticeships.Select(x => x.LegalEntityId))
            {
                var legalEntity = _dbContext.Value.AccountLegalEntities.Include(x => x.Account).First(x => x.Id == legalEntityId);
                var apprentices = query.request.BulkUploadApprenticeships.Where(x => x.LegalEntityId == legalEntityId).ToList();

                if (legalEntity.Account.LevyStatus == ApprenticeshipEmployerType.NonLevy)
                {
                    var errors = await GetReservationErrors(apprentices, legalEntity.AccountId, legalEntity.Id, apprentices.First().ProviderId);

                    List<BulkUploadDomainError> domainErrors = new List<BulkUploadDomainError>();

                    if (errors.ValidationErrors.Count > 0)
                    {

                        foreach (var erorr in errors.ValidationErrors)
                        {
                            domainErrors.Add(new BulkUploadDomainError(legalEntity.Name, string.Empty, string.Empty, string.Empty, erorr.Reason));
                        }

                        throw new DomainException(domainErrors);
                    }
                }
            }
        }

        private async Task<BulkValidationResults> GetReservationErrors(List<BulkUploadApprenticeship> list, long employerAccountId, long employerLegalEntityId, long? providerId)
        {
            List<Reservation> reservations = new List<Reservation>();
            foreach (var apprentice in list.Where(x =>
               !string.IsNullOrWhiteSpace(x.ULN)
               && x.StartDate.HasValue
               && x.EndDate.HasValue
               ))
            {
                reservations.Add(new Reservation
                {
                    StartDate = apprentice.StartDate.Value,
                    CourseId = apprentice.CourseCode,
                    AccountId = employerAccountId,
                    ProviderId = (uint?)providerId,
                    AccountLegalEntityId = employerLegalEntityId
                });
            }
            var result = await _reservationApiClient.BulkValidate(reservations, CancellationToken.None);
            return result;
        }
    }
}