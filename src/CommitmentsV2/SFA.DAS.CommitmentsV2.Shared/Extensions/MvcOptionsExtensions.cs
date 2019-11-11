using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Shared.Filters;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions
{
    public static class MvcOptionsExtensions
    {
        public static void AddValidation(this MvcOptions mvcOptions)
        {
            mvcOptions.Filters.Add<DomainExceptionRedirectGetFilterAttribute>();
            mvcOptions.Filters.Add<ValidateModelStateFilter>(int.MaxValue);
        }
    }
}
