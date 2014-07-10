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
    public partial class DashSummary : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CtrlPaging.Visible = false; // don't bother with paging on this module.
            try
            {

                #region "load templates"

                var t1 = "dashboardheader.html";
                var t2 = "dashboardbody.html";
                var t3 = "dashboardfooter.html";

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
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "cancel":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


        private void DisplayDataEntryRepeater()
        {
                //render the detail page
                base.DoDetail(rpData);
        }


    }

}
