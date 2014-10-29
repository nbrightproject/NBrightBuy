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
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using ManualPaymentProvider;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Providers
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ManualPayment : NBrightBuyAdminBase
    {



        #region Event Handlers

        private String _ctrlkey = "";
        private NBrightInfo _info;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                _ctrlkey = "manualpayment";
                _info = ProviderUtils.GetProviderSettings(_ctrlkey);
                var rpDataHTempl = ProviderUtils.GetTemplateData("settings.html");
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, StoreSettings.Current.Settings(), PortalSettings.HomeDirectory);
            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                Controls.Add(l);
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
                Controls.Add(l);
            }
        }

        private void PageLoad()
        {
            if (UserId > 0) // only logged in users can see data on this module.
            {
                // display header
                base.DoDetail(rpDataH, _info);

            }
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
                    param[0] = "ctrl=manualpayment";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "cancel":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion



        private void Update()
        {
            var modCtrl = new NBrightBuyController();
            var strXml = GenXmlFunctions.GetGenXml(rpDataH,"",StoreSettings.Current.FolderImagesMapPath);
            _info.XMLData = strXml;
            modCtrl.Update(_info);

            var resxDic = GenXmlFunctions.GetGenXmlResx(rpDataH);
            var genTempl = (GenXmlTemplate)rpDataH.ItemTemplate;
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

            var resxUpdate = NBrightBuyUtils.UpdateResxFields(resxDic, resxfolder, StoreSettings.Current.EditLanguage, true);

            // NOTE: For some reason this action restarts the application, not sure why, but it's a side effect that helps display the new resx change. So I'm leaving it for now!
            //  This restart doesn;t happen with the update of the settings page???

            //remove current setting from cache for reload
            Utils.RemoveCache("ManualPaymentProvider" + PortalSettings.Current.PortalId.ToString(""));

        }





    }

}
