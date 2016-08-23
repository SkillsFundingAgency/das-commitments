using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Data;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetUsers
{
    public class GetUsersQueryHandler : IAsyncRequestHandler<GetUsersQueryRequest, GetUsersQueryResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            if (userRepository == null)
                throw new ArgumentNullException(nameof(userRepository));
            _userRepository = userRepository;
        }

        public async Task<GetUsersQueryResponse> Handle(GetUsersQueryRequest message)
        {
            var users = await _userRepository.GetAllUsers();

            return new GetUsersQueryResponse
            {
                Users = users.UserList
            };
        }
    }
}