using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Shared.Filters;
using SFA.DAS.CommitmentsV2.Shared.ModelBinding;
using SFA.DAS.Validation.Mvc.Filters;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions;

public static class MvcOptionsExtensions
{
    public static void AddValidation(this MvcOptions mvcOptions)
    {
        mvcOptions.Filters.Add<DomainExceptionRedirectGetFilterAttribute>();
        mvcOptions.Filters.Add<ValidateModelStateFilter>(int.MaxValue);
    }

    public static void AddStringModelBinderProvider(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new StringModelBinderProvider());
    }
}