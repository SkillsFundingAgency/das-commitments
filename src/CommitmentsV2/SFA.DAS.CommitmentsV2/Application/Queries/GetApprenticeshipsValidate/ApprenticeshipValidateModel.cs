using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate
{
    public class ApprenticeshipValidateModel
    {
        public long ApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public string TrainingCode { get; set; }
        public string StandardUId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StopDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public long Ukprn { get; set; }
        public string EmployerName { get; set; }

        public static implicit operator ApprenticeshipValidateModel(Models.Apprenticeship apprenticeship) =>
            new ApprenticeshipValidateModel()
            {
                ApprenticeshipId = apprenticeship.Id,
                Uln = apprenticeship.Uln,
                TrainingCode = apprenticeship.CourseCode,
                StandardUId = apprenticeship.StandardUId,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate,
                StopDate = apprenticeship.StopDate,
                PaymentStatus = apprenticeship.PaymentStatus,
                EmployerName = apprenticeship.Cohort.AccountLegalEntity.Name,
                Ukprn = apprenticeship.Cohort.ProviderId
            };
    }
}
