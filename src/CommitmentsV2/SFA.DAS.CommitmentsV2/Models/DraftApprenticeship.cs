using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class DraftApprenticeship : Apprenticeship
    {
        internal DraftApprenticeship()
        {
        }

        public DraftApprenticeship(DraftApprenticeshipDetails source)
        {
            FirstName = source.FirstName;
            LastName = source.LastName;
            Uln = source.Uln;
            TrainingType = source.TrainingType;
            TrainingCode = source.TrainingCode;
            TrainingName = source.TrainingName;
            Cost = source.Cost;
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            DateOfBirth = source.DateOfBirth;
            EmployerRef = source.EmployerRef;
            ProviderRef = source.ProviderRef;
            ReservationId = source.ReservationId;
        }
    }
}