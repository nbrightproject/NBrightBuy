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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
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

        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private CartData _cartInfo;
        private Dictionary<String, NBrightInfo> _provList;
        private OrderData _orderData;
        private PaymentsInterface _prov;
 
        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpPaymentGateways.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {

                var pluginData = new PluginData(PortalSettings.Current.PortalId);
                _provList = pluginData.GetPaymentProviders();
                _cartInfo = new CartData(PortalId);

                var templOk = ModSettings.Get("paymentoktemplate");
                var templFail = ModSettings.Get("paymentfailtemplate");
                var templPayment = "";               

                if (_provList.Count == 0)
                {
                    #region "No Payment providers, so process as a ordering system"

                    var displayTempl = templOk;
                    if (!_cartInfo.IsValidated()) displayTempl = templFail;

                    rpDetailDisplay.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, displayTempl, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    _templateHeader = (GenXmlTemplate)rpDetailDisplay.ItemTemplate;

                    #endregion
                }
                else
                {
                    #region "Payment Details"

                    // display the payment method by default
                    templPayment = ModSettings.Get("paymentordersummary");
                    var templPaymentText = "";
                    var msg = "";
                    var orderid = Utils.RequestQueryStringParam(Context, "orderid");
                    if (Utils.IsNumeric(orderid))
                    {
                        // orderid exists, so must be return from bank; Process it!!
                        _orderData = new OrderData(PortalId, Convert.ToInt32(orderid));
                        _prov = PaymentsInterface.Instance(_orderData.PaymentProviderKey);
                        msg = _prov.ProcessPaymentReturn(Context);
                        if (msg == "") // no message so successful
                        {
                            _orderData = new OrderData(PortalId, Convert.ToInt32(orderid)); // get the updated order.
                            templPayment = templOk;
                        }
                        else
                        {
                            templPayment = templFail;
                        }
                    }
                    else
                    {
                        // not returning from bank, so display list of payment providers.
                        rpPaymentGateways.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(GetPaymentProviderTemplates(), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    }

                    templPaymentText = ModCtrl.GetTemplateData(ModSettings, templPayment, Utils.GetCurrentCulture(), DebugMode);
                    rpDetailDisplay.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(templPaymentText + msg, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    _templateHeader = (GenXmlTemplate)rpDetailDisplay.ItemTemplate;


                    #endregion
                }
                

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
            if (_provList.Count == 0)
            {
                #region "No Payment providers, so process as a ordering system"

                if (_cartInfo != null && _cartInfo.IsValidated())
                {

                    _cartInfo.SaveModelTransQty(); // move qty into trans
                    _cartInfo.ConvertToOrder(DebugMode);
                    _cartInfo.ApplyModelTransQty();

                    // Send emails
                    NBrightBuyUtils.SendEmailOrderToClient("ordercreatedclientemail.html", _cartInfo.PurchaseInfo.ItemID, "ordercreatedemailsubject");
                    NBrightBuyUtils.SendEmailToManager("ordercreatedemail.html", _cartInfo.PurchaseInfo);

                    var cartL = new List<NBrightInfo>();
                    cartL.Add(_cartInfo.GetInfo());

                    // display payment OK for order
                    rpDetailDisplay.DataSource = cartL;
                    rpDetailDisplay.DataBind();
                }

                #endregion
            }
            else
            {
                #region "Payment Details"

                // display return page
                DoDetail(rpDetailDisplay,_cartInfo.PurchaseInfo);
                DoDetail(rpPaymentGateways, _cartInfo.PurchaseInfo);


                #endregion
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
                    if (_cartInfo != null)
                    {
                        _cartInfo.SaveModelTransQty(); // move qty into trans
                        _cartInfo.ConvertToOrder(DebugMode);
                        var orderData = new OrderData(_cartInfo.PurchaseInfo.ItemID);
                        orderData.PaymentProviderKey = cArg;
                        orderData.SavePurchaseData();
                        var redirecturl = PaymentsInterface.Instance(orderData.PaymentProviderKey).RedirectForPayment(orderData);
                        if (redirecturl != "") Response.Redirect(redirecturl, true);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        #region "private methods"


        private String GetPaymentProviderTemplates()
        {
            var strRtn = "";
            foreach (var d in _provList)
            {
                var p = d.Value;
                var key = p.GetXmlProperty("genxml/textbox/ctrl");
                var prov = PaymentsInterface.Instance(key);
                if (prov != null) strRtn += prov.GetTemplate(_cartInfo.PurchaseInfo);
            }
            return strRtn;
        }


        #endregion

    }

}
