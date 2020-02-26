using System.Collections.Generic;

namespace System.Web.Mvc.Html
{
    /// <summary>
    /// Enumeration of button icon positions.
    /// </summary>
    public enum PvlButtonIconPosition
    {
        /// <summary>
        /// Left icon position
        /// </summary>
        Left,

        /// <summary>
        /// Right icon position
        /// </summary>
        Right
    }

    /// <summary>
    /// Available button classes.
    /// </summary>
    [Flags]
    public enum PvlButtonClasses
    {
        /// <summary>
        /// </summary>
        Raised = 1,
        
        /// <summary>
        /// </summary>
        Rounded = 2,
        
        /// <summary>
        /// </summary>
        SeveritySecondary = 4,
        
        /// <summary>
        /// </summary>
        SeveritySuccess = 8,
        
        /// <summary>
        /// </summary>
        SeverityInfo = 16,
        
        /// <summary>
        /// </summary>
        SeverityWarning = 32,

        /// <summary>
        /// </summary>
        SeverityDanger = 64,
    }

    /// <summary>
    /// Properties for button component.
    /// </summary>
    public class PvlButtonProperties
    {
        /// <summary>
        /// Text of the button.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute Label { get; set; }

        /// <summary>
        /// Name of the icon.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueAttribute Icon { get; set; }

        /// <summary>
        /// Position of the icon, valid values are "left" and "right".
        /// Required: no. Type: string. Default: "left".
        /// </summary>
        /// <remarks>This option can be used for binding icon position value to Vue's data property value.</remarks>
        public VueBinding IconPositionBinding { get; set; }

        /// <summary>
        /// Position of the icon.
        /// </summary>
        /// <remarks>This option can be used if only needs static icon position without any binding.</remarks>
        public PvlButtonIconPosition? IconPositionStatic { get; set; }

        /// <summary>
        /// Click event bound signature.
        /// Required: no. Type: string. Default: null.
        /// </summary>
        public VueValue OnClick { get; set; }

        /// <summary>
        /// When present, it specifies that the component should be disabled.
        /// Required: no. Type: boolean. Default: false.
        /// </summary>
        public VueAttribute Disabled { get; set; }

        /// <summary>
        /// Different color options are available as severity levels.
        /// A button can be raised by having <see cref="PvlButtonClasses.Raised"/> style class and similarly borders can be made rounded using <see cref="PvlButtonClasses.Rounded"/> class.
        /// </summary>
        public PvlButtonClasses? Classes { get; set; }
    }

    /// <summary>
    /// </summary>
    public static class PvlButtonExtensions
    {
        #region PvlButton
        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/button" />.
        /// </summary>
        public static MvcHtmlString PvlButton(this HtmlHelper htmlHelper, PvlButtonProperties pvlButtonProperties)
        {
            return PvlButton(htmlHelper, pvlButtonProperties, null);
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/button" />.
        /// </summary>
        public static MvcHtmlString PvlButton(this HtmlHelper htmlHelper, object htmlAttributes)
        {
            return PvlButton(htmlHelper, null, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/button" />.
        /// </summary>
        public static MvcHtmlString PvlButton(this HtmlHelper htmlHelper, IDictionary<string, object> htmlAttributes)
        {
            return PvlButton(htmlHelper, null, htmlAttributes);
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/button" />.
        /// </summary>
        public static MvcHtmlString PvlButton(this HtmlHelper htmlHelper, PvlButtonProperties pvlButtonProperties, object htmlAttributes)
        {
            return PvlButton(htmlHelper, pvlButtonProperties, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// See <see href="https://primefaces.org/primevue/#/button" />.
        /// </summary>
        public static MvcHtmlString PvlButton(this HtmlHelper htmlHelper, PvlButtonProperties pvlButtonProperties, IDictionary<string, object> htmlAttributes)
        {
            return PvlButtonHelper(htmlHelper, pvlButtonProperties, htmlAttributes);
        }
        #endregion

        private static MvcHtmlString PvlButtonHelper(HtmlHelper htmlHelper, PvlButtonProperties pvlButtonProperties, IDictionary<string, object> htmlAttributes)
        {
            var tagBuilder = new TagBuilder("pvl-button");
            tagBuilder.MergeAttributes(htmlAttributes);

            if (pvlButtonProperties != null)
            {
                tagBuilder.MergePvlAttribute(pvlButtonProperties.Label, "label");
                tagBuilder.MergePvlAttribute(pvlButtonProperties.Icon, "icon");
                tagBuilder.MergePvlAttribute(pvlButtonProperties.OnClick, "@click");
                tagBuilder.MergePvlAttribute(pvlButtonProperties.Disabled, "disabled");

                if (pvlButtonProperties.IconPositionStatic.HasValue)
                    tagBuilder.MergeAttribute("icon-pos", pvlButtonProperties.IconPositionStatic == PvlButtonIconPosition.Left ? "left" : pvlButtonProperties.IconPositionStatic == PvlButtonIconPosition.Right ? "right" : "");
                else 
                    tagBuilder.MergePvlAttribute(pvlButtonProperties.IconPositionBinding, "icon-pos");

                if (pvlButtonProperties.Classes.HasValue)
                {
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.Raised)) tagBuilder.AddCssClass("p-button-raised");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.Rounded)) tagBuilder.AddCssClass("p-button-rounded");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.SeveritySecondary)) tagBuilder.AddCssClass("p-button-secondary");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.SeveritySuccess)) tagBuilder.AddCssClass("p-button-success");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.SeverityInfo)) tagBuilder.AddCssClass("p-button-info");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.SeverityWarning)) tagBuilder.AddCssClass("p-button-warning");
                    if (pvlButtonProperties.Classes.Value.HasFlag(PvlButtonClasses.SeverityDanger)) tagBuilder.AddCssClass("p-button-danger");
                }
            }

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}
