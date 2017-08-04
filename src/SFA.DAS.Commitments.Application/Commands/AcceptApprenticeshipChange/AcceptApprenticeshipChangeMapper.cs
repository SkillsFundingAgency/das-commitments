using SFA.DAS.Commitments.Domain.Entities;
using System;
using System.Linq;

namespace SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange
{
    public class AcceptApprenticeshipChangeMapper : IAcceptApprenticeshipChangeMapper
    {
        public void ApplyUpdate(Apprenticeship apprenticeship, ApprenticeshipUpdate update)
        {
            apprenticeship.FirstName = string.IsNullOrEmpty(update.FirstName)
                ? apprenticeship.FirstName
                : update.FirstName;

            apprenticeship.LastName = string.IsNullOrEmpty(update.LastName)
                ? apprenticeship.LastName
                : update.LastName;

            apprenticeship.TrainingType = update.TrainingType ?? apprenticeship.TrainingType;

            if (!string.IsNullOrEmpty(update.TrainingCode)
                && !string.IsNullOrEmpty(update.TrainingName))
            {
                apprenticeship.TrainingCode = update.TrainingCode;
                apprenticeship.TrainingName = update.TrainingName;
            }

            apprenticeship.DateOfBirth = update.DateOfBirth ?? apprenticeship.DateOfBirth;

            apprenticeship.StartDate = update.StartDate ?? apprenticeship.StartDate;
            apprenticeship.EndDate = update.EndDate ?? apprenticeship.EndDate;

            UpdatePrice(apprenticeship, update);

        }

        private static void UpdatePrice(Apprenticeship apprenticeship, ApprenticeshipUpdate update)
        {
            apprenticeship.Cost = update.Cost ?? apprenticeship.Cost;

            if (update.Cost.HasValue)
            {
                if (apprenticeship.PriceHistory.Count > 1)
                    throw new InvalidOperationException("Multiple Prices History Items not expected.");

                apprenticeship.PriceHistory.Single().Cost = update.Cost.Value;
            }
        }
    }

    public interface IAcceptApprenticeshipChangeMapper
    {
        void ApplyUpdate(Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate);
    }
}
