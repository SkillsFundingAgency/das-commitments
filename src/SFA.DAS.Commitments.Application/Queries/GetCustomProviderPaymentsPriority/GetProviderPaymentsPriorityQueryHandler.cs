using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityQueryHandler : IAsyncRequestHandler<GetProviderPaymentsPriorityRequest, GetProviderPaymentsPriorityResponse>
    {
        private readonly IProviderPaymentRepository _providerRepository;

        public GetProviderPaymentsPriorityQueryHandler(IProviderPaymentRepository providerRepository)
        {
            if (providerRepository == null)
                throw new ArgumentNullException(nameof(providerRepository));

            _providerRepository = providerRepository;
        }

        public async Task<GetProviderPaymentsPriorityResponse> Handle(GetProviderPaymentsPriorityRequest message)
        {
            // TODO: LWA - Extract into validator
            if (message == null || message.EmployerAccountId == 0)
                throw new ValidationException("A valid EmployerAccountId must be set");

            var priorityItems = await _providerRepository.GetCustomProviderPaymentPriority(message.EmployerAccountId);

            return new GetProviderPaymentsPriorityResponse
            {
                // TODO: LWA - Map to Api Types
                Data = priorityItems.Select(x => new object()).ToList()
            };
        }
    }
}
