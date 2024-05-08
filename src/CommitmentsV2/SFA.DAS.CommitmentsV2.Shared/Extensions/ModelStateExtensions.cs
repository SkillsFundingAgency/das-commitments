using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddModelExceptionErrors(this ModelStateDictionary modelState, CommitmentsApiModelException exception, Func<string, string> fieldNameMapper = null)
        {
            if(exception?.Errors == null)
            {
                return;
            }

            foreach (var error in exception.Errors)
            {
                var field = fieldNameMapper == null ? error.Field : fieldNameMapper(error.Field);

                modelState.AddModelError(field, error.Message);
            }
        }
    }
}
