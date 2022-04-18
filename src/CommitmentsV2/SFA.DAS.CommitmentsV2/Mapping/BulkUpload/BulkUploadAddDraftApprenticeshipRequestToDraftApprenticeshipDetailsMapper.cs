using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper : IMapper<BulkUploadAddDraftApprenticeshipsCommand, List<DraftApprenticeshipDetails>>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;
        private readonly IReservationsApiClient _reservationApiClient;
        private readonly Lazy<ProviderCommitmentsDbContext> _providerDbContext;
        private Dictionary<string, Models.Cohort> _cahcedCohortDetails;
        private Dictionary<string, Models.AccountLegalEntity> _cahcedLegalEntities;

        public BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup, IReservationsApiClient reservationsApiClient, Lazy<ProviderCommitmentsDbContext> providerCommitmentsDbContext)
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
            _reservationApiClient = reservationsApiClient;
            _providerDbContext = providerCommitmentsDbContext;
            _cahcedCohortDetails = new Dictionary<string, Models.Cohort>();
            _cahcedLegalEntities = new Dictionary<string, Models.AccountLegalEntity>();
        }

        public async Task<List<DraftApprenticeshipDetails>> Map(BulkUploadAddDraftApprenticeshipsCommand command)
        {
            var draftApprenticeshipDetailsList = new List<DraftApprenticeshipDetails>();
            await MapReservation(command, CancellationToken.None);

            foreach (var source in command.BulkUploadDraftApprenticeships)
            {
                var result = new DraftApprenticeshipDetails
                {
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    Email = source.Email,
                    Uln = source.Uln,
                    Cost = source.Cost,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    DateOfBirth = source.DateOfBirth,
                    Reference = source.ProviderRef,
                    ReservationId = source.ReservationId,
                    DeliveryModel = Types.DeliveryModel.Regular,
                };
                await MapTrainingProgramme(source, result);
                draftApprenticeshipDetailsList.Add(result);
            }

            return draftApprenticeshipDetailsList;
        }

        private async Task MapTrainingProgramme(BulkUploadAddDraftApprenticeshipRequest source, DraftApprenticeshipDetails result)
        {
            var trainingProgrammeTask = GetCourse(source.CourseCode, source.StartDate);
            var trainingProgramme = await trainingProgrammeTask;
            result.TrainingProgramme = trainingProgramme;
            result.TrainingCourseVersion = trainingProgramme?.Version;
            result.TrainingCourseVersionConfirmed = trainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard;
            result.StandardUId = trainingProgramme?.StandardUId;
        }

        private Task<TrainingProgramme> GetCourse(string courseCode, DateTime? startDate)
        {
            if (startDate.HasValue && int.TryParse(courseCode, out _))
            {
                return _trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate.Value);
            }

            return _trainingProgrammeLookup.GetTrainingProgramme(courseCode);
        }

        private async Task MapReservation(BulkUploadAddDraftApprenticeshipsCommand requests, CancellationToken cancellationToken)
        {
            var request = CreateBulkUploadReservationRequest(requests);
            var results = await _reservationApiClient.BulkCreateReservationsWithNonLevy(request, cancellationToken);

            results.BulkCreateResults.ForEach(x => requests.BulkUploadDraftApprenticeships.First(y => y.Uln == x.ULN).ReservationId = x.ReservationId);

            //var legalEntities = requests.BulkUploadDraftApprenticeships.GroupBy(x => x.LegalEntityId).Select(y => new { Id = y.Key, NumberOfApprentices = y.Count(), DraftApprenticeships = y.ToList() });
            //foreach (var legalEntity in legalEntities)
            //{
            //    var reservationIds = await _reservationApiClient.BulkCreateReservations(legalEntity.Id.Value, new BulkCreateReservationsRequest { Count = ushort.Parse(legalEntity.NumberOfApprentices.ToString()) }, cancellationToken);

            //    for (int counter = 0; counter < legalEntity.NumberOfApprentices; counter++)
            //    {
            //        legalEntity.DraftApprenticeships[counter].ReservationId = reservationIds.ReservationIds[counter];
            //    }
            //}
        }

        private BulkCreateReservationsWithNonLevyRequest CreateBulkUploadReservationRequest(BulkUploadAddDraftApprenticeshipsCommand requests)
        {
            var reservationApiRequest = new BulkCreateReservationsWithNonLevyRequest
            {
                Reservations = requests.BulkUploadDraftApprenticeships.Select(x => new BulkCreateReservations
                {
                    AccountLegalEntityId = GetAccountLegalEntityId(x.AgreementId),
                    AccountId = GetAccountId(x.AgreementId),
                    AccountLegalEntityName = GetAccountLegalEntityName(x.AgreementId),
                    CourseId = x.CourseCode,
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    IsLevyAccount = IsLevy(x.AgreementId),
                    ProviderId = uint.Parse(x.ProviderId.ToString()),
                    StartDate = x.StartDate,
                    TransferSenderAccountId = GetTransferSenderId(x.CohortRef),
                    ULN = x.Uln,
                    UserId = Guid.Parse(requests.UserInfo.UserId)
                }).ToList()
            };

            return reservationApiRequest;
        }


        private long? GetTransferSenderId(string cohortRef) =>
                       GetCohortDetails(cohortRef)?.TransferSenderId;
        private long GetAccountLegalEntityId(string agreementId) =>
                        long.Parse(GetEmployerDetails(agreementId).LegalEntityId);

        private string GetAccountLegalEntityName(string agreementId) =>
                                GetEmployerDetails(agreementId).Name;

        private long GetAccountId(string agreementId) =>
            GetEmployerDetails(agreementId).AccountId;
        private bool IsLevy(string agreementId) =>
            GetEmployerDetails(agreementId).Account.LevyStatus == Types.ApprenticeshipEmployerType.Levy;


        private Models.Cohort GetCohortDetails(string cohortRef)
        {
            if (_cahcedCohortDetails.ContainsKey(cohortRef))
            {
                return _cahcedCohortDetails.GetValueOrDefault(cohortRef);
            }

            var cohort = _providerDbContext.Value.Cohorts
                .Include(x => x.AccountLegalEntity)
                .Include(x => x.Apprenticeships)
                .Where(x => x.Reference == cohortRef).FirstOrDefault();
            _cahcedCohortDetails.Add(cohortRef, cohort);

            return cohort;
        }

        private Models.AccountLegalEntity GetEmployerDetails(string agreementId)
        {
            if (_cahcedLegalEntities.ContainsKey(agreementId))
            {
                return _cahcedLegalEntities.GetValueOrDefault(agreementId);
            }

            var accountLegalEntity = _providerDbContext.Value.AccountLegalEntities
              .Include(x => x.Account)
              .Where(x => x.PublicHashedId == agreementId).FirstOrDefault();

            _cahcedLegalEntities.Add(agreementId, accountLegalEntity);
            return accountLegalEntity;
        }
    }
    
}
