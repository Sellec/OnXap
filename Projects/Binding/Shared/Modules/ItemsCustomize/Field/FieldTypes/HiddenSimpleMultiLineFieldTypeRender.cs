﻿using System.Collections.Generic;
#if NETFULL
using System.Web.Mvc;
using System.Web.Mvc.Html;
#elif NETCORE
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcHtmlString = Microsoft.AspNetCore.Html.IHtmlContent;
#endif

namespace OnXap.Modules.ItemsCustomize.Field.FieldTypes
{
    using Core;
    using Modules.ItemsCustomize.Data;
    using Modules.ItemsCustomize.Field;
    using Modules.ItemsCustomize.Field.FieldTypes;

#pragma warning disable CS1591 // todo внести комментарии.
    public class HiddenSimpleMultiLineFieldTypeRender : CoreComponentBase, ICustomFieldRender<HiddenSimpleMultiLineFieldType>
    {
        public virtual MvcHtmlString RenderHtmlEditor<TModel>(HtmlHelper<TModel> html, IField field, IDictionary<string, object> htmlAttributes, params object[] additionalParameters)
        {
            if (htmlAttributes == null) htmlAttributes = new Dictionary<string, object>();
            htmlAttributes["style"] = "display:none;";
            if (!string.IsNullOrEmpty(field.alias)) htmlAttributes["class"] = (htmlAttributes.GetValueOrDefault("class", null) ?? "") + " FieldAlias_" + field.alias;

            var value = (field as FieldData)?.ToString();
#if NETFULL
            return html.TextArea($"fieldValue_{field.IdField}", value, htmlAttributes);
#elif NETCORE
            return html.TextArea($"fieldValue_{field.IdField}", value, 0, 0, htmlAttributes);
#endif
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion
    }
}
