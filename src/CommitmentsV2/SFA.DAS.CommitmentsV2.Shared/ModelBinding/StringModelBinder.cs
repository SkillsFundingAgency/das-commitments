using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.CommitmentsV2.Shared.ModelBinding;

public class StringModelBinder(IModelBinder fallbackBinder) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        if (bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Length != 1)
        {
            return fallbackBinder.BindModelAsync(bindingContext);
        }

        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (value != null && !string.IsNullOrEmpty(value))
        {
            value = value.Replace("\t", " ").Trim();

            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }

        return fallbackBinder.BindModelAsync(bindingContext);
    }
}