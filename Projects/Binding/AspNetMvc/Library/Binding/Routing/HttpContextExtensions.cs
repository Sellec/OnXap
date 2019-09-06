using OnXap;

namespace System.Web.Mvc
{
    /// <summary>
    /// </summary>
    public static class HttpContextRoutingExtensions
    {
        const string EXTENSIONPREFIX = "HttpContextExtensions_";

        public static OnXApplication GetAppCore(this HttpContextBase context)
        {
            return context.Items.Contains(EXTENSIONPREFIX + "ApplicationCore") ? (OnXApplication)context.Items[EXTENSIONPREFIX + "ApplicationCore"] : null;
        }

        public static void SetAppCore(this HttpContext context, OnXApplication appCore)
        {
            context.Items[EXTENSIONPREFIX + "ApplicationCore"] = appCore;
        }
    }
}