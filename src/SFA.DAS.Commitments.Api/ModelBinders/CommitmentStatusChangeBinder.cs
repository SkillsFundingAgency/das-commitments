using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.ModelBinders
{
    public class CommitmentStatusChangeBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(CommitmentStatusChange))
            {
                return false;
            }

            var val = bindingContext.ValueProvider.GetValue(
            bindingContext.ModelName);
            if (val == null)
            {
                return false;
            }

            var key = val.RawValue as string;
            if (key == null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Wrong value type");
                return false;
            }

            bindingContext.Model = new CommitmentStatusChange();
            return true;
        }
    }
}