using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class CommitmentMapper : ICommitmentMapper
    {
        public CommitmentListItem MapFrom(CommitmentSummary source, CallerType callerType)
        {
            return new CommitmentListItem
            {
                Id = source.Id,
                Reference = source.Reference,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                EmployerAccountId = source.EmployerAccountId,
                LegalEntityId = source.LegalEntityId,
                LegalEntityName = source.LegalEntityName,
                CommitmentStatus = (Types.Commitment.Types.CommitmentStatus) source.CommitmentStatus,
                EditStatus = (Types.Commitment.Types.EditStatus) source.EditStatus,
                ApprenticeshipCount = source.ApprenticeshipCount,
                AgreementStatus = (Types.AgreementStatus) source.AgreementStatus,
                LastAction = (Types.Commitment.Types.LastAction) source.LastAction,
                CanBeApproved = callerType == CallerType.Employer
                        ? source.EmployerCanApproveCommitment
                        : source.ProviderCanApproveCommitment,
                EmployerLastUpdateInfo =
                    new LastUpdateInfo {Name = source.LastUpdatedByEmployerName, EmailAddress = source.LastUpdatedByEmployerEmail},
                ProviderLastUpdateInfo =
                    new LastUpdateInfo {Name = source.LastUpdatedByProviderName, EmailAddress = source.LastUpdatedByProviderEmail},
                Messages = MapMessagesFrom(source.Messages)
            };
        }

        public IEnumerable<CommitmentListItem> MapFrom(IEnumerable<CommitmentSummary> source, CallerType callerType)
        {
            return source.Select(x => MapFrom(x, callerType));
        }

        private List<MessageView> MapMessagesFrom(List<Message> messages)
        {
            return messages.Select(x => new MessageView
            {
                Message = x.Text,
                Author = x.Author,
                CreatedBy = x.CreatedBy == CallerType.Employer ? MessageCreator.Employer : MessageCreator.Provider,
                CreatedDateTime = x.CreatedDateTime
            }).ToList();
        }
    }
}