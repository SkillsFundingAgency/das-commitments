using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorResponse CreateErrorResponse(this ModelStateDictionary modelState)
        {
            var errors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ErrorDetail(key, x.ErrorMessage)))
                .ToList();

            return new ErrorResponse(errors);
        }
    }
}