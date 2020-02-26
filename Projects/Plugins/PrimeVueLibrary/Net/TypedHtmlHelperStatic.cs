using PrimeVue.Net;

namespace System.Web.Mvc.Html
{
    /// <summary>
    /// </summary>
    public static class TypedHtmlHelperStatic
    {
        /// <summary>
        /// Creates a new html helper for a specific request model.
        /// </summary>
        /// <typeparam name="TModel">The type of model waiting by specific <see cref="Controller"/> method.</typeparam>
        /// <param name="webViewPage"></param>
        /// <returns>Returns helper instance.</returns>
        /// <remarks>Using view's model is not the right way - controller's method may not wait such type like passed in view.</remarks>
        public static HtmlHelper<TModel> CreateHtmlHelper<TModel>(this WebViewPage webViewPage)
            where TModel : class, new()
        {
            var viewDataContainer = new ViewDataContainer<TModel>(new TModel());
            return new HtmlHelper<TModel>(
                new ViewContext(
                            new ControllerContext(webViewPage.Request.RequestContext, webViewPage.ViewContext.Controller),
                            webViewPage.ViewContext.View,
                            viewDataContainer.ViewData,
                            new TempDataDictionary(),
                            webViewPage.ViewContext.Writer),
                viewDataContainer);
        }

        /// <summary>
        /// Creates a new html helper for a specific request model.
        /// </summary>
        /// <typeparam name="TModel">The type of model waiting by specific <see cref="Controller"/> method.</typeparam>
        /// <param name="webViewPage"></param>
        /// <param name="modelFactory">Model instance factory for models that does not have opened parameterless constructor or needs to be created manually.</param>
        /// <returns>Returns helper instance.</returns>
        /// <remarks>Using view's model is not the right way - controller's method may not wait such type like passed in view.</remarks>
        public static HtmlHelper<TModel> CreateHtmlHelper<TModel>(this WebViewPage webViewPage, Func<TModel> modelFactory)
            where TModel : class
        {
            if (modelFactory == null) throw new ArgumentNullException(nameof(modelFactory));
            var viewDataContainer = new ViewDataContainer<TModel>(modelFactory());
            return new HtmlHelper<TModel>(
                new ViewContext(
                            new ControllerContext(webViewPage.Request.RequestContext, webViewPage.ViewContext.Controller),
                            webViewPage.ViewContext.View,
                            viewDataContainer.ViewData,
                            new TempDataDictionary(),
                            webViewPage.ViewContext.Writer),
                viewDataContainer);
        }

    }

}
