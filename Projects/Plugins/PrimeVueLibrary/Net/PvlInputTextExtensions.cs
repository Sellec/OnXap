using System.Collections.Generic;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace System.Web.Mvc.Html
{
    /// <summary>
    /// Properties for button component.
    /// </summary>
    public class PvlInputTextProperties
    {
        /// <summary>
        /// Value of the component. 
        /// Required: yes. Type: any. Default: null.
        /// </summary>
        public VueBinding Value { get; set; }

        /// <summary>
        /// When present, it specifies that the component should be disabled.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute Disabled { get; set; }

    }

    /// <summary>
    /// </summary>
    public static class PvlInputTextExtensions
    {
        private static readonly Dictionary<string, string> _editorDataTypes = new Dictionary<string, string>()
        {
            { DataType.PhoneNumber.ToString(), "tel" },
            { DataType.Url.ToString(), "url" },
            { DataType.EmailAddress.ToString(), "email" },
            { DataType.DateTime.ToString(), "datetime" },
            { DataType.Date.ToString(), "date" },
            { DataType.Time.ToString(), "time" }
        };

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/inputtext" />.
        /// </summary>
        public static MvcHtmlString PvlInputTextFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlInputTextProperties pvlInputTextProperties)
        {
            return PvlInputTextFor(htmlHelper, expression, pvlInputTextProperties, null);
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/inputtext" />.
        /// </summary>
        public static MvcHtmlString PvlInputTextFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlInputTextProperties pvlInputTextProperties, object htmlAttributes)
        {
            return PvlInputTextFor(htmlHelper, expression, pvlInputTextProperties, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/inputtext" />.
        /// </summary>
        public static MvcHtmlString PvlInputTextFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlInputTextProperties pvlInputTextProperties, IDictionary<string, object> htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            return PvlInputTextHelper(htmlHelper, pvlInputTextProperties, modelMetadata, ExpressionHelper.GetExpressionText(expression), true, htmlAttributes);
        }

        private static MvcHtmlString PvlInputTextHelper<TModel>(HtmlHelper<TModel> htmlHelper, PvlInputTextProperties pvlInputTextProperties, ModelMetadata metadata, string name, bool setId, IDictionary<string, object> htmlAttributes)
        {
            var fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (string.IsNullOrEmpty(fullHtmlFieldName)) throw new ArgumentException(nameof(name));

            if (pvlInputTextProperties == null) throw new ArgumentNullException(nameof(pvlInputTextProperties));
            if (pvlInputTextProperties.Value == null) throw new ArgumentNullException(nameof(pvlInputTextProperties) + "." + nameof(pvlInputTextProperties.Value));

            var tagBuilder = new TagBuilder("pvl-inputtext");

            var maxLengthAttribute = metadata.ContainerType.GetProperty(metadata.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetCustomAttribute<MaxLengthAttribute>(true);
            if (maxLengthAttribute != null)
            {
                tagBuilder.MergeAttribute("maxlength", maxLengthAttribute.Length.ToString());
            }

            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, true);
            tagBuilder.MergePvlAttribute(pvlInputTextProperties.Value, "value", true);
            tagBuilder.MergePvlAttribute(pvlInputTextProperties.Disabled, "disabled");

            if (setId)
            {
                tagBuilder.GenerateId(fullHtmlFieldName);
            }
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out ModelState modelState) && modelState.Errors.Count > 0)
            {
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            if (!string.IsNullOrEmpty(metadata.DataTypeName))
            {
                if (_editorDataTypes.TryGetValue(metadata.DataTypeName, out var inputDataType))
                {
                    tagBuilder.MergeAttribute("type", inputDataType, true);
                }
            }
            else
            {

            }

            tagBuilder.MergeAttributes(htmlHelper.GetUnobtrusiveValidationAttributes(name, metadata));
            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}
