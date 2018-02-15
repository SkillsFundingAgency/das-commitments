using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class CohortStatusChangeService
    {
        private readonly ICommitmentRepository _commitmentRepository;

        internal CohortStatusChangeService(ICommitmentRepository commitmentRepository)
        {
            _commitmentRepository = commitmentRepository;
        }

        internal async Task AddMessageToCommitment(Commitment commitment, string lastUpdatedByName, string messageText)
        {
            var message = new Message
            {
                Author = lastUpdatedByName,
                Text = messageText ?? string.Empty,
                CreatedBy = CallerType.Employer
            };
            commitment.Messages.Add(message);
            await _commitmentRepository.SaveMessage(commitment.Id, commitment.Messages.Last());
        }
    }
}
