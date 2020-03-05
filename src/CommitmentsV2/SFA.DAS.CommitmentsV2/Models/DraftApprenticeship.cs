﻿using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class DraftApprenticeship : ApprenticeshipBase, ITrackableEntity
    {
        public bool IsCompleteForParty(Party party)
        {
            switch (party)
            {
                case Party.Employer: return IsCompleteForEmployer;
                case Party.Provider: return IsCompleteForProvider;
                default:
                    throw new InvalidOperationException($"Cannot determine completeness for Party {party}");
            }
        }

        private bool IsCompleteForEmployer => 
            FirstName != null &&
            LastName != null &&
            Cost != null &&
            StartDate != null &&
            EndDate != null &&
            CourseCode != null &&
            DateOfBirth != null;

        private bool IsCompleteForProvider => 
            FirstName != null &&
            LastName != null &&
            Uln != null &&
            Cost != null &&
            StartDate != null &&
            EndDate != null &&
            CourseCode != null &&
            DateOfBirth != null;
        
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

        private void CheckIsCompleteForEmployer()
        {
            if (!IsCompleteForEmployer)
            {
                throw new DomainException(nameof(IsCompleteForEmployer), "Draft apprenticeship must be complete for employer");
            }
        }

        private void CheckIsCompleteForProvider()
        {
            if (!IsCompleteForProvider)
            {
                throw new DomainException(nameof(IsCompleteForProvider), "Draft apprenticeship must be complete for provider");
            }
        }

        private void CheckIsEmployerOrProvider(Party party)
        {
            if (party != Party.Employer && party != Party.Provider)
            {
                throw new DomainException(nameof(party), $"Party must be {Party.Employer} or {Party.Provider}; {party} is not valid");
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
    }
}