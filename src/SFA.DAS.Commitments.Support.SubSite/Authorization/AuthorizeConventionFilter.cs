using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace SFA.DAS.Commitments.Support.SubSite.Authorization
{
    public class AuthorizeConventionFilter : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            controller.Filters.Add(new AuthorizeFilter());
        }
    }
}