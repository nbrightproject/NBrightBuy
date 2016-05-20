using System;
using System.Collections.Generic;
using System.IO;
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

            if (ModSettings.Settings().ContainsKey("razortemplate") && ModSettings.Settings()["razortemplate"] != "")
            {
                RazorTemplate = ModSettings.Settings()["razortemplate"];
            }

            // insert page header text
            NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, "frontofficepageheader.cshtml", ControlPath, ThemeFolder, ModSettings.Settings());

            if (ModuleContext.Configuration != null)
            {
                if (String.IsNullOrEmpty(RazorTemplate)) RazorTemplate = ModuleConfiguration.DesktopModule.ModuleName + ".cshtml";

                // insert page header text
                NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, Path.GetFileNameWithoutExtension(RazorTemplate) + "_head" + Path.GetExtension(RazorTemplate), ControlPath, ThemeFolder, ModSettings.Settings());
            }
            var strOut = "<span class='container_" + ThemeFolder + "_" + RazorTemplate + "'>";

            Controls.AddAt(0, new LiteralControl("<div class='container_" + ThemeFolder.ToLower().Replace(" ","_") + "_" + RazorTemplate.ToLower().Replace(".cshtml","").Replace(" ", "_") + "'>"));
            Controls.AddAt(Controls.Count, new LiteralControl("</div>"));

        }

	}
}
