using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate
{
    public class GetApprenticeshipsValidateQuery : IRequest<GetApprenticeshipsValidateQueryResult>
    {
        public string LastName { get; }
        public DateTime DateOfBirth { get; }
        public string Email { get; }

        public GetApprenticeshipsValidateQuery(string lastName, DateTime dateOfBirth, string email)
        {
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Email = email;
        }
    }
}
