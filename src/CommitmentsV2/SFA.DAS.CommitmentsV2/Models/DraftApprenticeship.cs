using System;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class DraftApprenticeship : Apprenticeship
    {
        internal DraftApprenticeship()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public DraftApprenticeship(DraftApprenticeshipDetails source, Originator originator) : this()
        {
            FirstName = source.FirstName;
            LastName = source.LastName;
            Uln = source.Uln;
            TrainingType = source.TrainingProgramme?.ProgrammeType;
            TrainingCode = source.TrainingProgramme?.CourseCode;
            TrainingName = source.TrainingProgramme?.Name;
            Cost = source.Cost;
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            DateOfBirth = source.DateOfBirth;
            ReservationId = source.ReservationId;

            switch (originator)
            {
                case Originator.Employer:
                    EmployerRef = source.Reference;
                    break;
                case Originator.Provider:
                    ProviderRef = source.Reference;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(originator), originator, null);
            }
        }
    }
}