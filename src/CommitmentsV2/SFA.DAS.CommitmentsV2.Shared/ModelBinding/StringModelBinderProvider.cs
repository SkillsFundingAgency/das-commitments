using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.CommitmentsV2.Shared.ModelBinding;

public class StringModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.IsComplexType || context.Metadata.ModelType != typeof(string))
        {
            return null;
        }

        var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
        return new StringModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory));
    }
}