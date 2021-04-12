using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class EditApprenticeshipExtension
    {
        public static ApprenticeshipUpdate MapToApprenticeshipUpdate(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party, DateTime utcDateTime)
        {
            bool apprenticeshipUpdateCreated = false;
            var apprenticeshipUpdate = new ApprenticeshipUpdate();

            if (command.CourseCode != apprenticeship.CourseCode)
            {
                apprenticeshipUpdate.TrainingCode = command.CourseCode;
                apprenticeshipUpdateCreated = true;
            }

            if (command.DateOfBirth != apprenticeship.DateOfBirth)
            {
                apprenticeshipUpdate.DateOfBirth = command.DateOfBirth;
                apprenticeshipUpdateCreated = true;
            }

            if (command.EndDate != apprenticeship.EndDate)
            {
                apprenticeship.EndDate = command.EndDate;
                apprenticeshipUpdateCreated = true;
            }

            if (command.StartDate != apprenticeship.StartDate)
            {
                apprenticeshipUpdate.StartDate = command.StartDate;
                apprenticeshipUpdateCreated = true;
            }

            if (command.LastName != apprenticeship.LastName)
            {
                apprenticeshipUpdate.LastName = command.LastName;
                apprenticeshipUpdateCreated = true;
            }

            if (command.FirstName != apprenticeship.FirstName)
            {
                apprenticeshipUpdate.FirstName = command.FirstName;
                apprenticeshipUpdateCreated = true;
            }

            if (command.Cost != apprenticeship.PriceHistory.GetPrice(utcDateTime))
            {
                apprenticeshipUpdate.FirstName = command.FirstName;
                apprenticeshipUpdateCreated = true;
            }

            if (apprenticeshipUpdateCreated)
            {
                apprenticeship.Id = apprenticeship.Id;
                apprenticeshipUpdate.Originator = party == Party.Employer ? Originator.Employer : Originator.Provider;
                apprenticeshipUpdate.UpdateOrigin = ApprenticeshipUpdateOrigin.ChangeOfCircumstances;
                apprenticeshipUpdate.EffectiveFromDate = apprenticeship.StartDate;
            }

            return apprenticeshipUpdate;
        }

        public static bool ApprenticeshipUpdateRequired(this ApprenticeshipUpdate apprenticeshipUpdate)
        {
            return !string.IsNullOrWhiteSpace(apprenticeshipUpdate.FirstName)
                || !string.IsNullOrWhiteSpace(apprenticeshipUpdate.LastName)
                || !string.IsNullOrWhiteSpace(apprenticeshipUpdate.TrainingCode)
                || apprenticeshipUpdate.DateOfBirth != null
                || apprenticeshipUpdate.StartDate != null
                || apprenticeshipUpdate.EndDate != null
                || apprenticeshipUpdate.Cost != null;
        }

        public static bool EmployerReferenceUpdateRequired(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
        {
            return apprenticeship.EmployerRef != command.EmployerReference && party == Party.Employer;
        }

        public static bool ProviderReferenceUpdateRequired(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
        {
            return apprenticeship.ProviderRef != command.ProviderReference && party == Party.Provider;
        }

        public static bool ULNUpdateRequired(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
        {
            return apprenticeship.Uln != command.ULN && party == Party.Provider; ;
        }
    }
}
