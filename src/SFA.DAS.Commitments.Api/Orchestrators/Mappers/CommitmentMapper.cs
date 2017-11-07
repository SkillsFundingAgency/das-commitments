﻿using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class CommitmentMapper : ICommitmentMapper
    {
        private readonly ICommitmentRules _commitmentRules;

        public CommitmentMapper(ICommitmentRules commitmentRules)
        {
            _commitmentRules = commitmentRules;
        }

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

        public CommitmentView MapFrom(Commitment commitment, CallerType callerType)
        {
            return new CommitmentView
            {
                Id = commitment.Id,
                Reference = commitment.Reference,
                ProviderId = commitment.ProviderId,
                ProviderName = commitment.ProviderName,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                EditStatus = (Types.Commitment.Types.EditStatus)commitment.EditStatus,
                AgreementStatus = (AgreementStatus)_commitmentRules.DetermineAgreementStatus(commitment.Apprenticeships),
                LastAction = (Types.Commitment.Types.LastAction)commitment.LastAction,
                CanBeApproved = callerType == CallerType.Employer ? commitment.EmployerCanApproveCommitment : commitment.ProviderCanApproveCommitment,
                EmployerLastUpdateInfo = new LastUpdateInfo { Name = commitment.LastUpdatedByEmployerName, EmailAddress = commitment.LastUpdatedByEmployerEmail },
                ProviderLastUpdateInfo = new LastUpdateInfo { Name = commitment.LastUpdatedByProviderName, EmailAddress = commitment.LastUpdatedByProviderEmail },
                Apprenticeships = MapApprenticeshipsFrom(commitment.Apprenticeships, callerType),
                Messages = MapMessagesFrom(commitment.Messages)
            };
        }

        public Commitment MapFrom(Types.Commitment.Commitment commitment)
        {
            var domainCommitment = new Commitment
            {
                Reference = commitment.Reference,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityAddress = commitment.LegalEntityAddress,
                LegalEntityOrganisationType = (SFA.DAS.Common.Domain.Types.OrganisationType)commitment.LegalEntityOrganisationType,
                ProviderId = commitment.ProviderId,
                ProviderName = commitment.ProviderName,
                CommitmentStatus = (CommitmentStatus)commitment.CommitmentStatus,
                EditStatus = (EditStatus)commitment.EditStatus,
                LastAction = LastAction.None,
                LastUpdatedByEmployerName = commitment.EmployerLastUpdateInfo.Name,
                LastUpdatedByEmployerEmail = commitment.EmployerLastUpdateInfo.EmailAddress,
            };

            return domainCommitment;
        
    }

        //todo: could we reuse the apprenticeship mapper?       
        private static List<Types.Apprenticeship.Apprenticeship> MapApprenticeshipsFrom(List<Apprenticeship> apprenticeships, CallerType callerType)
        {
            return apprenticeships.Select(x => new Types.Apprenticeship.Apprenticeship
            {
                Id = x.Id,
                ULN = x.ULN,
                CommitmentId = x.CommitmentId,
                EmployerAccountId = x.EmployerAccountId,
                ProviderId = x.ProviderId,
                Reference = x.Reference,
                FirstName = x.FirstName,
                LastName = x.LastName,
                TrainingType = (Types.Apprenticeship.Types.TrainingType)x.TrainingType,
                TrainingCode = x.TrainingCode,
                TrainingName = x.TrainingName,
                Cost = x.Cost,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                AgreementStatus = (Types.AgreementStatus)x.AgreementStatus,
                PaymentStatus = (Types.Apprenticeship.Types.PaymentStatus)x.PaymentStatus,
                DateOfBirth = x.DateOfBirth,
                NINumber = x.NINumber,
                EmployerRef = x.EmployerRef,
                ProviderRef = x.ProviderRef,
                CanBeApproved = callerType == CallerType.Employer ? x.EmployerCanApproveApprenticeship : x.ProviderCanApproveApprenticeship
            }).ToList();
        }
    }
}