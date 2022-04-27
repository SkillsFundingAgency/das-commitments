using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper : IMapper<BulkUploadAddDraftApprenticeshipsCommand, List<DraftApprenticeshipDetails>>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;
        //private readonly IReservationsApiClient _reservationApiClient;
        //private readonly Lazy<ProviderCommitmentsDbContext> _providerDbContext;
        //private Dictionary<string, Models.Cohort> _cahcedCohortDetails;
        //private Dictionary<string, Models.AccountLegalEntity> _cahcedLegalEntities;

        public BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup
            //, IReservationsApiClient reservationsApiClient, Lazy<ProviderCommitmentsDbContext> providerCommitmentsDbContext
            )
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
            //_reservationApiClient = reservationsApiClient;
            //_providerDbContext = providerCommitmentsDbContext;
            //_cahcedCohortDetails = new Dictionary<string, Models.Cohort>();
            //_cahcedLegalEntities = new Dictionary<string, Models.AccountLegalEntity>();
        }

        public async Task<List<DraftApprenticeshipDetails>> Map(BulkUploadAddDraftApprenticeshipsCommand command)
        {
            var draftApprenticeshipDetailsList = new List<DraftApprenticeshipDetails>();
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

        //private void MapReservation(BulkUploadAddDraftApprenticeshipsCommand requests)
        //{
        //    requests.ReservationsWithValidation.BulkCreateResults.ForEach(x => requests.BulkUploadDraftApprenticeships.First(y => y.Uln == x.ULN).ReservationId = x.ReservationId);
        //}
    }
}
