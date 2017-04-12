using System;

using Newtonsoft.Json;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate
{
    public class UpdateApprenticeshipUpdateMapper : IUpdateApprenticeshipUpdateMapper
    {
        public Apprenticeship ApplyUpdate(Apprenticeship oldApprenticeship, ApprenticeshipUpdate update)
        {

            var json = JsonConvert.SerializeObject(oldApprenticeship);
            var ap = JsonConvert.DeserializeObject<Apprenticeship>(json);

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

            return ap;
        }
    }

    public interface IUpdateApprenticeshipUpdateMapper
    {
        Apprenticeship ApplyUpdate(Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate);
    }
}
