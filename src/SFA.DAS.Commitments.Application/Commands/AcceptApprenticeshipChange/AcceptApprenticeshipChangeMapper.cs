using SFA.DAS.Commitments.Domain.Entities;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange
{
    public class AcceptApprenticeshipChangeMapper : IAcceptApprenticeshipChangeMapper
    {
        private readonly ICurrentDateTime _currentDateTime;

        public AcceptApprenticeshipChangeMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

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

        private void UpdatePrice(Apprenticeship apprenticeship, ApprenticeshipUpdate update)
        {
            if (update.Cost.HasValue)
            {
                if (apprenticeship.PriceHistory.Count != 1)
                    throw new InvalidOperationException("Multiple Prices History Items not expected.");

                apprenticeship.Cost = update.Cost.Value;
                apprenticeship.PriceHistory[0].Cost = update.Cost.Value;
            }

            if (update.StartDate.HasValue)
            {
                if(apprenticeship.PriceHistory.Count != 1)
                    throw new InvalidOperationException("Multiple Prices History Items not expected.");

                apprenticeship.PriceHistory[0].FromDate = update.StartDate ?? apprenticeship.PriceHistory.Single().FromDate;
            }
        }
    }

    public interface IAcceptApprenticeshipChangeMapper
    {
        void ApplyUpdate(Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate);
    }
}
