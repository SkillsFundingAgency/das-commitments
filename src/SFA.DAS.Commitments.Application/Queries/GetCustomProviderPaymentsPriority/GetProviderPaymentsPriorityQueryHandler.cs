using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.ProviderPayment;
using SFA.DAS.Commitments.Domain.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityQueryHandler : IAsyncRequestHandler<GetProviderPaymentsPriorityRequest, GetProviderPaymentsPriorityResponse>
    {
        private readonly IProviderPaymentRepository _providerRepository;
        private readonly AbstractValidator<GetProviderPaymentsPriorityRequest> _validator;

        public GetProviderPaymentsPriorityQueryHandler(IProviderPaymentRepository providerRepository, AbstractValidator<GetProviderPaymentsPriorityRequest> validator)
        {
            if (providerRepository == null)
                throw new ArgumentNullException(nameof(providerRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _providerRepository = providerRepository;
            _validator = validator;
        }

        public async Task<GetProviderPaymentsPriorityResponse> Handle(GetProviderPaymentsPriorityRequest message)
        {
            _validator.ValidateAndThrow(message);

            var priorityItems = await _providerRepository.GetCustomProviderPaymentPriority(message.EmployerAccountId);

            return new GetProviderPaymentsPriorityResponse
            {
                Data = priorityItems.Select(x => new ProviderPaymentPriorityItem
                {
                    ProviderId = x.ProviderId,
                    ProviderName = x.ProviderName,
                    PriorityOrder = x.PriorityOrder
                }).ToList()
            };
        }
    }
}
