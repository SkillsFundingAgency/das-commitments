using System;
using System.Xml.Serialization;
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

        public DraftApprenticeship(DraftApprenticeshipDetails source, Party creator) : this()
        {
            Merge(source, creator);

            ReservationId = source.ReservationId;

            switch (creator)
            {
                case Party.Employer:
                    EmployerRef = source.Reference;
                    break;
                case Party.Provider:
                    ProviderRef = source.Reference;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(creator), creator, null);
            }
        }

        public void Merge(DraftApprenticeshipDetails source, Party modifyingParty)
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

            switch (modifyingParty)
            {
                case Party.Employer:
                    EmployerRef = source.Reference;
                    break;

                case Party.Provider:
                    ProviderRef = source.Reference;
                    break;
            }
        }
    }
}