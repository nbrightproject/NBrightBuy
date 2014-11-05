using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using NBrightDNN.controls;
using NBrightCore.common;
using System.Xml;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{
    public class NBrightBuyFrontOfficeBase : NBrightBuyBase
	{
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            NBrightBuyUtils.IncludePageHeaderDefault(ModCtrl, Page, "frontofficepageheader.html", DebugMode);
        }
	}
}
