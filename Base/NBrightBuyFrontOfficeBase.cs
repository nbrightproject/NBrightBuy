using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
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

            if (ModSettings.Settings().ContainsKey("themefolder") && ModSettings.Settings()["themefolder"] != "")
            {
                ThemeFolder = ModSettings.Settings()["themefolder"];
            }
            if (ThemeFolder == "") ThemeFolder = StoreSettings.Current.ThemeFolder;

            NBrightBuyUtils.IncludePageHeaderDefault(ModCtrl, Page, "frontofficepageheader.html", ThemeFolder, DebugMode);
            if (ModuleContext.Configuration != null)
            {
                NBrightBuyUtils.IncludePageHeaderDefault(ModCtrl, Page, "pageheader" + ModuleContext.Configuration.DesktopModule.ModuleName + ".html", ThemeFolder, DebugMode);                
            }

            Controls.AddAt(0, new LiteralControl("<div class='" + ThemeFolder + "'><!-- " + ThemeFolder + " Start -->"));
            Controls.AddAt(Controls.Count, new LiteralControl("</div><!-- " + ThemeFolder + " End -->"));

        }

	}
}
