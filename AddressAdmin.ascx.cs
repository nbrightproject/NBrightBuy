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

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class AddressAdmin : NBrightBuyBase
    {

        private String _catid = "";
        private String _catname = "";
        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private String _templH = "";
        private String _templD = "";
        private String _templinp = "";
        private String _templF = "";
        private String _entryid = "";
        private String _tabid = "";
        private AddressData _addressData;
        private const string NotifyRef = "addressbookupdated";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _addressData = new AddressData();

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {
                _templH = ModSettings.Get("txtdisplayheader");
                _templD = ModSettings.Get("txtdisplaybody");
                _templF = ModSettings.Get("txtdisplayfooter");
                _templinp = ModSettings.Get("txtinputform");

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, _templH, Utils.GetCurrentCulture(), DebugMode); 

                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                _templateHeader = (GenXmlTemplate)rpDataH.ItemTemplate;

                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, _templD, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, _templF, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                // Get Display Footer
                var rpInpTempl = ModCtrl.GetTemplateData(ModSettings, _templinp, Utils.GetCurrentCulture(), DebugMode);
                rpAddr.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpInpTempl, ModSettings.Settings(), PortalSettings.HomeDirectory); 


            }
            catch (Exception exc)
            {
                rpDataF.ItemTemplate = new GenXmlTemplate(exc.Message, ModSettings.Settings());
                // catch any error and allow processing to continue, output error as footer template.
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


            if (_templD.Trim() != "") // if we don;t have a template, don't do anything
            {
                var l = _addressData.GetAddressList();
                rpData.DataSource = l;
                rpData.DataBind();
            }

            #endregion

            base.DoDetail(rpDataH);
            base.DoDetail(rpDataF);
            var addrid = Utils.RequestParam(Context, "addressid");
            if (Utils.IsNumeric(addrid))
            {
                var objAddr = _addressData.GetAddress(Convert.ToInt32(addrid));
                if (objAddr == null) objAddr = new NBrightInfo(true); //assume new address
                base.DoDetail(rpAddr,objAddr);
            }
            else
            {
                base.DoDetail(rpAddr);                
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
                case "saveaddress":
                    _addressData.AddAddress(rpAddr);
                    var addrid = Utils.RequestParam(Context, "addressid");
                    if (Utils.IsNumeric(addrid) && ModSettings.Get("emailmanager") == "True")
                    {
                        var ad = _addressData.GetAddress(Convert.ToInt32(addrid));
                        if (ad != null)
                        {
                            var emailtemplate = ModSettings.Get("emailtemplate");
                            if (ModSettings.Get("emailmanageropt") == "2")
                            {
                                NBrightBuyUtils.SendEmailToManager(emailtemplate, ModSettings, ad, NotifyRef);
                            }
                            else
                            {
                                if (ad.GetXmlPropertyBool("genxml/hidden/default"))
                                {
                                    NBrightBuyUtils.SendEmailToManager(emailtemplate, ModSettings, ad, NotifyRef);
                                }
                            }
                        }
                    }
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef, NotifyCode.ok);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "deleteaddress":
                    _addressData.RemoveAddress(e.Item.ItemIndex);                        
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "editaddress":
                    param[0] = "addressid=" + e.Item.ItemIndex.ToString("");
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "newaddress":
                    param[0] = "addressid=-1";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


    }

}
