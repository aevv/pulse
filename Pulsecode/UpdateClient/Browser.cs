using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace UpdateClient
{
    class Browser : WebBrowser
    {
        protected override void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            base.OnNavigated(e);
        }
        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
           // this.Document.cook
            base.OnNavigating(e);
        }
    }
}
