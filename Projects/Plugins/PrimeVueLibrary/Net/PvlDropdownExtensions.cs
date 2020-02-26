using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Script.Serialization;

namespace System.Web.Mvc.Html
{
    /// <summary>
    /// Properties for dropdown component.
    /// </summary>
    public class PvlDropdownProperties
    {
        /// <summary>
        /// Value of the component. 
        /// Required: yes. Type: any. Default: null.
        /// </summary>
        public VueBinding Value { get; set; }

        /// <summary>
        /// An array of selectitems to display as the available options.
        /// Required: yes. Type: array. Default: null.
        /// </summary>
        public VueBinding Options { get; set; }

        /// <summary>
        /// Property name to use as the label of an option.
        /// Required: yes. Type: string. Default: null.
        /// </summary>
        public VueAttribute OptionLabel { get; set; }

        /// <summary>
        /// Property name to use as the value of an option, defaults to the option itself when not defined.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute OptionValue { get; set; }

        /// <summary>
        /// Property name to use as the disabled flag of an option, defaults to false when not defined.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute OptionDisabled { get; set; }

        /// <summary>
        /// Height of the viewport in pixels, a scrollbar is defined if height of list exceeds this value.
        /// Required: no. Type: string. Default: "200px".
        /// </summary>
        public VueAttribute ScrollHeight { get; set; }

        /// <summary>
        /// When specified, displays an input field to filter the items on keyup.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute Filter { get; set; }

        /// <summary>
        /// Placeholder text to show when filter input is empty.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute FilterPlaceholder { get; set; }

        /// <summary>
        /// When present, custom value instead of predefined options can be entered using the editable input field.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute Editable { get; set; }

        /// <summary>
        /// Default text to display when no option is selected.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute Placeholder { get; set; }

        /// <summary>
        /// When present, it specifies that the component should be disabled.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute Disabled { get; set; }

        /// <summary>
        /// A property to uniquely match the value in options for better performance.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute DataKey { get; set; }

        /// <summary>
        /// When enabled, a clear icon is displayed to clear the value.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute ShowClear { get; set; }

        /// <summary>
        /// Index of the element in tabbing order.
        /// Required: no. Type: number. Default: null.
        /// </summary>
        public VueAttribute TabIndex { get; set; }

        /// <summary>
        /// Establishes relationships between the component and label(s) where its value should be one or more element IDs.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute AriaLabelledBy { get; set; }

        /// <summary>
        /// Id of the element or "body" for document where the overlay should be appended to.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute AppendTo { get; set; }
    }

