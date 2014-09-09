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
    public partial class Payment : NBrightBuyBase
    {

        private String _catid = "";
        private String _catname = "";
        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private String _templH = "";
        private String _templD = "";
        private String _templDfoot = "";
        private String _templF = "";
        private String _entryid = "";
        private String _tabid = "";
        private CartData _cartInfo;

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            _cartInfo = new CartData(PortalId);

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpPaymentGateways.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {

                const string templOK = "paymentOK.html";
                const string templFAIL = "paymentFAIL.html";

                var displayTempl = templOK;
                if (!_cartInfo.IsValidated()) displayTempl = templFAIL;

                rpDetailDisplay.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, displayTempl, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);
                _templateHeader = (GenXmlTemplate)rpDetailDisplay.ItemTemplate;

                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, _templateHeader, ModSettings.Settings(), null, DebugMode);

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
            //TODO: fix this to work for payment gateways.
            // First step is to make the cart turn into a order and accept the order...no payment proccessing
            

            if (_cartInfo.IsValidated())
            {

                _cartInfo.SaveModelTransQty(); // move qty into trnas


                _cartInfo.ConvertToOrder(DebugMode);

                _cartInfo.ApplyModelTransQty();

                var cartL = new List<NBrightInfo>();
                cartL.Add(_cartInfo.GetInfo());

                // display header
                rpDetailDisplay.DataSource = cartL;
                rpDetailDisplay.DataBind();
            }
        }

        #endregion


        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];
            if (Utils.RequestParam(Context, "eid") != "") param[0] = "eid=" + Utils.RequestParam(Context, "eid");

            switch (e.CommandName.ToLower())
            {
                case "pay":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

    }

}
