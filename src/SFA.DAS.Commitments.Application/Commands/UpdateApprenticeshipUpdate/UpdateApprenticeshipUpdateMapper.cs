using System;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate
{
    public class UpdateApprenticeshipUpdateMapper : IUpdateApprenticeshipUpdateMapper
    {
        public void ApplyUpdate(Apprenticeship ap, ApprenticeshipUpdate update)
        {
            Func<string, string, string> changedOrNull = (a, edit) =>
                a == edit ? a : edit;

            ap.FirstName = string.IsNullOrEmpty(update.FirstName)
                ? ap.FirstName
                : update.FirstName;

            ap.LastName = string.IsNullOrEmpty(update.LastName)
                ? ap.LastName
                : update.LastName;

            ap.TrainingType = update.TrainingType ?? ap.TrainingType;

            if (!string.IsNullOrEmpty(update.TrainingCode)
                && !string.IsNullOrEmpty(update.TrainingName))
            {
                ap.TrainingCode = update.TrainingCode;
                ap.TrainingName = update.TrainingName;
            }

            ap.DateOfBirth = update.DateOfBirth ?? ap.DateOfBirth;

            ap.Cost = update.Cost ?? ap.Cost;

            ap.StartDate = update.StartDate ?? ap.StartDate;
            ap.EndDate = update.EndDate ?? ap.EndDate;
        }
    }

    public interface IUpdateApprenticeshipUpdateMapper
    {
        void ApplyUpdate(Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate);
    }
}