    /// <summary>
    /// </summary>
    public static class PvlDropdownExtensions
    {
        #region PvlDropdownFor
        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        public static MvcHtmlString PvlDropdownFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlDropdownProperties pvlDropdownProperties)
        {
            return PvlDropdownFor(htmlHelper, expression, pvlDropdownProperties, null);
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        public static MvcHtmlString PvlDropdownFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlDropdownProperties pvlDropdownProperties, object htmlAttributes)
        {
            return PvlDropdownFor(htmlHelper, expression, pvlDropdownProperties, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        public static MvcHtmlString PvlDropdownFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, PvlDropdownProperties pvlDropdownProperties, IDictionary<string, object> htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            return PvlDropdownHelper(htmlHelper, modelMetadata, ExpressionHelper.GetExpressionText(expression), true, pvlDropdownProperties, htmlAttributes);
        }
        #endregion

        #region PvlDropdownEnumFor
        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        /// <remarks>
        /// Options <see cref="PvlDropdownProperties.Options"/>, <see cref="PvlDropdownProperties.OptionDisabled"/>, <see cref="PvlDropdownProperties.OptionLabel"/> and <see cref="PvlDropdownProperties.OptionValue"/> are 
        /// ignored becouse this method generates static options source from property enum type.
        /// </remarks>
        public static MvcHtmlString PvlDropdownEnumFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, PvlDropdownProperties pvlDropdownProperties)
        {
            return PvlDropdownEnumFor(htmlHelper, expression, pvlDropdownProperties, null);
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        /// <remarks>
        /// Options <see cref="PvlDropdownProperties.Options"/>, <see cref="PvlDropdownProperties.OptionDisabled"/>, <see cref="PvlDropdownProperties.OptionLabel"/> and <see cref="PvlDropdownProperties.OptionValue"/> are 
        /// ignored becouse this method generates static options source from property enum type.
        /// </remarks>
        public static MvcHtmlString PvlDropdownEnumFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, PvlDropdownProperties pvlDropdownProperties, object htmlAttributes)
        {
            return PvlDropdownEnumFor(htmlHelper, expression, pvlDropdownProperties, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/dropdown" />.
        /// </summary>
        /// <remarks>
        /// Options <see cref="PvlDropdownProperties.Options"/>, <see cref="PvlDropdownProperties.OptionDisabled"/>, <see cref="PvlDropdownProperties.OptionLabel"/> and <see cref="PvlDropdownProperties.OptionValue"/> are 
        /// ignored becouse this method generates static options source from property enum type.
        /// </remarks>
        public static MvcHtmlString PvlDropdownEnumFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, PvlDropdownProperties pvlDropdownProperties, IDictionary<string, object> htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            if (!EnumHelper.IsValidForEnumHelper(modelMetadata.ModelType))
            {
                throw new ArgumentException($"Incorrect property type '{modelMetadata.ModelType.FullName}'", nameof(expression));
            }
            var expressionText = ExpressionHelper.GetExpressionText(expression);
            //string fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expressionText);
            Enum @enum = null;
            //if (!string.IsNullOrEmpty(fullHtmlFieldName))
            //{
            //    @enum = (htmlHelper.GetModelStateValue(fullHtmlFieldName, modelMetadata.ModelType) as Enum);
            //}
            if (@enum == null && !string.IsNullOrEmpty(expressionText))
            {
                @enum = (htmlHelper.ViewData.Eval(expressionText) as Enum);
            }
            if (@enum == null)
            {
                @enum = (modelMetadata.Model as Enum);
            }

            var options = EnumHelper.GetSelectList(modelMetadata.ModelType, @enum).Select(x => new
            {
                value = int.TryParse(x.Value, out int intValue) ? (object)intValue : (object)x.Value,
                text = x.Text,
                disabled = x.Disabled 
            }).ToArray();
            var optionFirst = options.FirstOrDefault();
            //if (!string.IsNullOrEmpty(optionLabel) && selectList.Count != 0 && string.IsNullOrEmpty(selectList[0].Text))
            //{
            //    selectList[0].Text = optionLabel;
            //    optionLabel = null;
            //}
            pvlDropdownProperties = pvlDropdownProperties == null ? new PvlDropdownProperties() : pvlDropdownProperties;

            var serializer = new JavaScriptSerializer();
            pvlDropdownProperties.Options = new VueBinding(serializer.Serialize(options));
            pvlDropdownProperties.OptionLabel = new VueValue(nameof(optionFirst.text));
            pvlDropdownProperties.OptionValue = new VueValue(nameof(optionFirst.value));
            pvlDropdownProperties.OptionDisabled = new VueValue(nameof(optionFirst.disabled));

            return PvlDropdownHelper(htmlHelper, modelMetadata, expressionText, true, pvlDropdownProperties, htmlAttributes);
        }
        #endregion

        private static MvcHtmlString PvlDropdownHelper<TModel>(HtmlHelper<TModel> htmlHelper, ModelMetadata metadata, string name, bool setId, PvlDropdownProperties pvlDropdownProperties, IDictionary<string, object> htmlAttributes)
        {
            var fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (string.IsNullOrEmpty(fullHtmlFieldName)) throw new ArgumentException(nameof(name));

            if (pvlDropdownProperties == null) throw new ArgumentNullException(nameof(pvlDropdownProperties));
            if (pvlDropdownProperties.Value == null) throw new ArgumentNullException(nameof(pvlDropdownProperties) + "." + nameof(pvlDropdownProperties.Value));
            if (pvlDropdownProperties.Options == null) throw new ArgumentNullException(nameof(pvlDropdownProperties) + "." + nameof(pvlDropdownProperties.Options));
            if (pvlDropdownProperties.OptionLabel == null) throw new ArgumentNullException(nameof(pvlDropdownProperties) + "." + nameof(pvlDropdownProperties.OptionLabel));

            var tagBuilder = new TagBuilder("pvl-dropdown");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, true);
            tagBuilder.MergePvlAttribute(pvlDropdownProperties.Value, "value", true);
            tagBuilder.MergePvlAttribute(pvlDropdownProperties.Options, "options");
            tagBuilder.MergePvlAttribute(pvlDropdownProperties.OptionLabel, "option-label");

            if (pvlDropdownProperties != null)
            {
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.OptionValue, "option-value");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.OptionDisabled, "option-disabled");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.ScrollHeight, "scroll-height");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.Filter, "filter");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.FilterPlaceholder, "filter-placeholder");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.Editable, "editable");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.Placeholder, "placeholder");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.Disabled, "disabled");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.DataKey, "data-key");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.ShowClear, "showclear");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.TabIndex, "tab-index");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.AriaLabelledBy, "aria-labelled-by");
                tagBuilder.MergePvlAttribute(pvlDropdownProperties.AppendTo, "append-to");
            }

            if (setId) tagBuilder.GenerateId(fullHtmlFieldName);
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out ModelState modelState) && modelState.Errors.Count > 0) tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);

            tagBuilder.MergeAttributes(htmlHelper.GetUnobtrusiveValidationAttributes(name, metadata));
            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

    }
}
