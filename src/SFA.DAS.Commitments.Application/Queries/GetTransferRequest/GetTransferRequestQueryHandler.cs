using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequest
{
    public sealed class GetTransferRequestQueryHandler : IAsyncRequestHandler<GetTransferRequestRequest, GetTransferRequestResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetTransferRequestRequest> _validator;

        private readonly ICommitmentRules _commitmentRules;

        public GetTransferRequestQueryHandler(
            ICommitmentRepository commitmentRepository, 
            AbstractValidator<GetTransferRequestRequest> validator,
            ICommitmentRules commitmentRules)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _commitmentRules = commitmentRules;
        }

        public async Task<GetTransferRequestResponse> Handle(GetTransferRequestRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var transferRequest = await _commitmentRepository.GetTransferRequest(message.TransferRequestId);

            if (transferRequest == null)
            {
                return new GetTransferRequestResponse { Data = null };
            }

            CheckAuthorization(message, transferRequest);

            return new GetTransferRequestResponse
            {
                Data = transferRequest
            };
        }

        private static void CheckAuthorization(GetTransferRequestRequest message, Domain.Entities.TransferRequest transferRequest)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.TransferSender:
                    if (transferRequest.SendingEmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Transfer Sender {message.Caller.Id} is not authorised to access transfer request {message.TransferRequestId}, expected Sender {transferRequest.SendingEmployerAccountId}");
                    break;
                case CallerType.TransferReceiver:
                    if (transferRequest.ReceivingEmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Transfer Receiver {message.Caller.Id} is not authorised to access transfer request {message.TransferRequestId}, expected Receiver {transferRequest.ReceivingEmployerAccountId}");
                    break;
                default:
                   throw new UnauthorizedException($"Only transfer senders and receivers are allowed to access transfer requests");
            }
        }
    }
}
