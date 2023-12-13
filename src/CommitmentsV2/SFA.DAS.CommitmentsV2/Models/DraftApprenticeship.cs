using System;
using System.Collections.Generic;
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
            (StartDate != null || ActualStartDate != null) &&
            EndDate != null &&
            CourseCode != null &&
            DateOfBirth != null &&
            (!apprenticeEmailRequired || Email != null || ContinuationOfId != null);


        private bool IsCompleteForProvider(bool apprenticeEmailRequired) => 
            IsCompleteForEmployer(apprenticeEmailRequired) &&
            Uln != null &&
            (!RecognisingPriorLearningStillNeedsToBeConsidered || !RecognisingPriorLearningExtendedStillNeedsToBeConsidered);

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
            else if (Uln != source.Uln)
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
            TrainingPrice = source.TrainingPrice;
            EndPointAssessmentPrice = source.EndPointAssessmentPrice;
            StartDate = source.StartDate;
            ActualStartDate = source.ActualStartDate;
            EndDate = source.EndDate;
            DateOfBirth = source.DateOfBirth;
            IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot;
            EmployerHasEditedCost = source.EmployerHasEditedCost;

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

            RecognisePriorLearning ??= source.RecognisePriorLearning;
            TrainingTotalHours ??= source.TrainingTotalHours;
            if (RecognisePriorLearning == true)
            {
                PriorLearning ??= new ApprenticeshipPriorLearning();
                PriorLearning.DurationReducedByHours ??= source.DurationReducedByHours;
                PriorLearning.IsDurationReducedByRpl ??= source.IsDurationReducedByRPL;
                PriorLearning.DurationReducedBy ??= source.DurationReducedBy;
                PriorLearning.PriceReducedBy ??= source.PriceReducedBy;
            }

            ClearPriorLearningWhenStartDateBeforeAug2022();
        }

        internal OverlappingTrainingDateRequest CreateOverlappingTrainingDateRequest(Party originatingParty, long previousApprenticeshipId, UserInfo userInfo, DateTime createdDate)
        {
            var overlap = new OverlappingTrainingDateRequest(this, previousApprenticeshipId, originatingParty, userInfo, createdDate);
            OverlappingTrainingDateRequests.Add(overlap);
            return overlap;
        }

        private void ClearPriorLearningWhenStartDateBeforeAug2022()
        {
            if (StartDate < Constants.RecognisePriorLearningBecomesRequiredOn)
            {
                RecognisePriorLearning = null;
                TrainingTotalHours = null;
                if (PriorLearning != null)
                {
                    PriorLearning.DurationReducedByHours = null;
                    PriorLearning.IsDurationReducedByRpl = null;
                    PriorLearning.DurationReducedBy = null;
                    PriorLearning.PriceReducedBy = null;
                }
            }
        }

        public bool IsOtherPartyApprovalRequiredForUpdate(DraftApprenticeshipDetails update, Party modifyingParty)
        {
            if (FirstName != update.FirstName) return true;
            if (LastName != update.LastName) return true;
            if (CostOrTotalPriceIsChanged(update, modifyingParty)) return true;
            if (StartDateIsChanged(update)) return true;
            if (EndDateIsChanged(update.EndDate)) return true;
            if (DateOfBirth != update.DateOfBirth) return true;
            if (DeliveryModel != update.DeliveryModel) return true;
            if (FlexibleEmployment?.EmploymentEndDate != update.EmploymentEndDate) return true;
            if (FlexibleEmployment?.EmploymentPrice != update.EmploymentPrice) return true;

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

        private bool StartDateIsChanged(DraftApprenticeshipDetails update)
        {
            if (IsNotTrue(IsOnFlexiPaymentPilot) && IsNotTrue(update.IsOnFlexiPaymentPilot) && StartDate != update.StartDate) return true;
            if (ActualStartDate.HasValue && update.ActualStartDate.HasValue && (ActualStartDate.Value.Year != update.ActualStartDate.Value.Year || ActualStartDate.Value.Month != update.ActualStartDate.Value.Month)) return true;
            if (update.ActualStartDate.HasValue && StartDate.HasValue && StartDateMonthOrYearIsChanged(update)) return true;
            if (update.StartDate.HasValue && ActualStartDate.HasValue && ActualStartDateMonthOrYearIsChanged(update)) return true;
            return false;
        }

        private bool EndDateIsChanged(DateTime? updatedEndDate)
        {
            if (updatedEndDate.HasValue != EndDate.HasValue) return true;
            if (updatedEndDate.HasValue && EndDate.HasValue && (updatedEndDate.Value.Month != EndDate.Value.Month || updatedEndDate.Value.Year != EndDate.Value.Year)) return true;
            return false;
        }

        private static bool IsNotTrue(bool? value) => !value.HasValue || !value.Value;

        private bool StartDateMonthOrYearIsChanged(DraftApprenticeshipDetails update)
        {
            return StartDate.Value.Month != update.ActualStartDate.Value.Month || StartDate.Value.Year != update.ActualStartDate.Value.Year;
        }

        private bool ActualStartDateMonthOrYearIsChanged(DraftApprenticeshipDetails update)
        {
            return ActualStartDate.Value.Month != update.StartDate.Value.Month || ActualStartDate.Value.Year != update.StartDate.Value.Year;
        }

        private bool CostOrTotalPriceIsChanged(DraftApprenticeshipDetails update, Party modifyingParty)
        {
            if (update.IsOnFlexiPaymentPilot.GetValueOrDefault() && modifyingParty == Party.Provider)
                return Cost != (update.TrainingPrice + update.EndPointAssessmentPrice);

            return Cost != update.Cost;
        }

        public void ValidateUpdateForChangeOfParty(DraftApprenticeshipDetails update)
        {
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
                TrainingTotalHours = null;
                PriorLearning.DurationReducedByHours = null;
                PriorLearning.IsDurationReducedByRpl = null;
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
            if (durationReducedBy.Value < 0)
            {
                throw new DomainException("ReducedDuration", "The number can't be negative");
            }
            if (durationReducedBy.Value > 999)
            {
                throw new DomainException("ReducedDuration", "The number of weeks must be 999 or less");
            }
            if (!priceReducedBy.HasValue)
            {
                throw new DomainException("ReducedPrice", "You must enter the price");
            }
            if (priceReducedBy.Value < 0)
            {
                throw new DomainException("ReducedPrice", "The number can't be negative");
            }
            if (priceReducedBy.Value > Constants.MaximumApprenticeshipCost)
            {
                throw new DomainException("ReducedPrice", "The price must be 100,000 or less");
            }

            if (RecognisePriorLearning != true)
            {
                throw new DomainException(nameof(RecognisePriorLearning), "Prior learning details can only be set after the apprentice has recognised prior learning");
            }

            PriorLearning ??= new ApprenticeshipPriorLearning();
            PriorLearning.DurationReducedBy = durationReducedBy;
            PriorLearning.PriceReducedBy = priceReducedBy;

            PriorLearning.DurationReducedByHours = null;
        }

        public void SetPriorLearningDetailsExtended(int? durationReducedByHours, int? priceReduction)
        {

            if (RecognisePriorLearning != true)
            {
                throw new DomainException(nameof(RecognisePriorLearning), "Prior learning details can only be set after the apprentice has recognised prior learning");
            }

            var errors = ValidateDraftApprenticeshipRplExtendedDetails(durationReducedByHours, priceReduction);
            errors.ThrowIfAny();

            PriorLearning ??= new ApprenticeshipPriorLearning();
            PriorLearning.DurationReducedByHours = durationReducedByHours;
            PriorLearning.PriceReducedBy = priceReduction;

            PriorLearning.DurationReducedBy = null;
        }

        private List<DomainError> ValidateDraftApprenticeshipRplExtendedDetails(int? durationReducedByHours, int? priceReduction)
        {
            var errors = new List<DomainError>();

            if (!durationReducedByHours.HasValue)
            {
                errors.Add(new DomainError("DurationReducedByHours", "You must enter the number of hours"));
            }
            else if (durationReducedByHours.Value < 0)
            {
                errors.Add(new DomainError("DurationReducedByHours", "The number can't be negative"));
            }
            else if (durationReducedByHours.Value > 999)
            {
                errors.Add(new DomainError("DurationReducedByHours", "The number of hours must be 999 or less"));
            }

            if (!priceReduction.HasValue)
            {
                errors.Add(new DomainError("ReducedPrice", "You must enter a price"));
            }
            else if (priceReduction.Value < 0)
            {
                errors.Add(new DomainError("ReducedPrice", "The price can't be negative"));
            }
            else if (priceReduction.Value > Constants.MaximumApprenticeshipCost)
            {
                errors.Add(new DomainError("ReducedPrice", "The price must be 100,000 or less"));
            }

            return errors;
        }

        public void SetPriorLearningData(int? trainingTotalHours, int? durationReducedByHours, bool? isDurationReducedByRpl, int? durationReducedBy, int? priceReduced, int minimumPriceReduction, int maximumTrainingTimeReduction)
        {

            if (RecognisePriorLearning != true)
            {
                throw new DomainException(nameof(RecognisePriorLearning), "Prior learning details can only be set after the apprentice has recognised prior learning");
            }

            var errors = ValidateDraftApprenticeshipRplData(trainingTotalHours, durationReducedByHours, isDurationReducedByRpl, durationReducedBy, priceReduced, minimumPriceReduction, maximumTrainingTimeReduction);
            errors.ThrowIfAny();

            PriorLearning ??= new ApprenticeshipPriorLearning();

            PriorLearning.DurationReducedByHours = durationReducedByHours;
            PriorLearning.IsDurationReducedByRpl = isDurationReducedByRpl;
            PriorLearning.DurationReducedBy = durationReducedBy;
            PriorLearning.PriceReducedBy = priceReduced;
            TrainingTotalHours = trainingTotalHours;

            if (isDurationReducedByRpl == false)
            {
                PriorLearning.DurationReducedBy = null;
            }
        }

        private List<DomainError> ValidateDraftApprenticeshipRplData(int? trainingTotalHours, int? durationReducedByHours, bool? isDurationReducedByRpl, int? durationReducedBy, int? priceReduced, int minimumPriceReduction, int maximumTrainingTimeReduction)
        {
            void CheckPriceReduced(List<DomainError> list1)
            {
                if (priceReduced.Value < minimumPriceReduction)
                {
                    list1.Add(new DomainError("priceReduced",
                        $"Total price reduction due to RPL must be {minimumPriceReduction} pounds or more"));
                }
                else if (priceReduced.Value > 18000)
                {
                    list1.Add(
                        new DomainError("priceReduced", "Total price reduction due to RPL must be 18,000 or less"));
                }
            }

            void CheckReductionReducedByIsValid(List<DomainError> domainErrors1)
            {
                if (durationReducedBy.Value < 1)
                {
                    domainErrors1.Add(new DomainError("durationReducedBy",
                        "Reduction in duration must be 1 week or more"));
                }
                else if (durationReducedBy.Value > 260)
                {
                    domainErrors1.Add(new DomainError("durationReducedBy",
                        "Reduction in duration must be 260 weeks or less"));
                }
            }

            void CheckHoursReductionIsSensible(List<DomainError> errors1)
            {
                if (trainingTotalHours < durationReducedByHours)
                {
                    errors1.Add(new DomainError("DurationReducedByHours",
                        "Total reduction in off-the-job training time due to RPL must be lower than the total off-the-job training time for this apprenticeship standard"));
                }
                else if (trainingTotalHours - durationReducedByHours < 278)
                {
                    errors1.Add(new DomainError("DurationReducedByHours",
                        "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the RPL reduction is too high"));
                }
            }

            void CheckDurationReducedByHours(List<DomainError> list)
            {
                if (durationReducedByHours.Value < 1)
                {
                    list.Add(new DomainError("DurationReducedByHours",
                        $"Total reduction in off-the-job training time due to RPL must be a number between 1 and {maximumTrainingTimeReduction}"));
                }
                else if (durationReducedByHours.Value > maximumTrainingTimeReduction)
                {
                    list.Add(new DomainError("DurationReducedByHours",
                        $"Total reduction in off-the-job training time due to RPL must be {maximumTrainingTimeReduction} hours or less"));
                }
            }

            void CheckTrainingTotalHours(List<DomainError> domainErrors)
            {
                if (trainingTotalHours.Value < 278)
                {
                    domainErrors.Add(new DomainError("trainingTotalHours",
                        "Total off-the-job training time for this apprenticeship standard must be 278 hours or more"));
                }
                else if (trainingTotalHours.Value > 9999)
                {
                    domainErrors.Add(new DomainError("trainingTotalHours",
                        "Total off-the-job training time for this apprenticeship standard must be 9,999 hours or less"));
                }
            }

            var errors = new List<DomainError>();

            if (trainingTotalHours.HasValue)
            {
                CheckTrainingTotalHours(errors);
            }

            if (durationReducedByHours.HasValue)
            {
                CheckDurationReducedByHours(errors);
            }

            if (trainingTotalHours.HasValue && durationReducedByHours.HasValue)
            {
                CheckHoursReductionIsSensible(errors);
            }

            if (isDurationReducedByRpl == true && durationReducedBy.HasValue)
            {
                CheckReductionReducedByIsValid(errors);
            }
            else if (isDurationReducedByRpl == false && durationReducedBy.HasValue)
            {
                errors.Add(new DomainError("isDurationReducedByRpl", "Reduction in duration should not have a value"));
            }

            if (priceReduced.HasValue)
            {
                CheckPriceReduced(errors);
            }

            return errors;
        }

        public bool HasEmployerChangedCostWhereProviderHasSetTotalAndEPAPrice(DraftApprenticeshipDetails update, Party modifyingParty)
        {
            if (modifyingParty != Party.Employer)
                return false;

            if (!IsOnFlexiPaymentPilot.GetValueOrDefault())
                return false;

            return Cost != update.Cost;
        }
    }
}