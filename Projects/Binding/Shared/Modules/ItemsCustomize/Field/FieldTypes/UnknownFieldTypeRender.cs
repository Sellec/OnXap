using System.Collections.Generic;
#if NETFULL
using System.Web.Mvc;
#elif NETCORE
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcHtmlString = Microsoft.AspNetCore.Html.IHtmlContent;
#endif

namespace OnXap.Modules.ItemsCustomize.Field.FieldTypes
{
    using Core;
    using Modules.ItemsCustomize.Field;
    using Modules.ItemsCustomize.Field.FieldTypes;

    sealed class UnknownFieldTypeRender : CoreComponentBase, ICustomFieldRender<UnknownFieldType>
    {
        public MvcHtmlString RenderHtmlEditor<TModel>(HtmlHelper<TModel> html, IField field, IDictionary<string, object> htmlAttributes, params object[] additionalParameters)
        {
            return null;
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
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
