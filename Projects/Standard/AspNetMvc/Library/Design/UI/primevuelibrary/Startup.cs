using OnUtils.Architecture.AppCore;
using PrimeVue.Net;
using System;
using System.Web.Mvc.Html;

namespace OnXap.Design.UI.primevuelibrary
{
    class Startup : IExecuteStart
    {
        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            // Данный код нужен для обязательной загрузки зависимости System.Web.Mvc.Html.PrimeVue без внесения её в web.config.
            var type = typeof(TypedHtmlHelperStatic);
            if (type == null) throw new InvalidProgramException();
        }
    }
}
