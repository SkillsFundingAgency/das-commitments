using FluentValidation.Attributes;
using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln
{
    public class GetApprenticeshipsByUlnRequest: IAsyncRequest<GetApprenticeshipsByUlnResponse>
    {
       public string Uln { get; set; }
        public long EmployerAccountId { get; set; }
    }
}
