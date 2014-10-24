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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using NBrightCore.common;
using NBrightCore.render;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class PrintView : CDefault
    {

        private String _itemid = "";
        private String _template = "";
        private String _printcode = "";
        private String _theme = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            try
            {
                _itemid = Utils.RequestParam(HttpContext.Current, "itemid");
                _template = Utils.RequestParam(HttpContext.Current, "template");
                _printcode = Utils.RequestParam(HttpContext.Current, "printcode");
                _theme = Utils.RequestParam(HttpContext.Current, "theme");
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
            var portalId = ((PortalSettings) HttpContext.Current.Items["PortalSettings"]).PortalId;
            var objUserInfo = UserController.GetCurrentUserInfo();
            //var settings = new Dictionary<String,String>();
            //foreach (var item in StoreSettings.Current.Settings())
            //{
            //    settings[item.Key] = item.Value;
            //}
            if (_theme == "") _theme = "Cygnus";
            //settings.Add("themefolder",_theme);

            if (_template != "")
            {
                switch (_printcode)
                {
                    case "printorder":
                        {
                            DisplayOrderData(portalId, objUserInfo, _itemid);
                            break;
                        }
                }           
                
            }
            else
            {
                var strOut = "***ERROR***  Invalid Template";
                var l = new Literal();
                l.Text = strOut;
                phData.Controls.Add(l);
            }
        }

        #endregion

        #region "print display functions"

        private void DisplayOrderData(int portalId, UserInfo userInfo, String entryId)
        {
            var strOut = "***ERROR***  Invalid Data";
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var orderData = new OrderData(portalId, Convert.ToInt32(entryId));
                if (orderData.PurchaseInfo.TypeCode == "ORDER")
                {
                    strOut = "***ERROR***  Invalid Security";
                    if (userInfo.UserID == orderData.UserId || userInfo.IsInRole(StoreSettings.ManagerRole) || userInfo.IsInRole(StoreSettings.EditorRole))
                    {
                        //check the payment provider for a print url
                        var shippingprovider = orderData.PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");
                        if (shippingprovider != "")
                        {
                            var shipprov = ShippingInterface.Instance(shippingprovider);
                            if (shipprov != null)
                            {
                                if (_template == "printdeliverylabel.htm")
                                {
                                    var printurl = shipprov.GetDeliveryLabelUrl(orderData.PurchaseInfo);
                                    if (printurl != "") Response.Redirect(printurl);                                    
                                }
                            }
                        }

                        // not provider label, so print template
                        var modCtrl = new NBrightBuyController();
                        var strTempl = modCtrl.GetTemplateData(-1, _template, Utils.GetCurrentCulture(), StoreSettings.Current.Settings(), StoreSettings.Current.DebugMode);

                        strOut = GenXmlFunctions.RenderRepeater(orderData.PurchaseInfo, strTempl, "", "XMLData", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                        if (_template.EndsWith(".xsl")) strOut = XslUtils.XslTransInMemory(orderData.PurchaseInfo.XMLData, strOut);                       
                    }
                }
            }
            var l = new Literal();
            l.Text = strOut;
            phData.Controls.Add(l);
        }

        #endregion

    }

}
