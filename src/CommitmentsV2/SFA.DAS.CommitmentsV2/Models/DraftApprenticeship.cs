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

        public DraftApprenticeship(DraftApprenticeshipDetails source, Party modifyingParty) : this()
        {
            Merge(source, modifyingParty);

            ReservationId = source.ReservationId;
        }

        public void Merge(DraftApprenticeshipDetails source, Party modifyingParty)
        {
            if (IsOtherPartyApprovalRequired(source))
            {
                AgreementStatus = AgreementStatus.NotAgreed;
            }

            FirstName = source.FirstName;
            LastName = source.LastName;
            if (modifyingParty == Party.Provider)
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
                case Party.Employer:
                    EmployerRef = source.Reference;
                    break;

                case Party.Provider:
                    ProviderRef = source.Reference;
                    break;
            }
        }

        private bool IsOtherPartyApprovalRequired(DraftApprenticeshipDetails update)
        {
            if (FirstName != update.FirstName) return true;
            if (LastName != update.LastName) return true;
            if (Cost != update.Cost) return true;
            if (StartDate != update.StartDate) return true;
            if (EndDate != update.EndDate) return true;
            if (DateOfBirth != update.DateOfBirth) return true;

            if (string.IsNullOrWhiteSpace(CourseCode))
            {
                if (update.TrainingProgramme != null) return true;
            }
            else
            {
                if (update.TrainingProgramme?.CourseCode != CourseCode) return true;
            }

            return false;
        }
    }
}