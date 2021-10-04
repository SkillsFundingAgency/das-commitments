using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class DraftApprenticeship : ApprenticeshipBase, ITrackableEntity
    {
        public bool IsCompleteForParty(Party party, bool apprenticeEmailRequired)
        {
            switch (party)
            {
                case Party.Employer: return IsCompleteForEmployer(apprenticeEmailRequired);
                case Party.Provider: return IsCompleteForProvider(apprenticeEmailRequired);
                default:
                    throw new InvalidOperationException($"Cannot determine completeness for Party {party}");
            }
        }

        private bool IsCompleteForEmployer(bool apprenticeEmailRequired) => 
            FirstName != null &&
            LastName != null &&
            Cost != null &&
            StartDate != null &&
            EndDate != null &&
            CourseCode != null &&
            DateOfBirth != null &&
            (!apprenticeEmailRequired || Email != null || ContinuationOfId != null);

        private bool IsCompleteForProvider(bool apprenticeEmailRequired) => 
            FirstName != null &&
            LastName != null &&
            Uln != null &&
            Cost != null &&
            StartDate != null &&
            EndDate != null &&
            CourseCode != null &&
            DateOfBirth != null &&
            (!apprenticeEmailRequired || Email != null || ContinuationOfId != null);

        public DraftApprenticeship()
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
            var selectedOption = StandardUId == source.StandardUId ? source.TrainingCourseOption : null;

            FirstName = source.FirstName;
            LastName = source.LastName;
            Email = source.Email;
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
            TrainingCourseVersion = source.TrainingCourseVersion;
            TrainingCourseVersionConfirmed = source.TrainingCourseVersionConfirmed;
            TrainingCourseOption = selectedOption;
            StandardUId = source.StandardUId;
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

        public bool IsOtherPartyApprovalRequiredForUpdate(DraftApprenticeshipDetails update)
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

        public void ValidateUpdateForChangeOfParty(DraftApprenticeshipDetails update)
        {
            if (update.FirstName != FirstName)
            {
                throw new DomainException(nameof(FirstName), "FirstName for DraftApprenticeship in ChangeOfPartyCohort cannot be modified");
            }

            if (update.LastName != LastName)
            {
                throw new DomainException(nameof(LastName), "LastName for DraftApprenticeship in ChangeOfPartyCohort cannot be modified");
            }

            if (update.DateOfBirth != DateOfBirth)
            {
                throw new DomainException(nameof(LastName), "DateOfBirth for DraftApprenticeship in ChangeOfPartyCohort cannot be modified");
            }

            if (update.TrainingProgramme?.CourseCode != CourseCode)
            {
                throw new DomainException(nameof(LastName), "CourseCode for DraftApprenticeship in ChangeOfPartyCohort cannot be modified");
            }
        }
    }
}