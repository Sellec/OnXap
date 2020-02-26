using System.Web.Mvc;

namespace PrimeVue.Net
{
    class ViewDataContainer<TRequestModel> : IViewDataContainer
        where TRequestModel : class
    {
        public ViewDataContainer(TRequestModel model)
        {
            ViewData = new ViewDataDictionary<TRequestModel>(model);
        }

        public ViewDataDictionary<TRequestModel> ViewData
        {
            get;
            set;
        }

        ViewDataDictionary IViewDataContainer.ViewData
        {
            get => ViewData;
            set => ViewData = (ViewDataDictionary<TRequestModel>)value;
        }
    }
}
