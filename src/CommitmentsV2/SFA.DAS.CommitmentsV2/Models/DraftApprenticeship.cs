using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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
            Merge(source, party);

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

        public void Merge(DraftApprenticeshipDetails source, Originator modifyingParty)
        {
            FirstName = source.FirstName;
            LastName = source.LastName;
            if (modifyingParty == Originator.Provider)
            {
                Uln = source.Uln;
            }
            else if(Uln != source.Uln)
            {
                throw new DomainException(nameof(Uln), "Only providers are allowed to update the Uln");
            }
            ProgrammeType = source.TrainingProgramme?.ProgrammeType;
            CourseCode = source.TrainingProgramme?.CourseCode;
            CourseName = source.TrainingProgramme?.Name;
            Cost = source.Cost;
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            DateOfBirth = source.DateOfBirth;

            switch (modifyingParty)
            {
                case Originator.Employer:
                    EmployerRef = source.Reference;
                    break;

                case Originator.Provider:
                    ProviderRef = source.Reference;
                    break;
            }
        }
    }
}