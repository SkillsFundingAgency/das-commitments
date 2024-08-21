using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString AddClassIfPropertyInError<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            string errorClass)
        {
            string GetFieldName()
            {
                var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
                    .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

                if (expressionProvider?.CreateModelExpression(htmlHelper.ViewContext.ViewBag, expression) is ModelExpression modelExpression)
                {
                    return modelExpression.Name;
                }

                return null;
            }
            var expressionText = GetFieldName();
            var fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expressionText);
            var state = htmlHelper.ViewData.ModelState[fullHtmlFieldName];

            if (state?.Errors == null || state.Errors.Count == 0)
            {
                return HtmlString.Empty;
            }

            return new HtmlString(errorClass);
        }
    }
}
