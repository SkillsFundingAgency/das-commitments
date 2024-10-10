using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace SFA.DAS.CommitmentsV2.Api.Authorization;

public class AuthorizeConventionFilter : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerName.Contains("provider"))
        {
            controller.Filters.Add(new AuthorizeFilter(Policies.Provider));
        }
        else if (controller.ControllerName.Contains("employer"))
        {
            controller.Filters.Add(new AuthorizeFilter(Policies.Employer));
        }
        else
        {
            controller.Filters.Add(new AuthorizeFilter());
        }
    }
}