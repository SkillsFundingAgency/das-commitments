using System;
using System.Threading;
using SFA.DAS.CommitmentsV2.Domain;
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
            if (source.DeliveryModel.HasValue)
            {
                DeliveryModel = source.DeliveryModel.Value;
            }

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

            if (source.DeliveryModel == Types.DeliveryModel.PortableFlexiJob)
            {
                FlexibleEmployment ??= new FlexibleEmployment();
                FlexibleEmployment.EmploymentEndDate = source.EmploymentEndDate;
                FlexibleEmployment.EmploymentPrice = source.EmploymentPrice;
            }
            else if (FlexibleEmployment != null)
            {
                FlexibleEmployment.EmploymentPrice = null;
                FlexibleEmployment.EmploymentEndDate = null;
            }

            RecognisePriorLearning = source.RecognisePriorLearning;
            if(RecognisePriorLearning == true)
            {
                PriorLearning ??= new ApprenticeshipPriorLearning();
                PriorLearning.DurationReducedBy = source.DurationReducedBy;
                PriorLearning.PriceReducedBy = source.PriceReducedBy;
            }
            
            ClearPriorLearningWhenStartDateBeforeAug2022();
        }

        internal OverlappingTrainingDateRequest CreateOverlappingTrainingDateRequest(Party originatingParty, long previousApprenticeshipId, UserInfo userInfo)
        {
            var overlap = new OverlappingTrainingDateRequest(this, previousApprenticeshipId, originatingParty, userInfo);
            OverlappingTrainingDateRequests.Add(overlap);
            return overlap;
        }

        private void ClearPriorLearningWhenStartDateBeforeAug2022()
        {
            if (StartDate < Constants.RecognisePriorLearningBecomesRequiredOn)
            {
                RecognisePriorLearning = null;
                if (PriorLearning != null)
                {
                    PriorLearning.DurationReducedBy = null;
                    PriorLearning.PriceReducedBy = null;
                }
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

        public void SetRecognisePriorLearning(bool? recognisePriorLearning)
        {
            if (!recognisePriorLearning.HasValue)
            {
                throw new DomainException("IsTherePriorLearning", "You must select yes or no");
            }

            RecognisePriorLearning = recognisePriorLearning;

            if (RecognisePriorLearning == false && PriorLearning != null)
            {
                PriorLearning.DurationReducedBy = null;
                PriorLearning.PriceReducedBy = null;
            }
        }

        public void SetPriorLearningDetails(int? durationReducedBy, int? priceReducedBy)
        {
            if (!durationReducedBy.HasValue)
            {
                throw new DomainException("ReducedDuration", "You must enter the number of weeks");
            }
            if (durationReducedBy.HasValue && durationReducedBy.Value < 0)
            {
                throw new DomainException("ReducedDuration", "The number can't be negative");
            }
            if (!priceReducedBy.HasValue)
            {
                throw new DomainException("ReducedPrice", "You must enter the price");
            }
            if (priceReducedBy.HasValue && priceReducedBy.Value < 0)
            {
                throw new DomainException("ReducedPrice", "The number can't be negative");
            }
            if (RecognisePriorLearning != true)
            {
                throw new DomainException(nameof(RecognisePriorLearning), "Prior learning details can only be set after the apprentice has recognised prior learning");
            }

            PriorLearning ??= new ApprenticeshipPriorLearning();
            PriorLearning.DurationReducedBy = durationReducedBy;
            PriorLearning.PriceReducedBy = priceReducedBy;
        }
    }
}