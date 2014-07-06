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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;
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
    public partial class Plugins : NBrightBuyBase
    {

        private GenXmlTemplate _templSearch; 
        private String _entryid = "";
        private Boolean _displayentrypage = false;

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            CtrlPaging.Visible = false; // don't bother with paging on this module.
            try
            {
                #region "set templates based on entry id (eid) from url"

                _entryid = Utils.RequestQueryStringParam(Context, "eid");

                if (_entryid != "") _displayentrypage = true;

                #endregion

                #region "load templates"

                var t1 = "pluginsheader.html";
                var t2 = "pluginsbody.html";
                var t3 = "pluginsfooter.html";

                if (Utils.IsNumeric(_entryid))
                {
                    t1 = "pluginsdetailheader.html";
                    t2 = "pluginsdetail.html";
                    t3 = "pluginsdetailfooter.html";
                }

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

                if (_displayentrypage)
                {
                    DisplayDataEntryRepeater(_entryid);
                }
                else
                {
                    var pluginData = new PluginData(PortalId);
                    rpData.DataSource = pluginData.GetPluginList();
                    rpData.DataBind();

                }
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
            var pluginData = new PluginData(PortalId);
            var strCacheKey = "bomenuhtml*" + Utils.GetCurrentCulture() + "*" + PortalId.ToString("");

            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    param[0] = "eid=" + cArg;
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "entryup":
                    if (Utils.IsNumeric(cArg))
                    {
                        var idx = Convert.ToInt32(cArg);
                        var p = pluginData.GetPlugin(idx);                        
                        pluginData.RemovePlugin(idx);
                        p.SetXmlProperty("genxml/hidden/index","");  //remove index so we add instead of update
                        pluginData.AddPlugin(p, idx - 1);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "entrydown":
                    if (Utils.IsNumeric(cArg))
                    {
                        var idx = Convert.ToInt32(cArg);
                        var p = pluginData.GetPlugin(idx);
                        pluginData.RemovePlugin(idx);
                        p.SetXmlProperty("genxml/hidden/index", ""); //remove index so we add instead of update
                        pluginData.AddPlugin(p, idx + 1);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "save":
                    if (Utils.IsNumeric(cArg))
                        pluginData.UpdatePlugin(rpData,Convert.ToInt32(cArg));
                    else
                        pluginData.AddPlugin(rpData);
                    HttpContext.Current.Session[strCacheKey] = "";                                          
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "add":
                    param[0] = "eid=-1";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "return":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


        private void DisplayDataEntryRepeater(String entryId)
        {
            if (Utils.IsNumeric(entryId))
            {
                var pluginData = new PluginData(PortalId);
                var pData = pluginData.GetPlugin(Convert.ToInt32(entryId));
                //render the detail page
                base.DoDetail(rpData, pData);

            }
        }


    }

}
