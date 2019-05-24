using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class DraftApprenticeship : Apprenticeship
    {
        internal DraftApprenticeship()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public DraftApprenticeship(DraftApprenticeshipDetails source, Originator party) : this()
        {
            FirstName = source.FirstName;
            LastName = source.LastName;
            Uln = source.Uln;
            ProgrammeType = source.TrainingProgramme?.ProgrammeType;
            CourseCode = source.TrainingProgramme?.CourseCode;
            CourseName = source.TrainingProgramme?.Name;
            Cost = source.Cost;
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            DateOfBirth = source.DateOfBirth;
            ReservationId = source.ReservationId;

            switch (party)
            {
                case Originator.Employer:
                    EmployerRef = source.Reference;
                    break;
                case Originator.Provider:
                    ProviderRef = source.Reference;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }
    }
}