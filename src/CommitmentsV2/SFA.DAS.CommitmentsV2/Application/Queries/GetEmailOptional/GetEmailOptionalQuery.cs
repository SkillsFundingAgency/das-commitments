using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional
{
    public class GetEmailOptionalQuery : IRequest<bool>
    {
        public long? EmployerId { get; }
        public long? ProviderId { get; }

        public GetEmailOptionalQuery(long employerId, long providerId)
            => (EmployerId, ProviderId) = (employerId, providerId);
    }
}
