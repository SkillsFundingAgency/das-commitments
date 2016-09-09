using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetUsers;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Data;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators
{
    public class HomeOrchestrator
    {
        private readonly IMediator _mediator;

        public HomeOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<SignInUserViewModel> GetUsers()
        {
            var actual = await _mediator.SendAsync(new GetUsersQueryRequest());

            return new SignInUserViewModel
            {
                AvailableUsers = actual.Users.Select(x =>
                                                new SignInUserModel
                                                {
                                                    Email = x.Email,
                                                    FirstName = x.FirstName,
                                                    LastName = x.LastName,
                                                    UserId = x.UserRef,
                                                    ProviderId = x.ProviderId
                                                }).ToList()
            };
        }
    }
}