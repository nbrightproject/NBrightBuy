// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {

                #region "load templates"

                var t1 = "settingsheader.html";
                var t2 = "settingsbody.html";
                var t3 = "settingsfooter.html";

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                #endregion


            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            #region "Data Repeater"
            if (UserId > 0) // only logged in users can see data on this module.
            {
                DisplayDataEntryRepeater();
            }

            #endregion

            // display header (Do header after the data return so the productcount works)
            base.DoDetail(rpDataH);

            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "save":
                    Update();
                    param[0] = "ctrl=settings";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "removelogo":
                    var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
                    if (settings != null && settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
                    {
                        settings.SetXmlProperty("genxml/hidden/hidemaillogo", "");
                        settings.SetXmlProperty("genxml/hidden/emaillogourl", "");
                        settings.SetXmlProperty("genxml/hidden/emaillogopath", "");
                        ModCtrl.Update(settings);
                    }
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "cancel":
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }

        #endregion

        private void Update()
        {
            var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
            if (settings == null)
            {
                settings = new NBrightInfo(true);
                settings.PortalId = PortalId;
                // use zero as moduleid so it's not picked up by the modules for their settings.
                // The normal GetList will get all moduleid OR moduleid=-1 
                settings.ModuleId = 0; 
                settings.ItemID = -1;
                settings.TypeCode = "SETTINGS";
                settings.GUIDKey = "NBrightBuySettings";
            }
            settings.XMLData = GenXmlFunctions.GetGenXml(rpData,"",StoreSettings.Current.FolderImagesMapPath);

            if (settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
            {
                settings.SetXmlProperty("genxml/hidden/emaillogourl", StoreSettings.Current.FolderImages + "/" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));
                settings.SetXmlProperty("genxml/hidden/emaillogopath", StoreSettings.Current.FolderImagesMapPath + "/" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));                
            }


            settings.SetXmlProperty("genxml/hidden/backofficetabid", PortalSettings.Current.ActiveTab.TabID.ToString(""));
            
            ModCtrl.Update(settings);

            if (StoreSettings.Current.DebugMode) settings.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "\\debug_Settings.xml");


            // create upload folders
            var folder = StoreSettings.Current.FolderImagesMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderDocumentsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderUploadsMapPath ;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            //Create default category grouptype
            var l = NBrightBuyUtils.GetCategoryGroups(EditLanguage, true);
            var g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "cat" select i;
            if (!g.Any()) CreateGroup("cat", "Categories");
            if (l.Count == 0)
            {
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "promo" select i;
                if (!g.Any()) CreateGroup("promo", "Promotions");
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "man" select i;
                if (!g.Any()) CreateGroup("man", "Manufacturer");
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "supp" select i;
                if (!g.Any()) CreateGroup("supp", "Supplier");
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "fea" select i;
                if (!g.Any()) CreateGroup("fea", "Features");
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "spec" select i;
                if (!g.Any()) CreateGroup("spec", "Specifications");
                g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "temp" select i;
                if (!g.Any()) CreateGroup("temp", "Temp");
            }

            //update resx fields
            var resxDic = GenXmlFunctions.GetGenXmlResx(rpData);
            var genTempl = (GenXmlTemplate)rpData.ItemTemplate;
            var resxfolders = genTempl.GetResxFolders();
            // we're only going to create resx files for this portal, so remove all other paths formt he folders list.
            var resxfolder = new List<String>();
            foreach (var p in resxfolders)
            {
                if (p.StartsWith(PortalSettings.HomeDirectory))
                {
                    resxfolder.Add(p);
                    break;
                }
            }

            var resxUpdate = NBrightBuyUtils.UpdateResxFields(resxDic, resxfolder, StoreSettings.Current.EditLanguage,true);

            //remove current setting from cache for reload
            HttpContext.Current.Items.Remove("NBBStoreSettings");
            Utils.RemoveCache("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""));

            if (resxUpdate) DataCache.ClearCache();

        }

        private void CreateGroup(String groupref, String name)
        {
            var n = new GroupData(-1, StoreSettings.Current.EditLanguage);
            n.Ref = groupref;
            n.Name = name;
            n.DataRecord.GUIDKey = groupref;
            n.Save();
        }


        private void DisplayDataEntryRepeater()
        {
                //render the detail page
                var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
                if (settings == null) settings = new NBrightInfo(true);
                base.DoDetail(rpData, settings);
        }


    }

}
