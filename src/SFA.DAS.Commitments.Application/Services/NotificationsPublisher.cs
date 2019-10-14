using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Application.Services
{
    public class NotificationsPublisher : INotificationsPublisher
    {
        public const string AmendedTemplate = "EmployerCohortNotification";
        public const string ApprovedTemplate = "EmployerCohortApproved";
        public const string ApprovedWithTransferTemplate = "EmployerTransferPendingFinalApproval";
        private readonly IEndpointInstance _endpointInstance;
        private readonly ICommitmentsLogger _logger;
        private readonly IEncodingService _encodingService;

        public NotificationsPublisher(IEndpointInstance endpointInstance, ICommitmentsLogger logger, IEncodingService encodingService)
        {
            _endpointInstance = endpointInstance;
            _logger = logger;
            _encodingService = encodingService;
        }

        public Task ProviderAmendedCohort(Commitment commitment)
        {
            var tokens = CreateDictionaryWithCommonTokens(commitment);
            tokens["provider_name"] = commitment.ProviderName;
            tokens["employer_hashed_account"] = _encodingService.Encode(commitment.EmployerAccountId, EncodingType.AccountId);

            var command = new SendEmailToEmployerCommand(commitment.EmployerAccountId, AmendedTemplate, tokens, commitment.LastUpdatedByEmployerEmail);

            return SendCommandAndLog(command,$"Provider: {commitment.ProviderId} CohortId: {commitment.Id} LastAction: {commitment.LastAction}");
        }

        public Task ProviderApprovedCohort(Commitment commitment)
        {
            SendEmailToEmployerCommand command = null;
            var tokens = CreateDictionaryWithCommonTokens(commitment);

            if (commitment.HasTransferSenderAssigned)
            {
                tokens["sender_name"] = commitment.TransferSenderName;
                tokens["provider_name"] = commitment.ProviderName;
                tokens["employer_hashed_account"] = _encodingService.Encode(commitment.EmployerAccountId, EncodingType.AccountId);
                command = new SendEmailToEmployerCommand(commitment.EmployerAccountId, ApprovedWithTransferTemplate, tokens, commitment.LastUpdatedByEmployerEmail);
            }
            else
            {
                command = new SendEmailToEmployerCommand(commitment.EmployerAccountId, ApprovedTemplate, tokens, commitment.LastUpdatedByEmployerEmail);
            }

            return SendCommandAndLog(command, $"Provider: {commitment.ProviderId} CohortId: {commitment.Id} LastAction: {commitment.LastAction}");
        }

        private Dictionary<string, string> CreateDictionaryWithCommonTokens(Commitment commitment)
        {
            return new Dictionary<string, string>
            {
                {"type", commitment.LastAction == LastAction.Approve ? "approval" : "review"},
                {"cohort_reference", commitment.Reference}
            };
        }

        private async Task SendCommandAndLog<TCommand>(TCommand @event, string message) where TCommand : class
        {
            var logMessage = $"Send {typeof(TCommand).Name} message. {message}";
            try
            {
                await _endpointInstance.Send(@event);
                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
                throw;
            }
        }

    }
}
