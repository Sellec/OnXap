namespace System.Web.Mvc.Html.PrimeVue
{
    /// <summary>
    /// Html helper for a specific request model.
    /// </summary>
    /// <typeparam name="TRequestModel">The type of model waiting by specific <see cref="Controller"/> method.</typeparam>
    /// <remarks>Using view's model is not the right way - controller's method may not wait such type like passed in view.</remarks>
    public partial class PrimeVueHtmlHelper<TRequestModel> 
        where TRequestModel : class
    {
        private HtmlHelper _htmlHelper = null;
        private ViewDataDictionary<TRequestModel> _viewData = null;

        internal PrimeVueHtmlHelper(HtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
            _viewData = new ViewDataDictionary<TRequestModel>();
        }

    }
}
