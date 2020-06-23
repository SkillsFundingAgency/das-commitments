using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.CommitmentsV2.Shared.ModelBinders
{
    public class ErrorSuppressModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var converter = TypeDescriptor.GetConverter(bindingContext.ModelType);

            try
            {
                var result = converter.ConvertFrom(valueResult.FirstValue);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (Exception)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
