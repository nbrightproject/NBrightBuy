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
    public partial class Payment : NBrightBuyFrontOfficeBase
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

                var orderid = Utils.RequestQueryStringParam(Context, "orderid");
                var templOk = ModSettings.Get("paymentoktemplate");
                var templFail = ModSettings.Get("paymentfailtemplate");
                var templHeader = "";
                var templFooter = "";
                var templText = "";

                if ((_provList.Count == 0 || _cartInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal") <= 0) && orderid == "")
                {
                    #region "No Payment providers, so process as a ordering system"

                    var displayTempl = templOk;
                    if (!_cartInfo.IsValidated()) displayTempl = templFail;

                    rpDetailDisplay.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, displayTempl, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    _templateHeader = (GenXmlTemplate) rpDetailDisplay.ItemTemplate;

                    // we may have voucher discounts that give a zero appliedtotal, so process.
                    var discountprov = DiscountCodeInterface.Instance();
                    if (discountprov != null)
                    {
                        discountprov.UpdatePercentUsage(PortalId, UserId, _cartInfo.PurchaseInfo);
                        discountprov.UpdateVoucherAmount(PortalId, UserId, _cartInfo.PurchaseInfo);
                    }

                    #endregion
                }
                else
                {
                    #region "Payment Details"

                    // display the payment method by default
                    templHeader = ModSettings.Get("paymentordersummary");
                    templFooter = ModSettings.Get("paymentfooter");
                    var templPaymentText = "";
                    var msg = "";
                    if (Utils.IsNumeric(orderid))
                    {
                        // orderid exists, so must be return from bank; Process it!!
                        _orderData = new OrderData(PortalId, Convert.ToInt32(orderid));
                        _prov = PaymentsInterface.Instance(_orderData.PaymentProviderKey);

                        msg = _prov.ProcessPaymentReturn(Context);
                        if (msg == "") // no message so successful
                        {
                            _orderData = new OrderData(PortalId, Convert.ToInt32(orderid)); // get the updated order.
                            _orderData.PaymentOk("050");
                            templText = templOk;
                        }
                        else
                        {
                            _orderData = new OrderData(PortalId, Convert.ToInt32(orderid)); // reload the order, becuase the status and typecode may have changed by the payment provider.
                            _orderData.AddAuditMessage(msg, "paymsg", "payment.ascx", "False");
                            _orderData.Save();
                            templText = templFail;
                        }
                        templFooter = ""; // return from bank, hide footer
                    }
                    else
                    {
                        // not returning from bank, so display list of payment providers.
                        rpPaymentGateways.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(GetPaymentProviderTemplates(), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    }

                    if (templText == "") templText = templHeader; // if we are NOT returning from bank, then display normal header summary template
                    templPaymentText = ModCtrl.GetTemplateData(ModSettings, templText, Utils.GetCurrentCulture(), DebugMode);

                    rpDetailDisplay.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(templPaymentText, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    _templateHeader = (GenXmlTemplate) rpDetailDisplay.ItemTemplate;

                    if (templFooter != "")
                    {
                        var templPaymentFooterText = ModCtrl.GetTemplateData(ModSettings, templFooter, Utils.GetCurrentCulture(), DebugMode);
                        rpDetailFooter.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(templPaymentFooterText, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    }

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
            if (UserId > 0 && _cartInfo.UserId == -1) // user may have just logged in.
            {
                _cartInfo.UserId = UserId;
                _cartInfo.Save();
            }

            var orderid = Utils.RequestQueryStringParam(Context, "orderid");

            if ((_provList.Count == 0 || _cartInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal") <= 0) && orderid == "")
            {
                #region "No Payment providers, so process as a ordering system"

                if (_cartInfo != null && _cartInfo.IsValidated())
                {

                    _cartInfo.SaveModelTransQty(); // move qty into trans
                    _cartInfo.ConvertToOrder(DebugMode);
                    _cartInfo.ApplyModelTransQty();

                    // Send emails
                    NBrightBuyUtils.SendEmailOrderToClient("ordercreatedclientemail.html", _cartInfo.PurchaseInfo.ItemID, "ordercreatedemailsubject");
                    NBrightBuyUtils.SendEmailToManager("ordercreatedemail.html", _cartInfo.PurchaseInfo, "ordercreatedemailsubject");

                    // update status to completed
                    _orderData = new OrderData(PortalId, _cartInfo.PurchaseInfo.ItemID);
                    _orderData.SavePurchaseData();

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

                if (Utils.IsNumeric(orderid))
                {
                    // orderid exists, so must be return from bank; Process it!!
                    _orderData = new OrderData(PortalId, Convert.ToInt32(orderid));
                    DoDetail(rpDetailDisplay, _orderData.PurchaseInfo);
                }
                else
                {
                    // display return page
                    DoDetail(rpDetailDisplay, _cartInfo.PurchaseInfo);
                    DoDetail(rpPaymentGateways, _cartInfo.PurchaseInfo);
                    DoDetail(rpDetailFooter, _cartInfo.PurchaseInfo);                    
                }


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

                        orderData.payselectionXml = GenXmlFunctions.GetGenXml(rpPaymentGateways, "", "");
                         
                        orderData.PaymentProviderKey = cArg.ToLower(); // provider keys should always be lowecase
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
                if (prov != null)
                {
                    var templ = prov.GetTemplate(_cartInfo.PurchaseInfo);
                    if (templ == "")
                    {
                        var msgcode = "noproviderdata_" + NotifyCode.warning.ToString();
                        templ = "<div>" + key + "</div>";
                        templ += NBrightBuyUtils.GetResxMessage(msgcode);
                    }
                    strRtn += templ;
                }
            }
            return strRtn;
        }


        #endregion

    }

}
