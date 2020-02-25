using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;

namespace System.Web.Mvc.Html.PrimeVue
{
    public partial class PrimeVueHtmlHelper<TRequestModel>
    {
        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression)
        {
            return InputTextFor(expression, (string)null);
        }

        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression, string format)
        {
            return InputTextFor(expression, format, null);
        }

        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression, object htmlAttributes)
        {
            return InputTextFor(expression, null, htmlAttributes);
        }

        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression, string format, object htmlAttributes)
        {
            return InputTextFor(expression, format, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return InputTextFor(expression, null, htmlAttributes);
        }

        public MvcHtmlString InputTextFor<TProperty>(Expression<Func<TRequestModel, TProperty>> expression, string format, IDictionary<string, object> htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, _viewData);
            return InputTextHelper(modelMetadata, modelMetadata.Model, ExpressionHelper.GetExpressionText(expression), format, htmlAttributes);
        }

        private MvcHtmlString InputTextHelper(ModelMetadata metadata, object model, string expression, string format, IDictionary<string, object> htmlAttributes)
        {
            return InputHelper(metadata, expression, true, format, htmlAttributes);
        }

        private MvcHtmlString InputHelper(ModelMetadata metadata, string name, bool setId, string format, IDictionary<string, object> htmlAttributes)
        {
            var fullHtmlFieldName = _htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (string.IsNullOrEmpty(fullHtmlFieldName))
            {
                throw new ArgumentException(nameof(name));
            }
            var tagBuilder = new TagBuilder("pvl-inputtext");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, true);

            if (setId)
            {
                tagBuilder.GenerateId(fullHtmlFieldName);
            }

            if (_htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out ModelState modelState) && modelState.Errors.Count > 0)
            {
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            tagBuilder.MergeAttributes(_htmlHelper.GetUnobtrusiveValidationAttributes(name, metadata));
            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }
    }
}
