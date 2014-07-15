using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using NBrightDNN.controls;
using NEvoWeb.Modules.NB_Store;
using NBrightCore.common;
using System.Xml;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{
    public class NBrightBuyAdminBase : NBrightBuyBase
	{

        protected override void OnLoad(EventArgs e)
        {
            if (UserInfo.IsInRole("Editor") || UserInfo.IsInRole("Manager"))
            {
                base.OnLoad(e);
            }
            else
            {
                // has no right to access, throw error page.
                Response.Redirect("~/Error.aspx", true);
            }
       }

	}
}
