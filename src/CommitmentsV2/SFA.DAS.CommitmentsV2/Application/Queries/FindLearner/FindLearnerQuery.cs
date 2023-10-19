using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.FindLearner
{
    public class FindLearnerQuery : IRequest<FindLearnerQueryResult>
    {
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }

        public FindLearnerQuery(string firstName, string lastName, DateTime dateOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
        }
    }
}
