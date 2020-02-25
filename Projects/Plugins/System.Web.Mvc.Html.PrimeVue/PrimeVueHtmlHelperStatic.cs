namespace System.Web.Mvc.Html
{
    using PrimeVue;

    /// <summary>
    /// </summary>
    public static class PrimeVueHtmlHelperStatic
    {
        /// <summary>
        /// Creates a new html helper for a specific request model.
        /// </summary>
        /// <typeparam name="TRequestModel">The type of model waiting by specific <see cref="Controller"/> method.</typeparam>
        /// <param name="htmlHelper"></param>
        /// <returns>Returns helper instance.</returns>
        /// <remarks>Using view's model is not the right way - controller's method may not wait such type like passed in view.</remarks>
        public static PrimeVueHtmlHelper<TRequestModel> CreatePrimeVueHelper<TRequestModel>(this HtmlHelper htmlHelper)
            where TRequestModel : class
        {
            return new PrimeVueHtmlHelper<TRequestModel>(htmlHelper);
        }
    }

}
