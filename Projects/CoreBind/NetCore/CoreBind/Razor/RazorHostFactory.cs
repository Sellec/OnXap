using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Mvc;
using System.Web.WebPages.Razor;

namespace OnXap.CoreBind.Razor
{
    public class RazorHostFactory : MvcWebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            return new RazorHost(virtualPath, physicalPath);
        }
    }
}
